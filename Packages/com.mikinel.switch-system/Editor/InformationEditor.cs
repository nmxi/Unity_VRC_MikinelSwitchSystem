using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace mikinel.vrc.SwitchSystem
{
    [CustomEditor(typeof(Information))]
    public class InformationEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset _informationUxml;
        
        public override VisualElement CreateInspectorGUI()
        {
            var container = _informationUxml.CloneTree();
            
            var root = new VisualElement();
            
            root.Add(container);

            var versionLabel = root.Q<Label>("Version");
            versionLabel.text = $"{Version.CurrentVersion()}";

            return container;
        }
    }
}