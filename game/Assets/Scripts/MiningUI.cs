using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MiningForge
{
    public partial class MiningGame
    {
        RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform)); var rt=go.GetComponent<RectTransform>(); rt.SetParent(parent,false);
            rt.anchorMin=rt.anchorMax=new Vector2(0,1); rt.pivot=new Vector2(0,1); rt.anchoredPosition=new Vector2(x,-y); rt.sizeDelta=new Vector2(w,h); return rt;
        }
        Image Box(Transform p,string name,float x,float y,float w,float h,Color c)
        {var rt=Rect(p,name,x,y,w,h);var img=rt.gameObject.AddComponent<Image>();img.color=c;img.raycastTarget=false;return img;}
        Text Label(Transform p,string text,float x,float y,float w,float h,int size,Color color)
        {
            var rt=Rect(p,"Text",x,y,w,h);var t=rt.gameObject.AddComponent<Text>();t.font=font;t.text=text;t.fontSize=size;t.color=color;
            t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
        Button Button(Transform p,string name,string text,float x,float y,float w,float h,UnityEngine.Events.UnityAction action, bool primary=false)
        {
            var img=Box(p,name,x,y,w,h,primary?new Color(.13f,.45f,.39f):new Color(.12f,.22f,.27f)); img.raycastTarget=true;
            var b=img.gameObject.AddComponent<Button>(); b.targetGraphic=img;
            var colors=b.colors; colors.normalColor=Color.white;colors.highlightedColor=new Color(1.2f,1.2f,1.2f);colors.pressedColor=new Color(.65f,.85f,.8f);colors.disabledColor=new Color(.40f,.45f,.48f);b.colors=colors;
            var navigation=b.navigation;navigation.mode=Navigation.Mode.None;b.navigation=navigation;
            Label(img.transform,text,16,0,w-32,h,21,white); b.onClick.AddListener(action);return b;
        }
        void BuildUI()
        {
            var canvasGO=new GameObject("Interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var c=canvasGO.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGO.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
            canvasRect=canvasGO.GetComponent<RectTransform>();
            new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            var root=canvasRect;
            Box(root,"Top",0,0,1600,108,ink);
            Label(root,"MINING FORGE",38,9,650,55,30,white);
            titleText=Label(root,"",39,65,900,28,17,muted);
            Label(root,"FIELD STUDY  /  001",1210,31,350,36,20,teal);
            Box(root,"Divider",38,107,1524,2,new Color(.18f,.34f,.36f));
            Label(root,"芯を守って、採り出す。",42,138,800,60,38,white);
            Label(root,"結晶  /  3つの根元を加工して分離",44,204,750,36,21,muted);
            toast=Label(Box(root,"Toast",38,672,805,108,panel).transform,"",22,10,762,88,22,white);
            Label(root,"1–3  部位選択     Q / W / E / R  技     Esc  閉じる",42,795,810,32,18,muted);
            Label(root,"試遊用：終了すると結果は消えます。時間制限はありません。",42,833,810,28,17,muted);
            miningPanel=Box(root,"MiningPanel",875,135,687,644,panel).gameObject;
            var mp=miningPanel.transform;
            focusText=Label(mp,"",24,14,320,42,26,white);
            qualityText=Label(mp,"",320,16,340,40,21,teal);
            Label(mp,"01  部位を選ぶ    緑＝適正帯  /  赤線＝致命傷",24,65,640,30,19,muted);
            for(int i=0;i<3;i++)
            {
                int part=i;
                parts[i]=Button(mp,"Part"+i,"",24,105+i*104,639,96,()=>SelectPart(part));
                selection[i]=parts[i].GetComponent<Image>();
                valuesText[i]=Label(parts[i].transform,"",14,3,600,30,21,white);
                var track=Box(parts[i].transform,"Track",14,42,536,9,new Color(.025f,.055f,.08f));
                gaugeFill[i]=Box(track.transform,"Progress",0,0,0,9,teal);
                Box(track.transform,"TargetBand",(config.centers[i]-config.halfWidth)*5.36f,-4,config.halfWidth*2*5.36f,17,new Color(.30f,.92f,.65f,.6f));
                centerMarks[i]=Box(track.transform,"Center",config.centers[i]*5.36f,-6,2,21,white).rectTransform;
                Box(track.transform,"Separation",(config.centers[i]-config.halfWidth-config.separationMargin)*5.36f,-3,2,15,muted);
                Box(track.transform,"Fatal",(config.centers[i]+config.halfWidth+config.fatalMargin)*5.36f,-6,3,21,red);
                Label(parts[i].transform,(config.centers[i]-config.halfWidth)+"–"+(config.centers[i]+config.halfWidth),556,31,78,32,17,teal);
                partStatus[i]=Label(parts[i].transform,"",14,59,600,28,17,muted);
            }
            Label(mp,"02  技を選ぶ    効果量は範囲内で変化",24,419,639,30,19,muted);
            string[] titles={"Q  通常打撃","W  強打","E  そっと削る","R  均等打ち"};
            for(int i=0;i<4;i++)
            {
                int sk=i;float x=24+(i%2)*326,y=459+(i/2)*78;
                skills[i]=Button(mp,"Skill"+i,"",x,y,313,68,()=>Strike(sk));
                Label(skills[i].transform,titles[i],13,0,285,38,22,white);
                Label(skills[i].transform,(i==3?"全3部位 ":"選択部位 ")+config.minimum[i]+"–"+config.maximum[i]+"    集中 −"+config.costs[i],13,36,285,28,17,muted);
            }
            Label(mp,"通常・強打：中心未満から10%で会心。中心で止まります。",24,610,641,28,16,muted);
            // Actions remain reachable below the panel at both supported resolutions.
            var actions=Rect(root,"Actions",875,786,687,105);
            statusText=Label(actions,"",0,0,687,30,18,white);
            collectButton=Button(actions,"Collect","原石を回収",0,35,251,55,Collect,true);
            closeButton=Button(actions,"Close","閉じる",263,35,146,55,ToggleOpen);
            abandonButton=Button(actions,"Abandon","放棄する",421,35,266,55,Abandon);
            // Hide the action row with the mining panel, without changing its reference layout.
            actions.gameObject.AddComponent<MirrorActive>().source=miningPanel;
            closedPanel=Box(root,"Closed",875,135,687,644,panel).gameObject;
            Label(closedPanel.transform,"採取を一時中断",28,50,620,65,34,white);
            Label(closedPanel.transform,"結晶の加工・集中力・抽選の状態は\nそのまま残っています。",28,150,620,100,24,muted);
            Button(closedPanel.transform,"Reopen","同じ結晶の採取に戻る",28,310,620,70,ToggleOpen,true);
            resultPanel=Box(root,"Result",875,135,687,644,panel).gameObject;
            resultText=Label(resultPanel.transform,"",30,27,627,443,26,white);
            Button(resultPanel.transform,"Retry","同じ条件でもう一度",30,493,627,60,()=>NewSample(seed),true);
            Button(resultPanel.transform,"NewSeed","別の乱数で試す",30,566,627,54,()=>NewSample(unchecked(seed+1)));
            var settings=Box(root,"Settings",38,595,805,60,panel).transform;
            Label(settings,"音量",14,10,65,38,18,muted);
            Button(settings,"VolumeDown","−",82,10,50,38,()=>{volume=Mathf.Max(0,volume-.1f);});
            var vl=Label(settings,"",141,10,80,38,18,white);vl.gameObject.AddComponent<LiveSetting>().read=()=>Mathf.RoundToInt(volume*100)+"%";
            Button(settings,"VolumeUp","+",208,10,50,38,()=>{volume=Mathf.Min(1,volume+.1f);});
            var sb=Button(settings,"Shake","",294,10,300,38,()=>{shake=shake==0?.35f:0;});
            sb.GetComponentInChildren<Text>().gameObject.AddComponent<LiveSetting>().read=()=>"画面揺れ："+(shake==0?"なし":"弱");
            Button(settings,"Quit","終了",637,10,151,38,()=>Application.Quit());
        }
    }
    public class LiveSetting : MonoBehaviour
    {
        public System.Func<string> read;
        void Update(){if(read!=null)GetComponent<Text>().text=read();}
    }
    public class MirrorActive : MonoBehaviour
    {
        public GameObject source;
        CanvasGroup group;
        void Awake(){group=gameObject.AddComponent<CanvasGroup>();}
        void Update(){bool visible=source&&source.activeSelf;group.alpha=visible?1:0;group.interactable=visible;group.blocksRaycasts=visible;}
    }
}
