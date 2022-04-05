using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BarelyAStudio.AnimatorConstantFieldsGenerator
{
    public class AnimatorConstantFieldsGeneratorSettings : ScriptableObject
    {
        public const string k_MyCustomSettingsPath = "Assets/AnimatorConstantFieldsGeneratorSettings.asset";

        public string @namespace = "GeneratedNamespace";

        public string fileName = "Assets/AnimatorConstants.cs";

        internal static AnimatorConstantFieldsGeneratorSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<AnimatorConstantFieldsGeneratorSettings>(k_MyCustomSettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<AnimatorConstantFieldsGeneratorSettings>();
                settings.@namespace = "GeneratedNamespace";
                settings.fileName = "Assets/AnimatorConstants.cs";
                AssetDatabase.CreateAsset(settings, k_MyCustomSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    // Register a SettingsProvider using IMGUI for the drawing framework:
    public static class CustomSettingsIMGUIRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider("Project/AnimatorConstantFieldsGeneratorSettings", SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "Animator Constant Fields Generator",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var settings = AnimatorConstantFieldsGeneratorSettings.GetSerializedSettings();
                    EditorGUILayout.PropertyField(settings.FindProperty("namespace"), new GUIContent("Namespace"));
                    EditorGUILayout.PropertyField(settings.FindProperty("fileName"), new GUIContent("File Name And Location"));
                    settings.ApplyModifiedPropertiesWithoutUndo();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[]
                {
                    "Animator", "Constants", "Fields"
                })
            };

            return provider;
        }
    }
}