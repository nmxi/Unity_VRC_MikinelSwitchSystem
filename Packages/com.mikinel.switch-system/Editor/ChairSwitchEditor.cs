using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Components;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ChairSwitch))]
    public class ChairSwitchEditor : OnOffSwitchEditor
    {
        [SerializeField] private VisualTreeAsset _chairSwitchUxml;
        
        protected SerializedProperty _chairObjectsProperty;
        
        protected ReorderableList _chairObjectsReorderableList;
        
        protected override VisualElement CreateUniqueSettingsGUI()
        {
            base.CreateUniqueSettingsGUI();
            
            var container = _chairSwitchUxml.CloneTree();
            
            var sectionTitle = container.Q<Label>("SectionTitle");
            sectionTitle.text = "Chair Switch Settings";
            
            var mainContainer = container.Q<IMGUIContainer>("MainContainer");
            mainContainer.onGUIHandler = () =>
            {
                if (_chairObjectsReorderableList == null)
                {
                    _chairObjectsReorderableList = EditorUtil.SinglePropertyReorderableList<VRCStation>(
                        serializedObject,
                        _chairObjectsProperty,
                        _languageDataSet.text_chairObjects
                    );
                }
                _chairObjectsReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };
            
            return container;
        }
        
        protected override void FindProperties()
        {
            base.FindProperties();
            
            _chairObjectsProperty = serializedObject.FindProperty("_vrcStations");
        }
    }
}