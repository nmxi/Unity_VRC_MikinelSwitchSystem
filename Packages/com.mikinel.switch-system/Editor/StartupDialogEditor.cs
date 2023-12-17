using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [InitializeOnLoad]
    public class StartupDialogEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset _startupDialogUxml;

        static StartupDialogEditor()
        {
            Debug.Log("StartupDialogEditor");
            
            Debug.Log("Reload language data");
            Config.ReloadLanguageData();
        }
    }
}