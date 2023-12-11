using UnityEngine;

namespace mikinel.vrc.SwitchSystem
{
    public class StateSwitch : SwitchBase
    {
        [Space]
        [SerializeField] private int _maxState = 1;

        protected override void OnInteract()
        {
            SetLocalState(localState == _maxState ? 0 : localState + 1);
        }
    }
}