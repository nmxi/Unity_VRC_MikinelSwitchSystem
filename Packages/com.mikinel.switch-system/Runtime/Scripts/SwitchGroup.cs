using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace mikinel.vrc.SwitchSystem
{
    /// <summary>
    /// 1つのスイッチの入力を他のスイッチの動作に紐付ける
    /// </summary>
    [AddComponentMenu("mikinel/SwitchSystem/SwitchGroup")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SwitchGroup : UdonSharpBehaviour
    {
        public SwitchBase[] switches;

        private const string debugLogPrefix = "[<color=#ffa500ff>mikinel SwitchSystem</color>]";
        public bool debugLog;

        public const int MAX_SWITCHES = 16;

        private void Start()
        {
            if (switches == null || switches.Length == 0)
            {
                DebugLogError($"SwitchGroup: スイッチが設定されていません");
                return;
            }
            
            if (switches.Length > MAX_SWITCHES)
            {
                DebugLogError($"SwitchGroup: スイッチの数が{MAX_SWITCHES}を超えています");
                return;
            }

            var self = gameObject.GetComponent<UdonBehaviour>();
            for (var i = 0; i < switches.Length; i++)
            {
                var switchBase = switches[i];
                if (switchBase == null)
                {
                    DebugLogError($"SwitchGroup: _switches[{i}]が設定されていません");
                    return;
                }
                
                switchBase.AddOnStateChangedCallback(self, $"{nameof(OnStateChanged)}_{i}");
            }
        }

        private void OnDestroy()
        {
            //dispose
            foreach (var switchBase in switches)
            {
                if (switchBase == null)
                {
                    continue;
                }
                
                switchBase.RemoveOnStateChangedCallback();
            }
        }

        public void OnStateChanged(int switchIndex)
        {
            DebugLog($"OnStateChanged: switchIndex={switchIndex}");

            //どれか1つがOnになったら他のスイッチをOffにする
            for (var i = 0; i < switches.Length; i++)
            {
                if (i == switchIndex)
                {
                    continue;
                }

                if (switches[i].EnableLinkMode)
                {
                    continue;
                }
                    
                switches[i].SetLocalState(0);
            }
        }

        #region StringToMethod
        
        //directly interact
        public void OnStateChanged_0() => OnStateChanged(0);
        public void OnStateChanged_1() => OnStateChanged(1);
        public void OnStateChanged_2() => OnStateChanged(2);
        public void OnStateChanged_3() => OnStateChanged(3);
        public void OnStateChanged_4() => OnStateChanged(4);
        public void OnStateChanged_5() => OnStateChanged(5);
        public void OnStateChanged_6() => OnStateChanged(6);
        public void OnStateChanged_7() => OnStateChanged(7);
        public void OnStateChanged_8() => OnStateChanged(8);
        public void OnStateChanged_9() => OnStateChanged(9);
        public void OnStateChanged_10() => OnStateChanged(10);
        public void OnStateChanged_11() => OnStateChanged(11);
        public void OnStateChanged_12() => OnStateChanged(12);
        public void OnStateChanged_13() => OnStateChanged(13);
        public void OnStateChanged_14() => OnStateChanged(14);
        public void OnStateChanged_15() => OnStateChanged(15);

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