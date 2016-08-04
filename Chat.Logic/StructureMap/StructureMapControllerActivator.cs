using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;
using StructureMap;

namespace Chat.Logic.StructureMap
{
    public class StructureMapControllerActivator : IControllerActivator
    {
        private readonly IContainer _container;

        public StructureMapControllerActivator()
        {
            _container = StructureMapFactory.GetContainer();
        }

        public IController Create(RequestContext requestContext, Type controllerType)
        {
            return (IController)_container.GetInstance(controllerType);
        }
    }

    public class StructureMapWebApiActivator : IHttpControllerActivator
    {
        private readonly IContainer _container;

        public StructureMapWebApiActivator(HttpConfiguration configuration)
        {
            _container = StructureMapFactory.GetContainer();
        }

        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor,
            Type controllerType)
        {
            return (IHttpController) _container.GetInstance(controllerType);
        }
    }
}