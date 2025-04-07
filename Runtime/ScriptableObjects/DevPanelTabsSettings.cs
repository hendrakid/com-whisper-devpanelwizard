using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.whisper.devpanelwizard
{
    public class DevPanelTabsSettings : ScriptableObject
    {
        public List<DevPanelTab> tabs = new();
    }

    [Serializable]
    public class DevPanelTab
    {
        public ExecutionType executionType;
        public string tabName;
        public TabContentType contentType;
        public ActionInvokerSettings actionInvokerSettings; // Add this line
    }

    public enum TabContentType
    {
        Empty,
        ActionInvoker,
        Custom1,
        Custom2,
        // etc.
    }
    public enum ExecutionType
    {
        Editor,
        Runtime,
    }
}