using UnityEngine;
using VRC.SDKBase;

namespace mikinel.vrc.SwitchSystem
{
    [AddComponentMenu("mikinel/SwitchSystem/OnOffSwitch/MirrorSwitch")]
    public class MirrorSwitch : OnOffSwitch
    {
        [Space]
        [SerializeField] private VRC_MirrorReflection[] _mirrors;

        protected override void OnStateChanged(int state)
        {
            base.OnStateChanged(state);
            
            var isOn = state == 1;
            
            foreach (var mirror in _mirrors)
            {
                mirror.gameObject.SetActive(isOn);
            }
        }
    }
}