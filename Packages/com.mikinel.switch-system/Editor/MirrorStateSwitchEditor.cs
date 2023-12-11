using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MirrorStateSwitch))]
    public class MirrorStateSwitchEditor : StateSwitchEditor
    {
        [SerializeField] private VisualTreeAsset _mirrorStateSwitchUxml;

        private SerializedProperty _mirrorStatesProperty;
        private SerializedProperty _mirrorObjectsProperty;

        private ReorderableList _mirrorStatesReorderableList;

        protected override VisualElement CreateUniqueSettingsGUI()
        {
            base.CreateUniqueSettingsGUI();
            
            var container = _mirrorStateSwitchUxml.CloneTree();
            
            var sectionTitle = container.Q<Label>("SectionTitle");
            sectionTitle.text = _languageDataSet.text_mirrorStateSwitchSettings;
            
            var mainContainer = container.Q<IMGUIContainer>("MainContainer");
            mainContainer.onGUIHandler = () =>
            {
                if (_mirrorStatesReorderableList == null)
                {
                    _mirrorStatesReorderableList = EditorUtil.StateAndPropertyReorderableList<VRC_MirrorReflection>(
                        serializedObject,
                        _mirrorStatesProperty,
                        _mirrorObjectsProperty,
                        _languageDataSet.text_mirrorObjects,
                        "Mirror"
                    );
                }
                _mirrorStatesReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };

            return container;
        }
        
        protected override void FindProperties()
        {
            base.FindProperties();
            
            _mirrorStatesProperty = serializedObject.FindProperty("_states");
            _mirrorObjectsProperty = serializedObject.FindProperty("_mirrors");
        }
    }
}