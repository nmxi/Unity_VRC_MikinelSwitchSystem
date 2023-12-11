using UnityEngine;

namespace mikinel.vrc.SwitchSystem
{
    [AddComponentMenu("mikinel/SwitchSystem/OnOffSwitch/ColliderSwitch")]
    public class ColliderSwitch : OnOffSwitch
    {
        [Space]
        [SerializeField] private Collider[] _colliders;
        
        protected override void OnStateChanged(int state)
        {
            base.OnStateChanged(state);
            
            var isOn = state == 1;
            
            foreach (var collider in _colliders)
            {
                if (collider == null)
                {
                    continue;
                }
                
                collider.enabled = isOn;
            }
        }
    }   
}