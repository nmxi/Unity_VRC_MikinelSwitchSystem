using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static mikinel.vrc.SwitchSystem.EditorUtil;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StateSwitch))]
    public class StateSwitchEditor : SwitchBaseEditor
    {
        protected SerializedProperty _maxStateProperty;
        
        public override VisualElement CreateInspectorGUI()
        {
            var root = base.CreateInspectorGUI();

            var container = new VisualElement();
            
            //State
            var stateField = new IntegerField("Initialize State");
            stateField.label = _languageDataSet.text_initializeState;
            stateField.BindProperty(_localStateProperty);
            stateField.RegisterValueChangedCallback(evt =>
            {
                var value = evt.newValue;
                if (evt.newValue < 0)
                {
                    value = 0;
                    stateField.SetValueWithoutNotify(value);
                }

                _localStateProperty.intValue = value;
                SetVariable(_switchBase, typeof(SwitchBase), "_localState", value);
                EditorUtility.SetDirty(_switchBase);
            });
            container.Add(stateField);
            stateField.style.display = _linkModeProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex; 
            _linkModeToggle.RegisterValueChangedCallback(evt =>
            {
                stateField.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex; 
            });
            
            //MaxState
            var maxStateField = new IntegerField("MaxState");
            maxStateField.label = _languageDataSet.text_maxState;
            maxStateField.BindProperty(_maxStateProperty);
            maxStateField.RegisterValueChangedCallback(evt =>
            {
                var value = evt.newValue;
                if (evt.newValue < 0)
                {
                    value = 0;
                    maxStateField.SetValueWithoutNotify(value);
                }
                
                _maxStateProperty.intValue = value;
                SetVariable(_switchBase, typeof(StateSwitch), "_maxState", value);
                EditorUtility.SetDirty(_switchBase);
            });
            container.Add(maxStateField);
            maxStateField.style.display = _linkModeProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex; 
            _linkModeToggle.RegisterValueChangedCallback(evt =>
            {
                maxStateField.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex; 
            });
            
            root.Q<VisualElement>("State").Add(container);
            
            return root;
        }
        
        protected override void FindProperties()
        {
            base.FindProperties();

            _maxStateProperty = serializedObject.FindProperty("_maxState");
        }
    }
}