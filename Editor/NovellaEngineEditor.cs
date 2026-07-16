using Novella.Core;
using UnityEditor;
using UnityEngine;

namespace Novella.EditorTools
{
    [CustomEditor(typeof(NovellaEngine))]
    public class NovellaEngineEditor : UnityEditor.Editor
    {
        private static readonly (string field, string label, System.Type iface)[] OverrideSlots =
        {
            ("_messageWindowOverride",  "Message Window",   typeof(IMessageWindow)),
            ("_backgroundOverride",     "Background",       typeof(IBackgroundDisplay)),
            ("_characterLayerOverride", "Character Layer",  typeof(ICharacterDisplay)),
            ("_audioOverride",          "Audio Player",     typeof(IAudioPlayer)),
            ("_choiceUIOverride",       "Choice UI",        typeof(IChoiceUI)),
            ("_backlogUIOverride",      "Backlog UI",       typeof(IBacklogUI)),
            ("_menuUIOverride",         "Menu UI",          typeof(IMenuUI)),
        };

        private bool _showOverrides = true;
        private bool _showTheme = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Default UI Controllers
            EditorGUILayout.LabelField("UI Controllers", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MessageWindow"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Background"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterLayer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Audio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ChoiceUI"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BacklogUI"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MenuUI"));

            EditorGUILayout.Space(8);

            // Custom UI Overrides
            _showOverrides = EditorGUILayout.Foldout(_showOverrides, "Custom UI Overrides", true, EditorStyles.foldoutHeader);
            if (_showOverrides)
            {
                EditorGUILayout.HelpBox(
                    "UI Controllers の代わりに、インターフェースを実装した独自の MonoBehaviour をアサインできます。\n" +
                    "アサインされた場合、そちらが優先されます。",
                    MessageType.Info);

                EditorGUI.indentLevel++;
                foreach (var (field, label, iface) in OverrideSlots)
                {
                    var prop = serializedObject.FindProperty(field);
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(prop, new GUIContent(label));

                    // Validation indicator
                    var mb = prop.objectReferenceValue as MonoBehaviour;
                    if (mb != null)
                    {
                        if (iface.IsInstanceOfType(mb))
                        {
                            var prevColor = GUI.color;
                            GUI.color = Color.green;
                            GUILayout.Label("OK", GUILayout.Width(24));
                            GUI.color = prevColor;
                        }
                        else
                        {
                            var prevColor = GUI.color;
                            GUI.color = Color.red;
                            GUILayout.Label("!!", GUILayout.Width(24));
                            GUI.color = prevColor;
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.HelpBox(
                                $"{mb.GetType().Name} は {iface.Name} を実装していません。",
                                MessageType.Error);
                            continue;
                        }
                    }
                    else
                    {
                        GUILayout.Label("--", GUILayout.Width(24));
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(8);

            // UI Theme
            _showTheme = EditorGUILayout.Foldout(_showTheme, "UI Theme", true, EditorStyles.foldoutHeader);
            if (_showTheme)
            {
                EditorGUILayout.HelpBox(
                    "UI Theme をアサインすると、フォント・色・サイズが一括で適用されます。\n" +
                    "Assets > Create > Novella > UI Theme で作成できます。",
                    MessageType.Info);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UITheme"));
            }

            EditorGUILayout.Space(8);

            // Remaining fields
            EditorGUILayout.LabelField("Auto / Skip", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoLabel"));

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Character Definitions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterDefinitions"), true);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Scene Definitions (Recollection)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SceneDefinitions"), true);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Save Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_saveSlotCount"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
