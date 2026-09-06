using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MiningForge.Reference;

namespace MiningForge
{
    public partial class ForgeGame
    {
        Sprite spark;
        void MakeAudio()
        {
            audioSource=gameObject.AddComponent<AudioSource>();audioSource.volume=volume;audioSource.spatialBlend=0;
            strike=Resources.Load<AudioClip>("Audio/impactMining_001");
            chime=Tone("Forge chime",new[]{660f,880f,1320f},.12f);
            heatSound=Tone("Forge heat",new[]{220f,330f,440f},.11f);
            errorSound=Tone("Forge unavailable",new[]{180f,120f},.12f);
            var tx=Resources.Load<Texture2D>("Art/spark_01");spark=Sprite.Create(tx,new Rect(0,0,tx.width,tx.height),Vector2.one*.5f);
        }
        AudioClip Tone(string name,float[] notes,float duration)
        {
            const int rate=44100;var samples=new float[Mathf.CeilToInt(rate*duration*notes.Length)];
            for(int i=0;i<samples.Length;i++){int n=Mathf.Min(notes.Length-1,(int)(i/(rate*duration)));float t=i/(float)rate-n*duration;samples[i]=Mathf.Sin(t*notes[n]*Mathf.PI*2)*Mathf.Min(1,t*160)*Mathf.Exp(-t*17)*.22f;}
            var clip=AudioClip.Create(name,samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        IEnumerator Animate(ForgeAction action,Technique used)
        {
            foreach(var hit in action.Hits)
            {
                var cell=Session.Recipe.Cells[hit.Cell];float x=CellX+cell.Column*CellW+CellW/2,y=CellY+cell.Row*CellH+CellH/2;
                var pivot=Rect(root,"Hammer swing",x,y,1,1);
                Box(pivot,"Handle",-5,-104,10,113,new Color(.52f,.29f,.12f));Box(pivot,"Handle highlight",-4,-104,2,113,gold);
                Box(pivot,"Hammer steel",-30,-121,60,33,new Color(.52f,.58f,.62f));Box(pivot,"Hammer face",-30,-121,60,5,paper);
                float t=0;while(t<.13f){t+=Time.deltaTime;pivot.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(-50,10,t/.13f));yield return null;}
                audioSource.pitch=used==Technique.Gentle?1.25f:used==Technique.Triple||used==Technique.SuperQuad?.8f:1;audioSource.PlayOneShot(strike);
                Destroy(pivot.gameObject);
                for(int n=0;n<(hit.Critical?18:9);n++)
                {
                    var p=Box(root,"Spark",x,y,12,12,hit.Critical?paper:gold);p.sprite=spark;
                    StartCoroutine(Particle(p,new Vector2(UnityEngine.Random.Range(-190f,190f),UnityEngine.Random.Range(80f,240f))));
                }
                string display=hit.Miss?"ミス":(hit.After-hit.Before).ToString()+(hit.Critical?"  会心！":"");
                var label=Label(root,display,x-72,y-51,230,61,32,hit.Critical?gold:paper);StartCoroutine(FloatText(label));
                feedback.text=ForgeTechniques.All[(int)used].Name+"   "+display;
                if(hit.Critical)audioSource.PlayOneShot(chime);
                float time=0;while(time<.15f){time+=Time.deltaTime;shownValues[hit.Cell]=Mathf.Lerp(hit.Before,hit.After,time/.15f);RefreshBoard();((RectTransform)boardRoot).anchoredPosition=new Vector2(Mathf.Sin(time*110)*shake*8,0);yield return null;}shownValues[hit.Cell]=hit.After;((RectTransform)boardRoot).anchoredPosition=Vector2.zero;
            }
            audioSource.pitch=1;
            if(action.Hits.Count==0){audioSource.PlayOneShot(used==Technique.Ultimate?chime:heatSound);feedback.text=used==Technique.Ultimate?"次の行動に 会心の力が宿った。":"活性度が "+action.AfterActivity+" になった。";yield return new WaitForSeconds(.3f);}
            if(!string.IsNullOrEmpty(action.EventMessage))feedback.text=action.EventMessage;
            if(action.Charged){feedback.text="ひっさつチャージ！";audioSource.PlayOneShot(chime);}
            busy=false;Show(View.Commands);
        }
        IEnumerator Particle(Image image,Vector2 velocity)
        {float t=0;while(t<.5f&&image){t+=Time.deltaTime;velocity.y-=Time.deltaTime*460;image.rectTransform.anchoredPosition+=velocity*Time.deltaTime;var color=image.color;color.a=1-t/.5f;image.color=color;yield return null;}if(image)Destroy(image.gameObject);}
        IEnumerator FloatText(Text text)
        {float t=0;while(t<.9f&&text){t+=Time.deltaTime;text.rectTransform.anchoredPosition+=Vector2.up*Time.deltaTime*33;var color=text.color;color.a=1-t/.9f;text.color=color;yield return null;}if(text)Destroy(text.gameObject);}
    }
}
