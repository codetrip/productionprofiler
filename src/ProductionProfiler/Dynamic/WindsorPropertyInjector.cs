
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using ProductionProfiler.Core.IoC;
using ProductionProfiler.Core.Profiling;

namespace ProductionProfiler.Core.Dynamic
{
    public abstract class PropertyInjectorBase
    {
        protected static TProperty ResolveProperty<TProperty>()
        {
            return ProfilerContext.Container.Resolve<TProperty>();
        }
    }

    public static class WindsorPropertyInjectorExtension
    {
        #region constants

        private const string DynamicNamespace = "Asos.Core.Dynamic";
        private const bool KeepTempFiles = true;
        private static readonly string _dynamicClassName = "PropertyInjector_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
        private static readonly string _fqDynamicClassName = DynamicNamespace + "." + _dynamicClassName;

        #endregion

        private static readonly Dictionary<Type, Action<object>> _delegateRepository = new Dictionary<Type, Action<object>>();
        private static readonly object _syncLock = new object();

        /// <summary>
        /// Injects all properties on the component from Windsor (type resolution only).
        /// Note: this should only be used for components that cannot just be loaded straight from the container (e.g. Action Filters).
        /// </summary>
        public static void InjectProperties(this object component)
        {
            if (component == null)
                return;

            var componentType = component.GetType();

            if (!_delegateRepository.ContainsKey(componentType))
            {
                lock (_syncLock)
                {
                    if (!_delegateRepository.ContainsKey(componentType))
                        _delegateRepository[component.GetType()] = BuildPropertyInjector(componentType);
                }
            }

            _delegateRepository[componentType](component);
        }

        private static Action<object> BuildPropertyInjector(Type componentType)
        {
            CodeNamespace ns = BuildNamespace(componentType);
            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            codeCompileUnit.Namespaces.Add(ns);

            CompilerResults compilerResults = DynamicCodeGeneratorHelper.Compile(codeCompileUnit, KeepTempFiles, new [] {"System.Web.dll", "System.dll"});

            Type summaryWriterType = compilerResults.CompiledAssembly.GetType(_fqDynamicClassName);

            return (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), summaryWriterType, "InjectProperties");
        }

        /// <summary>
        /// Creates a populated namespace containing code for a class
        /// that has a InjectProperties method for T.
        /// </summary>
        private static CodeNamespace BuildNamespace(Type componentType)
        {
            CodeNamespace ns = new CodeNamespace(DynamicNamespace);
            CodeTypeDeclaration cls = new CodeTypeDeclaration(_dynamicClassName);
            ns.Types.Add(cls);

            cls.BaseTypes.Add(typeof(PropertyInjectorBase));

            //public static void InjectProperties(T target)
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            method.Name = "InjectProperties";
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "target"));
            cls.Members.Add(method);

            // <type> targetAsType = (<type>) target;
            var targetAsTypeDeclaration = new CodeVariableDeclarationStatement(componentType, "targetAsType", new CodeCastExpression(componentType, new CodeVariableReferenceExpression("target")));
            method.Statements.Add(targetAsTypeDeclaration);

            foreach (PropertyInfo prop in componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Any()))
            {
                if (prop.GetSetMethod(false) == null)
                {
                    continue;
                }

                // Only generate the assignement if the type can be resolved
                if (ProfilerContext.Container.HasObject(prop.PropertyType))
                {
                    // target.Property = ResolveProperty<PropertyType>()
                    CodeAssignStatement assignStatement = new CodeAssignStatement(
                        new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("targetAsType"), prop.Name),
                        new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(_dynamicClassName), "ResolveProperty", new CodeTypeReference(prop.PropertyType))));

                    method.Statements.Add(assignStatement);
                }
            }

            return ns;
        }
    }

}