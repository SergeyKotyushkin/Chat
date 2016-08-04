using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;
using Chat.Logic.Elastic;
using Chat.Logic.StructureMap;

namespace Chat.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            StructureMapFactory.Init();
            var container = StructureMapFactory.GetContainer();
            container.Configure(x => x.For<IControllerActivator>().Use<StructureMapControllerActivator>());

            DependencyResolver.SetResolver(new StructureMapDependencyResolver(container));
            var config = GlobalConfiguration.Configuration;
            config.Services.Replace(typeof(IHttpControllerActivator), new StructureMapWebApiActivator(config));

            ElasticRepository.ElasticSearchCreateIndices();
        }
    }
}