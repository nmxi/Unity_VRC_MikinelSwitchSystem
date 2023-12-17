using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace mikinel.vrc.SwitchSystem.Editor
{
    public static class Config
    {
        public static readonly string packagePath = "Packages/com.mikinel.switch-system";
        
        public static readonly string isFirstLaunchEditorPrefsKey = "mikinel.vrc.SwitchSystem.IsFirstLaunch";
        public static readonly string enableTutorialInfoEditorPrefsKey = "mikinel.vrc.SwitchSystem.EnableTutorialInfo";
        public static readonly string languageEditorPrefsKey = "mikinel.vrc.SwitchSystem.Language";
        
        public static readonly string defaultLanguageCode = "en";

        [MenuItem("MikinelTools/SwitchSystem/Reload Lang Data")]
        public static void ReloadLanguageData()
        {
            //Editor/langにあるScriptableObjectを取得
            var languageDataSets = AssetDatabase.FindAssets("t:LanguageDataSet", new[] { $"{packagePath}/Editor/lang" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<LanguageDataSet>)
                .ToList();
            
            AddMenuItem("MikinelTools/SwitchSystem/Set Language/en", 0, () => Debug.Log("en"));

            for (var i = 0; i < languageDataSets.Count; i++)
            {
                var languageDataSet = languageDataSets[i];
                Debug.Log($"Load lang : {languageDataSet.languageDisplayName}");

                var displayName = languageDataSet.languageCode;
                var name = $"MikinelTools/SwitchSystem/Set Language/{displayName}";

                AddMenuItem(name, i, () => SetLanguage(languageDataSet.languageCode));
            }
            
            var internalUpdateAllMenus = typeof(EditorUtility).GetMethod("Internal_UpdateAllMenus", BindingFlags.Static | BindingFlags.NonPublic);
            internalUpdateAllMenus?.Invoke(null, null);
        }

        private static void AddMenuItem(string name, int priority, Action action)
        {
            var addMenuItemMethod = typeof(Menu).GetMethod("AddMenuItem", BindingFlags.Static | BindingFlags.NonPublic);
            addMenuItemMethod?.Invoke(null, new object[] { name, null, null, priority, action, null });
        }
        
        public static void SetLanguage(string language)
        {
            Debug.Log($"SetLanguage : {language}");
            
            EditorPrefs.SetString(languageEditorPrefsKey, language);

            var tmp = Selection.activeObject;
            Selection.activeObject = null;
            EditorApplication.delayCall += () => Selection.activeObject = tmp;
        }
        
        public static LanguageDataSet GetLanguageDataSet()
        {
            var language = EditorPrefs.GetString(languageEditorPrefsKey, defaultLanguageCode);
            var languageDataSet = AssetDatabase.FindAssets($"t:LanguageDataSet {language}", new[] { $"{packagePath}/Editor/lang" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<LanguageDataSet>)
                .FirstOrDefault();
            
            if (languageDataSet == null)
            {
                Debug.LogError($"LanguageDataSet not found : {language}");
                
                ReloadLanguageData();
                
                SetLanguage(defaultLanguageCode);
                
                return GetLanguageDataSet();
            }

            return languageDataSet;
        }
    }
}