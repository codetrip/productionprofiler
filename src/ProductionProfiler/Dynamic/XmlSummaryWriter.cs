using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Serialization;

namespace ProductionProfiler.Core.Dynamic
{
    /// <summary>
    /// Base class for the dynamically generated SummaryWriter classes.
    /// </summary>
    public abstract class XmlEntitySummaryWriterBase
    {
        protected static void AddDictionary<TKey, TValue>(string name, IDictionary<TKey, TValue> value, XElement xml)
        {
            XElement nestedType = new XElement(DynamicCodeHelper.CleanName(name));

            if (value == null)
            {
                AddSimpleType(name, null, nestedType);
                return;
            }

            foreach (KeyValuePair<TKey, TValue> pair in value)
            {
                nestedType.Add(DynamicCodeHelper.IsSimpleType(typeof(TValue))
                    ? new XElement(DynamicCodeHelper.CleanName(pair.Key.ToString()), pair.Value.ToString())
                    : new XElement(DynamicCodeHelper.CleanName(pair.Key.ToString()), pair.Value.GetSummary()));
            }

            xml.Add(nestedType);
        }

        protected static void AddDefault<T>(string name, T value, XElement xml)
        {
            XElement nestedType = new XElement(DynamicCodeHelper.CleanName(name));

            if (value is IEnumerable)
            {
                foreach (object enumerableProperty in (IEnumerable)value)
                {
                    nestedType.Add(new XElement(DynamicCodeHelper.CleanName(enumerableProperty.GetType().Name), enumerableProperty.ToString()));
                }
            }
            else
            {
                if (DynamicCodeHelper.IsSimpleType(typeof(T)))
                    AddSimpleType(name, value, nestedType);
                else
                    nestedType.Add(value.GetSummary());
            }

            xml.Add(nestedType);
        }

        protected static void AddSingleParameterGenericType<T, T2>(string name, object value, XElement xml)
        {
            XElement nestedType = new XElement(DynamicCodeHelper.CleanName(name));

            if (value == null)
            {
                AddSimpleType(name, null, nestedType);
                return;
            }

            if (value is IEnumerable && typeof(T).IsGenericType)
            {
                foreach (T2 enumerableProperty in (IEnumerable)value)
                {
                    nestedType.Add(DynamicCodeHelper.IsSimpleType(typeof(T2))
                        ? new XElement(DynamicCodeHelper.CleanName(enumerableProperty.GetType().Name), ((object)enumerableProperty ?? "null").ToString())
                        : enumerableProperty.GetSummary());
                }
            }
            else if (value is IEnumerable)
            {
                foreach (T2 enumerableProperty in (IEnumerable)value)
                {
                    nestedType.Add(DynamicCodeHelper.IsSimpleType(typeof(T2))
                        ? new XElement(DynamicCodeHelper.CleanName(enumerableProperty.GetType().Name), enumerableProperty.ToString())
                        : enumerableProperty.GetSummary());
                }
            }
            else if (value is T2)
            {
                nestedType.Add(((T2)value).GetSummary());
            }
            else if (value is T)
            {
                nestedType.Add(((T)value).GetSummary());
            }

            if (nestedType.Elements().Count() > 0)
                xml.Add(nestedType);
        }

        protected static void AddSimpleType(string name, object value, XElement xml)
        {
            xml.Add(new XElement(DynamicCodeHelper.CleanName(name), (value ?? "null").ToString()));
        }
    }

    /// <summary>
    /// Delegate for a method that writes a Summary string for an entity of type T.
    /// </summary>
    public delegate XElement XmlSummaryWriterDelegate<in T>(T entity);

    public static class XmlSummaryWriter
    {
        private const string DynamicNamespace = "ProductionProfiler.Dynamic";
        private const bool KeepTempFiles = true;
        private static readonly string _dynamicClassName = "XmlEntitySummaryWriter_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
        private static readonly string _fqDynamicClassName = DynamicNamespace + "." + _dynamicClassName;
        private static readonly Dictionary<Type, Func<object, XElement>> _delegateRepository = new Dictionary<Type, Func<object, XElement>>();
        private static readonly object _syncLock = new object();

        public static XElement GetSummary(this object entity)
        {
            if (entity == null)
                return null;

            var componentType = entity.GetType();

            if (!_delegateRepository.ContainsKey(componentType))
            {
                lock (_syncLock)
                {
                    if (!_delegateRepository.ContainsKey(componentType))
                        _delegateRepository[componentType] = BuildSummaryWriter(componentType);
                }
            }

            return _delegateRepository[componentType](entity);
        }

