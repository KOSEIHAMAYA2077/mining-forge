using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MiningForge.Board;

namespace MiningForge
{
    public partial class BoardGame
    {
        RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {var go=new GameObject(name,typeof(RectTransform));var rt=go.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;}
        Image Box(Transform parent,string name,float x,float y,float w,float h,Color color)
        {var rt=Rect(parent,name,x,y,w,h);var im=rt.gameObject.AddComponent<Image>();im.color=color;im.raycastTarget=false;return im;}
        Transform Panel(Transform parent,string name,float x,float y,float w,float h)
        {
            Box(parent,name+"Shadow",x+5,y+7,w,h,new Color(0,0,0,.36f));
            var p=Box(parent,name,x,y,w,h,ink);Box(p.transform,"Top",0,0,w,2,line);Box(p.transform,"Bottom",0,h-2,w,2,line);Box(p.transform,"Left",0,0,2,h,line);Box(p.transform,"Right",w-2,0,2,h,line);return p.transform;
        }
        Text Text(Transform parent,string value,float x,float y,float w,float h,int size,Color color)
        {var rt=Rect(parent,"Text",x,y,w,h);var t=rt.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=color;t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;return t;}
        Button Button(Transform parent,string name,string label,float x,float y,float w,float h,UnityEngine.Events.UnityAction action,bool primary=false)
        {
            var im=Box(parent,name,x,y,w,h,primary?new Color(.18f,.36f,.35f):new Color(.12f,.17f,.20f));im.raycastTarget=true;
            var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;
            var nc=b.navigation;nc.mode=Navigation.Mode.None;b.navigation=nc;
            var colors=b.colors;colors.highlightedColor=new Color(1.3f,1.3f,1.2f);colors.pressedColor=new Color(.6f,.85f,.8f);colors.disabledColor=new Color(.4f,.4f,.4f);b.colors=colors;
            Text(im.transform,label,17,0,w-30,h,22,paper);b.onClick.AddListener(action);return b;
        }
        Sprite Tile(int column,int row)
        {var tex=Resources.Load<Texture2D>("BoardArt/roguelikeDungeon_transparent");tex.filterMode=FilterMode.Point;return Sprite.Create(tex,new Rect(column*17,tex.height-row*17-16,16,16),new Vector2(.5f,.5f),16);}
        void MakeUI()
        {
            var canvasGO=new GameObject("Board Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvasGO.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var sc=canvasGO.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1600,900);sc.matchWidthOrHeight=.5f;
            root=canvasGO.transform;new GameObject("Board Input",typeof(EventSystem),typeof(StandaloneInputModule));
            for(int y=0;y<15;y++)for(int x=0;x<25;x++)
            {var tile=Box(root,"Cave floor",x*64,y*64,64,64,new Color(.22f,.26f,.29f));tile.sprite=Tile(8+(x+y)%3,10);}
            for(int i=0;i<18;i++){var stone=Box(root,"Cave detail",i*99-12,i%2==0?92:708,76,120,new Color(.35f,.39f,.37f));stone.sprite=Tile(i%4,0);}
            Box(root,"Vignette",0,0,1600,900,new Color(.015f,.027f,.048f,.38f));
            Box(root,"Header",0,0,1600,84,bg);Text(root,"MINING FORGE",36,10,420,54,29,paper);Text(root,"晶脈の標本  /  採取試験",424,16,540,44,24,gold);modeLabel=Text(root,"",1130,24,434,36,17,muted);
            var activity=Panel(root,"Activity",386,101,1174,78);
            activityLabel=Text(activity,"",25,4,780,45,27,paper);Text(activity,"高いほど削れやすい",853,9,280,39,18,muted);
            Box(activity,"Track",27,58,710,7,new Color(.025f,.04f,.065f));activityFill=Box(activity,"Fill",27,58,355,7,gold);
            Text(activity,"打撃1回で −50",859,40,260,30,17,muted);
            var menuPanel=Panel(root,"Commands",36,101,326,616);Text(menuPanel,"採取の手順",23,13,280,43,23,gold);
            menuRoot=Rect(menuPanel,"Menu",16,65,294,534);
            var focus=Panel(root,"Focus",36,734,326,131);focusLabel=Text(focus,"",23,9,280,45,24,paper);
            Box(focus,"Track",25,64,246,7,bg);focusFill=Box(focus,"Fill",25,64,246,7,cyan);Text(focus,"時間では減りません",25,82,272,32,17,muted);
            var field=Panel(root,"Specimen",386,192,1174,535);motionRoot=Rect(field,"Motion",0,0,1174,535);boardRoot=motionRoot;
            // One connected mineral silhouette following the occupied L-shaped board.
            var silhouette=Rect(boardRoot,"Specimen silhouette",377,3,290,524);var outline=silhouette.gameObject.AddComponent<OreOutlineGraphic>();outline.raycastTarget=false;
            for(int r=0;r<4;r++)for(int c=0;c<2;c++)
            {
                int idx=r*2+c, rr=r,cc=c;float x=382+c*140,y=10+r*128;int cell=FindCell(r,c);
                var holder=Rect(boardRoot,"Cell "+r+","+c,x,y,132,120);
                if(cell>=0)
                {
                    var ore=holder.gameObject.AddComponent<OreCellGraphic>();ore.Index=cell;ore.TopRow=r;ore.RightColumn=c;ore.raycastTarget=false;oreCells[cell]=ore;
                }
                var frame=Box(holder,cell>=0?"Position "+config.cells[cell].id:"Empty position",0,0,132,120,new Color(.3f,.4f,.45f,.15f));
                frame.type=Image.Type.Sliced;frame.raycastTarget=true;
                // Borders are drawn by a lightweight graphic, so the mineral remains visible.
                frame.color=Color.clear;
                var edge=frame.gameObject.AddComponent<Outline>();edge.effectDistance=new Vector2(2,-2);edge.effectColor=Color.clear;
                var b=frame.gameObject.AddComponent<Button>();b.targetGraphic=frame;var nv=b.navigation;nv.mode=Navigation.Mode.None;b.navigation=nv;
                b.onClick.AddListener(()=>SelectCell(rr,cc));cellButtons[idx]=b;
                var borderRect=Rect(holder,"Border",0,0,132,120);var border=borderRect.gameObject.AddComponent<BoardBorderGraphic>();border.raycastTarget=false;
                border.gameObject.AddComponent<Outline>().effectDistance=new Vector2(1,-1);cellFrames[idx]=border;
                // Image has no sprite and is transparent; border graphic provides geometry.
                if(cell>=0){Box(holder,"ID plate",6,5,32,33,new Color(.025f,.06f,.08f,.88f));Text(holder,config.cells[cell].id,14,4,25,34,20,paper);}
                else Text(holder,"·",58,30,35,48,30,new Color(.34f,.42f,.44f));
                var hook=frame.gameObject.AddComponent<BoardHover>();hook.enter=()=>SelectCell(rr,cc);
            }
            for(int i=0;i<6;i++)
            {
                var spec=config.cells[i];float x=spec.column==0?22:686,y=spec.row*128+3;
                var g=Rect(boardRoot,"Gauge "+spec.id,x,y,459,121);
                gaugeLabels[i]=Text(g,"",0,0,400,32,21,paper);
                var track=Box(g,"Gauge track",0,42,292,10,bg);
                fills[i]=Box(track.transform,"Progress",0,0,0,10,cyan);
                Box(track.transform,"Band",(spec.center-config.band)/160f*292,-5,config.band*2/160f*292,20,new Color(.34f,.77f,.5f,.55f));
                Box(track.transform,"Center",spec.center/160f*292,-7,2,24,paper);
                Box(track.transform,"Separation",(spec.center-config.band-config.separationMargin)/160f*292,-3,1,16,muted);
                Box(track.transform,"Break",(spec.center+config.band+config.damageMargin)/160f*292,-5,2,20,red);
                normalRanges[i]=Box(track.transform,"Normal prediction",0,-8,0,4,gold);
                criticalRanges[i]=Box(track.transform,"Critical prediction",0,13,0,4,cyan);
                gaugeStatus[i]=Text(g,"",0,59,335,24,15,muted);
                forecasts[i]=Text(g,"",0,83,355,44,14,gold);
                if(spec.column==0)Box(g,"Connector",302,44,47,1,new Color(.41f,.48f,.48f,.6f));
            }
            Text(boardRoot,"◆",855,308,180,58,40,gold);Text(boardRoot,"芯を守り、形を残す",718,372,380,46,24,paper);
            Text(boardRoot,"緑：適正帯    白：中心\n赤線：破損    灰線：分離",718,425,395,73,20,muted);
            var info=Panel(root,"Action information",386,744,1174,121);description=Text(info,"",21,3,1130,41,22,paper);previewLabel=Text(info,"",21,43,1127,65,18,muted);
            feedback=Text(root,"",388,866,1168,30,17,gold);
            Text(root,"↑↓←→ 選択  /  Enter 決定  /  Esc 戻る",37,866,345,29,13,muted);
            modalRoot=Rect(root,"Overlay",390,197,1163,525);
        }
        int FindCell(int r,int c){for(int i=0;i<config.cells.Length;i++)if(config.cells[i].row==r&&config.cells[i].column==c)return i;return -1;}
        void AddMenu(string label,UnityEngine.Events.UnityAction action,float y,bool primary=false)
        {var b=Button(menuRoot,"Menu "+label,label,0,y,294,52,action,primary);menuButtons.Add(b);}
        void RebuildMenu()
        {
            foreach(Transform child in menuRoot)Destroy(child.gameObject);foreach(Transform child in modalRoot)Destroy(child.gameObject);menuButtons.Clear();
            switch(screen)
            {
                case BoardScreen.Commands:
                    AddMenu("たたく",()=>Choose(Skill.Tap),0);AddMenu("特技",()=>Open(BoardScreen.Skills),65);AddMenu("詳しく見る",()=>Open(BoardScreen.Details),130);AddMenu("採取する",()=>Open(BoardScreen.Collect),195,true);
                    Text(menuRoot,"主コマンド以外",12,267,270,32,16,muted);
                    AddMenu("閉じる",()=>Open(BoardScreen.Paused),307);AddMenu("試遊をやり直す",()=>Open(BoardScreen.Reset),372);AddMenu("設定",()=>Open(BoardScreen.Settings),437);break;
                case BoardScreen.Skills:
                    for(int i=1;i<8;i++)
                    {
                        int sk=i;AddMenu(skillNames[i]+"   −"+config.costs[i],()=>Choose((Skill)sk),(i-1)*59);
                        var label=menuButtons[menuButtons.Count-1].GetComponentInChildren<Text>();label.fontSize=19;label.rectTransform.sizeDelta=new Vector2(264,29);
                        string effect=i<6?skillShapes[i]+"  +"+BoardSession.Scaled(config.minimum[i],session.Activity)+"–"+BoardSession.Scaled(config.maximum[i],session.Activity):skillShapes[i];
                        Text(menuButtons[menuButtons.Count-1].transform,effect,17,28,264,24,14,muted);
                    }
                    AddMenu("戻る",()=>Open(BoardScreen.Commands),437);break;
                case BoardScreen.Target:case BoardScreen.Adjust:
                    Text(menuRoot,skillNames[(int)skill],13,0,276,46,26,gold);
                    Text(menuRoot,skillShapes[(int)skill]+"\n集中力 −"+config.costs[(int)skill],13,61,268,80,21,paper);
                    Text(menuRoot,screen==BoardScreen.Target?"位置を選んで決定。\n範囲に空きマスが\n含まれる技は使えません。":"効果を確認して決定。\nマスの値は変わりません。",13,159,268,135,20,muted);
                    AddMenu("決定して実行",Confirm,323,true);AddMenu("戻る",Back,390);break;
                case BoardScreen.Details:
                    AddMenu("盤面に戻る",()=>Open(BoardScreen.Commands),0,true);Details(false);break;
                case BoardScreen.Collect:
                    AddMenu("採取を終了する",()=>End(false),0,true);AddMenu("まだ続ける",()=>Open(BoardScreen.Commands),65);Details(true);break;
                case BoardScreen.Paused:
                    Text(menuRoot,"中断中\n状態は保持しています",12,0,277,91,21,paper);AddMenu("同じ結晶に戻る",()=>Open(BoardScreen.Commands),126,true);AddMenu("放棄する",()=>Open(BoardScreen.Abandon),191);AddMenu("アプリを終了",()=>Application.Quit(),256);Text(menuRoot,"アプリ終了時は試遊結果が\n消えます。報酬の保存は\nまだありません。",12,325,275,143,18,muted);break;
                case BoardScreen.Abandon:
                    Text(menuRoot,"報酬なしでこの結晶を\n放棄しますか？",12,0,277,105,22,paper);AddMenu("放棄を確定",()=>End(true),140);AddMenu("続ける",()=>Open(BoardScreen.Commands),205);break;
                case BoardScreen.Reset:
                    Text(menuRoot,"今の進行と試遊結果は\n消えます。やり直しますか？",12,0,277,111,20,paper);AddMenu("同じ条件でやり直す",()=>NewSession(seed),142,true);AddMenu("別の乱数でやり直す",()=>NewSession(seed+1),207);AddMenu("戻る",Back,272);break;
                case BoardScreen.Settings:
                    Text(menuRoot,"音量 "+Mathf.RoundToInt(volume*100)+"%",12,0,280,45,24,paper);
                    AddMenu("音量を下げる",()=>{volume=Mathf.Max(0,volume-.1f);source.volume=volume;RebuildMenu();},66);AddMenu("音量を上げる",()=>{volume=Mathf.Min(1,volume+.1f);source.volume=volume;RebuildMenu();},131);
                    AddMenu("画面揺れ："+(shake==0?"なし":"弱"),()=>{shake=shake==0?.35f:0;RebuildMenu();},196);AddMenu("戻る",Back,326);break;
                case BoardScreen.Result:
                    AddMenu("同じ条件でもう一度",()=>NewSession(seed),0,true);AddMenu("別の乱数で試す",()=>NewSession(seed+1),65);AddMenu("設定",()=>Open(BoardScreen.Settings),130);AddMenu("終了",()=>Application.Quit(),195);
                    var panel=Panel(modalRoot,"Result",0,0,1158,520);
                    Text(panel,session.RewardId!=null?"採取成功":"採取終了",37,22,1000,67,37,gold);
                    Text(panel,session.RewardId!=null?MiningSession.Rank(session.Quality)+"の原石 × 1    品質 "+session.Quality:"未回収  ·  報酬なし",39,130,1000,71,32,paper);
                    Text(panel,"手数 "+session.Actions+"    残り集中力 "+session.Focus+"    最終活性 "+session.Activity,39,236,1070,52,24,paper);
                    Text(panel,"試遊用の結果です。工房・永続保存は次の段階。\n次は同じ条件で、違う技や順番を試せます。",39,330,1070,107,24,muted);break;
            }
            menu=Mathf.Clamp(menu,0,Mathf.Max(0,menuButtons.Count-1));RefreshMenuSelection();
        }
        void RefreshMenuSelection()
        {
            for(int i=0;i<menuButtons.Count;i++)if(menuButtons[i])
                menuButtons[i].GetComponent<Image>().color=i==menu?new Color(.22f,.34f,.34f):new Color(.12f,.17f,.20f);
            if((screen==BoardScreen.Target||screen==BoardScreen.Adjust)&&menuButtons.Count>0)menuButtons[0].interactable=!session.Busy&&session.Forecast(skill,row,column).Valid;
            for(int idx=0;idx<8;idx++)if(cellFrames[idx])cellFrames[idx].GetComponent<BoardBorderGraphic>().color=cellFrames[idx].color;
        }
        void Details(bool confirm)
        {
            var p=Panel(modalRoot,"Detailed assessment",0,0,1158,520);
            Text(p,confirm?"この状態で採取しますか？":"現在の仕上がり",27,15,1094,54,30,gold);
            Text(p,(session.Separated?MiningSession.Rank(session.Quality)+"の原石 × 1 を回収できます":"未分離が残っています。終了しても報酬はありません")+(session.HasBroken?"\n破損部位があるため品質は最大39です。":""),28,81,1090,75,23,paper);
            for(int i=0;i<6;i++)
            {var c=config.cells[i];Text(p,c.id+"   "+session.Value(i)+" / "+c.center+"    帯 "+session.Lower(i)+"–"+session.Upper(i)+"    分離 "+session.Separation(i)+"以上    "+(session.Broken(i)?"×破損":session.Value(i)>session.Upper(i)?"!帯越え":session.InBand(i)?"適正帯":session.Value(i)>=session.Separation(i)?"分離済み":"未分離"),30,169+i*45,1093,42,21,session.Broken(i)?red:paper);}
            Text(p,"品質 "+session.Quality+" / 100    ・    確認だけでは集中力・活性は減りません",30,453,1080,42,20,muted);
        }
    }
    public class BoardHover:MonoBehaviour,IPointerEnterHandler{public System.Action enter;public void OnPointerEnter(PointerEventData e){enter?.Invoke();}}
    public class BoardBorderGraphic:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {vh.Clear();var r=rectTransform.rect;float t=2;Quad(vh,0,0,r.width,t);Quad(vh,0,-r.height+t,r.width,t);Quad(vh,0,-r.height,t,r.height);Quad(vh,r.width-t,-r.height,t,r.height);}
        void Quad(VertexHelper vh,float x,float y,float w,float h){int n=vh.currentVertCount;vh.AddVert(new Vector3(x,y),color,Vector2.zero);vh.AddVert(new Vector3(x+w,y),color,Vector2.zero);vh.AddVert(new Vector3(x+w,y+h),color,Vector2.zero);vh.AddVert(new Vector3(x,y+h),color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
    }
}
