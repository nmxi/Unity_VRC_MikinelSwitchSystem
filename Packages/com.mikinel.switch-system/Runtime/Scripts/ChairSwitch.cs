using UnityEngine;
using VRC.SDKBase;

namespace mikinel.vrc.SwitchSystem
{
    [AddComponentMenu("mikinel/SwitchSystem/OnOffSwitch/ChairSwitch")]
    public class ChairSwitch : OnOffSwitch
    {
        [Space] 
        [SerializeField] private bool _isControlRenderer;   //イスのOnOffと同時にRendererのOnOffも行うか
        [SerializeField] private GameObject[] _chairObjects;
        
        protected override void OnStateChanged(int state)
        {
            base.OnStateChanged(state);

            var isOn = state == 1;

            foreach (var chairObject in _chairObjects)
            {
                if (chairObject == null)
                {
                    continue;
                }
                
                //chairObjectの全ての子、またはchairObjectからVRCStationを取得する
                var vrcStation = chairObject.GetComponentInChildren<VRCStation>();
                if (vrcStation == null)
                {
                    Debug.LogError($"{chairObject.name} has no VRCStation");
                    continue;
                }

                if (!isOn)
                {
#if UNITY_EDITOR
                    //NOTE : ClientSimの場合はプレイヤーが座っていない場合Errorが出るので、処置
                    var isSitNow = Vector3.Distance(vrcStation.stationEnterPlayerLocation.position,
                        Networking.LocalPlayer.GetPosition()) < 0.01f;

                    if (isSitNow)
                    {
                        vrcStation.ExitStation(Networking.LocalPlayer);
                    }
#else
                    vrcStation.ExitStation(Networking.LocalPlayer);
#endif
                }
                
                vrcStation.GetComponent<Collider>().enabled = isOn;
                
                //子オブジェクトに含まれるRendererのOnOff
                if (_isControlRenderer)
                {
                    var renderers = chairObject.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        renderer.enabled = isOn;
                    }
                }
            }
        }
    }
}