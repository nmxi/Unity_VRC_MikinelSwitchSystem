using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ColliderSwitch))]
    public class ColliderSwitchEditor : OnOffSwitchEditor
    {
        [SerializeField] private VisualTreeAsset _colliderSwitchUxml;
        
        protected SerializedProperty _colliderObjectsProperty;
        
        protected ReorderableList _colliderObjectsReorderableList;

        protected override VisualElement CreateUniqueSettingsGUI()
        {
            base.CreateUniqueSettingsGUI();
            
            var container = _colliderSwitchUxml.CloneTree();
            
            var sectionTitle = container.Q<Label>("SectionTitle");
            sectionTitle.text = _languageDataSet.text_colliderSwitchSettings;
            
            var mainContainer = container.Q<IMGUIContainer>("MainContainer");
            mainContainer.onGUIHandler = () =>
            {
                if (_colliderObjectsReorderableList == null)
                {
                    _colliderObjectsReorderableList = EditorUtil.SinglePropertyReorderableList<Collider>(
                        serializedObject,
                        _colliderObjectsProperty,
                        _languageDataSet.text_colliderObjects
                    );
                }
                _colliderObjectsReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };
            
            return container;
        }
        
        protected override void FindProperties()
        {
            base.FindProperties();
            
            _colliderObjectsProperty = serializedObject.FindProperty("_colliders");
        }
    }
}