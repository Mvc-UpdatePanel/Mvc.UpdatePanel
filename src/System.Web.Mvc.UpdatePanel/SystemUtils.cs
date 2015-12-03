using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.Mvc
{
    public static class SystemUtils
    {
        public static WebViewPage GetViewPage(this HtmlHelper helper) {
            return helper.ViewDataContainer as WebViewPage;
        }
        public static WebViewPage GetViewPage(this AjaxHelper helper)
        {
            return helper.ViewDataContainer as WebViewPage;
        }

        public static IEnumerable<ControllerContext> ActionContextsStack(this WebViewPage page)
        {
            var ctx = page.ViewContext.Controller.ControllerContext;
            while (ctx != null)
            {
                yield return ctx;
                ctx = ctx.ParentActionViewContext!=null? ctx.ParentActionViewContext.Controller.ControllerContext : null;
            }
        }

        public static IEnumerable<ControllerContext> ActionContextsStack(this ControllerContext context)
        {
            var ctx = (context is ViewContext)? (context as ViewContext).Controller.ControllerContext: context;
            while (ctx != null)
            {
                yield return ctx;
                ctx = ctx.ParentActionViewContext != null ? ctx.ParentActionViewContext.Controller.ControllerContext : null;
            }
        }

        public static IDictionary<string, object> ActionData(this WebViewPage page, bool rootNotCurrent = true)
        {
            var actionsCount = page.ActionContextsStack().Count();
            var cc = rootNotCurrent? page.ActionContextsStack().Last() : page.ActionContextsStack().First();
            return cc.Controller.ViewData.AddOrGetExisting("UpdatePanels.ActionData", () => new Dictionary<string, object>());
        }

        public static IDictionary<string, object> ActionData(this ControllerContext context, bool rootNotCurrent = true)
        {
            var actionsCount = context.ActionContextsStack().Count();
            var cc = rootNotCurrent ? context.ActionContextsStack().Last() : context.ActionContextsStack().First();
            return cc.Controller.ViewData.AddOrGetExisting("UpdatePanels.ActionData", () => new Dictionary<string, object>());
        }



        public static IDictionary<string, object> ViewData(this WebViewPage page) { return page.ViewData; }

        /// <summary>
        /// Taken from here: http://stackoverflow.com/a/27545010/508797
        ///     Adds query string value to an existing url, both absolute and relative URI's are supported.
        /// </summary>
        /// <example>
        /// <code>
        ///     // returns "www.domain.com/test?param1=val1&amp;param2=val2&amp;param3=val3"
        ///     new Uri("www.domain.com/test?param1=val1").ExtendQuery(new Dictionary&lt;string, string&gt; { { "param2", "val2" }, { "param3", "val3" } }); 
        /// 
        ///     // returns "/test?param1=val1&amp;param2=val2&amp;param3=val3"
        ///     new Uri("/test?param1=val1").ExtendQuery(new Dictionary&lt;string, string&gt; { { "param2", "val2" }, { "param3", "val3" } }); 
        /// </code>
        /// </example>
        /// <param name="uri"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Uri ExtendQuery(this Uri uri, IDictionary<string, string> values)
        {
            var baseUrl = uri.ToString();
            var queryString = string.Empty;
            if (baseUrl.Contains("?"))
            {
                var urlSplit = baseUrl.Split('?');
                baseUrl = urlSplit[0];
                queryString = urlSplit.Length > 1 ? urlSplit[1] : string.Empty;
            }

            NameValueCollection queryCollection = HttpUtility.ParseQueryString(queryString);
            foreach (var kvp in values ?? new Dictionary<string, string>())
            {
                queryCollection[kvp.Key] = kvp.Value;
            }
            var uriKind = uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative;
            return queryCollection.Count == 0
              ? new Uri(baseUrl, uriKind)
              : new Uri(string.Format("{0}?{1}", baseUrl, queryCollection), uriKind);
        }


        //public static void PushWriter(this ControllerContext context, TextWriter writer)
        //{
        //    //var viewCtx = context as ViewContext;
        //    //var data = viewCtx != null ? viewCtx.TempData : context.Controller.TempData;
        //    //data.AddOrGetExisting("UpdatePanelExtensions.Writers", () => new Stack<TextWriter>(new[] { viewCtx != null ? view }));
        //}
        //public static TextWriter PopWriter(this ControllerContext context)
        //{
        //    return null;
        //}

        public static void PushWriter(this WebViewPage page, TextWriter writer)
        {
            page.OutputStack.Push(writer);
            page.ViewContext.Writer = writer;
        }
        public static TextWriter PopWriter(this WebViewPage page)
        {
            var res = page.OutputStack.Pop();
            page.ViewContext.Writer = page.OutputStack.Peek();
            return res;
        }





    }


    /// <summary>
    /// Extensions helping to work with dictionary easier
    /// Latest version is here: https://gist.github.com/pmunin/28d0ba1acce677736c5e
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Tries to get value by key from the dictionary. If cannot find, return the default TValue.
        /// </summary>
        /// <typeparam name="TKey">key type of the dictionary</typeparam>
        /// <typeparam name="TValue">Value type of the dictionary</typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue resValue = default(TValue);
            if (!dictionary.TryGetValue(key, out resValue))
                return default(TValue);
            return resValue;
        }

        /// <summary>
        /// Tries to get value by key from the dictionary. If cannot find, return the default value supplied.
        /// </summary>
        /// <typeparam name="TKey">key type of the dictionary</typeparam>
        /// <typeparam name="TValue">Value type of the dictionary</typeparam>
        /// <typeparam name="TCastValue">Type to cast return value to</typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="defaultCastIfNotExist">default value returned if the key-value pair does not exist in the dictionary</param>
        /// <returns></returns>
        public static TCastValue TryGetValue<TKey, TValue, TCastValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TCastValue defaultCastIfNotExist)
            where TCastValue : TValue
        {
            TValue resValue = default(TValue);
            if (!dictionary.TryGetValue(key, out resValue))
                return defaultCastIfNotExist;
            return (TCastValue)resValue;
        }

        /// <summary>
        /// Tries to get value by key from the dictionary. If cannot find, generate it using function supplied.
        /// </summary>
        /// <typeparam name="TKey">key type of the dictionary</typeparam>
        /// <typeparam name="TValue">Value type of the dictionary</typeparam>
        /// <typeparam name="TCastValue">Type to cast return value to</typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="getDefaultValueIfNotExist">function that generates value if the key-value pair does not exist in the dictionary</param>
        /// <returns></returns>
        public static TCastValue TryGetValue<TKey, TValue, TCastValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TCastValue> getDefaultValueIfNotExist)
            where TCastValue : TValue
        {
            TValue resValue = default(TValue);
            if (!dictionary.TryGetValue(key, out resValue))
                return getDefaultValueIfNotExist();
            return (TCastValue)resValue;
        }


        /// <summary>
        /// Tries to get and return value from dictionary and if it does not contain the key or it's no longer valid (checks with supplied function), 
        /// it generates new value and add it using supplied function. Can be used by HttpContext.Items
        /// </summary>
        /// <typeparam name="TCastValue"></typeparam>
        /// <param name="storage"></param>
        /// <param name="key"></param>
        /// <param name="createValueToAddIfNotExist">
        /// generates value to add to the dictionary in case dictionary does not contain the key or current value is not valid accodding to isStillValid parameter
        /// </param>
        /// <param name="isStillValid">function that checks if existing value is still valid</param>
        /// <returns></returns>
        public static TCastValue AddOrGetExisting<TCastValue>(this System.Collections.IDictionary storage, object key, Func<TCastValue> createValueToAddIfNotExist, Func<object, TCastValue, bool> isStillValid = null)
        {
            var hasValue = storage.Contains(key);
            var res = (TCastValue)storage[key];
            if (hasValue && (isStillValid == null || isStillValid(key, res))) return res;
            storage[key] = res = createValueToAddIfNotExist();
            return res;
        }

        /// <summary>
        /// Tries to get and return value from dictionary and if it does not contain the key or it's no longer valid (checks with supplied function), 
        /// it generates new value and add it using supplied function. Can be used by HttpContext.Items
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TCastValue">Cast type</typeparam>
        /// <param name="storage"></param>
        /// <param name="key"></param>
        /// <param name="createValueToAddIfNotExist">
        /// generates value to add to the dictionary in case dictionary does not contain the key or current value is not valid accodding to isStillValid parameter
        /// </param>
        /// <param name="isStillValid">function that checks if existing value is still valid</param>
        /// <returns></returns>
        public static TCastValue AddOrGetExisting<TKey, TCastValue, TValue>(this IDictionary<TKey, TValue> storage, TKey key, Func<TCastValue> createValueToAddIfNotExist, Func<TKey, TCastValue, bool> isStillValid = null)
            where TCastValue : TValue
        {
            TValue value = default(TValue);
            TCastValue result = default(TCastValue);
            var hasValue = storage.TryGetValue(key, out value);
            if (hasValue && (isStillValid == null || isStillValid(key, (TCastValue)value))) return (TCastValue)value;
            storage[key] = result = createValueToAddIfNotExist();
            return result;
        }

    }


}
