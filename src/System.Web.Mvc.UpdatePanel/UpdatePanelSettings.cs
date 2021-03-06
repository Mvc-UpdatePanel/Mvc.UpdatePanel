﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace System.Web.Mvc
{
    public class UpdatePanelSettings
    {
        /// <summary>
        /// Javascript function executed after creating update panel
        /// </summary>
        public string JSInit { get; set; }
        
        /// <summary>
        /// Javascript function executed if error occurs on server
        /// </summary>
        public string JSOnError { get; set; }

        /// <summary>
        /// Javascript function executed if error occurs on server
        /// </summary>
        public string JSOnSuccess { get; set; }
        /// <summary>
        /// Optional Id, if not supplied Panel will have ID autogenerated by index
        /// </summary>
        public string Id { get; set; }

        public UpdatePanelLoadMode LoadMode { get; set; }

        /// <summary>
        /// Custom rendering of UpdatePanel loading div
        /// </summary>
        [ScriptIgnore]
        public UpdatePanelRenderLoadingDiv RenderLoadingDiv { get; set; }

        /// <summary>
        /// Custom rendering of UpdatePanel container
        /// </summary>
        [ScriptIgnore]
        public UpdatePanelRenderCustomParentContainer RenderParentContainer { get; set; }

        public NameValueCollection Parameters { get; protected set; }

        public UpdatePanelSettings()
        {
            Parameters = new NameValueCollection();
        }

    }

    public enum UpdatePanelLoadMode
    {
        RequestOnDocumentReady=0,
        RenderWithParent=1,
        ManualOnly=2
    }

    public delegate object UpdatePanelRenderCustomParentContainer(UpdatePanelScope updatePanel);
    public delegate object UpdatePanelRenderLoadingDiv(UpdatePanelScope updatePanel);
}
