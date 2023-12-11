using UnityEngine;
using VRC.SDKBase;

namespace mikinel.vrc.SwitchSystem
{
    [AddComponentMenu("mikinel/SwitchSystem/StateSwitch/MirrorStateSwitch")]
    public class MirrorStateSwitch : StateSwitch
    {
        [Space] 
        [SerializeField] private int[] _states;
        [SerializeField] private VRC_MirrorReflection[] _mirrors;
        
        protected override void OnStateChanged(int state)
        {
            base.OnStateChanged(state);

            foreach (var mirror in _mirrors)
            {
                if (mirror == null)
                {
                    continue;
                }
                
                mirror.gameObject.SetActive(false);
            }

            for (var i = 0; i < _mirrors.Length; i++)
            {
                if (_mirrors[i] == null)
                {
                    Debug.LogError($"{gameObject.name} _mirrors[{i}] is null");
                    continue;
                }
                
                if (state == _states[i])
                {
                    _mirrors[i].gameObject.SetActive(true);
                }
            }
        }
    }
}