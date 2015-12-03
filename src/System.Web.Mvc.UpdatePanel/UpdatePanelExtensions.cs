using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.WebPages;

namespace System.Web.Mvc
{
    public static class UpdatePanelExtensions
    {
        /// <summary>
        /// Starts UpdatePanel container
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="config">Optional configuration</param>
        /// <returns></returns>
        public static UpdatePanelScope BeginUpdatePanel(this AjaxHelper helper, Action<UpdatePanelSettings> config = null)
        {
            if (!IsFilterRegistered())
                throw new InvalidOperationException("UpdatePanelFilter must be registered in GlobalFilters");
            //throw new NotImplementedException();
            var settings = new UpdatePanelSettings();
            if (config != null)
                config(settings);
            var panel = new UpdatePanelScope(helper.GetViewPage(), settings);
            panel.Page.UpdatePanelsStack().Push(panel);
            var siblings = panel.SiblingsList();
            siblings.Add(panel);

            var html = _UpdatePanelTemplate_cshtml.BeginUpdatePanel(panel);
            html.WriteTo(panel.Page.Output);

            panel.PushContext();

            return panel;
        }
        const string UpdatePanelsChildrenCountName = "UpdatePanels.ChildrenCount";

        public static void EndUpdatePanel(this UpdatePanelScope panel)
        {
            panel.PopContext();

            var html = _UpdatePanelTemplate_cshtml.EndUpdatePanel(panel);
            html.WriteTo(panel.Page.Output);

            panel.Page.UpdatePanelsStack().Pop();
        }

        public static UpdatePanelScope Parent(this UpdatePanelScope panel)
        {
            return panel.Parents().FirstOrDefault();
        }

        public static string Id(this UpdatePanelScope panel, bool recursive = true)
        {
            if (!recursive)
                return panel.Data().AddOrGetExisting("Id", () => panel.Settings.Id ?? ((panel.IndexInParent() + 1).ToString()));

            var parents = panel.Parents().Reverse();

            return string.Join("/", parents.Concat(new[] { panel }).Select(p => p.Id(false)));
        }

        public static IEnumerable<UpdatePanelScope> Parents(this UpdatePanelScope panel)
        {
            return panel.Page.UpdatePanelsStack().SkipWhile(p => p != panel).Skip(1);
        }

        /// <summary>
        /// Retrieve call back URL
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        public static string GetCallbackUrl(this UpdatePanelScope panel)
        {
            var ctx = panel.Page.ActionContextsStack().Last();
            var originalUrl = new Uri(ctx.HttpContext.Request.RawUrl, UriKind.Relative);
            var url = originalUrl.ExtendQuery(new Dictionary<string, string> { { UpdatePanelUrlParameterName, panel.Id() } });
            return url.ToString();
        }

        public const string UpdatePanelUrlParameterName = "_updatePanel";

        public static UpdatePanelRequestType Requested(this UpdatePanelScope panel)
        {
            return panel.Data().AddOrGetExisting("UpdatePanelRequestType", () =>
            {
                var actionContext = panel.ActionContext();
                var currentId = panel.Id();
                var requestedId = actionContext.UpdatePanelRequestedId();

                if (string.IsNullOrEmpty(requestedId)) return UpdatePanelRequestType.EntireView;
                if (requestedId == currentId) return UpdatePanelRequestType.CurrentPanel;
                if (requestedId.StartsWith(currentId)) return UpdatePanelRequestType.ChildPanel;
                if (currentId.StartsWith(requestedId)) return UpdatePanelRequestType.Parent;
                return UpdatePanelRequestType.OtherPanel;
            });
        }

        public static bool IsFilterRegistered()
        {
            return GlobalFilters.Filters.Select(f => f.Instance).OfType<UpdatePanelFilter>().Any();
        }

        public static bool ShouldExecute(this UpdatePanelScope panel)
        {
            var parent = panel.Parent();
            var parentShouldExecute = parent != null ? parent.ShouldExecute() : true;
            switch (panel.Requested())
            {
                case UpdatePanelRequestType.CurrentPanel:
                case UpdatePanelRequestType.ChildPanel:
                    return true;

                case UpdatePanelRequestType.EntireView:
                case UpdatePanelRequestType.Parent:
                    return parentShouldExecute && panel.Settings.LoadMode == UpdatePanelLoadMode.RenderWithParent;

                case UpdatePanelRequestType.OtherPanel:
                default:
                    return false;
            }
        }



    }

