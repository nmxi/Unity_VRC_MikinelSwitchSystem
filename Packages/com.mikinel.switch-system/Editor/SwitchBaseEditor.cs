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
    [CustomEditor(typeof(SwitchBase), true)]
    public class SwitchBaseEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset _baseUxml;

        protected LanguageDataSet _languageDataSet;

        protected SwitchBase _switchBase;

        protected SerializedProperty _linkModeProperty;
        protected SerializedProperty _linkTargetSwitchProperty;

        protected SerializedProperty _syncedSyncModeProperty;
        protected SerializedProperty _localStateProperty;

        protected SerializedProperty _interactionTextProperty;

        protected SerializedProperty _isPlayInteractAudioProperty;
        protected SerializedProperty _interactAudioSourceProperty;
        protected SerializedProperty _interactAudioProperty;
        protected SerializedProperty _interactAudioVolumeProperty;
        
        protected SerializedProperty _showSyncModeSuffixProperty;
        protected SerializedProperty _syncModeSuffixProperty;
        protected SerializedProperty _udonEventStatesProperty;
        protected SerializedProperty _udonEventTargetsProperty;
        protected SerializedProperty _udonEventNamesProperty;
        protected SerializedProperty _debugLogProperty;
        
        protected SerializedProperty _enableAnyStatesProperty;
        protected SerializedProperty _enableObjectControlProperty;
        protected SerializedProperty _controlStatesProperty;
        protected SerializedProperty _controlObjectsProperty;
        
        protected SerializedProperty _enableAnimatorStateControlProperty;
        protected SerializedProperty _animatorStateControlTargetParameterNameProperty;
        protected SerializedProperty _stateControlTargetAnimatorsProperty;
        
        protected SerializedProperty _enableAnimatorTriggerControlProperty;
        protected SerializedProperty _animatorTriggerControlTargetParameterNameProperty;
        protected SerializedProperty _triggerControlTargetAnimatorsProperty;
        
        protected ReorderableList _udonEventReorderableList;
        protected ReorderableList _controlObjectsReorderableList;
        protected ReorderableList _stateControlTargetAnimatorsReorderableList;
        protected ReorderableList _triggerControlTargetAnimatorsReorderableList;

        protected Toggle _linkModeToggle;
        
        private readonly List<string> _syncModeList = new List<string> {"Local", "Global"};

        public override VisualElement CreateInspectorGUI()
        {
            FindProperties();
            serializedObject.Update();
            
            InitLocalization();

            var container = _baseUxml.CloneTree();

            var root = new VisualElement();

            root.Add(container);
            
            var isMultiEdit = targets.Length > 1;
            
            var header = root.Q<VisualElement>("Header");
            var version = header.Query().Descendents<Label>("Version").First();
            version.text = $"{Version.CurrentVersion}";
            
            //EnableLinkMode
            _linkModeToggle = root.Q<Toggle>("EnableLinkMode");
            _linkModeToggle.label = _languageDataSet.text_enableLinkMode;
            _linkModeToggle.BindProperty(_linkModeProperty);
            var linkTargetSwitch = root.Q<ObjectField>("LinkTargetSwitch");
            linkTargetSwitch.label = _languageDataSet.text_linkTargetSwitch;
            linkTargetSwitch.objectType = target.GetType();
            linkTargetSwitch.BindProperty(_linkTargetSwitchProperty);
            linkTargetSwitch.style.display = _linkModeProperty.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            _linkModeToggle.RegisterValueChangedCallback(evt =>
            {
                linkTargetSwitch.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            linkTargetSwitch.RegisterValueChangedCallback(evt =>
            {
                UpdateGeneralSettingsInfo(root);
            });

            var TypeInfo = header.Query().Descendents<Label>("TypeInfo").First();
            var typeText = target.GetType().Name;
            //大文字の前にスペースを入れる
            for (var i = 1; i < typeText.Length; i++)
            {
                if (char.IsUpper(typeText[i]))
                {
                    typeText = typeText.Insert(i, " ");
                    i++;
                }
            }
            
            var enableLinkModeText = $" (Enable Link Mode)";
            if(_linkModeProperty.boolValue)
            {
                typeText += enableLinkModeText;
            }
            _linkModeToggle.RegisterValueChangedCallback(evt =>
            {
                typeText = evt.newValue ? typeText + enableLinkModeText : typeText.Replace(enableLinkModeText, "");
                TypeInfo.text = typeText;
            });
            
            TypeInfo.text = typeText;
            
            //Disable in Playing Mode and Multi Edit
            var body = root.Q<VisualElement>("Body");
            body.SetEnabled(!isMultiEdit);

            var canNotMultiEditLabel = root.Q<VisualElement>("CanNotMultiEditLabel");
            canNotMultiEditLabel.style.display = isMultiEdit ? DisplayStyle.Flex : DisplayStyle.None;
            
            UpdateGeneralSettingsInfo(root);
            
            //GeneralSettings
            var generalSettingsLabel = root.Query<VisualElement>("GeneralSettingsSection")
                .Descendents<Label>("Title").First();
            generalSettingsLabel.text = _languageDataSet.text_generalSettings;

            //SyncMode
            var modePopupField = new PopupField<string>(_languageDataSet.text_syncMode, _syncModeList, _syncedSyncModeProperty.intValue);
            modePopupField.RegisterValueChangedCallback(evt =>
            {
                _syncedSyncModeProperty.intValue = evt.newValue == "Local" ? 0 : 1;
                SetVariable(_switchBase, typeof(SwitchBase), "_syncedSyncMode", _syncedSyncModeProperty.intValue);
                EditorUtility.SetDirty(_switchBase);
            });
            root.Q<VisualElement>("Mode").Add(modePopupField);
            modePopupField.style.display = _linkModeProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
            _linkModeToggle.RegisterValueChangedCallback(evt =>
            {
                modePopupField.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
            });

            //InteractionText
            //NOTE : UIElementsのTextFieldではアルファベット以外が入力できないため、IMGUIのTextFieldを使用する
            var InteractionTextArea = root.Q<IMGUIContainer>("InteractionTextArea");
            InteractionTextArea.onGUIHandler = () =>
            {
                if (serializedObject == null || _switchBase == null)
                {
                    return;
                }
                
                serializedObject.Update();
                
                //文字色を変更する
                var style = new GUIStyle(EditorStyles.label);
                style.normal.textColor = Color.white;
                
                //HorizontalGroupを作成する
                EditorGUILayout.BeginHorizontal();
                
                //labelを表示する
                EditorGUILayout.LabelField(
                    _languageDataSet.text_interactionText,
                    style,
                    GUILayout.Width(EditorGUIUtility.labelWidth - 1)
                );
                
                EditorGUILayout.PropertyField(
                    _interactionTextProperty,
                    GUIContent.none,
                    GUILayout.Width(EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 11)
                );
                
                EditorGUILayout.EndHorizontal();
                
                if (serializedObject.hasModifiedProperties)
                {
                    SetVariable(_switchBase, typeof(SwitchBase), "_interactionText", _interactionTextProperty.stringValue);
                    EditorUtility.SetDirty(_switchBase);
                }
            };
            
            //Proximity
            var proximity = root.Q<FloatField>("Proximity");
            proximity.label = _languageDataSet.text_proximity;
            proximity.value = _switchBase.GetProximity();
            proximity.RegisterValueChangedCallback(evt =>
            {
                _switchBase.SetProximity(evt.newValue);
            });

            //InteractAudio
            var interactAudioLabel = root.Query<VisualElement>("InteractionAudioSection")
                .Descendents<Label>("Title").First();
            interactAudioLabel.text = _languageDataSet.text_interactAudio;
            
            //IsPlayInteractSound
            var toggle = root.Q<Toggle>("IsPlayInteractSound");
            toggle.label = _languageDataSet.text_enableInteractionAudio;
            toggle.BindProperty(_isPlayInteractAudioProperty);

            //InteractAudioSource
            var interactAudioSource = root.Q<ObjectField>("InteractAudioSource");
            interactAudioSource.label = _languageDataSet.text_interactAudioSource;
            interactAudioSource.objectType = typeof(AudioSource);
            interactAudioSource.BindProperty(_interactAudioSourceProperty);
            UpdateInteractAudioSectionInfo(root);

            //InteractAudioClip
            var interactAudio = root.Q<ObjectField>("InteractAudioClip");
            interactAudio.label = _languageDataSet.text_interactAudioClip;
            interactAudio.objectType = typeof(AudioClip);
            interactAudio.BindProperty(_interactAudioProperty);

            //InteractAudioVolume
            var interactAudioVolume = root.Q<VisualElement>("InteractAudioVolume");
            var interactAudioVolumeSlider = interactAudioVolume.Query().Descendents<Slider>("FloatSlider").First();
            var interactAudioVolumeField = interactAudioVolume.Query().Descendents<FloatField>("FloatField").First();
            interactAudioVolumeSlider.label = _languageDataSet.text_interactAudioVolume;
            interactAudioVolumeSlider.BindProperty(_interactAudioVolumeProperty);
            interactAudioVolumeField.BindProperty(_interactAudioVolumeProperty);
            
            interactAudio.style.display = toggle.value ? DisplayStyle.Flex : DisplayStyle.None;
            interactAudioVolume.style.display = toggle.value ? DisplayStyle.Flex : DisplayStyle.None;
            
            toggle.RegisterValueChangedCallback(evt =>
            {
                interactAudio.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                interactAudioVolume.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            //UniqueSettings
            var uniqueSettingsLayout = CreateUniqueSettingsGUI();
            root.Q<VisualElement>("UniqueSettingsSection").Add(uniqueSettingsLayout);
            uniqueSettingsLayout.style.display = _linkModeProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
            _linkModeToggle.RegisterValueChangedCallback(evt =>
            {
                uniqueSettingsLayout.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
            });

            //AdvancedSettings
            var advancedSettingsFoldout = root.Q<Foldout>("AdvancedSettingsFoldout");
            advancedSettingsFoldout.text = _languageDataSet.text_advancedSettings;

            var showSyncModeSuffix = root.Q<Toggle>("ShowSyncModeSuffix");
            showSyncModeSuffix.label = _languageDataSet.text_showSyncModeSuffix;
            showSyncModeSuffix.BindProperty(_showSyncModeSuffixProperty);
            
            //SuffixFormat
            var suffixFormat = root.Q<TextField>("SuffixFormat");
            suffixFormat.label = _languageDataSet.text_suffixFormat;
            suffixFormat.BindProperty(_syncModeSuffixProperty);
            
            var suffixFormatInfo = root.Q<IMGUIContainer>("SuffixFormatInfo");
            suffixFormatInfo.onGUIHandler = () =>
            {
                var infoText = $"{ReplacePlaceholder(_languageDataSet.text_suffixSample, new []{SwitchBase.syncModeSuffixReplacement})}\n" +
                               $"Local → {_interactionTextProperty.stringValue}/Local\n" +
                               $"Global → {_interactionTextProperty.stringValue}/Global";

                GUI.color = Color.white;
                EditorGUILayout.HelpBox(infoText, MessageType.Info);
            };
            
            //UdonEvent
            var stateChangedCustomEventsLabel = root.Q<Label>("StateChangedCustomEvents");
            stateChangedCustomEventsLabel.text = _languageDataSet.text_stateChangedCustomEvent;
            
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
                        _languageDataSet.text_udonEvents
                    );
                }
                _udonEventReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };
            
            var debugLog = root.Q<Toggle>("DebugLog");
            debugLog.label = _languageDataSet.text_debugLog;
            debugLog.BindProperty(_debugLogProperty);

            suffixFormat.SetEnabled(showSyncModeSuffix.value);
            suffixFormatInfo.SetEnabled(showSyncModeSuffix.value);
            
            showSyncModeSuffix.RegisterValueChangedCallback(evt =>
            {
                suffixFormat.SetEnabled(evt.newValue);
                suffixFormatInfo.SetEnabled(evt.newValue);
            });

            UpdateAnimatorControlSectionInfo(root);

            //EnableObjectControl
            var enableObjectControl = root.Q<Toggle>("EnableObjectControl");
            enableObjectControl.label = _languageDataSet.text_enableObjectControl;
            enableObjectControl.BindProperty(_enableObjectControlProperty);

            //ControlTargetObjects
            var controlTargetObjects = root.Q<IMGUIContainer>("ControlTargetObjects");
            controlTargetObjects.SetEnabled(_enableObjectControlProperty.boolValue);
            enableObjectControl.RegisterValueChangedCallback(evt =>
            {
                controlTargetObjects.SetEnabled(evt.newValue);
            });
            controlTargetObjects.onGUIHandler = () =>
            {
                if (_controlObjectsReorderableList == null)
                {
                    _controlObjectsReorderableList = StateAndPropertyReorderableList<GameObject>(
                        serializedObject,
                        _controlStatesProperty,
                        _controlObjectsProperty,
                        _languageDataSet.text_controlTargetObjects,
                        "GameObject"
                    );
                }
                _controlObjectsReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };
            
            //EnableAnimatorStateControl
            var enableAnimatorControl = root.Q<Toggle>("EnableAnimatorStateControl");
            enableAnimatorControl.label = _languageDataSet.text_enableAnimatorStateControl;
            enableAnimatorControl.BindProperty(_enableAnimatorStateControlProperty);

            var animatorTargetParameter = root.Q<TextField>("AnimatorTargetParameter");
            animatorTargetParameter.label = _languageDataSet.text_targetAnimatorIntParameter;
            animatorTargetParameter.BindProperty(_animatorStateControlTargetParameterNameProperty);
            animatorTargetParameter.SetEnabled(_enableAnimatorStateControlProperty.boolValue);

            var targetAnimators = root.Q<IMGUIContainer>("StateControlTargetAnimators");
            targetAnimators.SetEnabled(_enableAnimatorStateControlProperty.boolValue);
            enableAnimatorControl.RegisterValueChangedCallback(evt =>
            {
                animatorTargetParameter.SetEnabled(evt.newValue);
                targetAnimators.SetEnabled(evt.newValue);
            });
            targetAnimators.onGUIHandler = () =>
            {
                if (_stateControlTargetAnimatorsReorderableList == null)
                {
                    _stateControlTargetAnimatorsReorderableList = SinglePropertyReorderableList<Animator>(
                        serializedObject,
                        _stateControlTargetAnimatorsProperty,
                        _languageDataSet.text_targetAnimators
                    );
                }
                _stateControlTargetAnimatorsReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };
            
            //EnableAnimatorTriggerControl
            var enableAnimatorTriggerControl = root.Q<Toggle>("EnableAnimatorTriggerControl");
            enableAnimatorTriggerControl.label = _languageDataSet.text_enableAnimatorTriggerControl;
            enableAnimatorTriggerControl.BindProperty(_enableAnimatorTriggerControlProperty);
            
            var animatorTriggerTargetParameter = root.Q<TextField>("AnimatorTriggerTargetParameter");
            animatorTriggerTargetParameter.label = _languageDataSet.text_targetAnimatorTriggerParameter;
            animatorTriggerTargetParameter.SetEnabled(_enableAnimatorTriggerControlProperty.boolValue);
            animatorTriggerTargetParameter.BindProperty(_animatorTriggerControlTargetParameterNameProperty);

            var triggerTargetAnimators = root.Q<IMGUIContainer>("TriggerControlTargetAnimators");
            triggerTargetAnimators.SetEnabled(_enableAnimatorTriggerControlProperty.boolValue);
            enableAnimatorTriggerControl.RegisterValueChangedCallback(evt =>
            {
                animatorTriggerTargetParameter.SetEnabled(evt.newValue);
                triggerTargetAnimators.SetEnabled(evt.newValue);
            });
            triggerTargetAnimators.onGUIHandler = () =>
            {
                if (_triggerControlTargetAnimatorsReorderableList == null)
                {
                    _triggerControlTargetAnimatorsReorderableList = SinglePropertyReorderableList<Animator>(
                        serializedObject,
                        _triggerControlTargetAnimatorsProperty,
                        _languageDataSet.text_targetAnimators
                    );
                }
                _triggerControlTargetAnimatorsReorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            };

            return root;
        }

        private void UpdateGeneralSettingsInfo(VisualElement root)
        {
            //GeneralSettingsInfo
            var generalSettingsInfo = root.Q<IMGUIContainer>("GeneralSettingsSectionInfo");
            generalSettingsInfo.onGUIHandler = () =>
            {
                if (serializedObject == null || _switchBase == null)
                {
                    return;
                }
                
                serializedObject.Update();
                var collider = _switchBase.GetComponent<Collider>();
                if (collider == null)
                {
                    //Colliderが無い場合はColliderを追加するように促す
                    var infoText = ReplacePlaceholder(_languageDataSet.text_notFoundColliderWarning, new[] { _switchBase.gameObject.name });
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox(infoText, MessageType.Error);
                }
                else if (!collider.enabled)
                {
                    //Colliderが無効になっている場合はColliderを有効にするように促す
                    var infoText = ReplacePlaceholder(_languageDataSet.text_disabledColliderWarning, new[] { _switchBase.gameObject.name });
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox(infoText, MessageType.Error);
                }

                if (_linkModeProperty.boolValue)
                {
                    if(_linkTargetSwitchProperty.objectReferenceValue == null)
                    {
                        //LinkTargetSwitchがnullの場合は設定するように促す
                        var infoText = ReplacePlaceholder(_languageDataSet.text_linkTargetSwitchNullWarning, new[] { _switchBase.gameObject.name });
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox(infoText, MessageType.Error);
                    }
                    else if(_linkTargetSwitchProperty.objectReferenceValue == target)
                    {
                        //LinkTargetSwitchが自身の場合は設定しないように促す
                        var infoText = ReplacePlaceholder(_languageDataSet.text_linkTargetSwitchSelfWarning, new[] { _switchBase.gameObject.name });
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox(infoText, MessageType.Error);
                    }

                    if (_linkTargetSwitchProperty != null)
                    {
                        //LinkTargetSwitchに設定されているスイッチの設定がLinkMode=trueの場合は警告を出す
                        var linkTargetSwitch = _linkTargetSwitchProperty.objectReferenceValue as SwitchBase;
                        if (linkTargetSwitch != null &&
                            linkTargetSwitch.IsEnableLinkMode)
                        {
                            var infoText = ReplacePlaceholder(_languageDataSet.text_linkTargetSwitchIsEnableLinkModeWarning, new[] { _switchBase.gameObject.name });
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox(infoText, MessageType.Error);
                        }
                    }
                }
            };
        }

        private void UpdateInteractAudioSectionInfo(VisualElement root)
        {
            //InteractAudioInfo
            var interactAudioInfo = root.Q<IMGUIContainer>("InteractAudioSectionInfo");
            interactAudioInfo.onGUIHandler = () =>
            {
                if (serializedObject == null || _switchBase == null)
                {
                    return;
                }
                
                serializedObject.Update();
                if (_interactAudioSourceProperty.objectReferenceValue == null &&
                    _isPlayInteractAudioProperty.boolValue)
                {
                    //AudioSourceをSwitchBaseと同じGameObjectに追加するように促す
                    var infoText = ReplacePlaceholder(_languageDataSet.text_notFoundAudioSourceWarning, new[] { _switchBase.gameObject.name });
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox(infoText, MessageType.Error);
                }
                else if (!_interactAudioSourceProperty.objectReferenceValue &&
                         _isPlayInteractAudioProperty.boolValue)
                {
                    //AudioSourceが無効になっている場合はAudioSourceを有効にするように促す
                    var infoText = ReplacePlaceholder(_languageDataSet.text_disableAudioSourceWarning, new[] { _switchBase.gameObject.name });
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox(infoText, MessageType.Error);
                }
            };
        }

        private void UpdateAnimatorControlSectionInfo(VisualElement root)
        {
            //BaseSettingsInfo
            var baseSettingsFoldout = root.Q<Foldout>("BaseSettingsFoldout");
            baseSettingsFoldout.text = _languageDataSet.text_baseSettings;
            
            var baseSettingsSectionInfo = root.Q<IMGUIContainer>("BaseSettingsSectionInfo");
            baseSettingsSectionInfo.onGUIHandler = () =>
            {
                if (serializedObject == null || _switchBase == null)
                {
                    return;
                }
                
                serializedObject.Update();
                var isIntParameterNull = string.IsNullOrEmpty(_animatorStateControlTargetParameterNameProperty.stringValue) &&
                                        _enableAnimatorStateControlProperty.boolValue;
                var isTriggerParameterNull = string.IsNullOrEmpty(_animatorTriggerControlTargetParameterNameProperty.stringValue) &&
                                             _enableAnimatorTriggerControlProperty.boolValue;
                if (isIntParameterNull || isTriggerParameterNull)
                {
                    var infoText = ReplacePlaceholder(_languageDataSet.text_animatorParameterNameWarning, new[] { _switchBase.gameObject.name });
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox(infoText, MessageType.Error);
                }
            };
        }

        protected virtual VisualElement CreateUniqueSettingsGUI() { return null; }
        
        protected virtual void FindProperties()
        {
            _switchBase = target as SwitchBase;
            
            _linkModeProperty = serializedObject.FindProperty("_isEnableLinkMode");
            _linkTargetSwitchProperty = serializedObject.FindProperty("_linkTargetSwitch");
            
            _syncedSyncModeProperty = serializedObject.FindProperty("syncMode");
            _localStateProperty = serializedObject.FindProperty("_localState");
            
            _interactionTextProperty = serializedObject.FindProperty("_interactionText");
            
            _isPlayInteractAudioProperty = serializedObject.FindProperty("_isPlayInteractAudio");
            _interactAudioSourceProperty = serializedObject.FindProperty("_interactAudioSource");
            _interactAudioProperty = serializedObject.FindProperty("_interactAudioClip");
            _interactAudioVolumeProperty = serializedObject.FindProperty("_interactAudioVolume");
            
            _showSyncModeSuffixProperty = serializedObject.FindProperty("_showSyncModeSuffix");
            _syncModeSuffixProperty = serializedObject.FindProperty("_syncModeSuffix");
            _debugLogProperty = serializedObject.FindProperty("debugLog");
            
            _enableAnyStatesProperty = serializedObject.FindProperty("_udonEventEnableAnyStates");
            _udonEventStatesProperty = serializedObject.FindProperty("_udonEventStates");
            _udonEventTargetsProperty = serializedObject.FindProperty("_udonEventTargets");
            _udonEventNamesProperty = serializedObject.FindProperty("_udonEventNames");
            
            _enableObjectControlProperty = serializedObject.FindProperty("_enableObjectControl");
            _controlStatesProperty = serializedObject.FindProperty("_controlStates");
            _controlObjectsProperty = serializedObject.FindProperty("_controlObjects");
            
            _enableAnimatorStateControlProperty = serializedObject.FindProperty("_enableAnimatorStateControl");
            _animatorStateControlTargetParameterNameProperty = serializedObject.FindProperty("_animatorStateControlTargetParameterName");
            _stateControlTargetAnimatorsProperty = serializedObject.FindProperty("_stateControlTargetAnimators");
            
            _enableAnimatorTriggerControlProperty = serializedObject.FindProperty("_enableAnimatorTriggerControl");
            _animatorTriggerControlTargetParameterNameProperty = serializedObject.FindProperty("_animatorTriggerControlTargetParameterName");
            _triggerControlTargetAnimatorsProperty = serializedObject.FindProperty("_triggerControlTargetAnimators");
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
    }
}