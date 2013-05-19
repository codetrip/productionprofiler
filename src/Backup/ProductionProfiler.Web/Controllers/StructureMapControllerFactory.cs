using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using StructureMap;

namespace ProductionProfiler.Web.Controllers
{
    public class StructureMapControllerFactory : DefaultControllerFactory
    {
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                try
                {
                    return base.GetControllerInstance(requestContext, controllerType);
                }
                catch (HttpException)
                {
                    return null;
                }
            }

            try
            {
                IController controller = ObjectFactory.GetInstance(controllerType) as IController;

                if (controller != null)
                    return controller;
            }
            catch (StructureMapException)
            {
                System.Diagnostics.Trace.Write(ObjectFactory.WhatDoIHave());
                throw;
            }

            return base.GetControllerInstance(requestContext, controllerType);
        }
    }
}