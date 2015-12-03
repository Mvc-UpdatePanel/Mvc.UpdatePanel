using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace UpdatePanelDemo
{
    public static class Utils
    {
        public static object Collapsible(object content)
        {
            var id = "random_"+Guid.NewGuid().ToString("N").Substring(0,4);
            var res = new StringBuilder();
            res.AppendLine("<button class='btn btn-primary' type='button' data-toggle='collapse' data-target='#{0}' aria-expanded='false' aria-controls='{0}'>X</button>"
                , id);
            res.AppendLine("<div class='collapse' id='{0}'><div class='well'>{1}</div></div>"
                , id, content);

            return new MvcHtmlString(res.ToString());
        }

        public static void AppendLine(this StringBuilder sb, string format, params object[] args)
        {
            sb.AppendLine(string.Format(format, args: args));
        }


        public static IHtmlString ContextInfo(TextWriter writer)
        {
            var res = new StringBuilder();
            res.AppendLine("<pre style='margin:10px'>");
            res.AppendLine("TextWriter");
            res.AppendLine("Type: {0}", writer.GetType().Name);
            res.AppendLine("TextWriter.GetHashCode:{0}", writer.GetHashCode());
            res.AppendLine("TextWriter.ToString:{0}", writer.ToString());
            res.AppendLine("</pre>");
            return new MvcHtmlString(res.ToString());
        }

        public static IHtmlString ContextInfo(WebViewPage page)
        {
            //ResultExecutingContext

            var res = new StringBuilder();
            res.AppendLine("<pre style='margin:10px'>");
            res.AppendLine("Page context");
            res.AppendLine("OutputStack.Count:{0}", page.OutputStack.Count);
            res.AppendLine("Output:{0} {1}", page.Output.GetHashCode(), Collapsible(ContextInfo(page.Output)));
            res.AppendLine("ViewContext:{0} {1}", page.ViewContext.GetHashCode(), Collapsible(ContextInfo(page.ViewContext)));
            res.AppendLine("Html.ViewContext:{0} {1}", page.Html.ViewContext.GetHashCode(), Collapsible(ContextInfo(page.Html.ViewContext)));
            res.AppendLine("HttpContext {0} {1}", page.Context.GetHashCode(), Collapsible(ContextInfo(page.Context)));
            res.AppendLine("PageContext {0} {1}", page.PageContext.GetHashCode(), Collapsible(ContextInfo(page.PageContext)));
            res.AppendLine("</pre>");
            return new MvcHtmlString(res.ToString());
        }

        public static IHtmlString ContextInfo(HttpContextBase context)
        {
            var res = new StringBuilder();
            res.AppendLine("<pre style='margin:10px'>");
            res.AppendLine("HttpContext");
            res.AppendLine("Response.Output: {0} {1}", context.Response.Output.GetHashCode(), Collapsible(ContextInfo(context.Response.Output)));
            res.AppendLine("</pre>");
            return new MvcHtmlString(res.ToString());
        }

        public static IHtmlString ContextInfo(WebPageContext context)
        {
            var res = new StringBuilder();
            res.AppendLine("<pre style='margin:10px'>");
            res.AppendLine("WebPageContext: {0}", context.GetHashCode());
            res.AppendLine("</pre>");
            return new MvcHtmlString(res.ToString());
        }


        public static IHtmlString ContextInfo(ControllerContext context)
        {
            var res = new StringBuilder();
            res.AppendLine("<pre style='margin:10px'>");
            var viewCtx = context as ViewContext;
            if (viewCtx == null)
            {
                res.AppendLine("ControllerContext properties");
            }
            else
            {
                res.AppendLine("ViewContext properties");
                res.AppendFormat("Writer: {0} {1}", viewCtx.Writer.GetHashCode(), Collapsible(ContextInfo(viewCtx.Writer)));
            }

            res.AppendLine("</pre>");
            return new MvcHtmlString(res.ToString());
        }
    }
}
