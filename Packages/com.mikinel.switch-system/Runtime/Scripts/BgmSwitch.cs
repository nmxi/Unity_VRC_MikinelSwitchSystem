﻿using UnityEngine;

namespace mikinel.vrc.SwitchSystem
{
    [AddComponentMenu("mikinel/SwitchSystem/OnOffSwitch/BgmSwitch")]
    public class BgmSwitch : OnOffSwitch
    {
        [Space]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField, Range(0f, 1f)] private float _maxBgmVolume = 0.1f;
        [SerializeField] private bool _resetOnEnable = true;    //スイッチがOnになったときに音源をはじめから再生するか
        [SerializeField] private bool _enableFade;
        [SerializeField] private float _fadeTime = 1f;
        
        //Advanced
        public bool forceMasterMute;
        
        private bool _isInitialized;
        private float _targetVolume;

        private float _fadeDeltaVolume;
        private float _lastMaxBgmVolume;

        private void Update()
        {
            if (IsEnableLinkMode)
            {
                return;
            }
            
            if(_lastMaxBgmVolume != _maxBgmVolume)
            {
                //MaxBgmVolumeが変更されたとき
                _lastMaxBgmVolume = _maxBgmVolume;
                _targetVolume = localState == 1 ? _maxBgmVolume : 0f;
                _audioSource.volume = _targetVolume;
            }
            
            var _fadeDeltaVolume = _fadeTime <= 0f ? _maxBgmVolume : _maxBgmVolume / (_fadeTime / Time.deltaTime);

            if (!_enableFade)
            {
                //フェードしない場合

                if (forceMasterMute)
                {
                    //MasterMuteが有効な場合
                    _audioSource.volume = 0f;
                }
                else
                {
                    //MasterMuteが無効な場合
                    _audioSource.volume = _targetVolume;   
                }
            }
            else
            {
                //フェードする場合
                
                if (forceMasterMute)
                {
                    //MasterMuteが有効な場合
                    if (_audioSource.volume > 0f)
                    {
                        //強制的にフェードアウト
                        _audioSource.volume = _audioSource.volume - _fadeDeltaVolume > 0f
                            ? _audioSource.volume - _fadeDeltaVolume
                            : 0f;
                    }
                }
                else
                {
                    //MasterMuteが無効な場合
                    if (_audioSource.volume < _targetVolume)
                    {
                        //フェードイン
                        _audioSource.volume = _audioSource.volume + _fadeDeltaVolume < _targetVolume
                            ? _audioSource.volume + _fadeDeltaVolume
                            : _targetVolume;
                    }
                    else
                    {
                        //フェードアウト
                        _audioSource.volume = _audioSource.volume - _fadeDeltaVolume > _targetVolume
                            ? _audioSource.volume - _fadeDeltaVolume
                            : _targetVolume;
                    }      
                }
            }
        }

        protected override void OnStateChanged(int state)
        {
            base.OnStateChanged(state);

            if (IsEnableLinkMode)
            {
                return;
            }
            
            var isOn = state == 1;
            
            if (!_isInitialized)
            {
                //初回のみフェード無しで音量を変更する
                _audioSource.volume = isOn ? _maxBgmVolume : 0f;
                _audioSource.loop = true;
                _audioSource.Play();
                
                _isInitialized = true;
            }
            
            if (_resetOnEnable && isOn)
            {
                //スイッチがOnになったときに音源をはじめから再生する
                _audioSource.time = 0f;
            }
            
            _targetVolume = isOn ? _maxBgmVolume : 0f;
        }
        
        public void EnableForceMasterMute() => forceMasterMute = true;
        public void DisableForceMasterMute() => forceMasterMute = false;
        
        public void SetMaxBgmVolume(float volume)
        {
            _maxBgmVolume = volume;
        }
        
        public void SetResetOnEnable(bool value)
        {
            _resetOnEnable = value;
        }
        
        public void SetEnableFade(bool value)
        {
            _enableFade = value;
        }

        public void SetFadeTime(float time)
        {
            _fadeTime = time;
        }
    }
}