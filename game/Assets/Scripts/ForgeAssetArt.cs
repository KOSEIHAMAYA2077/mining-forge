using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MiningForge
{
    public partial class ForgeGame
    {
        readonly Dictionary<string,Sprite> artSprites=new Dictionary<string,Sprite>();
        readonly List<ForgeArtCracks> artCracks=new List<ForgeArtCracks>();
        Image artBoard,artDrop;
        bool artWaiting;
        int artHits,artCollections;
        float artContactError;
        [Serializable] class PickPivot {public float x,y;}
        Sprite ArtSprite(string name)
        {
            if(!artSprites.TryGetValue(name,out var sprite)){sprite=Resources.Load<Sprite>("MiningPrototype/BoardArt/"+name);if(!sprite)throw new InvalidOperationException("Missing Blender art: "+name);artSprites[name]=sprite;}return sprite;
        }
        Image ArtImage(Transform parent,string name,string sprite,float x,float y,float w,float h)
        {var image=Box(parent,name,x,y,w,h,Color.white);image.sprite=ArtSprite(sprite);return image;}
        void DrawArtBoard()
        {
            ingot=null;artCracks.Clear();
            artBoard=ArtImage(boardRoot,"Blender mineral slab",Session.Recipe.Id,CellX-13.2f,CellY-24,CellW*2*1.1f,CellH*4*1.1f);
            for(int i=0;i<Session.Recipe.Cells.Length;i++)
            {
                var c=Session.Recipe.Cells[i];var rect=Rect(boardRoot,"Surface fractures "+i,CellX+c.Column*CellW,CellY+c.Row*CellH,CellW,CellH);
                var cracks=rect.gameObject.AddComponent<ForgeArtCracks>();cracks.raycastTarget=false;cracks.Seed=i;artCracks.Add(cracks);
            }
        }
        void RefreshArt()
        {
            if(!artBoard||Session==null)return;
            artBoard.color=Color.Lerp(new Color(.55f,.63f,.69f),Color.white,Mathf.Clamp01(Session.Activity/1200f));
            for(int i=0;i<artCracks.Count;i++)
            {
                var c=Session.Recipe.Cells[i];float value=busy&&shownValues!=null?shownValues[i]:Session.Value(i);
                artCracks[i].color=new Color(.025f,.028f,.03f,Mathf.Clamp01(value/c.Target)*.8f);
            }
        }
        IEnumerator ArtPickSwing(float x,float y,int cell)
        {
            var info=JsonUtility.FromJson<PickPivot>(Resources.Load<TextAsset>("MiningPrototype/BoardArt/pick-pivot").text);
            var pick=ArtImage(root,"Blender pickaxe swing","pickaxe",x,y,230,230);pick.rectTransform.pivot=new Vector2(info.x,info.y);
            float time=0;while(time<.22f){time+=Time.deltaTime;float t=Mathf.SmoothStep(0,1,time/.22f);pick.rectTransform.anchoredPosition=new Vector2(x+45*(1-t),-y+100*(1-t));pick.rectTransform.localRotation=Quaternion.Euler(0,0,-65*(1-t));yield return null;}
            pick.rectTransform.anchoredPosition=new Vector2(x,-y);pick.rectTransform.localRotation=Quaternion.identity;artContactError=Vector2.Distance(pick.rectTransform.anchoredPosition,new Vector2(x,-y));artHits++;
            var flash=Box(root,"Contact light",x-20,y-20,40,40,new Color(1,.88f,.6f,.85f));flash.sprite=spark;Destroy(flash.gameObject,.09f);
            yield return new WaitForSeconds(.06f);Destroy(pick.gameObject);
        }
        void ArtDebris(float x,float y,int count)
        {
            for(int i=0;i<count;i++)
            {
                var chip=ArtImage(root,"Rock fragment","chip",x,y,UnityEngine.Random.Range(9,23),UnityEngine.Random.Range(9,23));chip.rectTransform.localRotation=Quaternion.Euler(0,0,UnityEngine.Random.Range(0,360));
                StartCoroutine(Particle(chip,new Vector2(UnityEngine.Random.Range(-130f,130f),UnityEngine.Random.Range(100f,220f))));
            }
            var tex=Resources.Load<Texture2D>("Art/smoke_01");var dust=Box(root,"Rock powder",x-30,y-25,60,60,new Color(.55f,.61f,.65f,.4f));dust.sprite=Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),Vector2.one*.5f);
            StartCoroutine(ArtDust(dust));
        }
        IEnumerator ArtDust(Image dust)
        {float t=0;var sprite=dust.sprite;while(t<.5f&&dust){t+=Time.deltaTime;dust.rectTransform.sizeDelta=Vector2.one*(60+t*75);dust.color=new Color(.55f,.61f,.65f,.4f*(1-t/.5f));yield return null;}if(dust)Destroy(dust.gameObject);Destroy(sprite);}
        IEnumerator ReleaseArt()
        {
            artWaiting=false;Clear(overlay);Clear(menuRoot);buttons.Clear();feedback.text="加工した原石を取り出しています。";
            foreach(var c in Session.Recipe.Cells)ArtDebris(CellX+c.Column*CellW+CellW/2,CellY+c.Row*CellH+CellH/2,7);
            float t=0;while(t<.45f){t+=Time.deltaTime;if(artBoard)artBoard.color=new Color(1,1,1,1-t/.45f);yield return null;}
            Clear(boardRoot);artDrop=ArtImage(boardRoot,"Released mineral","crystal",CellX+48,CellY+110,170,170);artDrop.raycastTarget=true;
            var button=artDrop.gameObject.AddComponent<Button>();button.targetGraphic=artDrop;button.onClick.AddListener(CollectArt);
            Label(boardRoot,"原石を回収",CellX+28,CellY+310,275,55,27,gold);
            var take=Button(menuRoot,"原石を回収",0,0,282,53,CollectArt);take.onClick.RemoveAllListeners();take.onClick.AddListener(CollectArt);buttons.Add(take);
            feedback.text="原石をクリック、または Enter で回収します。";artWaiting=true;
            while(artWaiting){if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter))CollectArt();yield return null;}
            t=0;Vector2 from=artDrop.rectTransform.anchoredPosition;
            while(t<.45f){t+=Time.deltaTime;artDrop.rectTransform.anchoredPosition=Vector2.Lerp(from,new Vector2(440,-65),t/.45f);artDrop.rectTransform.localScale=Vector3.one*Mathf.Lerp(1,.2f,t/.45f);yield return null;}
            audioSource.PlayOneShot(chime);busy=false;feedback.text="原石を1個 回収しました。";Show(View.Result);
        }
        void CollectArt()
        {if(!artWaiting)return;artWaiting=false;artCollections++;if(artDrop)artDrop.raycastTarget=false;}
    }
    public class ForgeArtCracks:MaskableGraphic
    {
        public int Seed;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var random=new System.Random(Seed+17);float w=rectTransform.rect.width,h=rectTransform.rect.height;
            for(int branch=0;branch<3;branch++)
            {
                Vector2 p=new Vector2(w*.5f,-h*.5f);float angle=(float)(random.NextDouble()*Math.PI*2);
                for(int j=0;j<3;j++){Vector2 q=p+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*12;q.x=Mathf.Clamp(q.x,5,w-5);q.y=Mathf.Clamp(q.y,-h+5,-5);var n=(q-p).normalized;var side=new Vector2(-n.y,n.x)*1.1f;int index=vh.currentVertCount;vh.AddVert(p-side,color,Vector2.zero);vh.AddVert(p+side,color,Vector2.zero);vh.AddVert(q+side,color,Vector2.zero);vh.AddVert(q-side,color,Vector2.zero);vh.AddTriangle(index,index+1,index+2);vh.AddTriangle(index,index+2,index+3);p=q;angle+=(float)(random.NextDouble()-.5);}
            }
        }
    }
}
