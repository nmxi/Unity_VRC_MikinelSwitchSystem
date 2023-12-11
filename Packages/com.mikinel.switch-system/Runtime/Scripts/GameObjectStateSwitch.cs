using UnityEngine;

namespace mikinel.vrc.SwitchSystem
{
    [AddComponentMenu("mikinel/SwitchSystem/StateSwitch/GameObjectStateSwitch")]
    public class GameObjectStateSwitch : StateSwitch
    {
        [Space] 
        [SerializeField] private int[] _states;
        [SerializeField] private GameObject[] _gameObjects;
        
        protected override void OnStateChanged(int state)
        {
            base.OnStateChanged(state);

            foreach (var gameObject in _gameObjects)
            {
                if (gameObject == null)
                {
                    continue;
                }
                
                gameObject.SetActive(false);
            }

            for (var i = 0; i < _gameObjects.Length; i++)
            {
                if (_gameObjects[i] == null)
                {
                    Debug.LogError($"{gameObject.name} _gameObjects[{i}] is null");
                    continue;
                }
                
                if (state == _states[i])
                {
                    _gameObjects[i].SetActive(true);
                }
            }
        }
    }
}