        /// <summary>
        /// Creates a SummaryWriter delegate for T.
        /// </summary>
        private static Func<object, XElement> BuildSummaryWriter(Type t)
        {
            CodeNamespace ns = BuildNamespace(t);
            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            codeCompileUnit.Namespaces.Add(ns);

            CompilerResults compilerResults = DynamicCodeHelper.Compile(codeCompileUnit,
                KeepTempFiles,
                "System.dll",
                "System.Drawing.dll",
                "System.Xml.dll",
                "System.Xml.Linq.dll");

            Type summaryWriterType = compilerResults.CompiledAssembly.GetType(_fqDynamicClassName);

            return (Func<object, XElement>)Delegate.CreateDelegate(typeof(Func<object, XElement>), summaryWriterType, "GetSummary");
        }

        /// <summary>
        /// Creates a populated namespace containing code for a class
        /// that has a GetSummary method for T.
        /// </summary>
        private static CodeNamespace BuildNamespace(Type t)
        {
            CodeNamespace ns = new CodeNamespace(DynamicNamespace);
            CodeTypeDeclaration cls = new CodeTypeDeclaration(_dynamicClassName);
            ns.Types.Add(cls);

            cls.BaseTypes.Add(typeof(XmlEntitySummaryWriterBase));

            //public static string GetSummary(T entity)
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            method.Name = "GetSummary";
            method.Parameters.Add(new CodeParameterDeclarationExpression(t, "entity"));
            method.ReturnType = new CodeTypeReference(typeof(XElement));
            cls.Members.Add(method);

            //XElement xElement = new XElement()
            CodeVariableDeclarationStatement initXElement = new CodeVariableDeclarationStatement(
                typeof(XElement), "xElement",
                new CodeObjectCreateExpression(typeof(XElement),
                    new CodePrimitiveExpression(DynamicCodeHelper.CleanName(t.Name))));

            method.Statements.Add(initXElement);

            CodeVariableReferenceExpression entity = new CodeVariableReferenceExpression("entity");

            foreach (PropertyInfo prop in t.GetProperties())
            {
                if (!prop.CanRead)
                    continue;

                if (prop.GetIndexParameters().Any())
                    continue;

                if (prop.GetCustomAttributes(typeof(XmlIgnoreAttribute), false).Length > 0)
                    continue;

                if (prop.GetCustomAttributes(typeof(NonSerializedAttribute), false).Length > 0)
                    continue;

                if (DynamicCodeHelper.IsSimpleType(prop.PropertyType))
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(_dynamicClassName),
                            "AddSimpleType",
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else if (prop.PropertyType.IsGenericType && prop.PropertyType.Name.Contains("Dictionary`2"))
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddDictionary",
                                new[]
                                {
                                    new CodeTypeReference(prop.PropertyType.GetGenericArguments()[0]),
                                    new CodeTypeReference(prop.PropertyType.GetGenericArguments()[1])
                                }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else if (prop.PropertyType.BaseType != null && prop.PropertyType.BaseType.Name.Contains("Dictionary`2"))
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddDictionary",
                                new[]
                                {
                                    new CodeTypeReference(prop.PropertyType.BaseType.GetGenericArguments()[0]),
                                    new CodeTypeReference(prop.PropertyType.BaseType.GetGenericArguments()[1])
                                }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else if (prop.PropertyType.IsGenericType)
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddSingleParameterGenericType",
                                new[]
                                {
                                    new CodeTypeReference(prop.PropertyType),
                                    new CodeTypeReference(prop.PropertyType.GetGenericArguments()[0])
                                }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else if (prop.PropertyType.BaseType != null && prop.PropertyType.BaseType == typeof(Array))
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddSingleParameterGenericType",
                                new[]
                                {
                                    new CodeTypeReference(prop.PropertyType),
                                    new CodeTypeReference(prop.PropertyType.FullName.Replace("[]", "")),
                                }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else if (prop.PropertyType.BaseType != null && prop.PropertyType.BaseType.IsGenericType)
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddSingleParameterGenericType",
                                new[]
                                {
                                    new CodeTypeReference(prop.PropertyType),
                                    new CodeTypeReference(prop.PropertyType.BaseType.GetGenericArguments()[0]),
                                }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
                else
                {
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(_dynamicClassName),
                                "AddDefault",
                                new[]
                                {
                                    new CodeTypeReference(prop.PropertyType),
                                }),
                            new CodePrimitiveExpression(prop.Name),
                            new CodePropertyReferenceExpression(entity, prop.Name),
                            new CodeVariableReferenceExpression("xElement")));
                }
            }

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodeVariableReferenceExpression("xElement"));

            method.Statements.Add(returnStatement);

            return ns;
        }
    }
}
