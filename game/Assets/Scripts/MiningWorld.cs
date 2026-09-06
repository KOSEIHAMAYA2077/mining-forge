using System.Collections;
using UnityEngine;

namespace MiningForge
{
    public partial class MiningGame
    {
        Material Material(string name,Color color,bool emission=false)
        {
            var m=new Material(Resources.Load<Material>("Materials/"+(emission?"Crystal":"Rock")));m.name=name;m.color=color;
            m.SetFloat("_Smoothness",emission?.65f:.12f);
            if(emission){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*.45f);}return m;
        }
        GameObject Primitive(PrimitiveType type,string name,Vector3 position,Vector3 scale,Material mat)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;return go;
        }
        GameObject Rock(string name,Vector3 position,Vector3 scale)
        {
            var prefab=Resources.Load<GameObject>("Art/"+name);var go=Instantiate(prefab,position,Quaternion.identity);
            go.transform.localScale=scale;foreach(var r in go.GetComponentsInChildren<Renderer>())r.sharedMaterial=rockMat;return go;
        }
        void BuildWorld()
        {
            var backdrop=new GameObject("Background camera",typeof(Camera)).GetComponent<Camera>();
            backdrop.depth=-10;backdrop.cullingMask=0;backdrop.clearFlags=CameraClearFlags.SolidColor;backdrop.backgroundColor=ink;
            cam=new GameObject("Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();cam.tag="MainCamera";
            cam.rect=new Rect(0,0,.545f,1);cam.backgroundColor=new Color(.045f,.09f,.13f);cam.clearFlags=CameraClearFlags.SolidColor;
            cam.transform.position=new Vector3(5,5,-9);cam.transform.LookAt(new Vector3(0,.9f,0));cam.orthographic=true;cam.orthographicSize=4.1f;cameraHome=cam.transform.position;
            RenderSettings.ambientLight=new Color(.34f,.47f,.56f);RenderSettings.fog=true;RenderSettings.fogColor=cam.backgroundColor;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.026f;
            var light=new GameObject("Warm key",typeof(Light)).GetComponent<Light>();light.type=LightType.Directional;light.intensity=1.7f;light.color=new Color(1,.88f,.67f);light.transform.rotation=Quaternion.Euler(48,-30,0);light.shadows=LightShadows.Soft;
            var rim=new GameObject("Crystal glow",typeof(Light)).GetComponent<Light>();rim.type=LightType.Point;rim.color=new Color(.2f,.9f,1);rim.intensity=1.8f;rim.range=4;rim.transform.position=new Vector3(0,2,1);
            rockMat=Material("Basalt",new Color(.24f,.31f,.34f));crystalMat=Material("Crystal",new Color(.18f,.65f,.68f),true);brightMat=Material("Selected crystal",new Color(.43f,.92f,.78f),true);
            var ground=Material("Ground",new Color(.12f,.19f,.22f));
            Primitive(PrimitiveType.Cylinder,"Display bed",new Vector3(0,-.38f,0),new Vector3(4.5f,.24f,3.4f),ground);
            Primitive(PrimitiveType.Plane,"Floor",new Vector3(0,-.65f,0),Vector3.one*4,ground);
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI*2/12;Rock(i%2==0?"rock_largeA":"rock_largeC",new Vector3(Mathf.Cos(a)*3.3f,-.5f,Mathf.Sin(a)*2.8f),Vector3.one*(i%3==0?1.3f:.65f));
            }
            for(int i=0;i<3;i++)
            {
                shells[i]=new GameObject("Mother rock "+i).transform;shells[i].position=points[i]-Vector3.up*.65f;
                var r=Rock("rock_largeA",shells[i].position,new Vector3(1.1f,.9f,1));r.transform.SetParent(shells[i],true);
                var go=new GameObject("Crystal "+i,typeof(MeshFilter),typeof(MeshRenderer),typeof(BoxCollider),typeof(PartMarker));
                go.transform.position=points[i]-Vector3.up*.1f;go.transform.rotation=Quaternion.Euler(0,i*35,(i-1)*-13);
                go.GetComponent<MeshFilter>().sharedMesh=CrystalMesh(.40f,1.4f+i%2*.45f);go.GetComponent<MeshRenderer>().sharedMaterial=crystalMat;
                go.GetComponent<BoxCollider>().center=new Vector3(0,.65f,0);go.GetComponent<BoxCollider>().size=new Vector3(.85f,1.9f,.85f);go.GetComponent<PartMarker>().index=i;crystals[i]=go.transform;
                // A physical numbered plaque next to each root, facing the camera.
                var tag=new GameObject("Root number "+i,typeof(TextMesh));tag.transform.position=points[i]+new Vector3(0,-.3f,-.65f);tag.transform.rotation=cam.transform.rotation;
                var tm=tag.GetComponent<TextMesh>();tm.text=(i+1).ToString();tm.font=font;tm.fontSize=64;tm.characterSize=.055f;tm.anchor=TextAnchor.MiddleCenter;tm.color=new Color(.9f,1,.91f);tag.GetComponent<Renderer>().sharedMaterial=font.material;
            }
            // Small warm survey lamp gives a scale cue and anchors the diorama.
            var metal=Material("Tool metal",new Color(.56f,.60f,.58f));var amber=Material("Lamp light",new Color(1,.62f,.18f),true);
            Primitive(PrimitiveType.Cylinder,"Lamp body",new Vector3(-2,.03f,-.4f),new Vector3(.32f,.42f,.32f),metal);
            Primitive(PrimitiveType.Sphere,"Lamp glass",new Vector3(-2,.45f,-.4f),new Vector3(.37f,.5f,.37f),amber);
        }
        Mesh CrystalMesh(float radius,float height)
        {
            var vertices=new System.Collections.Generic.List<Vector3>();var triangles=new System.Collections.Generic.List<int>();
            for(int i=0;i<6;i++)
            {
                float a=i*Mathf.PI/3,b=(i+1)*Mathf.PI/3;
                Vector3 lo=new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius), next=new Vector3(Mathf.Cos(b)*radius,0,Mathf.Sin(b)*radius);
                Vector3 hi=lo+Vector3.up*height*.72f,hiNext=next+Vector3.up*height*.72f,tip=Vector3.up*height;
                int n=vertices.Count;vertices.AddRange(new[]{lo,hi,next,next,hi,hiNext,hi,tip,hiNext});
                for(int j=0;j<9;j++)triangles.Add(n+j);
            }
            var mesh=new Mesh();mesh.name="Six-sided crystal";mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();return mesh;
        }
        void Burst(Vector3 point,Color color,int count)
        {
            for(int i=0;i<count;i++)
            {
                bool dust=i%3==0;var go=GameObject.CreatePrimitive(PrimitiveType.Quad);Destroy(go.GetComponent<Collider>());
                var m=new Material(Resources.Load<Material>("Materials/Particle"));
                m.SetTexture("_BaseMap",Resources.Load<Texture2D>("Art/"+(dust?"smoke_01":"spark_01")));m.color=dust?new Color(.65f,.72f,.72f,.6f):color;go.GetComponent<Renderer>().sharedMaterial=m;
                go.transform.position=point;go.transform.rotation=cam.transform.rotation;go.transform.localScale=Vector3.one*(dust?.26f:.11f);
                StartCoroutine(Fly(go,m,Random.insideUnitSphere*1.6f+Vector3.up*1.5f,dust));
            }
        }
        IEnumerator Fly(GameObject go,Material mat,Vector3 velocity,bool dust)
        {
            float t=0;Color color=mat.color;
            while(t<.7f){t+=Time.deltaTime;velocity+=Vector3.down*Time.deltaTime*3;go.transform.position+=velocity*Time.deltaTime;go.transform.localScale*=1+Time.deltaTime*(dust?1:-.5f);mat.color=new Color(color.r,color.g,color.b,color.a*(1-t/.7f));yield return null;}
            Destroy(go);Destroy(mat);
        }
        IEnumerator PickSwing(Vector3 target,int skill)
        {
            var tool=new GameObject("Pick strike");var handle=Primitive(PrimitiveType.Cylinder,"Handle",Vector3.zero,new Vector3(.07f,.5f,.07f),rockMat);handle.transform.SetParent(tool.transform,false);
            var head=Primitive(PrimitiveType.Cube,"Pick head",Vector3.zero,new Vector3(.65f,.13f,.13f),brightMat);head.transform.SetParent(tool.transform,false);head.transform.localPosition=Vector3.up*.48f;
            float t=0;while(t<.25f){t+=Time.deltaTime;tool.transform.position=target+new Vector3(.3f,1.1f-t*2,0);tool.transform.rotation=Quaternion.Euler(0,0,Mathf.Lerp(-100,-25,t/.25f));yield return null;}Destroy(tool);
        }
    }
}
