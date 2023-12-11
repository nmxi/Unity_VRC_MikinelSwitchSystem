using UnityEngine;

namespace mikinel.vrc.SwitchSystem
{
    [AddComponentMenu("mikinel/SwitchSystem/OnOffSwitch/GameObjectSwitch")]
    public class GameObjectSwitch : OnOffSwitch
    {
        [Space] 
        [SerializeField] private GameObject[] _gameObjects;
        
        protected override void OnStateChanged(int state)
        {
            base.OnStateChanged(state);

            var isOn = state == 1;

            foreach (var gameObject in _gameObjects)
            {
                if (gameObject == null)
                {
                    continue;
                }
                
                gameObject.SetActive(isOn);
            }
        }
    }
}