using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace mikinel.vrc.SwitchSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [DisallowMultipleComponent]
    public abstract class SwitchBase : UdonSharpBehaviour
    {
        [SerializeField] private bool _enableLinkMode;
        [SerializeField] private SwitchBase _linkTargetSwitch;
        public bool EnableLinkMode => _enableLinkMode;

        //NOTE : SwitchのStateについて
        //Stateは0から始まる
        //Stateは0のときOff、1以上のときOnとして扱う
        //現状、Stateの最大値は設けていない
        
        //UdonSynced Variables
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SyncedSyncMode))] 
        private int _syncedSyncMode = SYNC_MODE_LOCAL;
        
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(SyncedCurrentState))] 
        private int _syncedCurrentState;
        
        public int SyncedSyncMode
        {
            get => _syncedSyncMode;
            set
            {
                _syncedSyncMode = value;
                UpdateInteractionText();    //Suffixの更新
            }
        }

        public int SyncedCurrentState
        {
            get => _syncedCurrentState;
            set
            {
                _syncedCurrentState = value;
                SetLocalState(_syncedCurrentState);
            }
        }
        
        /// <summary>
        /// synced_syncModeを更新する
        /// </summary>
        public void SetSyncMode(int syncMode)
        {
            if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            
            SyncedSyncMode = syncMode;
            SyncedCurrentState = localState;  //現在のlocalStateを全員に同期する
            RequestSerialization();
        }

        [SerializeField] private string _interactionText;    //ボタンに表示するテキスト

        [SerializeField] private int _localState;
        public int localState => _localState;

        public void SetState(int newState)
        {
            if (localState == newState)
            {
                return;
            }
            if (SyncedSyncMode == SYNC_MODE_GLOBAL)
            {
                // SyncedSyncModeがGlobalのときはSyncedCurrentStateを更新する
                if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
                {
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);    
                }
            
                SyncedCurrentState = newState;
                RequestSerialization();
            }
            else
            {
                SetLocalState(newState);
            }
        }
        
        public void SetLocalState(int newState)
        {
            if (_localState == newState)
            {
                return;
            }
            
            _localState = newState;
            OnStateChanged(_localState);

            InvokeLinkedSwitchStateChangedCallback();
            
            if (_localState > 0)
            {
                InvokeSwitchStateChangedCallback();
            }
        }

        //Settings
        [Space]
        [SerializeField] private bool _isPlayInteractAudio = true;
        [SerializeField] private AudioSource _interactAudioSource;
        [SerializeField] private AudioClip _interactAudioClip;
        [SerializeField] private float _interactAudioVolume = 1f;
        
        [Space]
        [SerializeField] private bool _showSyncModeSuffix;
        [SerializeField] private string _syncModeSuffix = $"/{syncModeSuffixReplacement}";
        public const string syncModeSuffixReplacement = "{SYNC_MODE}";
        
        //UdonEventSettings
        [Space]
        [SerializeField] private bool[] _udonEventEnableAnyStates;
        [SerializeField] private int[] _udonEventStates;
        [SerializeField] private UdonBehaviour[] _udonEventTargets;
        [SerializeField] private string[] _udonEventNames;
        
        //BaseSettings
        [Space]
        [SerializeField] private bool _enableObjectControl;
        [SerializeField] private int[] _controlStates;    //_enableObjectsと同じ長さでなければならない
        [SerializeField] private GameObject[] _controlObjects;
        
        [Space]
        [SerializeField] private bool _enableAnimatorStateControl = false;
        [SerializeField] private string _animatorStateControlTargetParameterName = "State";
        [SerializeField] private Animator[] _stateControlTargetAnimators;
        
        [Space]
        [SerializeField] private bool _enableAnimatorTriggerControl = false;
        [SerializeField] private string _animatorTriggerControlTargetParameterName = "OnInteract";
        [SerializeField] private Animator[] _triggerControlTargetAnimators;

        //CallbackToSwitchGroup
        public UdonBehaviour switchGroupUdonBehaviour;
        public string OnStateChangedCallbackMethodName;

        //CallbackToLinkedSwitches
        public UdonBehaviour[] linkedUdonBehaviours;
        public string[] OnLinkedStateChangedCallbackMethodNames;

        private const string debugLogPrefix = "[<color=#ffa500ff>mikinel SwitchSystem</color>]";
        public bool debugLog;
        
        public const int SYNC_MODE_LOCAL = 0;
        public const int SYNC_MODE_GLOBAL = 1;

        private int lastState = 0;
        private bool lastShowSyncModeSuffix = false;

        private void Start()
        {
            if (_enableLinkMode)
            {
                if(_linkTargetSwitch == null)
                {
                    DebugLogError($"link target switch is null");
                    InteractionText = $"ERROR : SWITCH IS BROKEN";
                    enabled = false;
                    return;
                }
                
                if(_linkTargetSwitch.EnableLinkMode && _enableLinkMode)
                {
                    DebugLogError($"link target is already enable link mode");
                    InteractionText = $"ERROR : SWITCH IS BROKEN";
                    enabled = false;
                    return;
                }
                
                //LocalModeに変更する
                ChangeSyncModeToLocalMode();
                
                var self = gameObject.GetComponent<UdonBehaviour>();
                _linkTargetSwitch.AddOnLinkSwitchStateChangedCallback(self, nameof(OnInvokeLinkSwitchStateChangedCallback));
            }

            //Initialize
            UpdateInteractionText();
            
            OnInitialized();
        }

        public override void Interact()
        {
            if(_enableLinkMode && 
               _linkTargetSwitch != null &&
               _linkTargetSwitch.EnableLinkMode)
            {
                DebugLogError($"CopycatTarget is already enable copycat mode");
                InteractionText = $"ERROR : SWITCH IS BROKEN";
                return;
            }
            
            if(_isPlayInteractAudio)
            {
                PlayInteractSound();
            }

            //Invoke Custom udon events
            for (var index = 0; index < _udonEventStates.Length; index++)
            {
                if(_udonEventStates[index] != _localState && !_udonEventEnableAnyStates[index])
                {
                    continue;
                }
                
                if (_udonEventTargets[index] == null)
                {
                    Debug.LogError($"{gameObject.name} _udonEventTargets[{index}] is null");
                    continue;
                }
                
                if (string.IsNullOrEmpty(_udonEventNames[index]))
                {
                    Debug.LogError($"{gameObject.name} _udonEventNames[{index}] is null or empty");
                    continue;
                }
                
                _udonEventTargets[index].SendCustomEvent(_udonEventNames[index]);
            }

            //LinkMode
            if(_enableLinkMode)
            {
                if (_linkTargetSwitch == null)
                {
                    DebugLogError($"CopycatTarget is null");
                    return;
                }
                
                _linkTargetSwitch.Interact();
                return;
            }
            
            OnInteract();
        }

        private void PlayInteractSound()
        {
            _interactAudioSource.PlayOneShot(_interactAudioClip, _interactAudioVolume);
        }

        /// <summary>
        /// 初期化時に呼ばれる
        /// </summary>
        protected virtual void OnInitialized()
        {
            OnStateChanged(localState);
        }

        /// <summary>
        /// stateが変更されたときに呼ばれる
        /// </summary>
        protected virtual void OnStateChanged(int newState)
        {
            if (_enableObjectControl)
            {
                if (_controlStates.Length == _controlObjects.Length)
                {
                    //Disable all objects
                    foreach (var obj in _controlObjects)
                    {
                        obj.SetActive(false);
                    }
            
                    //Enable objects
                    for (var i = 0; i < _controlStates.Length; i++)
                    {
                        if (_controlStates[i] == newState)
                        {
                            if (_controlObjects[i] == null)
                            {
                                Debug.LogError($"{gameObject.name} _controlObjects[{i}] is null");
                                continue;
                            }
                            
                            _controlObjects[i].SetActive(true);
                        }
                    }
                }
                else
                {
                    Debug.LogError("enableStateとenableObjectsの長さが一致しません");
                }
            }
            
            if (_enableAnimatorStateControl)
            {
                if (!string.IsNullOrEmpty(_animatorStateControlTargetParameterName))
                {
                    for (var i = 0; i < _stateControlTargetAnimators.Length; i++)
                    {
                        var animator = _stateControlTargetAnimators[i];

                        if (animator == null)
                        {
                            Debug.LogError($"{gameObject.name} _targetAnimators[{i}] is null");
                            continue;
                        }

                        animator.SetInteger(_animatorStateControlTargetParameterName, newState);
                    }   
                }
                else
                {
                    Debug.LogError($"{gameObject.name} _animatorTargetParameter is null or empty");
                }
            }

            if (_enableAnimatorTriggerControl)
            {
                if (!string.IsNullOrEmpty(_animatorTriggerControlTargetParameterName))
                {
                    for (var i = 0; i < _triggerControlTargetAnimators.Length; i++)
                    {
                        var animator = _triggerControlTargetAnimators[i];

                        if (animator == null)
                        {
                            Debug.LogError($"{gameObject.name} _targetAnimators[{i}] is null");
                            continue;
                        }

                        animator.SetTrigger(_animatorTriggerControlTargetParameterName);
                    }   
                }
                else
                {
                    Debug.LogError($"{gameObject.name} _animatorTargetParameter is null or empty");
                }
            }
        }

        /// <summary>
        /// Interact時に呼ばれる
        /// Interactを直接呼び出さずにこちらをOverrideして使う
        /// </summary>
        protected virtual void OnInteract() { }

        private void UpdateInteractionText()
        {
            var suffix = "";
            if (_showSyncModeSuffix)
            {
                //Suffixのパターンを置換する
                var replacement = SyncedSyncMode == SYNC_MODE_LOCAL ? "Local" : "Global";
                var startIndex = _syncModeSuffix.IndexOf(syncModeSuffixReplacement);
                if (startIndex != -1)
                {
                    suffix = 
                        _syncModeSuffix.Substring(0, startIndex) 
                        + replacement 
                        + _syncModeSuffix.Substring(startIndex + syncModeSuffixReplacement.Length);
                }
            }
                
            base.InteractionText = _interactionText + suffix;
        }

        #region OnStateChangedCallbackToSwitchGroup

        public void AddOnStateChangedCallback(UdonBehaviour udonBehaviour, string switchStateChangedCallBackMethodName)
        {
            switchGroupUdonBehaviour = udonBehaviour;
            OnStateChangedCallbackMethodName = switchStateChangedCallBackMethodName;
        }
        
        public void RemoveOnStateChangedCallback()
        {
            switchGroupUdonBehaviour = null;
            OnStateChangedCallbackMethodName = null;
        }

        private void InvokeSwitchStateChangedCallback()
        {
            if (switchGroupUdonBehaviour == null ||
                string.IsNullOrEmpty(OnStateChangedCallbackMethodName))
            {
                return;
            }
            
            switchGroupUdonBehaviour.SendCustomEvent(OnStateChangedCallbackMethodName);
        }

        #endregion

        #region OnLinkSwitchStateChangedCallback
        
        public void AddOnLinkSwitchStateChangedCallback(UdonBehaviour udonBehaviour, string callbackMethodName)
        {
            var newCopycatUdonBehaviours = new UdonBehaviour[linkedUdonBehaviours == null ? 1 : linkedUdonBehaviours.Length + 1];
            var newCallbackMethodNames = new string[OnLinkedStateChangedCallbackMethodNames == null ? 1 : OnLinkedStateChangedCallbackMethodNames.Length + 1];
            
            if (linkedUdonBehaviours != null)
            {
                linkedUdonBehaviours.CopyTo(newCopycatUdonBehaviours, 0);
                OnLinkedStateChangedCallbackMethodNames.CopyTo(newCallbackMethodNames, 0);
            }
            
            newCopycatUdonBehaviours[newCopycatUdonBehaviours.Length - 1] = udonBehaviour;
            newCallbackMethodNames[newCallbackMethodNames.Length - 1] = callbackMethodName;
            
            linkedUdonBehaviours = newCopycatUdonBehaviours;
            OnLinkedStateChangedCallbackMethodNames = newCallbackMethodNames;
        }

        public void InvokeLinkedSwitchStateChangedCallback()
        {
            for (var i = 0; i < linkedUdonBehaviours.Length; i++)
            {
                if (linkedUdonBehaviours[i] == null || string.IsNullOrEmpty(OnLinkedStateChangedCallbackMethodNames[i]))
                {
                    continue;
                }
                
                linkedUdonBehaviours[i].SendCustomEvent(OnLinkedStateChangedCallbackMethodNames[i]);
            }
        }
        
        public void OnInvokeLinkSwitchStateChangedCallback()
        {
            if (_linkTargetSwitch == null)
            {
                DebugLogError($"LinkTargetSwitch is null");
                return;
            }
            
            //LinkTargetSwitchのStateをコピーする
            SetLocalState(_linkTargetSwitch.localState);
        }

        #endregion

        #region ChangeSyncMode

        /// <summary>
        /// 同期モードをローカルモードに変更する
        /// </summary>
        public void ChangeSyncModeToGlobalMode() => SetSyncMode(SYNC_MODE_GLOBAL);
        
        /// <summary>
        /// 同期モードをグローバルモードに変更する
        /// </summary>
        public void ChangeSyncModeToLocalMode() => SetSyncMode(SYNC_MODE_LOCAL);

        #endregion

        #region DebugLog

        protected void DebugLog(string message)
        {
            if (!debugLog)
            {
                return;
            }
            
            Debug.Log($"{debugLogPrefix} {message}");
        }
        
        protected void DebugLogError(string message)
        {
            if (!debugLog)
            {
                return;
            }
            
            Debug.LogError($"{debugLogPrefix} {message}");
        }
        
        #endregion
    }   
}