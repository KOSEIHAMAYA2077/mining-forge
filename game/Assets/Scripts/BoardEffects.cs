using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MiningForge.Board;

namespace MiningForge
{
    public partial class BoardGame
    {
        void MakeAudio()
        {
            source=gameObject.AddComponent<AudioSource>();source.spatialBlend=0;source.volume=volume;
            tapSound=Resources.Load<AudioClip>("Audio/impactMining_001");strongSound=Resources.Load<AudioClip>("Audio/impactMining_004");
            gentleSound=MakeTone("Fine scrape",new[]{1800f,1500f,1200f},.06f,.65f);
            critSound=MakeTone("Critical",new[]{784f,1175f,1568f},.12f);
            bandSound=MakeTone("In band",new[]{523f,659f},.10f);
            overSound=MakeTone("Over band",new[]{420f,300f},.12f);
            brokenSound=MakeTone("Broken",new[]{130f,91f,56f},.14f,.45f);
            collectSound=MakeTone("Collected",new[]{523f,659f,784f,1047f},.17f);
            adjustSound=MakeTone("Activity",new[]{260f,390f,520f},.1f);
        }
        AudioClip MakeTone(string name,float[] notes,float seconds,float noise=0)
        {
            const int rate=44100;var data=new float[(int)(rate*seconds*notes.Length)];var rng=new System.Random(27);
            for(int i=0;i<data.Length;i++){int n=Mathf.Min(notes.Length-1,(int)(i/(rate*seconds)));float t=i/(float)rate-n*seconds;float env=Mathf.Min(1,t*400)*Mathf.Exp(-t*15);data[i]=(Mathf.Sin(t*notes[n]*Mathf.PI*2)*.22f+((float)rng.NextDouble()*2-1)*noise*.16f)*env;}
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        IEnumerator Animate(CellHit[] hits,Skill used)
        {
            bool broken=false,over=false,critical=false,band=false;string message="";
            foreach(var hit in hits)
            {
                broken|=hit.newlyBroken;over|=hit.over;critical|=hit.critical;band|=hit.enteredBand;
                var c=config.cells[hit.cell];Vector2 point=new Vector2(382+c.column*140+66,-10-c.row*128-56);
                Particles(point,hit.newlyBroken?red:hit.critical?gold:cyan,hit.critical?22:13);
                StartCoroutine(Swing(point,used));StartCoroutine(PopText(point,c.id+" +"+(hit.after-hit.before)+(hit.critical?" 会心":""),hit.critical?gold:paper));
                message+=(message.Length==0?"":"   ")+c.id+" +"+(hit.after-hit.before)+(hit.critical?" ◆会心":hit.exact?" ◆中心":hit.newlyBroken?" ×破損":hit.over?" !帯越え":hit.enteredBand?" ✓適正":"");
            }
            if(hits.Length==0){source.pitch=used==Skill.Calm?.7f:1;source.PlayOneShot(adjustSound);message="活性を"+(used==Skill.Raise?"上げました":"鎮めました")+"。現在 "+session.Activity;}
            else {source.pitch=used==Skill.Strong?.75f:1;source.PlayOneShot(used==Skill.Gentle?gentleSound:used==Skill.Strong?strongSound:tapSound);}
            feedback.text=message;
            float t=0;
            while(t<.36f){t+=Time.deltaTime;motionRoot.anchoredPosition=new Vector2(Mathf.Sin(t*86)*(1-t/.36f)*shake*(critical?9:5),0);yield return null;}
            motionRoot.anchoredPosition=Vector2.zero;source.pitch=1;
            // One state cue per command, in the specification's priority order.
            if(broken)source.PlayOneShot(brokenSound);else if(over)source.PlayOneShot(overSound);else if(critical)source.PlayOneShot(critSound);else if(band)source.PlayOneShot(bandSound);
            session.FinishAnimation();Open(BoardScreen.Commands);
        }
        IEnumerator Swing(Vector2 point,Skill used)
        {
            var pivot=Rect(boardRoot,"Pick animation",point.x,-point.y,1,1);
            var handle=Box(pivot,"Handle",-5,-85,10,91,new Color(.54f,.35f,.18f));
            Box(pivot,"Handle light",-3,-85,3,90,gold);
            var head=Rect(pivot,"Pick head",-29,-91,62,22);var graphic=head.gameObject.AddComponent<PickGraphic>();graphic.color=paper;graphic.raycastTarget=false;
            float t=0;while(t<.22f){t+=Time.deltaTime;pivot.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(-65,25,t/.22f));yield return null;}Destroy(pivot.gameObject);
        }
        void Particles(Vector2 point,Color color,int count,Transform parent=null)
        {
            for(int i=0;i<count;i++)
            {
                bool dust=i%3==0;float size=dust?46:10;
                var im=Box(parent?parent:boardRoot,"Debris",point.x-size/2,-point.y-size/2,size,size,dust?new Color(.67f,.72f,.69f,.55f):color);
                if(dust||i%2==0){var tx=Resources.Load<Texture2D>("Art/"+(dust?"smoke_01":"spark_01"));im.sprite=Sprite.Create(tx,new Rect(0,0,tx.width,tx.height),new Vector2(.5f,.5f));}
                StartCoroutine(Fly(im,new Vector2(UnityEngine.Random.Range(-150,150),UnityEngine.Random.Range(45,150)),dust));
            }
        }
        IEnumerator Fly(Image im,Vector2 speed,bool dust)
        {
            float t=0;Color color=im.color;while(t<.65f){t+=Time.deltaTime;speed.y-=Time.deltaTime*380;im.rectTransform.anchoredPosition+=speed*Time.deltaTime;im.color=new Color(color.r,color.g,color.b,color.a*(1-t/.65f));im.rectTransform.localScale*=1+Time.deltaTime*(dust?1.1f:-.3f);yield return null;}Destroy(im.gameObject);
        }
        IEnumerator PopText(Vector2 point,string value,Color color)
        {
            var text=Text(boardRoot,value,point.x-60,-point.y-27,215,46,22,color);text.transform.SetAsLastSibling();float t=0;
            while(t<.8f){t+=Time.deltaTime;text.rectTransform.anchoredPosition+=Vector2.up*Time.deltaTime*35;text.color=new Color(color.r,color.g,color.b,1-t/.8f);yield return null;}Destroy(text.gameObject);
        }
        IEnumerator Recovery()
        {
            yield return null;
            for(int i=0;i<6;i++){Particles(new Vector2(850+i*80,-475),gold,10,root);yield return new WaitForSeconds(.055f);}
        }
    }
    public class PickGraphic:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {vh.Clear();OreCellGraphic.Poly(vh,color,new Vector2(0,-16),new Vector2(14,-2),new Vector2(49,-2),new Vector2(62,-20),new Vector2(43,-10),new Vector2(20,-10));}
    }
}
