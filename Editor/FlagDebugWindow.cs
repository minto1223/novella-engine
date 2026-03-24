#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Novella.Core;

/// <summary>
/// Novella > Flag Debug Window
/// プレイモード中にフラグの一覧を表示・編集できるエディタウィンドウ。
/// </summary>
public class FlagDebugWindow : EditorWindow
{
    private Vector2 _scrollPos;
    private string _newFlagName = "";
    private string _newFlagValue = "";

    [MenuItem("Novella/Flag Debug Window")]
    public static void ShowWindow()
    {
        GetWindow<FlagDebugWindow>("Flag Debug");
    }

    private void OnGUI()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Play mode only.", MessageType.Info);
            return;
        }

        var engine = FindFirstObjectByType<NovellaEngine>();
        if (engine == null)
        {
            EditorGUILayout.HelpBox("NovellaEngine not found.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Current Script", engine.CurrentScriptPath ?? "(none)");
        EditorGUILayout.LabelField("Command Index", engine.CurrentIndex.ToString());
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Flags", EditorStyles.boldLabel);

        var flags = engine.Flags.GetAll();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        var keysToRemove = new List<string>();
        var keysToUpdate = new Dictionary<string, string>();

        foreach (var kv in flags)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(kv.Key, GUILayout.Width(150));
            string newVal = EditorGUILayout.TextField(kv.Value);
            if (newVal != kv.Value)
                keysToUpdate[kv.Key] = newVal;

            if (GUILayout.Button("X", GUILayout.Width(25)))
                keysToRemove.Add(kv.Key);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // 変更を適用
        foreach (var kv in keysToUpdate)
            engine.Flags.Set(kv.Key, kv.Value);

        foreach (var key in keysToRemove)
        {
            var all = engine.Flags.GetAll();
            all.Remove(key);
            engine.Flags.SetAll(all);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Add Flag", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        _newFlagName = EditorGUILayout.TextField(_newFlagName, GUILayout.Width(150));
        _newFlagValue = EditorGUILayout.TextField(_newFlagValue);
        if (GUILayout.Button("+", GUILayout.Width(25)) && !string.IsNullOrEmpty(_newFlagName))
        {
            engine.Flags.Set(_newFlagName, _newFlagValue);
            _newFlagName = "";
            _newFlagValue = "";
        }
        EditorGUILayout.EndHorizontal();

        if (flags.Count > 0)
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Clear All Flags"))
                engine.Flags.Clear();
        }

        Repaint();
    }
}
#endif
