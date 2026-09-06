using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MiningForge.Reference;

namespace MiningForge
{
    public partial class ForgeGame : MonoBehaviour
    {
        enum View { Setup, Commands, Skills, Target, ConfirmAction, Details, Finish, Result, Inventory, Settings }
        const float CellX=822, CellY=237, CellW=132, CellH=120;
        readonly Color ink=new Color(.023f,.034f,.05f,.94f), paper=new Color(.96f,.93f,.84f), gold=new Color(.94f,.72f,.35f), dim=new Color(.53f,.58f,.60f), cyan=new Color(.3f,.8f,.95f);
        Font font; Transform root, menuRoot, boardRoot, overlay;
        Text title, activityLabel, focusLabel, effectLabel, feedback, hint;
        Image focusFill;
        readonly List<Button> buttons=new List<Button>();
        readonly List<Button> options=new List<Button>();
        readonly Sprite[] metalSprites=new Sprite[8];
        ForgeIngotGraphic ingot;
        bool optionFocus;
        int optionIndex;
        float[] shownValues;
        View settingsReturn=View.Setup;
        int inventoryPage;
        readonly List<Image> faces=new List<Image>(), bars=new List<Image>();
        readonly List<Text> labels=new List<Text>();
        readonly List<ForgeFrame> frames=new List<ForgeFrame>();
        ForgeWorkshop workshop=new ForgeWorkshop();
        ForgeSession Session=>workshop.Active;
        ForgeCalibration calibration;
        View view; Technique technique;
        int level=60, seed=1701, recipeIndex=2, page, menuIndex, row, column;
        bool busy, debugNumbers, smoke;
        float volume=.55f, shake=.25f;
        AudioSource audioSource; AudioClip strike, chime, heatSound, errorSound;
        ForgedItem resultItem;
        void Start()
        {
            Application.targetFrameRate=60;
            font=Resources.Load<Font>("Fonts/NotoSansCJKjp-Regular");
            calibration=JsonUtility.FromJson<ForgeCalibration>(Resources.Load<TextAsset>("ForgeCalibration").text);
            var args=Environment.GetCommandLineArgs(); smoke=Array.IndexOf(args,"--forge-smoke")>=0;debugNumbers=Array.IndexOf(args,"--forge-debug")>=0;
            var cam=new GameObject("Forge camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=ink;cam.cullingMask=0;
            MakeAudio();MakeInterface();Show(View.Setup);
            if(smoke)StartCoroutine(Smoke());
        }
        RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform));var rt=go.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;
        }
        Image Box(Transform parent,string name,float x,float y,float w,float h,Color color)
        {var rt=Rect(parent,name,x,y,w,h);var im=rt.gameObject.AddComponent<Image>();im.color=color;im.raycastTarget=false;return im;}
        Text Label(Transform parent,string text,float x,float y,float w,float h,int size,Color color)
        {var rt=Rect(parent,"Label",x,y,w,h);var t=rt.gameObject.AddComponent<Text>();t.font=font;t.text=text;t.fontSize=size;t.color=color;t.raycastTarget=false;t.alignment=TextAnchor.MiddleLeft;t.horizontalOverflow=HorizontalWrapMode.Wrap;return t;}
        ForgeFrame Frame(Transform parent,float x,float y,float w,float h,Color color,float thickness=2)
        {var rt=Rect(parent,"Frame",x,y,w,h);var f=rt.gameObject.AddComponent<ForgeFrame>();f.color=color;f.Thickness=thickness;f.raycastTarget=false;return f;}
        Transform Panel(Transform parent,string name,float x,float y,float w,float h)
        {Box(parent,name+" shadow",x+5,y+7,w,h,new Color(0,0,0,.4f));var im=Box(parent,name,x,y,w,h,ink);Frame(im.transform,0,0,w,h,new Color(.58f,.49f,.32f,.7f));Frame(im.transform,5,5,w-10,h-10,new Color(.58f,.49f,.32f,.2f),1);return im.transform;}
        void Clear(Transform parent){foreach(Transform child in parent){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
        Button Button(Transform parent,string text,float x,float y,float w,float h,Action action,bool enabled=true)
        {
            var image=Box(parent,"Button "+text,x,y,w,h,new Color(.12f,.14f,.16f,.75f));image.raycastTarget=true;
            var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;b.interactable=enabled;
            var nav=b.navigation;nav.mode=Navigation.Mode.None;b.navigation=nav;
            var colors=b.colors;colors.highlightedColor=new Color(1.5f,1.4f,1.1f);colors.disabledColor=new Color(.45f,.45f,.45f,.6f);b.colors=colors;
            Label(image.transform,text,18,0,w-30,h,23,paper);
            b.onClick.AddListener(()=>{if(!busy)action();});return b;
        }
        void Add(string text,Action action,bool enabled=true)
        {buttons.Add(Button(menuRoot,text,0,buttons.Count*61,282,53,action,enabled));}
        void MakeInterface()
        {
            var go=new GameObject("Forge Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;root=go.transform;
            new GameObject("Forge input",typeof(EventSystem),typeof(StandaloneInputModule));
            var texture=Resources.Load<Texture2D>("ForgeArt/workbench");var background=Box(root,"Workbench",0,0,1600,900,Color.white);background.sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f);
            Box(root,"Reading shade",0,0,1600,900,new Color(.01f,.02f,.04f,.22f));
            Label(root,"鍛冶の試作",40,20,320,57,30,paper);title=Label(root,"",412,24,740,51,27,gold);
            Label(root,"再現作業中 · 数値の照合あり",1180,33,390,36,17,dim);
            var menuPanel=Panel(root,"Commands",38,123,324,540);Label(menuPanel,"どうする？",22,13,275,45,23,gold);menuRoot=Rect(menuPanel,"Command entries",21,69,282,465);
            var focusPanel=Panel(root,"Concentration",38,680,324,134);focusLabel=Label(focusPanel,"集中力",22,9,280,50,26,paper);Box(focusPanel,"Track",24,76,274,8,new Color(.01f,.02f,.03f));focusFill=Box(focusPanel,"Fill",24,76,274,8,cyan);
            Label(focusPanel,"技を使うと消費します",24,94,280,26,16,dim);
            activityLabel=Label(root,"",715,115,555,69,37,gold);effectLabel=Label(root,"",434,177,1090,43,23,paper);
            boardRoot=Rect(root,"Board",0,0,1600,900);
            var lower=Panel(root,"Action message",412,771,1147,80);feedback=Label(lower,"",20,8,1104,65,22,paper);
            hint=Label(root,"",40,857,1490,31,17,dim);
            overlay=Rect(root,"Modal overlay",405,224,1160,530);
        }
        void Show(View next)
        {view=next;menuIndex=0;Rebuild();Refresh();}
        void Rebuild()
        {
            Clear(menuRoot);Clear(overlay);buttons.Clear();options.Clear();
            bool active=Session!=null&&!Session.Ended;
            switch(view)
            {
                case View.Setup:
                    Add("この地金を鍛える",()=>Begin(recipeIndex));
                    Add("鍛冶レベル  "+level,()=>{int[] choices={12,26,38,55,60,99};int i=Array.IndexOf(choices,level);level=choices[(i+1)%choices.Length];Show(View.Setup);});
                    Add("うちなおし",()=>Show(View.Inventory),workshop.Items.Count>0);Add("設定",()=>{settingsReturn=View.Setup;Show(View.Settings);});Add("終了",()=>Application.Quit());
                    var setup=Panel(overlay,"Preparation",0,0,1147,526);
                    Label(setup,"地金を選ぶ",27,12,1060,50,28,gold);
                    for(int i=0;i<ForgeRecipe.Samples.Length;i++){int index=i;var r=ForgeRecipe.Samples[i];options.Add(Button(setup,(i==recipeIndex?"◆ ":"   ")+r.Name+"  ·  "+r.Cells.Length+"部位   "+EffectName(r.Effect),28,82+i*57,1088,49,()=>{recipeIndex=index;optionFocus=false;Show(View.Setup);}));}
                    Label(setup,"試験用のレシピ・素材です。原作の全装備データではありません。",28,440,1090,31,18,dim);
                    Label(setup,"素材 "+workshop.Materials+"   ／   宝珠 "+workshop.Pearls+"   ·   選択中の必要素材 "+ForgeRecipe.Samples[recipeIndex].MaterialCost,28,477,1090,30,19,paper);
                    break;
                case View.Commands:
                    Add("たたく",()=>Choose(Technique.Tap));Add("とくぎ",()=>{page=0;Show(View.Skills);});Add("くわしくみる",()=>Show(View.Details));Add("しあげる",()=>Show(View.Finish));
                    if(Session.Charged)Add("ひっさつ",()=>Choose(Technique.Ultimate));
                    Add("設定",()=>{settingsReturn=View.Commands;Show(View.Settings);});break;
                case View.Skills:
                    var unlocked=new List<Technique>();for(int i=1;i<ForgeTechniques.All.Length;i++)if(i!=(int)Technique.Ultimate&&ForgeTechniques.All[i].Level<=level)unlocked.Add((Technique)i);
                    int pages=Math.Max(1,(unlocked.Count+4)/5);page=Math.Max(0,Math.Min(page,pages-1));
                    for(int i=page*5;i<Math.Min(unlocked.Count,page*5+5);i++){var t=unlocked[i];Add(ForgeTechniques.All[(int)t].Name+"    "+Session.Cost(t),()=>Choose(t),Session.Focus>=Session.Cost(t));}
                    Add("次のページ  "+(page+1)+" / "+pages,()=>{page=(page+1)%pages;Show(View.Skills);});Add("戻る",()=>Show(View.Commands));break;
                case View.Target:case View.ConfirmAction:
                    Add("決定して実行",Confirm,Session.CanUse(technique,row,column)==null);Add("戻る",()=>Show(technique==Technique.Tap?View.Commands:View.Skills));
                    var info=ForgeTechniques.All[(int)technique];Label(menuRoot,info.Name,12,156,264,45,27,gold);Label(menuRoot,info.Description,12,210,258,125,22,paper);Label(menuRoot,"集中力  "+Session.Cost(technique),12,348,264,43,23,paper);break;
                case View.Details:case View.Finish:
                    if(view==View.Finish)Add("仕上げる",Finish);
                    Add("まだ続ける",()=>Show(View.Commands));Details();break;
                case View.Result:
                    Add("次の地金へ",()=>{seed++;Show(View.Setup);});Add("この品をうちなおす",()=>Begin(recipeIndex,resultItem),resultItem!=null&&resultItem.Plus<3&&workshop.Pearls>=resultItem.Recipe.PearlCost);Add("所持品",()=>Show(View.Inventory));Add("終了",()=>Application.Quit());
                    var result=Panel(overlay,"Result",0,0,1147,526);var a=Session.Assess();
                    Label(result,"仕上がり",32,22,1050,54,31,gold);Label(result,a.Label,32,105,1050,80,47,paper);
                    Label(result,resultItem.Recipe.Name+(resultItem.Plus>0?" ＋"+resultItem.Plus:""),32,220,1050,65,32,gold);
                    Label(result,"手数 "+Session.Actions+"   ／   残り集中力 "+Session.Focus+"   ／   宝珠 "+workshop.Pearls,32,322,1050,52,23,paper);
                    Label(result,"この試作はアプリ終了で所持品がリセットされます。\n品質係数と一部確率は原作照合中です。",32,410,1050,83,20,dim);break;
                case View.Inventory:
                    Add("戻る",()=>Show(Session!=null&&Session.Ended?View.Result:View.Setup));
                    int inventoryPages=Math.Max(1,(workshop.Items.Count+5)/6);inventoryPage=Mathf.Clamp(inventoryPage,0,inventoryPages-1);
                    Add("次のページ  "+(inventoryPage+1)+" / "+inventoryPages,()=>{inventoryPage=(inventoryPage+1)%inventoryPages;Show(View.Inventory);});
                    var inventory=Panel(overlay,"Inventory",0,0,1147,526);Label(inventory,"うちなおす品を選ぶ  ·  宝珠 "+workshop.Pearls,28,20,1070,48,28,gold);
                    int start=inventoryPage*6;for(int i=start;i<Math.Min(workshop.Items.Count,start+6);i++){var item=workshop.Items[i];options.Add(Button(inventory,item.Recipe.Name+" ＋"+item.Plus+"   宝珠 "+item.Recipe.PearlCost,28,91+(i-start)*63,1088,54,()=>{optionFocus=false;Begin(Array.IndexOf(ForgeRecipe.Samples,item.Recipe),item);},item.Plus<3&&workshop.Pearls>=item.Recipe.PearlCost));}
                    break;
                case View.Settings:
                    Add("音量 −",()=>{volume=Mathf.Max(0,volume-.1f);audioSource.volume=volume;Show(View.Settings);});Add("音量 ＋",()=>{volume=Mathf.Min(1,volume+.1f);audioSource.volume=volume;Show(View.Settings);});Add("揺れ "+(shake==0?"なし":"弱"),()=>{shake=shake==0?.25f:0;Show(View.Settings);});Add("戻る",()=>Show(settingsReturn));
                    Label(menuRoot,"音量 "+Mathf.RoundToInt(volume*100)+"%",12,289,256,49,25,paper);break;
            }
            if(active&&view!=View.Setup&&view!=View.Inventory)DrawBoard();else{Clear(boardRoot);faces.Clear();bars.Clear();labels.Clear();frames.Clear();}
            if(options.Count==0)optionFocus=false;optionIndex=Mathf.Clamp(optionIndex,0,Mathf.Max(0,options.Count-1));HighlightMenu();
        }
        void Begin(int index,ForgedItem item=null)
        {
            if(!workshop.Begin(ForgeRecipe.Samples[index],level,calibration,seed,item)){feedback.text="素材または宝珠が足りません。";return;}
            recipeIndex=index;row=column=0;technique=Technique.Tap;resultItem=null;
            feedback.text="中心の印を狙って 地金を仕上げよう。";Show(View.Commands);
        }
        void Choose(Technique t){technique=t;Show(ForgeTechniques.Targets(t)?View.Target:View.ConfirmAction);}
        void Confirm()
        {
            if(busy||Session==null)return;
            shownValues=new float[Session.Recipe.Cells.Length];for(int i=0;i<shownValues.Length;i++)shownValues[i]=Session.Value(i);
            var action=Session.Execute(technique,row,column);
            if(!action.Valid){feedback.text=action.Error;audioSource.PlayOneShot(errorSound);return;}
            busy=true;Refresh();StartCoroutine(Animate(action,technique));
        }
        void Finish()
        {
            if(busy)return;resultItem=workshop.Finish();if(resultItem==null)return;
            audioSource.PlayOneShot(chime);feedback.text="地金が仕上がりました。";Show(View.Result);
        }
        void Details()
        {
            var p=Panel(overlay,"Assessment",0,0,1147,526);var a=Session.Assess();
            Label(p,view==View.Finish?"この仕上がりで完成させますか？":"いま仕上げると……",30,18,1080,53,29,gold);
            Label(p,a.Label,30,92,1070,69,40,paper);
            Label(p,"中心ぴったりの部位が多いほど よい仕上がりに近づきます。",30,189,1060,61,23,paper);
            Label(p,"現在の集中力  "+Session.Focus+" / "+Session.MaximumFocus+"\n活性度  "+Session.Activity,30,295,1060,103,25,paper);
            Label(p,debugNumbers?"開発表示：誤差合計 "+a.Error+" ／ 仮の評価係数を含みます":"確認だけでは 集中力も活性度も減りません。",30,449,1080,48,21,dim);
        }
        static string EffectName(MaterialEffect effect)
        {return new[]{"特性なし","戻る地金","威力が変化","集中力が変化","一部位が強化"}[(int)effect];}
        void DrawBoard()
        {
            ((RectTransform)boardRoot).anchoredPosition=Vector2.zero;
            Clear(boardRoot);faces.Clear();bars.Clear();labels.Clear();frames.Clear();
            var texture=Resources.Load<Texture2D>("ForgeArt/hot-metal");
            var ingotRect=Rect(boardRoot,"Shaped metal",CellX,CellY,CellW*2,CellH*4);ingot=ingotRect.gameObject.AddComponent<ForgeIngotGraphic>();ingot.Shape=Session.Recipe.Id;ingot.raycastTarget=false;
            for(int i=0;i<Session.Recipe.Cells.Length;i++)
            {
                int index=i;var cell=Session.Recipe.Cells[i];float x=CellX+cell.Column*CellW,y=CellY+cell.Row*CellH;
                var metal=Box(boardRoot,"Cell surface "+i,x,y,CellW,CellH,Color.clear);
                int spriteIndex=cell.Row*2+cell.Column;
                if(!metalSprites[spriteIndex])metalSprites[spriteIndex]=Sprite.Create(texture,new Rect(cell.Column*texture.width/2f,(3-cell.Row)*texture.height/4f,texture.width/2f,texture.height/4f),Vector2.one*.5f);
                metal.sprite=metalSprites[spriteIndex];
                faces.Add(metal);Frame(boardRoot,x,y,CellW,CellH,new Color(.85f,.83f,.70f,.17f),1);
                var target=Box(boardRoot,"Target "+i,x,y,CellW,CellH,Color.clear);target.raycastTarget=true;var button=target.gameObject.AddComponent<Button>();button.targetGraphic=target;
                var nav=button.navigation;nav.mode=Navigation.Mode.None;button.navigation=nav;
                button.onClick.AddListener(()=>{if(view==View.Target&&!busy){row=cell.Row;column=technique==Technique.Diagonal?0:cell.Column;Refresh();}});
                var hover=target.gameObject.AddComponent<ForgeHover>();hover.Enter=()=>{if(view==View.Target&&!busy){row=cell.Row;column=technique==Technique.Diagonal?0:cell.Column;Refresh();}};
                frames.Add(Frame(boardRoot,x+3,y+3,CellW-6,CellH-6,Color.clear,3));
                Label(boardRoot,(i+1).ToString(),x+9,y+3,25,30,17,new Color(1,1,.9f,.65f));
                float gx=cell.Column==0?449:1142,gy=y+36;
                Box(boardRoot,"Gauge outline",gx-3,gy-3,278,26,new Color(.55f,.52f,.41f));
                Box(boardRoot,"Gauge track",gx,gy,272,20,new Color(.01f,.035f,.05f));
                float max=cell.Target*1.45f;
                var fill=Box(boardRoot,"Progress "+i,gx,gy,0,20,cyan);bars.Add(fill);
                Box(boardRoot,"Success zone",gx+(cell.Target-cell.Band)/max*272,gy,cell.Band*2/max*272,20,new Color(.48f,.95f,.59f,.55f));
                Box(boardRoot,"Center tick",gx+cell.Target/max*272-1,gy-7,2,35,gold);
                Label(boardRoot,"▼",gx+cell.Target/max*272-10,gy-28,22,26,18,gold);
                labels.Add(Label(boardRoot,"",gx,gy+28,275,43,18,dim));
                float connectorX=cell.Column==0?gx+285:CellX+CellW*2+7;
                Box(boardRoot,"Connector",connectorX,gy+10,cell.Column==0?CellX-connectorX-8:gx-connectorX-7,1,new Color(.68f,.63f,.49f,.6f));
            }
            RefreshBoard();
        }
        void RefreshBoard()
        {
            if(Session==null||faces.Count!=Session.Recipe.Cells.Length)return;
            var selected=Session.Footprint(technique,row,column);bool selecting=view==View.Target||view==View.ConfirmAction;
            bool valid=Session.CanUse(technique,row,column)==null;
            for(int i=0;i<faces.Count;i++)
            {
                var c=Session.Recipe.Cells[i];float shown=busy&&shownValues!=null?shownValues[i]:Session.Value(i);int value=Mathf.RoundToInt(shown);bool exact=value==c.Target;bool inBand=Math.Abs(value-c.Target)<=c.Band;
                if(ingot)ingot.color=Color.Lerp(new Color(.28f,.24f,.22f),Color.white,Mathf.Clamp01(Session.Activity/1400f));
                bars[i].rectTransform.sizeDelta=new Vector2(272*Mathf.Clamp01(shown/(c.Target*1.45f)),20);
                bars[i].color=exact?new Color(1,.60f,.13f):cyan;
                frames[i].color=selecting&&selected.Contains(i)?valid?gold:new Color(1,.3f,.25f):i==Session.BoostedCell?cyan:Color.clear;
                labels[i].text=debugNumbers?value+" / "+c.Target:exact?"◆ 中心ぴったり":inBand?"成功ゾーン":value>c.Target+c.Band?"少したたきすぎている":"";
                labels[i].color=exact?gold:dim;
            }
        }
        void Refresh()
        {
            bool active=Session!=null&&!Session.Ended;
            title.text=(active?Session.Recipe:ForgeRecipe.Samples[recipeIndex]).Name+"   ·   鍛冶 Lv "+level;
            activityLabel.text=active?"活性度  "+Session.Activity:view==View.Result?"仕上がり":view==View.Inventory?"所持品":"地金の準備";
            focusLabel.text=active?"集中力  "+Session.Focus+" / "+Session.MaximumFocus:"集中力  "+ForgeSession.FocusForLevel(level);
            focusFill.rectTransform.sizeDelta=new Vector2(active?274f*Session.Focus/Session.MaximumFocus:274,8);
            effectLabel.text=active?(Session.Guaranteed?"◆ 次の行動が必ず会心になる":Session.EventLabel!=""?Session.EventLabel:Session.Charged?"◆ ひっさつチャージ！":""):"";
            hint.text=view==View.Target?"↑↓←→ 位置選択   ／   Enter 実行   ／   Esc 戻る   ·   マウス：部位 → 決定して実行":options.Count>0?"Tab 左の操作／右の一覧   ／   ↑↓ 選択   ／   Enter 決定   ／   Esc 戻る":"↑↓ 選択   ／   Enter 決定   ／   Esc 戻る";
            if(view==View.Target||view==View.ConfirmAction)feedback.text=Session.CanUse(technique,row,column)??ForgeTechniques.All[(int)technique].Description;
            foreach(var b in buttons)b.interactable=b.interactable&&!busy;
            if((view==View.Target||view==View.ConfirmAction)&&buttons.Count>0)buttons[0].interactable=!busy&&Session.CanUse(technique,row,column)==null;
            RefreshBoard();HighlightMenu();
        }
        void HighlightMenu()
        {
            for(int i=0;i<buttons.Count;i++)buttons[i].GetComponent<Image>().color=!optionFocus&&i==menuIndex?new Color(.31f,.26f,.16f,.9f):new Color(.1f,.12f,.14f,.8f);
            for(int i=0;i<options.Count;i++)options[i].GetComponent<Image>().color=optionFocus&&i==optionIndex?new Color(.31f,.26f,.16f,.9f):new Color(.1f,.12f,.14f,.8f);
        }
        void Update()
        {
            if(busy||root==null)return;
            if(Input.GetKeyDown(KeyCode.Tab)&&options.Count>0){optionFocus=!optionFocus;HighlightMenu();}
            if(Input.GetKeyDown(KeyCode.Escape)){if(view==View.Target||view==View.ConfirmAction)Show(View.Skills);else if(view!=View.Commands&&view!=View.Setup&&view!=View.Result)Show(Session!=null&&!Session.Ended?View.Commands:View.Setup);return;}
            int dy=(Input.GetKeyDown(KeyCode.DownArrow)?1:0)-(Input.GetKeyDown(KeyCode.UpArrow)?1:0);
            int dx=(Input.GetKeyDown(KeyCode.RightArrow)?1:0)-(Input.GetKeyDown(KeyCode.LeftArrow)?1:0);
            if(view==View.Target&&(dy!=0||dx!=0)){row=Mathf.Clamp(row+dy,0,3);column=technique==Technique.Diagonal?0:Mathf.Clamp(column+dx,0,1);Refresh();}
            else if(dy!=0&&optionFocus&&options.Count>0){optionIndex=(optionIndex+dy+options.Count)%options.Count;HighlightMenu();}
            else if(dy!=0&&buttons.Count>0){menuIndex=(menuIndex+dy+buttons.Count)%buttons.Count;HighlightMenu();}
            if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter))
            {if(view==View.Target)Confirm();else if(optionFocus&&options.Count>0&&options[optionIndex].interactable)options[optionIndex].onClick.Invoke();else if(!optionFocus&&buttons.Count>0&&buttons[menuIndex].interactable)buttons[menuIndex].onClick.Invoke();}
        }
    }
    public sealed class ForgeHover:MonoBehaviour,IPointerEnterHandler
    {public Action Enter;public void OnPointerEnter(PointerEventData e){Enter?.Invoke();}}
    public sealed class ForgeFrame:MaskableGraphic
    {
        public float Thickness=2;
        protected override void OnPopulateMesh(VertexHelper vh)
        {vh.Clear();float w=rectTransform.rect.width,h=rectTransform.rect.height,t=Thickness;Quad(vh,0,0,w,-t);Quad(vh,0,-h+t,w,-t);Quad(vh,0,0,t,-h);Quad(vh,w-t,0,t,-h);}
        void Quad(VertexHelper vh,float x,float y,float w,float h)
        {int n=vh.currentVertCount;vh.AddVert(new Vector3(x,y),color,Vector2.zero);vh.AddVert(new Vector3(x+w,y),color,Vector2.zero);vh.AddVert(new Vector3(x+w,y+h),color,Vector2.zero);vh.AddVert(new Vector3(x,y+h),color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
    }
}
