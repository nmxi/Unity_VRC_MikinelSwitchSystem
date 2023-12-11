using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static mikinel.vrc.SwitchSystem.EditorUtil;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(OnOffSwitch), true)]
    public class OnOffSwitchEditor : SwitchBaseEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = base.CreateInspectorGUI();
            
            var container = new VisualElement();

            //State
            var statePopupField = new PopupField<string>("Initialize State", onOffStateList, _localStateProperty.intValue);
            statePopupField.RegisterValueChangedCallback(evt =>
            {
                _localStateProperty.intValue = evt.newValue == "Off" ? 0 : 1;
                SetVariable(_switchBase, typeof(SwitchBase), "_localState", _localStateProperty.intValue);
                EditorUtility.SetDirty(_switchBase);
            });
            statePopupField.label = _languageDataSet.text_initializeState;
            container.Add(statePopupField);
            statePopupField.style.display = _linkModeProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
            _linkModeToggle.RegisterValueChangedCallback(evt =>
            {
                statePopupField.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
            });
            
            root.Q<VisualElement>("State").Add(container);
            
            //ControlTargetObjects
            var controlTargetObjects = root.Q<IMGUIContainer>("ControlTargetObjects");
            controlTargetObjects.onGUIHandler = () =>
            {
                if (_controlObjectsReorderableList == null)
                {
                    _controlObjectsReorderableList = StateAndPropertyReorderableList<GameObject>(
                        serializedObject,
                        _controlStatesProperty,
                        _controlObjectsProperty,
                        _languageDataSet.text_controlTargetObjects,
                        "GameObject",
                        true
                    );
                }
                _controlObjectsReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };
            
            //UdonEvent (Override)
            var udonEventList = root.Q<IMGUIContainer>("UdonEventList");
            udonEventList.onGUIHandler = () =>
            {
                if (_udonEventReorderableList == null)
                {
                    _udonEventReorderableList = StateUdonCustomEventsReorderableList(
                        serializedObject,
                        _enableAnyStatesProperty,
                        _udonEventStatesProperty,
                        _udonEventTargetsProperty,
                        _udonEventNamesProperty,
                        _languageDataSet.text_udonEvents,
                        true
                    );
                }
                _udonEventReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };


            return root;
        }
    }
}