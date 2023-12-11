using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GameObjectStateSwitch))]
    public class GameObjectStateSwitchEditor : StateSwitchEditor
    {
        [SerializeField] private VisualTreeAsset _gameObjectStateSwitchUxml;

        private SerializedProperty _gameObjectStatesProperty;
        private SerializedProperty _gameObjectsProperty;

        private ReorderableList _gameObjectReorderableList;

        protected override VisualElement CreateUniqueSettingsGUI()
        {
            base.CreateUniqueSettingsGUI();
            
            var container = _gameObjectStateSwitchUxml.CloneTree();

            var sectionTitle = container.Q<Label>("SectionTitle");
            sectionTitle.text = _languageDataSet.text_gameObjectStateSwitchSettings;
            
            var mainContainer = container.Q<IMGUIContainer>("MainContainer");
            mainContainer.onGUIHandler = () =>
            {
                if (_gameObjectReorderableList == null)
                {
                    _gameObjectReorderableList = EditorUtil.StateAndPropertyReorderableList<GameObject>(
                        serializedObject,
                        _gameObjectStatesProperty,
                        _gameObjectsProperty,
                        _languageDataSet.text_enableObjects,
                        "GameObject"
                    );
                }
                _gameObjectReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };

            return container;
        }
        
        protected override void FindProperties()
        {
            base.FindProperties();
            
            _gameObjectStatesProperty = serializedObject.FindProperty("_states");
            _gameObjectsProperty = serializedObject.FindProperty("_gameObjects");
        }
    }
}