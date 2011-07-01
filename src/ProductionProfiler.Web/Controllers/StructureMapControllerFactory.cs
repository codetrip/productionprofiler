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

        //public override void ReleaseController(IController controller)
        //{
        //    try
        //    {
        //        if(HttpContext.Current != null)
        //        {
        //            ObjectFactory.
        //        }
                
        //    }
        //    catch (ArgumentNullException)
        //    {
        //        //if there is an exception in the instantiation of the controller
        //        //we get an argumentnull exception here.  Not sure how to fix it
        //        //so lets just swallow it as otherwise it masks the real exception.
        //    }
        //}
    }
}