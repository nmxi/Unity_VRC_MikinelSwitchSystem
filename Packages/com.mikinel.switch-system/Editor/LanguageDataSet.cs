using UnityEngine;

namespace mikinel.vrc.SwitchSystem.Editor
{
    [CreateAssetMenu(fileName = "Lang", menuName = "mikinel/SwitchSystem/LanguageDataSet")]
    public class LanguageDataSet : ScriptableObject
    {
        public string languageCode = "en";  //ISO639
        public string languageDisplayName = "English";
        
        //GeneralSettings
        [Space]
        public string text_generalSettings = "General Settings";
        public string text_enableLinkMode = "Enable Link Mode";
        public string text_linkTargetSwitch = "Link Target Switch";
        public string text_syncMode = "SyncMode";
        public string text_maxState = "Max State";
        public string text_initializeState = "Initialize State";
        public string text_interactionText = "Interaction Text";
        public string text_proximity = "Proximity (m)";
        public string text_notFoundColliderWarning = "Not found Collider. Please add Collider to {0}. If you don't have a collider, you can't interact with the switch.";
        public string text_disabledColliderWarning = "Disable Collider. Please enable Collider to {0}. If you don't have a collider, you can't interact with the switch.";
        public string text_linkTargetSwitchNullWarning = "Link Target Switch is null. Please set Link Target Switch to {0}";
        public string text_linkTargetSwitchSelfWarning = "Link Target Switch is same as this switch. Please set Link Target Switch to other switch.";
        public string text_linkTargetSwitchIsEnableLinkModeWarning = "Link Target Switch is enabled Link Mode. Please set the Link Target Switch to a switch with Link mode disabled.";

        //MirrorSwitch
        public string text_mirrorSwitchSettings = "Mirror Switch Settings";
        
        //MirrorStateSwitch
        public string text_mirrorStateSwitchSettings = "Mirror State Switch Settings";
        public string text_mirrorObjects = "Mirror Objects";
        
        //GameObjectSwitch
        public string text_gameObjectSwitchSettings = "GameObject Switch Settings";
        public string text_gameObjects = "GameObjects";
        
        //GameObjectStateSwitch
        public string text_gameObjectStateSwitchSettings = "GameObject State Switch Settings";
        public string text_enableObjects = "Enable Objects";

        //BgmSwitch
        public string text_bgmSwitchSettings = "BGM Switch Settings";
        public string text_audioSource = "Audio Source";
        public string text_resetOnEnable = "Reset On Enable";
        public string text_audioVolume = "Audio Volume";
        public string text_enableFade = "Enable Fade";
        public string text_fadeTime = "Fade Time (s)";
        public string text_audioSourceNotFoundWarning = "AudioSource is null.";

        //ColliderSwitch
        public string text_colliderSwitchSettings = "Collider Switch Settings";
        public string text_colliderObjects = "Collider Objects";
        
        //ChairSwitch
        public string text_chairSwitchSettings = "Chair Switch Settings";
        public string text_chairObjects = "Chair Objects";
        
        //InteractAudio
        public string text_interactAudio = "Interact Audio";
        public string text_enableInteractionAudio = "Enable Interaction Audio";
        public string text_interactAudioClip = "Interact Audio Clip";
        public string text_interactAudioVolume = "Interact Audio Volume";
        public string text_notFoundAudioSourceWarning = "Not found AudioSource. Please add AudioSource to {0}";
        public string text_disableAudioSourceWarning = "Disable AudioSource. Please enable AudioSource to {0}";
        
        //AdvancedSettings
        public string text_advancedSettings = "Advanced Settings";
        public string text_showSyncModeSuffix = "Show Sync Mode Suffix";
        public string text_suffixFormat = "Suffix Format";
        public string text_suffixSample = "Suffix Sample : {0}";
        public string text_stateChangedCustomEvent = "State Changed Custom Event";
        public string text_udonEvents = "Udon Events";
        public string text_debugLog = "Debug Log";
        
        //BaseSettings
        public string text_baseSettings = "Switch Design Settings";
        public string text_interactAudioSource = "Interact Audio Source";
        public string text_enableObjectControl = "Enable Object Control";
        public string text_controlTargetObjects = "Control Target Objects";
        public string text_enableAnimatorStateControl = "Enable Animator State Control";
        public string text_targetAnimatorIntParameter = "Target Parameter (int)";
        public string text_targetAnimators = "Target Animators";
        public string text_enableAnimatorTriggerControl = "Enable Animator Trigger Control";
        public string text_targetAnimatorTriggerParameter = "Target Parameter (trigger)";
        public string text_animatorParameterNameWarning = "Animator Parameter Name is empty. Please set Animator Parameter Name to {0}";

        //SwitchGroup
        public string text_sameSwitchWarning = "Same Switch is included. This may cause unexpected behavior.";
        public string text_onOffSwitchGroupTutorialInfo = "When one of them is turned on, all the others are turned off.";
        public string text_mixedSyncModeWarning = "SyncMode is mixed. This may cause unexpected behavior.";
        public string text_linkModeWarning = "Link Mode is enabled. This switch is automatically ignore from the group.";
        public string text_onOffSwitches = "OnOff Switches";
    }
}