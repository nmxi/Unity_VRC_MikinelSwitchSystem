using UnityEditor;
using UnityEditor.UIElements;
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
        
        protected SerializedProperty _isControlRendererProperty;
        protected SerializedProperty _chairObjectsProperty;
        
        protected ReorderableList _chairObjectsReorderableList;
        
        protected override VisualElement CreateUniqueSettingsGUI()
        {
            base.CreateUniqueSettingsGUI();
            
            var container = _chairSwitchUxml.CloneTree();
            
            var sectionTitle = container.Q<Label>("SectionTitle");
            sectionTitle.text = _languageDataSet.text_chairSwitchSettings;
            
            var controlRendererToggle = container.Q<Toggle>("ControlRenderer");
            controlRendererToggle.BindProperty(_isControlRendererProperty);
            controlRendererToggle.label = _languageDataSet.text_controlRenderer;
            controlRendererToggle.RegisterCallback((ChangeEvent<bool> evt) =>
            {
                _isControlRendererProperty.boolValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });

            var mainContainer = container.Q<IMGUIContainer>("MainContainer");
            mainContainer.onGUIHandler = () =>
            {
                if (_chairObjectsReorderableList == null)
                {
                    _chairObjectsReorderableList = EditorUtil.SinglePropertyReorderableList<GameObject>(
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
            
            _isControlRendererProperty = serializedObject.FindProperty("_isControlRenderer");
            _chairObjectsProperty = serializedObject.FindProperty("_chairObjects");
        }
    }
}