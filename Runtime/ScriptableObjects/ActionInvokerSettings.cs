using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.whisper.devpanelwizard
{
    public class ActionInvokerSettings : ScriptableObject
    {
        public List<MonoScript> monitoredScripts = new();
    }
}