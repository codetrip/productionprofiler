using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Windsor;
using ProductionProfiler.Core.Factory;

namespace ProductionProfiler.Web.Controllers
{
    public class WindsorControllerFactory : DefaultControllerFactory
    {
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                try
                {
                    return base.GetControllerInstance(requestContext, null);
                }
                catch (HttpException)
                {
                    return null;
                }
            }

            IContainerAccessor application = requestContext.HttpContext.ApplicationInstance as IContainerAccessor;

            if(application != null)
            {
                IController controller = (IController)application.Container.Resolve(controllerType);
                return controller;
            }

            return base.GetControllerInstance(requestContext, controllerType);
        }

        public override void ReleaseController(IController controller)
        {
            try
            {
                var httpContext = HttpContextFactory.GetHttpContext();
                if (httpContext != null)
                {
                    IContainerAccessor application = httpContext.ApplicationInstance as IContainerAccessor;

                    if (application != null)
                    {
                        application.Container.Release(controller);
                    }
                }
                
            }
            catch (ArgumentNullException)
            {
                //if there is an exception in the instantiation of the controller
                //we get an argumentnull exception here.  Not sure how to fix it
                //so lets just swallow it as otherwise it masks the real exception.
            }
        }
    }
}