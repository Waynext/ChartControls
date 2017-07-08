using DataSource.Properties;
using MyParser.Dzh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace DataSource
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        public const string dzhKey = "DzhFolderHelp";

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            LoadDzh();
        }

        private void LoadDzh()
        {
            if(!string.IsNullOrEmpty(Settings.Default.DzhFolder))
            {
                DZHFolderHelp helper = new DZHFolderHelp(Settings.Default.DzhFolder, false);
                this.Application.Add(dzhKey, helper);
            }
            
        }
    }
}