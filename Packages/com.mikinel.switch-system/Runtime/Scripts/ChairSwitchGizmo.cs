using UnityEditor;
using UnityEngine;

namespace mikinel.vrc.SwitchSystem
{
    [DisallowMultipleComponent]
    public class ChairSwitchGizmo : MonoBehaviour
    {
        private ChairSwitch _chairSwitch;

        public void OnDrawGizmosSelected()
        {
            if(_chairSwitch == null)
            {
                _chairSwitch = GetComponent<ChairSwitch>();
            }
        
            if (_chairSwitch == null)
            {
                return;
            }
        
            var field = _chairSwitch.GetType().GetField("_chairObjects",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            var chairObjects = field.GetValue(_chairSwitch) as GameObject[];
        
            Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
            foreach (var chairObject in chairObjects)
            {
                if (chairObject == null)
                {
                    continue;
                }
                
                var rootPos = chairObject.transform.position;
                var distance = Vector3.Distance(rootPos, SceneView.lastActiveSceneView.camera.transform.position);

                // 距離が一定値以下の場合のみラベルを描画
                if (distance <= 10f) // ここで距離の閾値を設定
                {
                    //子オブジェクトからmeshの含まれるオブジェクトを全て取得
                    var meshFilters = chairObject.GetComponentsInChildren<MeshFilter>();
                    if (meshFilters != null || meshFilters.Length <= 0)
                    {
                        foreach (var meshFilter in meshFilters)
                        {
                            var gameObject = meshFilter.gameObject;
                            var mesh = meshFilter.sharedMesh;
                            var pos = gameObject.transform.position;
                            var rotation = gameObject.transform.rotation;
                            var scale = gameObject.transform.lossyScale;
                    
                            Gizmos.DrawMesh(mesh, pos, rotation, scale);   
                        }
                    }
                    else
                    {
                        Gizmos.DrawSphere(rootPos, 0.2f);
                    }

                    var text = chairObject.name;
                    var style = new GUIStyle
                    {
                        normal = { textColor = Color.white },
                        fontSize = 16,
                        alignment = TextAnchor.MiddleCenter
                    };

                    Handles.Label(rootPos + Vector3.up * 0.4f, text, style);
                }
            }
        }
    }
}