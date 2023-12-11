using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GameObjectSwitch))]
    public class GameObjectSwitchEditor : OnOffSwitchEditor
    {
        [SerializeField] private VisualTreeAsset _gameObjectSwitchUxml;

        private SerializedProperty _gameObjectsProperty;

        private ReorderableList _gameObjectsReorderableList; 
        
        protected override VisualElement CreateUniqueSettingsGUI()
        {
            base.CreateUniqueSettingsGUI();
            
            var container = _gameObjectSwitchUxml.CloneTree();

            var sectionTitle = container.Q<Label>("SectionTitle");
            sectionTitle.text = _languageDataSet.text_gameObjectSwitchSettings;
            
            var mainContainer = container.Q<IMGUIContainer>("MainContainer");
            mainContainer.onGUIHandler = () =>
            {
                if (_gameObjectsReorderableList == null)
                {
                    _gameObjectsReorderableList = EditorUtil.SinglePropertyReorderableList<GameObject>(
                        serializedObject,
                        _gameObjectsProperty,
                        _languageDataSet.text_gameObjects
                    );
                }
                _gameObjectsReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };
            
            return container;
        }

        protected override void FindProperties()
        {
            base.FindProperties();
            
            _gameObjectsProperty = serializedObject.FindProperty("_gameObjects");
        }
    }
}