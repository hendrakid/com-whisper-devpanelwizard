using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace com.whisper.devpanelwizard
{
    public class DevPanelWizard : EditorWindow
    {
        private DevPanelTabsSettings tabSettings;
        private int selectedTab = 0;
        private Vector2 scrollPos;
        private const string settingsPath = "Assets/Plugins/DevPanelWizard/Runtime/Settings";
        private const float tabHeight = 28;
        private const float tabMinWidth = 100;
        private const float tabMaxWidth = 200;
        private const float removeButtonWidth = 24;
        private const float tabPadding = 6;

        [MenuItem("Tools/Dev Panel Wizard")]
        public static void ShowWindow()
        {
            GetWindow<DevPanelWizard>("Dev Panel Wizard");
        }

        private void OnEnable()
        {
            LoadOrCreateTabSettings();
        }

        private void LoadOrCreateTabSettings()
        {
            string assetPath = $"{settingsPath}/DevPanelTabsSettings.asset";

            if (!Directory.Exists(settingsPath))
                Directory.CreateDirectory(settingsPath);

            tabSettings = AssetDatabase.LoadAssetAtPath<DevPanelTabsSettings>(assetPath);

            if (tabSettings == null)
            {
                tabSettings = ScriptableObject.CreateInstance<DevPanelTabsSettings>();
                AssetDatabase.CreateAsset(tabSettings, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        private void OnGUI()
        {
            if (tabSettings == null) return;

            float windowWidth = position.width - 10;
            float usedWidth = 0;

            if (tabSettings.tabs.Count == 0)
            {
                AddTabButton();
                return;
            }
            // ✅ Vertical stack for entire layout
            EditorGUILayout.BeginVertical();

            // ✅ TABS

            List<int> currentRow = new List<int>();

            for (int i = 0; i < tabSettings.tabs.Count; i++)
            {
                float totalTabWidth = tabMinWidth + removeButtonWidth + tabPadding;

                // Wrap to next line if needed
                if (usedWidth + totalTabWidth > windowWidth)
                {
                    DrawTabRow(currentRow);
                    currentRow.Clear();
                    usedWidth = 0;
                }

                currentRow.Add(i);
                usedWidth += totalTabWidth;
            }

            EditorGUILayout.BeginHorizontal();
            if (currentRow.Count > 0)
                DrawTabRow(currentRow);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10); // ✅ Spacer between tab and content

            // ✅ CONTENT
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (tabSettings.tabs.Count > 0 && selectedTab < tabSettings.tabs.Count)
            {
                var tab = tabSettings.tabs[selectedTab];

                EditorGUI.BeginChangeCheck();

                tab.executionType = (ExecutionType)EditorGUILayout.EnumPopup("Execution Type", tab.executionType);
                tab.tabName = EditorGUILayout.TextField("Tab Name", tab.tabName);

                TabContentType oldType = tab.contentType;
                TabContentType newType = (TabContentType)EditorGUILayout.EnumPopup("Content Type", oldType);

                if (EditorGUI.EndChangeCheck())
                {
                    if (oldType != newType)
                    {
                        tab.contentType = newType;

                        if (newType == TabContentType.ActionInvoker && tab.actionInvokerSettings == null)
                        {
                            if (!Directory.Exists(settingsPath))
                                Directory.CreateDirectory(settingsPath);

                            string safeTabName = string.Join("_", tab.tabName.Split(Path.GetInvalidFileNameChars()));
                            string guid = System.Guid.NewGuid().ToString().Substring(0, 8);
                            string assetFileName = $"ActionInvokerSettings_{safeTabName}_{guid}.asset";
                            string assetPath = Path.Combine(settingsPath, assetFileName);

                            var settingsAsset = ScriptableObject.CreateInstance<ActionInvokerSettings>();
                            AssetDatabase.CreateAsset(settingsAsset, assetPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();

                            tab.actionInvokerSettings = settingsAsset;
                        }

                        EditorUtility.SetDirty(tabSettings);
                        Repaint();
                    }
                }

                switch (tab.contentType)
                {
                    case TabContentType.ActionInvoker:
                        if (tab.actionInvokerSettings == null)
                        {
                            EditorGUILayout.HelpBox("Missing ActionInvokerSettings", MessageType.Warning);
                        }
                        else
                        {
                            ActionInvokerUI.Draw(tab);
                        }
                        break;

                    case TabContentType.Custom1:
                        EditorGUILayout.LabelField("Custom 1 Content");
                        break;

                    case TabContentType.Custom2:
                        EditorGUILayout.LabelField("Custom 2 Content");
                        break;

                    case TabContentType.Empty:
                    default:
                        EditorGUILayout.LabelField("Empty Tab");
                        break;
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical(); // ✅ End full vertical layout
        }

        private void AddTabButton()
        {
            // ✅ Add button row
            if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(tabHeight)))
            {
                var newTab = new DevPanelTab
                {
                    tabName = "New Tab",
                    contentType = TabContentType.Empty
                };

                tabSettings.tabs.Add(newTab);
                selectedTab = tabSettings.tabs.Count - 1;
                EditorUtility.SetDirty(tabSettings);
            }
        }

        private void DrawTabRow(List<int> indices)
        {
            EditorGUILayout.BeginHorizontal();

            foreach (int i in indices)
            {
                var tab = tabSettings.tabs[i];
                bool isSelected = i == selectedTab;

                GUIStyle tabStyle = new GUIStyle(EditorStyles.toolbarButton)
                {
                    fixedHeight = tabHeight,
                    padding = new RectOffset(10, 10, 4, 4),
                    margin = new RectOffset(2, 2, 2, 2)
                };

                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;

                if (GUILayout.Toggle(isSelected, tab.tabName, tabStyle, GUILayout.Width(tabMinWidth)))
                    selectedTab = i;

                GUI.backgroundColor = Color.red;

                if (GUILayout.Button("x", GUILayout.Width(30), GUILayout.Height(tabHeight)))
                {
                    bool confirm = EditorUtility.DisplayDialog(
                        "Remove Tab",
                        $"Are you sure you want to remove the tab '{tab.tabName}'?",
                        "Yes", "Cancel"
                    );
                    if (confirm)
                    {
                        DeleteTabAt(i);
                        GUI.backgroundColor = originalColor;
                        GUIUtility.ExitGUI();
                    }
                }

                GUI.backgroundColor = originalColor;
            }
            if (tabSettings.tabs.Count != 0 && indices.LastOrDefault() + 1 == tabSettings.tabs.Count)
                AddTabButton();

            EditorGUILayout.EndHorizontal();
        }

        private void DeleteTabAt(int index)
        {
            var tab = tabSettings.tabs[index];

            if (tab.actionInvokerSettings != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(tab.actionInvokerSettings);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }

            tabSettings.tabs.RemoveAt(index);
            selectedTab = Mathf.Clamp(selectedTab, 0, tabSettings.tabs.Count - 1);
            EditorUtility.SetDirty(tabSettings);
            AssetDatabase.SaveAssets();
        }
    }
}
