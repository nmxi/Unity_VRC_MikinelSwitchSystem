using UnityEditor;

namespace mikinel.vrc.SwitchSystem.Editor
{
    public static class Config
    {
        public static readonly string isFirstLaunchEditorPrefsKey = "mikinel.vrc.SwitchSystem.IsFirstLaunch";
        public static readonly string enableTutorialInfoEditorPrefsKey = "mikinel.vrc.SwitchSystem.EnableTutorialInfo";
        public static readonly string languageEditorPrefsKey = "mikinel.vrc.SwitchSystem.Language";
        
        [MenuItem("MikinelTools/SwitchSystem/Set Language/English")]
        public static void SetLanguage_EN() => SetLanguage("en");
        [MenuItem("MikinelTools/SwitchSystem/Set Language/日本語")]
        public static void SetLanguage_JP() => SetLanguage("ja");
        
        public static void SetLanguage(string language)
        {
            EditorPrefs.SetString(languageEditorPrefsKey, language);

            var tmp = Selection.activeObject;
            Selection.activeObject = null;
            EditorApplication.delayCall += () => Selection.activeObject = tmp;
        }
    }
}