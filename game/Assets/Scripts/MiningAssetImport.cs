using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace MiningForge
{
    public class MiningAssetImport : MonoBehaviour
    {
        IEnumerator Start()
        {
            var ore=Instantiate(Resources.Load<GameObject>("MiningPrototype/Prefabs/OreNode"));
            var pick=Instantiate(Resources.Load<GameObject>("MiningPrototype/Prefabs/Pickaxe"),new Vector3(1.35f,0,0),Quaternion.identity);
            var cam=new GameObject("Camera",typeof(Camera)).GetComponent<Camera>();cam.transform.position=new Vector3(3,2.7f,-5);cam.transform.LookAt(new Vector3(.5f,.6f,0));cam.orthographic=true;cam.orthographicSize=1.4f;cam.backgroundColor=new Color(.06f,.09f,.13f);cam.clearFlags=CameraClearFlags.SolidColor;
            var light=new GameObject("Light",typeof(Light)).GetComponent<Light>();light.type=LightType.Directional;light.intensity=2;light.transform.rotation=Quaternion.Euler(35,-30,0);RenderSettings.ambientLight=Color.gray;
            Screen.SetResolution(1281,720,false);yield return new WaitForSeconds(.4f);Screen.SetResolution(1280,720,false);yield return new WaitForSeconds(1);
            foreach(var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))foreach(var m in r.sharedMaterials)if(!m||!m.shader.isSupported)throw new Exception("Unsupported imported material");
            var b=ore.GetComponent<BoxCollider>().bounds;if(b.size.y<1||b.size.y>2||b.size.x<.8f||b.size.x>1.5f)throw new Exception("Ore import scale/axis incorrect "+b);
            Transform tip=null;foreach(var t in pick.GetComponentsInChildren<Transform>())if(t.name=="StrikeTip")tip=t;if(!tip||Mathf.Abs(tip.position.y-.59f)>.03f)throw new Exception("Pick axis incorrect");
            yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../artifacts/MiningAssets"));Directory.CreateDirectory(dir);File.WriteAllBytes(Path.Combine(dir,"00-import.png"),image.EncodeToPNG());Destroy(image);
            Debug.Log("ASSET_IMPORT_OK scale, axis, materials, collider, strike tip");Application.Quit(0);
        }
    }
}
