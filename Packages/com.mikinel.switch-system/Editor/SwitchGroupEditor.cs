using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static mikinel.vrc.SwitchSystem.EditorUtil;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SwitchGroup), true)]
    public class SwitchGroupEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset _baseUxml;

        protected bool _enableTutorialInfo;
        protected LanguageDataSet _languageDataSet;
        
        protected SwitchGroup OnSwitchGroup;
        
        protected SerializedProperty _switchesProperty;
        protected SerializedProperty _debugLogProperty;
        
        protected ReorderableList _onOffSwitchesList;

        private readonly List<string> _modeList = new List<string> {"Others Off", "All Sync"};
        
        public override VisualElement CreateInspectorGUI()
        {
            FindProperties();
            serializedObject.Update();
            
            InitLocalization();
            InitInformation();
            
            var container = _baseUxml.CloneTree();
            
            var root = new VisualElement();

            root.Add(container);
            
            var isMultiEdit = targets.Length > 1;
            
            //Disable in Playing Mode
            var body = root.Q<VisualElement>("Body");
            body.Q<VisualElement>("GeneralSettingsSection")
                .SetEnabled(!isMultiEdit);

            var canNotMultiEditLabel = root.Q<VisualElement>("CanNotMultiEditLabel");
            canNotMultiEditLabel.style.display = isMultiEdit ? DisplayStyle.Flex : DisplayStyle.None;
            
            //GeneralSettingsInfo
            var generalSettingsSectionLabel = root.Q<VisualElement>("GeneralSettingsSection")
                .Query<Label>("Title").First();
            generalSettingsSectionLabel.text = _languageDataSet.text_generalSettings;
            
            var interactAudioInfo = root.Q<IMGUIContainer>("GeneralSettingsSectionInfo");
            interactAudioInfo.onGUIHandler = () =>
            {
                if (_enableTutorialInfo)
                {
                    //チュートリアル情報を表示する
                    var infoText = _languageDataSet.text_onOffSwitchGroupTutorialInfo;
                    EditorGUILayout.HelpBox(infoText, MessageType.Info);
                }
                
                if (serializedObject == null || OnSwitchGroup == null)
                {
                    return;
                }
                
                serializedObject.Update();
                
                //_onOffSwitchesPropertyのsynced_syncModeをチェックして混在している場合は警告を出す
                var mode = -1;
                var foundSameSwitch = false;
                var isMixed = false;
                var foundEnabledLinkModeSwitch = false;
                for (var i = 0; i < _switchesProperty.arraySize; i++)
                {
                    for (var j = 0; j < _switchesProperty.arraySize; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }
                        
                        if (_switchesProperty.GetArrayElementAtIndex(i).objectReferenceValue == _switchesProperty.GetArrayElementAtIndex(j).objectReferenceValue)
                        {
                            foundSameSwitch = true;
                            break;
                        }
                    }
                    
                    var switchProperty = _switchesProperty.GetArrayElementAtIndex(i);
                    var switchBase = switchProperty.objectReferenceValue as SwitchBase;
                    if(switchBase == null)
                    {
                        continue;
                    }

                    if (!isMixed)
                    {
                        if(mode == -1)
                        {
                            mode = switchBase.SyncedSyncMode;
                        }
                        else if(mode != switchBase.SyncedSyncMode)
                        {
                            isMixed = true;
                        }
                    }

                    if (switchBase.EnableLinkMode)
                    {
                        foundEnabledLinkModeSwitch = true;
                    }
                }

                if (foundSameSwitch)
                {
                    //同じスイッチが複数存在する場合、予期せぬ動作をする可能性があることを警告する
                    var infoText = _languageDataSet.text_sameSwitchWarning;
                    EditorGUILayout.HelpBox(infoText, MessageType.Warning);
                }
                
                if (isMixed)
                {
                    //SyncModeが混在している場合、予期せぬ動作をする可能性があることを警告する
                    var infoText = _languageDataSet.text_mixedSyncModeWarning;
                    EditorGUILayout.HelpBox(infoText, MessageType.Warning);
                }
                
                if (foundEnabledLinkModeSwitch)
                {
                    //CopycatModeがある場合、予期せぬ動作をする可能性があることを警告する
                    var infoText = _languageDataSet.text_linkModeWarning;
                    EditorGUILayout.HelpBox(infoText, MessageType.Warning);
                }
            };

            //OnOffSwitchesList
            var onOffSwitchesList = root.Q<IMGUIContainer>("OnOffSwitchesList");
            onOffSwitchesList.onGUIHandler = () =>
            {
                //OnOffSwitches
                if (_onOffSwitchesList == null)
                {
                    _onOffSwitchesList = SwitchBaseReorderableList(
                        serializedObject,
                        _switchesProperty,
                        _languageDataSet.text_onOffSwitches
                    );
                }
                _onOffSwitchesList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };
            onOffSwitchesList.SetEnabled(!isMultiEdit);
            
            //DebugLog
            var debugLogToggle = root.Q<Toggle>("DebugLog");
            debugLogToggle.label = _languageDataSet.text_debugLog;
            debugLogToggle.BindProperty(_debugLogProperty);

            return root;
        }

        protected virtual void FindProperties()
        {
            OnSwitchGroup = target as SwitchGroup;
            
            _switchesProperty = serializedObject.FindProperty("switches");
            _debugLogProperty = serializedObject.FindProperty("debugLog");
        }
        
        private void InitLocalization()
        {
            //このスクリプトの存在するパスを取得する
            var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            
            //同じディレクトリ内にあるlangディレクトリ内のLanguageDataSetクラスのScriptableObjectを全て取得する
            path = Path.Combine(Path.GetDirectoryName(path), "lang");

            var language = EditorPrefs.GetString(Config.languageEditorPrefsKey, "en");

            GetLanguageDataSet(path, language, out _languageDataSet);
        }

        private void InitInformation()
        {
            _enableTutorialInfo = EditorPrefs.GetBool(Config.enableTutorialInfoEditorPrefsKey, true);
        }
    }
}