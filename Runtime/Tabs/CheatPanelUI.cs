
// Editor/CheatPanelUI.cs
using UnityEditor;
using UnityEngine;

public static class CheatPanelUI
{
    private static Vector2 scroll;

    public static void Draw()
    {
        EditorGUILayout.LabelField("Cheat Panel", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        if (GUILayout.Button("Give Player 1000 Coins"))
        {
            Debug.Log("Cheat: +1000 coins");
        }

        if (GUILayout.Button("Refill Health"))
        {
            Debug.Log("Cheat: Full Health");
        }

        if (GUILayout.Button("Unlock All Levels"))
        {
            Debug.Log("Cheat: All Levels Unlocked");
        }

        if (GUILayout.Button("Teleport to Debug Location"))
        {
            Debug.Log("Cheat: Teleporting Player");
        }

        EditorGUILayout.EndScrollView();
    }
}