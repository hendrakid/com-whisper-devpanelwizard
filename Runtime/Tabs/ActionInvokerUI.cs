using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace com.whisper.devpanelwizard
{
    public static class ActionInvokerUI
    {
        private static ActionInvokerSettings _cachedSettings;
        private static Vector2 scroll;
        private static Dictionary<System.Type, bool> foldouts = new();
        private static Dictionary<string, bool> instanceFoldouts = new();
        private static Dictionary<string, object[]> methodParams = new();
        private static Dictionary<Object, List<MethodInfo>> cachedMethodMap = new();
        private static Dictionary<Object, List<FieldInfo>> cachedFieldMap = new();
        private static double lastUpdateTime;
        private const double updateInterval = 1.0;
        private static DevPanelTab devPanelTab;


        public static void Draw(DevPanelTab _devPanelTab)
        {
            if (_devPanelTab == null)
            {
                EditorGUILayout.HelpBox("No ActionInvokerSettings provided.", MessageType.Warning);
                return;
            }
            devPanelTab = _devPanelTab;
            _cachedSettings = devPanelTab.actionInvokerSettings;

            if (_cachedSettings == null)
            {
                EditorGUILayout.HelpBox("No ActionInvokerSettings provided.", MessageType.Warning);
                return;
            }

            DrawInternal(); // Move the full Draw body logic here
        }
        public static void DrawInternal()
        {
            if (_cachedSettings == null)
                _cachedSettings = LoadOrFindSettings();

            if (_cachedSettings == null)
            {
                EditorGUILayout.HelpBox("Create or assign an ActionInvokerSettings asset.", MessageType.Info);
                if (GUILayout.Button("Create Settings Asset"))
                {
                    var asset = ScriptableObject.CreateInstance<ActionInvokerSettings>();
                    var path = EditorUtility.SaveFilePanelInProject("Save Settings", "ActionInvokerSettings", "asset", "Choose save location");
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();
                        _cachedSettings = asset;
                    }
                }
                return;
            }

            EditorGUILayout.Space(10);

            for (int i = 0; i < _cachedSettings.monitoredScripts.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _cachedSettings.monitoredScripts[i] = (MonoScript)EditorGUILayout.ObjectField(_cachedSettings.monitoredScripts[i], typeof(MonoScript), false);

                if (GUILayout.Button("✕", GUILayout.Width(20)))
                    _cachedSettings.monitoredScripts.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Script"))
            {
                _cachedSettings.monitoredScripts.Add(null);
                EditorUtility.SetDirty(_cachedSettings);
            }

            EditorGUILayout.Space(10);
            scroll = EditorGUILayout.BeginScrollView(scroll);

            if (EditorApplication.timeSinceStartup - lastUpdateTime > updateInterval)
            {
                cachedMethodMap.Clear();
                cachedFieldMap.Clear();
                lastUpdateTime = EditorApplication.timeSinceStartup;
            }

            if ((devPanelTab.executionType == ExecutionType.Runtime && EditorApplication.isPlaying) || (devPanelTab.executionType == ExecutionType.Editor && !EditorApplication.isPlaying))
            {
                foreach (var script in _cachedSettings.monitoredScripts.Where(s => s != null))
                {
                    var type = script.GetClass();
                    if (type == null || !typeof(MonoBehaviour).IsAssignableFrom(type)) continue;

                    var instances = Object.FindObjectsOfType(type);
                    if (instances.Length == 0)
                        continue;

                    instances = instances.OrderBy(i => i.name).ToArray();

                    if (!foldouts.ContainsKey(type))
                        foldouts[type] = false;

                    foldouts[type] = EditorGUILayout.Foldout(foldouts[type], $"{type.Name.ToUpper()}", true, EditorStyles.foldoutHeader);
                    EditorGUILayout.Space(6);

                    if (!foldouts[type]) continue;

                    EditorGUI.indentLevel++;


                    foreach (var instance in instances)
                    {
                        string instanceKey = instance.GetInstanceID().ToString();
                        if (!instanceFoldouts.ContainsKey(instanceKey))
                            instanceFoldouts[instanceKey] = false;

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        bool previousState = instanceFoldouts[instanceKey];

                        EditorGUILayout.BeginHorizontal();

                        instanceFoldouts[instanceKey] = EditorGUILayout.Foldout(previousState, instance.name, true);

                        GUIContent imagePing = EditorGUIUtility.IconContent("Transform Icon"); // You can replace with other icons
                        if (GUILayout.Button(imagePing, GUILayout.Width(25), GUILayout.Height(18)))
                        {
                            EditorGUIUtility.PingObject(instance);
                        }

                        EditorGUILayout.EndHorizontal();


                        if (instanceFoldouts[instanceKey])
                        {
                            EditorGUI.indentLevel++;
                            #region field
                            if (!cachedFieldMap.ContainsKey(instance))
                            {
                                cachedFieldMap[instance] = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(f => f.GetCustomAttribute<ExposeVariableAttribute>() != null)
                                    .ToList();
                            }

                            var fields = cachedFieldMap[instance];

                            if (fields.Any())
                            {
                                EditorGUILayout.Space(6);
                                EditorGUILayout.BeginVertical("box");
                                int originalIndent = EditorGUI.indentLevel;
                                EditorGUI.indentLevel = 0; // ← this removes the auto left indent
                                foreach (var field in fields)
                                {
                                    object val = field.GetValue(instance);
                                    object newVal = DrawField(field.FieldType, field.Name, val);
                                    if (!Equals(val, newVal))
                                        field.SetValue(instance, newVal);
                                }
                                EditorGUI.indentLevel = originalIndent; // ← restore it after drawing
                                EditorGUILayout.EndVertical();
                            }
                            #endregion

                            #region method
                            if (!cachedMethodMap.ContainsKey(instance))
                            {
                                cachedMethodMap[instance] = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(m => m.GetCustomAttribute<ActionButtonAttribute>() != null)
                                    .ToList();
                            }

                            var methods = cachedMethodMap[instance];

                            EditorGUILayout.Space(5);
                            EditorGUILayout.BeginVertical("box");

                            float buttonWidth = 200f;
                            float padding = 10f;
                            float spacing = 8f;

                            float viewWidth = EditorGUIUtility.currentViewWidth - 2 * padding;
                            int maxPerRow = Mathf.Max(1, Mathf.FloorToInt(viewWidth / (buttonWidth + spacing)));

                            int methodIndex = 0;

                            while (methodIndex < methods.Count)
                            {
                                // Get method range for this row
                                int rowStart = methodIndex;
                                int rowEnd = Mathf.Min(methodIndex + maxPerRow, methods.Count);

                                // First pass: Calculate max parameters in this row
                                int maxParamsInRow = 0;
                                for (int i = rowStart; i < rowEnd; i++)
                                {
                                    var method = methods[i];
                                    maxParamsInRow = Mathf.Max(maxParamsInRow, method.GetParameters().Length);
                                }

                                // Calculate consistent height based on max params
                                float paramHeight = 20f;
                                float buttonHeight = 25f;
                                float paddingHeight = 10f;
                                float totalHeight = maxParamsInRow * paramHeight + buttonHeight + paddingHeight;

                                // Second pass: Draw each method box with consistent height
                                EditorGUILayout.BeginHorizontal();

                                for (int i = 0; i < maxPerRow && methodIndex < methods.Count; i++, methodIndex++)
                                {
                                    var method = methods[methodIndex];
                                    string key = instance.GetInstanceID() + method.Name;
                                    var parameters = method.GetParameters();

                                    if (!methodParams.ContainsKey(key))
                                        methodParams[key] = new object[parameters.Length];

                                    EditorGUILayout.BeginVertical("box", GUILayout.Width(buttonWidth), GUILayout.Height(totalHeight));

                                    GUILayout.FlexibleSpace(); // Push content down a little if there's empty space

                                    if (parameters.Length == 0)
                                    {
                                        //GUILayout.FlexibleSpace(); // Extra push to vertically align
                                        if (GUILayout.Button(method.Name, GUILayout.Height(buttonHeight)))
                                            method.Invoke(instance, null);
                                    }
                                    else
                                    {
                                        float paramFieldWidth = buttonWidth - 10f; // margin from box

                                        for (int j = 0; j < parameters.Length; j++)
                                            methodParams[key][j] = DrawParam(parameters[j], methodParams[key][j], paramFieldWidth);

                                        if (GUILayout.Button(method.Name, GUILayout.Height(buttonHeight)))
                                            method.Invoke(instance, methodParams[key]);
                                    }

                                    //GUILayout.FlexibleSpace(); // Fill any leftover space

                                    EditorGUILayout.EndVertical();
                                }

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.EndVertical();


                            #endregion

                            EditorGUI.indentLevel--;
                        }

                        EditorGUILayout.EndVertical();
                    }

                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndScrollView();

            EditorUtility.SetDirty(_cachedSettings);
        }

        private static ActionInvokerSettings LoadOrFindSettings()
        {
            var guids = AssetDatabase.FindAssets("t:ActionInvokerSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<ActionInvokerSettings>(path);
            }
            return null;
        }

        private static object DrawParam(ParameterInfo param, object currentValue, float fieldWidth)
        {
            var type = param.ParameterType;
            var label = ObjectNames.NicifyVariableName(param.Name);

            float labelWidth = 80f;
            float fieldOnlyWidth = fieldWidth - labelWidth - 4f;

            int originalIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0; // ← this removes the auto left indent

            EditorGUILayout.BeginHorizontal(GUILayout.Width(fieldWidth));

            EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));

            if (type == typeof(int))
                currentValue = EditorGUILayout.IntField(currentValue != null ? (int)currentValue : 0, GUILayout.Width(fieldOnlyWidth));
            else if (type == typeof(float))
                currentValue = EditorGUILayout.FloatField(currentValue != null ? (float)currentValue : 0f, GUILayout.Width(fieldOnlyWidth));
            else if (type == typeof(bool))
                currentValue = EditorGUILayout.Toggle(currentValue != null && (bool)currentValue, GUILayout.Width(fieldOnlyWidth));
            else if (type == typeof(string))
                currentValue = EditorGUILayout.TextField(currentValue != null ? (string)currentValue : "", GUILayout.Width(fieldOnlyWidth));
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                currentValue = EditorGUILayout.ObjectField(currentValue as UnityEngine.Object, type, true, GUILayout.Width(fieldOnlyWidth));
            else
                GUILayout.Label($"Unsupported: {type.Name}", GUILayout.Width(fieldOnlyWidth));

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel = originalIndent; // ← restore it after drawing

            return currentValue;
        }




        private static object DrawField(System.Type t, string label, object val)
        {
            if (t == typeof(int)) return EditorGUILayout.IntField(label, val is int i ? i : 0);
            if (t == typeof(float)) return EditorGUILayout.FloatField(label, val is float f ? f : 0f);
            if (t == typeof(string)) return EditorGUILayout.TextField(label, val as string ?? "");
            if (t == typeof(bool)) return EditorGUILayout.Toggle(label, val is bool b && b);
            if (t == typeof(Vector2)) return EditorGUILayout.Vector2Field(label, val is Vector2 v2 ? v2 : Vector2.zero);
            if (t == typeof(Vector3)) return EditorGUILayout.Vector3Field(label, val is Vector3 v3 ? v3 : Vector3.zero);
            if (t == typeof(GameObject)) return EditorGUILayout.ObjectField(label, val as GameObject, typeof(GameObject), true);
            if (t == typeof(Transform)) return EditorGUILayout.ObjectField(label, val as Transform, typeof(Transform), true);
            if (t.IsEnum) return EditorGUILayout.EnumPopup(label, val as System.Enum ?? (System.Enum)System.Enum.GetValues(t).GetValue(0));

            EditorGUILayout.LabelField(label, $"Unsupported type: {t.Name}");
            return val;
        }
    }
}