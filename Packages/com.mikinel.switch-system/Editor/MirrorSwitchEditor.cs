using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MirrorSwitch))]
    public class MirrorSwitchEditor : OnOffSwitchEditor
    {
        [SerializeField] private VisualTreeAsset _mirrorSwitchUxml;

        private SerializedProperty _mirrorObjectsProperty;

        private ReorderableList _mirrorObjectsReorderableList; 
        
        protected override VisualElement CreateUniqueSettingsGUI()
        {
            base.CreateUniqueSettingsGUI();
            
            var container = _mirrorSwitchUxml.CloneTree();

            var sectionTitle = container.Q<Label>("SectionTitle");
            sectionTitle.text = _languageDataSet.text_mirrorSwitchSettings;
            
            var mainContainer = container.Q<IMGUIContainer>("MainContainer");
            mainContainer.onGUIHandler = () =>
            {
                if (_mirrorObjectsReorderableList == null)
                {
                    _mirrorObjectsReorderableList = EditorUtil.SinglePropertyReorderableList<VRC_MirrorReflection>(
                        serializedObject,
                        _mirrorObjectsProperty,
                        _languageDataSet.text_mirrorObjects
                    );
                }
                _mirrorObjectsReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };
            
            return container;
        }

        protected override void FindProperties()
        {
            base.FindProperties();
            
            _mirrorObjectsProperty = serializedObject.FindProperty("_mirrors");
        }
    }
}