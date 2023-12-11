using UnityEngine;
using VRC.SDKBase;

namespace mikinel.vrc.SwitchSystem
{
    [AddComponentMenu("mikinel/SwitchSystem/OnOffSwitch/ChairSwitch")]
    public class ChairSwitch : OnOffSwitch
    {
        [Space] 
        [SerializeField] private VRCStation[] _vrcStations;
        
        protected override void OnStateChanged(int state)
        {
            base.OnStateChanged(state);

            var isOn = state == 1;

            foreach (var vrcStation in _vrcStations)
            {
                if (vrcStation == null)
                {
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
            }
        }
    }
}