using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.Mvc
{
    public class UpdatePanelScope : IDisposable
    {
        WebViewPage page;
        public WebViewPage Page { get { return page; } }
        string runtimeId = Guid.NewGuid().ToString();

        public UpdatePanelScope(WebViewPage page, UpdatePanelSettings settings)
        {
            this.page = page;
            this.Settings = settings;
        }
        //public IDictionary<string, object> RequestData()
        //{ return HttpConte.Items.AddOrGetExisting("UpdatePanelContext", () => new Dictionary<string, object>()); }

        public IDictionary<string, object> Data()
        {
            return data;
        }

        Dictionary<string, object> data = new Dictionary<string, object>();


        public UpdatePanelSettings Settings {
            get; protected set;
        }

        public void Dispose()
        {
            this.EndUpdatePanel();
        }


    }


    public class UpdatePanelFilter: IActionFilter, IResultFilter
    {

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var writer = filterContext.Controller.ControllerContext.RootWriter(false);
            if (writer != null)
            {
                filterContext.HttpContext.Response.ClearContent();
                filterContext.HttpContext.Response.Write(writer.GetStringBuilder().ToString());
            }
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }
    }

}
