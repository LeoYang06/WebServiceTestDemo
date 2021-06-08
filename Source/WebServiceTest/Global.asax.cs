using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace WebServiceTest
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string path = Request.Url.LocalPath;
            if (!path.Contains(".asmx"))
            {
                if (path.Contains(@"testService/Add/"))
                {
                    Context.RewritePath(path.Replace("testService/Add/", "testService.asmx/Add"));
                }
                else if (path.Contains(@"testService/Add"))
                {
                    Context.RewritePath(path.Replace("testService/Add", "testService.asmx/Add"));
                }
                else if (path.Contains(@"testService/"))
                {
                    Context.RewritePath(path.Replace("testService/", "testService.asmx"));
                }
                else if (path.Contains(@"testService"))
                {
                    Context.RewritePath(path.Replace("testService", "testService.asmx"));
                }
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}