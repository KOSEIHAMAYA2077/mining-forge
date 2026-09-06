using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MiningForge
{
    public partial class MiningGame : MonoBehaviour
    {
        public const string Version = "P0a-01";
        MiningConfig config;
        MiningSession session;
        int selected, seed = 1701;
        bool opened = true, confirmAbandon;
        float volume = .65f, shake = .35f;
        Camera cam;
        Vector3 cameraHome;
        Font font;
        AudioSource sound;
        AudioClip[] impacts;
        AudioClip chime, success, damage;
        readonly Transform[] crystals = new Transform[3], shells = new Transform[3];
        readonly Vector3[] points = { new Vector3(-1.05f, .7f, 0), new Vector3(.25f, .8f, .25f), new Vector3(1.3f, .6f, -.25f) };
        readonly List<GameObject> worldObjects = new List<GameObject>();
        Material rockMat, crystalMat, brightMat;
        Text focusText, qualityText, statusText, toast, resultText, titleText;
        GameObject miningPanel, resultPanel, closedPanel;
        Button collectButton, closeButton, abandonButton;
        readonly Button[] skills = new Button[4], parts = new Button[3];
        readonly Text[] valuesText = new Text[3], partStatus = new Text[3];
        readonly Image[] gaugeFill = new Image[3], selection = new Image[3];
        readonly RectTransform[] centerMarks = new RectTransform[3];
        readonly string[] names = { "左の根元", "中央の根元", "右の根元" };
        readonly Color ink = new Color(.035f,.075f,.11f), panel = new Color(.055f,.115f,.15f,.97f), muted = new Color(.59f,.70f,.74f), white = new Color(.90f,.95f,.93f), teal = new Color(.26f,.87f,.72f), gold = new Color(1f,.77f,.38f), red = new Color(1f,.43f,.38f);
        RectTransform canvasRect;

        void Start()
        {
            Application.targetFrameRate = 60;
            config = JsonUtility.FromJson<MiningConfig>(Resources.Load<TextAsset>("MiningConfig").text);
            font = Resources.Load<Font>("Fonts/NotoSansCJKjp-Regular");
            sound = gameObject.AddComponent<AudioSource>(); sound.spatialBlend = 0;
            impacts = new AudioClip[5]; for(int i=0;i<5;i++) impacts[i] = Resources.Load<AudioClip>("Audio/impactMining_00"+i);
            chime = Tone("Band", new []{659f, 880f}, .22f);
            success = Tone("Recovery", new []{523f,659f,784f,1047f}, .18f);
            damage = Tone("Damage", new []{180f,120f,80f}, .13f);
            BuildWorld(); BuildUI(); NewSample(seed);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--smoke-test") >= 0) StartCoroutine(SmokeTest());
        }

        void Update()
        {
            SampleAudio();
            if(Input.GetKeyDown(KeyCode.Escape) && !session.Busy && session.State==NodeState.Active) ToggleOpen();
            if(!opened || session.State!=NodeState.Active) return;
            if(Input.GetKeyDown(KeyCode.Alpha1)) SelectPart(0);
            if(Input.GetKeyDown(KeyCode.Alpha2)) SelectPart(1);
            if(Input.GetKeyDown(KeyCode.Alpha3)) SelectPart(2);
            if(Input.GetKeyDown(KeyCode.Q)) Strike(0);
            if(Input.GetKeyDown(KeyCode.W)) Strike(1);
            if(Input.GetKeyDown(KeyCode.E)) Strike(2);
            if(Input.GetKeyDown(KeyCode.R)) Strike(3);
            if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if(Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hit))
                {
                    var marker=hit.collider.GetComponent<PartMarker>(); if(marker) SelectPart(marker.index);
                }
            }
        }

        void NewSample(int newSeed)
        {
            if(session != null && session.Busy) return;
            seed=newSeed; session=new MiningSession(config,seed); selected=0; opened=true; confirmAbandon=false;
            foreach(var t in crystals) t.gameObject.SetActive(true);
            for(int i=0;i<3;i++) {shells[i].localScale=Vector3.one; crystals[i].localScale=Vector3.one;}
            toast.text="まず部位を選び、強打で大きく進める。\n緑の帯が近づいたら、そっと削ろう。";
            Refresh();
        }

        void SelectPart(int part)
        {
            if(session.Busy) return; selected=part; confirmAbandon=false; Refresh();
        }
        void ToggleOpen()
        {
            if(session.Busy) return; opened=!opened; confirmAbandon=false; Refresh();
        }
        void Strike(int skill)
        {
            if(!opened) return;
            var hits=session.TryStrike(skill,selected); if(hits.Length==0) return;
            confirmAbandon=false; Refresh(); StartCoroutine(AnimateStrike(hits,skill));
        }
        IEnumerator AnimateStrike(Strike[] hits, int skill)
        {
            bool crit=false, band=false, over=false;
            string message="";
            foreach(var hit in hits)
            {
                crit|=hit.critical; band|=hit.enteredBand; over|=hit.overBand;
                message+=(message.Length>0?"  /  ":"")+(hit.part+1)+"番 +"+hit.amount+(hit.critical?" 会心!":"");
                Burst(points[hit.part]+Vector3.up*.4f,hit.critical?gold:hit.overBand?red:teal,hit.critical?24:12);
                StartCoroutine(PickSwing(points[hit.part],skill));
            }
            sound.pitch=skill==1?.78f:skill==2?1.25f:1;
            sound.PlayOneShot(impacts[session.Actions%impacts.Length],volume*(skill==2?.55f:1));
            toast.text=message+"\n"+(crit?"会心！ 芯を守り、中心で止まる。":over?"削りすぎ。赤い境界まで進めると壊れます。":band?"適正帯に到達！ この部位は仕上がり。":"打撃が入りました。次の一手を選ぼう。");
            float elapsed=0;
            while(elapsed<.38f)
            {
                elapsed+=Time.deltaTime;
                cam.transform.position=cameraHome+cam.transform.right*(Mathf.Sin(elapsed*90)*(.38f-elapsed)*shake*(crit?.22f:.1f));
                foreach(var hit in hits) crystals[hit.part].localScale=Vector3.one*(1+Mathf.Sin(elapsed*35)*.08f*(1-elapsed/.4f));
                yield return null;
            }
            cam.transform.position=cameraHome;
            foreach(var hit in hits)
            {
                crystals[hit.part].localScale=Vector3.one;
                float shrink=Mathf.Lerp(1,.38f,Mathf.Clamp01((float)session.Value(hit.part)/config.centers[hit.part]));
                shells[hit.part].localScale=new Vector3(shrink,shrink,shrink);
            }
            sound.pitch=1;
            if(session.State==NodeState.Destroyed) {sound.PlayOneShot(damage,volume); toast.text="致命的損傷。結晶の芯が割れてしまった。"; foreach(var t in crystals) t.localScale=new Vector3(1,.35f,1);}
            else if(crit||band) sound.PlayOneShot(chime,volume*.65f);
            session.FinishAnimation(); Refresh();
        }
        void Collect()
        {
            if(!session.TryCollect()) return;
            sound.pitch=1; sound.PlayOneShot(session.Quality>=80?success:session.Quality>=40?chime:damage,volume);
            Burst(new Vector3(0,1.8f,0),session.Quality>=80?gold:teal,38);
            toast.text="原石を1個、回収しました。"; Refresh();
        }
        void Abandon()
        {
            if(!confirmAbandon){confirmAbandon=true;Refresh();return;}
            if(session.TryAbandon()){toast.text="この結晶の採取を終了しました。報酬はありません。";Refresh();}
        }

        void Refresh()
        {
            bool active=session.State==NodeState.Active;
            miningPanel.SetActive(active&&opened); closedPanel.SetActive(active&&!opened); resultPanel.SetActive(!active);
            titleText.text="結晶の試掘   /   "+Version+"   /   SEED "+seed;
            focusText.text="集中力  "+session.Focus+" / "+config.focus;
            qualityText.text="見込み品質  "+session.Quality+"  ·  "+MiningSession.Rank(session.Quality);
            for(int i=0;i<3;i++)
            {
                selection[i].color=selected==i?new Color(.10f,.27f,.29f):panel;
                valuesText[i].text=(i+1)+"  "+names[i]+"       "+session.Value(i)+"  /  中心 "+config.centers[i];
                gaugeFill[i].rectTransform.sizeDelta=new Vector2(536*Mathf.Clamp01(session.Value(i)/100f),9);
                gaugeFill[i].color=session.Value(i)>session.Upper(i)?red:teal;
                partStatus[i].text=session.Value(i)>=session.Fatal(i)?"× 致命傷：回収できません":session.Value(i)>session.Upper(i)?"! 削りすぎ  /  "+session.Fatal(i)+"で致命傷":session.InBand(i)?"✓ 適正帯：仕上がり":session.Value(i)>=session.Separation(i)?"分離OK  /  あと少しで適正帯":"未分離  /  "+session.Separation(i)+"以上で外せます";
                partStatus[i].color=session.Value(i)>session.Upper(i)?red:session.InBand(i)?teal:muted;
                parts[i].interactable=!session.Busy;
                crystals[i].GetComponent<Renderer>().sharedMaterial=selected==i?brightMat:crystalMat;
            }
            for(int i=0;i<4;i++) skills[i].interactable=session.CanStrike(i);
            collectButton.interactable=session.CanCollect;
            closeButton.interactable=!session.Busy; abandonButton.interactable=!session.Busy;
            abandonButton.GetComponentInChildren<Text>().text=confirmAbandon?"本当に放棄（もう一度）":"放棄する";
            statusText.text=session.Busy?"打撃中…":session.CanCollect?"分離できます。仕上げるか、ここで回収。":session.Focus<2?"集中力不足。未分離のため放棄で終了。":"3部位を分離ラインまで進めると回収できます。";
            if(!active)
                resultText.text=session.State==NodeState.Collected?"採取成功\n\n"+MiningSession.Rank(session.Quality)+"の原石 × 1\n品質 "+session.Quality+" / 100\n\n使った手数  "+session.Actions+"\n残った集中力  "+session.Focus+"\n\nこの原石は試遊結果です。\n工房・持ち帰り保存はP0bで追加予定。":session.State==NodeState.Destroyed?"結晶が壊れました\n\n報酬なし\n\n赤い境界が致命傷のラインです。\n帯の近くでは「そっと削る」を。":"採取を放棄しました\n\n報酬なし\n\n次の試遊で、別の採り方を試せます。";
        }

        AudioClip Tone(string name,float[] notes,float duration)
        {
            int rate=44100; float[] data=new float[(int)(rate*duration*notes.Length)];
            for(int i=0;i<data.Length;i++) {int n=Math.Min(notes.Length-1,(int)(i/(rate*duration))); float t=i/(float)rate-n*duration; data[i]=(Mathf.Sin(t*notes[n]*Mathf.PI*2)+.2f*Mathf.Sin(t*notes[n]*Mathf.PI*4))*.18f*Mathf.Exp(-t*12)*Mathf.Min(1,t*180);}
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
    }
    public class PartMarker : MonoBehaviour { public int index; }
}