    public enum UpdatePanelRequestType
    {
        EntireView,
        OtherPanel,
        Parent,
        CurrentPanel,
        ChildPanel
    }

    internal static class UpdatePanelInternalExtensions
    {

        public static Stack<UpdatePanelScope> UpdatePanelsStack(this WebViewPage page)
        {
            return page.ActionData().AddOrGetExisting("UpdatePanelsStack", () => new Stack<UpdatePanelScope>());
        }

        public static IDictionary<string, object> ActionData(this UpdatePanelScope panel, bool rootNotCurrent = true)
        {
            return panel.Page.ActionData();
        }


        /// <summary>
        /// Returns Data dictionary from Parent update panel or from Root
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ParentData(this UpdatePanelScope panel)
        {
            var parent = panel.Parent();
            var res = parent != null ? parent.Data() : panel.Page.ActionData();
            return res;
        }

        public static List<UpdatePanelScope> SiblingsList(this UpdatePanelScope panel)
        {
            return panel.ParentData().AddOrGetExisting("UpdatePanel.Children", () => new List<UpdatePanelScope>());
        }

        public static int IndexInParent(this UpdatePanelScope panel)
        {
            return panel.SiblingsList().IndexOf(panel);
        }



        public static void PushContext(this UpdatePanelScope scope)
        {
            var panelShouldRender = scope.ShouldRender();
            var parentShouldRender = scope.ShouldRenderParent();

            var startRendering = parentShouldRender == false && panelShouldRender == true;
            var endRendering = parentShouldRender == true && panelShouldRender == false;

            if (startRendering)
            {
                scope.Page.PushWriter(scope.RootWriter());
            }
            else if (endRendering)
            {
                scope.Page.PushWriter(TextWriter.Null);
            }
        }

        public static StringWriter RootWriter(this UpdatePanelScope scope, bool createIfNotExists = true)
        {
            var writer =
                createIfNotExists ?
                    scope.ActionData(true).AddOrGetExisting("UpdatePanels.Renderer", () => new StringWriter())
                    : scope.ActionData(true).TryGetValue("UpdatePanels.Renderer", null as StringWriter)
                    ;
            return writer;
        }

        public static StringWriter RootWriter(this ControllerContext context, bool createIfNotExists = true)
        {
            var writer =
                createIfNotExists ?
                    context.ActionData(true).AddOrGetExisting("UpdatePanels.Renderer", () => new StringWriter())
                    : context.ActionData(true).TryGetValue("UpdatePanels.Renderer", null as StringWriter)
                    ;
            return writer;
        }


        public static void PopContext(this UpdatePanelScope scope)
        {
            var panelShouldRender = scope.ShouldRender();
            var parentShouldRender = scope.ShouldRenderParent();

            var startRendering = parentShouldRender == false && panelShouldRender == true;
            var endRendering = parentShouldRender == true && panelShouldRender == false;

            if (startRendering || endRendering)
            {
                //var rootWriter = scope.RootWriter(false);
                var writer = scope.Page.PopWriter();
                //if (writer == rootWriter)
                //{
                //    scope.Page.Context.Response.ClearContent();
                //    scope.Page.Context.Response.Output.Write(rootWriter.GetStringBuilder().ToString());
                //    scope.Page.Context.Response.Output = TextWriter.Null;
                //}
            }
        }

        public static string UpdatePanelRequestedId(this ControllerContext context)
        {
            return context.HttpContext.Request.QueryString[UpdatePanelExtensions.UpdatePanelUrlParameterName];
        }

        public static ControllerContext ActionContext(this UpdatePanelScope panel)
        {
            return panel.Page.ActionContextsStack().Last();
        }


        public static bool ShouldRender(this UpdatePanelScope panel)
        {
            var parent = panel.Parent();
            var parentShouldRender = parent != null ? parent.ShouldRender() : true; //root is true
            switch (panel.Requested())
            {
                case UpdatePanelRequestType.CurrentPanel:
                    return true;

                case UpdatePanelRequestType.EntireView:
                case UpdatePanelRequestType.Parent:
                    return parentShouldRender && panel.Settings.LoadMode == UpdatePanelLoadMode.RenderWithParent;

                case UpdatePanelRequestType.OtherPanel:
                case UpdatePanelRequestType.ChildPanel:
                default:
                    return false;
            }
        }




        public static bool ShouldRenderParent(this UpdatePanelScope scope)
        {
            var parent = scope.Parent();
            if (parent != null) return parent.ShouldRender();
            return scope.Requested() == UpdatePanelRequestType.EntireView;
        }

    }
}
