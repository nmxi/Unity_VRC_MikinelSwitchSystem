using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BgmSwitch))]
    public class BgmSwitchEditor : OnOffSwitchEditor
    {
        [SerializeField] private VisualTreeAsset _bgmSwitchUxml;
        
        protected SerializedProperty _audioSourceProperty;
        protected SerializedProperty _audioVolumeProperty;
        protected SerializedProperty _resetOnEnableProperty;
        protected SerializedProperty _enableFadeProperty;
        protected SerializedProperty _fadeTimeProperty;

        protected override VisualElement CreateUniqueSettingsGUI()
        {
            base.CreateUniqueSettingsGUI();
            
            var container = _bgmSwitchUxml.CloneTree();
            
            //BgmSwitchSectionInfo
            var bgmSwitchSectionInfo = container.Q<IMGUIContainer>("BgmSwitchSectionInfo");
            bgmSwitchSectionInfo.onGUIHandler = () =>
            {
                serializedObject.Update();
                
                if (_audioSourceProperty.objectReferenceValue == null)
                {
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox(_languageDataSet.text_audioSourceNotFoundWarning, MessageType.Error);
                }
            };
            
            //BgmAudioSource
            var bgmAudioSourceLabel = container.Q<VisualElement>("BgmSwitchSection")
                .Query<Label>().First();
            bgmAudioSourceLabel.text = _languageDataSet.text_bgmSwitchSettings;
            
            var bgmAudioSource = container.Q<ObjectField>("AudioSource");
            bgmAudioSource.label = _languageDataSet.text_audioSource;
            bgmAudioSource.objectType = typeof(AudioSource);
            bgmAudioSource.BindProperty(_audioSourceProperty);

            //BgmAudioVolume
            var interactAudioVolume = container.Q<VisualElement>("AudioVolume");
            var interactAudioVolumeSlider = interactAudioVolume.Query().Descendents<Slider>("FloatSlider").First();
            interactAudioVolumeSlider.label = _languageDataSet.text_audioVolume;
            var interactAudioVolumeField = interactAudioVolume.Query().Descendents<FloatField>("FloatField").First();
            interactAudioVolumeSlider.BindProperty(_audioVolumeProperty);
            interactAudioVolumeField.BindProperty(_audioVolumeProperty);
            
            //ResetOnEnable
            var resetOnEnable = container.Q<Toggle>("ResetOnEnable");
            resetOnEnable.label = _languageDataSet.text_resetOnEnable;
            resetOnEnable.BindProperty(_resetOnEnableProperty);

            //BgmEnableFade
            var bgmEnableFade = container.Q<Toggle>("EnableFade");
            bgmEnableFade.label = _languageDataSet.text_enableFade;
            bgmEnableFade.BindProperty(_enableFadeProperty);
            
            //BgmFadeTime
            var bgmFadeTime = container.Q<FloatField>("FadeTime");
            bgmFadeTime.label = _languageDataSet.text_fadeTime;
            bgmFadeTime.RegisterValueChangedCallback(value =>
            {
                var newValue = Mathf.Max(0f, value.newValue);
                bgmFadeTime.SetValueWithoutNotify(newValue);
            });
            bgmFadeTime.BindProperty(_fadeTimeProperty);
            bgmFadeTime.SetEnabled(_enableFadeProperty.boolValue);
            bgmEnableFade.RegisterValueChangedCallback(evt =>
            {
                bgmFadeTime.SetEnabled(evt.newValue);
            });
            
            return container;
        }

        protected override void FindProperties()
        {
            base.FindProperties();

            _audioSourceProperty = serializedObject.FindProperty("_audioSource");
            _audioVolumeProperty = serializedObject.FindProperty("_maxBgmVolume");
            _resetOnEnableProperty = serializedObject.FindProperty("_resetOnEnable");
            _enableFadeProperty = serializedObject.FindProperty("_enableFade");
            _fadeTimeProperty = serializedObject.FindProperty("_fadeTime");
        }
    }
}