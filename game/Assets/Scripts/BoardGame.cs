using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MiningForge.Board;

namespace MiningForge
{
    public enum BoardScreen { Commands, Skills, Target, Adjust, Details, Collect, Paused, Abandon, Reset, Settings, Result }
    public partial class BoardGame : MonoBehaviour
    {
        public const string Version="P0a-02";
        BoardConfig config; BoardSession session; BoardScreen screen;
        Skill skill=Skill.Tap; int row,column,menu,seed=1701; bool fixedActivity,smoke;
        Font font; Transform root,boardRoot,menuRoot,modalRoot;
        RectTransform motionRoot;
        Text focusLabel,activityLabel,modeLabel,description,feedback,previewLabel;
        Image activityFill,focusFill;
        readonly Text[] gaugeLabels=new Text[6],gaugeStatus=new Text[6],forecasts=new Text[6];
        readonly Image[] fills=new Image[6],normalRanges=new Image[6],criticalRanges=new Image[6];
        readonly BoardBorderGraphic[] cellFrames=new BoardBorderGraphic[8];
        readonly OreCellGraphic[] oreCells=new OreCellGraphic[6];
        readonly GameObject[] cracks=new GameObject[6];
        readonly Button[] cellButtons=new Button[8];
        readonly System.Collections.Generic.List<Button> menuButtons=new System.Collections.Generic.List<Button>();
        readonly string[] skillNames={"たたく","強打","そっと削る","横打ち","縦打ち","四点打ち","活性を上げる","活性を鎮める"};
        readonly string[] skillShapes={"1マス","1マス・大きく","1マス・繊細に","右隣と2マス","下隣と2マス","右下へ2×2","活性 +300","活性 −200"};
        readonly Color bg=new Color(.035f,.052f,.075f),ink=new Color(.065f,.087f,.115f,.98f),line=new Color(.28f,.33f,.35f),paper=new Color(.94f,.90f,.78f),muted=new Color(.62f,.69f,.71f),gold=new Color(.95f,.72f,.35f),cyan=new Color(.32f,.87f,.84f),red=new Color(1,.38f,.33f);
        float volume=.65f,shake=.35f;
        AudioSource source; AudioClip tapSound,gentleSound,strongSound,critSound,bandSound,overSound,brokenSound,collectSound,adjustSound;
        float audioPeak; readonly float[] audioData=new float[256];
        void Start()
        {
            config=JsonUtility.FromJson<BoardConfig>(Resources.Load<TextAsset>("BoardConfig").text);
            font=Resources.Load<Font>("Fonts/NotoSansCJKjp-Regular");
            var args=Environment.GetCommandLineArgs();fixedActivity=Array.IndexOf(args,"--fixed-activity")>=0;smoke=Array.IndexOf(args,"--smoke-test")>=0;
            int si=Array.IndexOf(args,"--seed");if(si>=0&&si+1<args.Length)int.TryParse(args[si+1],out seed);
            var camera=new GameObject("2D camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=bg;camera.orthographic=true;camera.cullingMask=0;
            Application.targetFrameRate=60;MakeAudio();MakeUI();NewSession(seed);
            if(smoke)StartCoroutine(Smoke());
        }
        void NewSession(int nextSeed)
        {seed=nextSeed;session=new BoardSession(config,seed,fixedActivity);row=column=menu=0;skill=Skill.Tap;screen=BoardScreen.Commands;feedback.text="まずは「特技」で範囲を選び、重なるマスを見比べよう。";RebuildMenu();Refresh();}
        void Update()
        {
            if(session==null)return;
            if(smoke){AudioListener.GetOutputData(audioData,0);foreach(float s in audioData)audioPeak=Mathf.Max(audioPeak,Mathf.Abs(s));}
            if(session.Busy)return;
            if(Input.GetKeyDown(KeyCode.Escape)){Back();return;}
            int dx=(Input.GetKeyDown(KeyCode.RightArrow)?1:0)-(Input.GetKeyDown(KeyCode.LeftArrow)?1:0);
            int dy=(Input.GetKeyDown(KeyCode.DownArrow)?1:0)-(Input.GetKeyDown(KeyCode.UpArrow)?1:0);
            if(screen==BoardScreen.Target && (dx!=0||dy!=0)){row=Mathf.Clamp(row+dy,0,3);column=Mathf.Clamp(column+dx,0,1);Refresh();}
            else if(dy!=0&&menuButtons.Count>0){menu=(menu+dy+menuButtons.Count)%menuButtons.Count;RefreshMenuSelection();}
            if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if(screen==BoardScreen.Target)Confirm();
                else if(menuButtons.Count>0&&menuButtons[menu].interactable)menuButtons[menu].onClick.Invoke();
            }
        }
        void Open(BoardScreen next){if(session.Busy)return;screen=next;menu=0;RebuildMenu();Refresh();}
        void Choose(Skill next){if(session.Busy)return;skill=next;feedback.text="";Open(next>=Skill.Raise?BoardScreen.Adjust:BoardScreen.Target);}
        void SelectCell(int r,int c){if(screen!=BoardScreen.Target||session.Busy)return;row=r;column=c;feedback.text="";Refresh();}
        void Back()
        {
            if(session.Busy)return;
            switch(screen)
            {
                case BoardScreen.Target:case BoardScreen.Adjust:Open(skill==Skill.Tap?BoardScreen.Commands:BoardScreen.Skills);break;
                case BoardScreen.Commands:Open(BoardScreen.Paused);break;
                case BoardScreen.Result:break;
                default:Open(session.Ended?BoardScreen.Result:BoardScreen.Commands);break;
            }
        }
        void Confirm()
        {
            if(session.Busy)return;
            var hits=session.Execute(skill,row,column);if(hits==null){feedback.text=session.Forecast(skill,row,column).error;return;}
            Refresh();StartCoroutine(Animate(hits,skill));
        }
        void End(bool abandon)
        {
            if(!session.End(abandon))return;
            source.PlayOneShot(session.RewardId!=null?collectSound:overSound);
            feedback.text=session.RewardId!=null?MiningSession.Rank(session.Quality)+"の原石を1個回収しました。":"採取を終了しました。報酬はありません。";
            if(session.RewardId!=null)StartCoroutine(Recovery());Open(BoardScreen.Result);
        }
        void Refresh()
        {
            focusLabel.text="集中力  "+session.Focus+" / "+config.focus;focusFill.rectTransform.sizeDelta=new Vector2(246f*session.Focus/config.focus,7);
            activityLabel.text="活性  "+session.Activity+"    ／    効果倍率 "+BoardSession.Multiplier(session.Activity).ToString("0.00");
            activityFill.rectTransform.sizeDelta=new Vector2(710f*session.Activity/2000,7);
            modeLabel.text=Version+"  ·  SEED "+seed+(fixedActivity?"  ·  比較：活性固定1000":"");
            var p=session.Forecast(skill,row,column);bool targeting=screen==BoardScreen.Target&&!session.Busy;
            for(int r=0;r<4;r++)for(int c=0;c<2;c++)
            {
                int idx=r*2+c;bool anchor=r==row&&c==column;
                int w=skill==Skill.Horizontal||skill==Skill.Square?2:1,h=skill==Skill.Vertical||skill==Skill.Square?2:1;
                bool affected=targeting&&r>=row&&r<row+h&&c>=column&&c<column+w;
                cellFrames[idx].color=affected?(p.Valid?gold:red):new Color(.32f,.40f,.41f,.38f);
                cellButtons[idx].interactable=!session.Busy&&targeting;
                cellFrames[idx].gameObject.GetComponent<Outline>().effectColor=affected?(anchor?paper:gold):Color.clear;
            }
            for(int i=0;i<6;i++)
            {
                int v=session.Value(i),t=config.cells[i].center;
                gaugeLabels[i].text=config.cells[i].id+"    "+v+" / "+t;
                gaugeStatus[i].text=session.Broken(i)?"× 破損・損傷品になります":v>session.Upper(i)?"! 帯越え  "+session.Damage(i)+"で破損":v==t?"◆ 中心ぴったり":session.InBand(i)?"✓ 適正帯":v>=session.Separation(i)?"分離済み":"未分離  ·  "+session.Separation(i)+"以上";
                gaugeStatus[i].color=v>session.Upper(i)?red:session.InBand(i)?cyan:muted;
                fills[i].rectTransform.sizeDelta=new Vector2(292*Mathf.Clamp01(v/160f),10);fills[i].color=v>session.Upper(i)?red:cyan;
                normalRanges[i].gameObject.SetActive(false);criticalRanges[i].gameObject.SetActive(false);forecasts[i].text="";
                oreCells[i].Progress=Mathf.Clamp01(v/(float)t);oreCells[i].Damaged=v>session.Upper(i);oreCells[i].Broken=session.Broken(i);oreCells[i].SetVerticesDirty();
                if(targeting)
                    foreach(var f in p.cells)if(f.cell==i)
                    {
                        SetRange(normalRanges[i],f.minimum,f.maximum);normalRanges[i].color=p.Valid?gold:red;
                        forecasts[i].text="通常 "+f.minimum+"–"+f.maximum+(f.maximum>=session.Damage(i)?"  ×破損注意":f.maximum>session.Upper(i)?"  !帯越え":f.minimum>=session.Separation(i)&&v<session.Separation(i)?"  分離へ":"");
                        if(f.canCritical){SetRange(criticalRanges[i],f.criticalMinimum,f.criticalMaximum);forecasts[i].text+="\n会心10%  "+f.criticalMinimum+"–"+f.criticalMaximum;}
                    }
            }
            if(targeting||screen==BoardScreen.Adjust)
            {
                int lo=BoardSession.Scaled(config.minimum[(int)skill],session.Activity),hi=BoardSession.Scaled(config.maximum[(int)skill],session.Activity);
                description.text=skillNames[(int)skill]+"  ·  "+skillShapes[(int)skill]+"  ·  集中 −"+p.cost+(skill<Skill.Raise?"  ·  1マス +"+lo+"–"+hi:"");
                previewLabel.text=(p.Valid?"決定前の予測  ／  活性 "+session.Activity+" → "+p.nextActivity:p.error)+(targeting?"\n黄＝通常の到達範囲  ·  水色の印＝会心の到達範囲":"\nマスの値は変わりません。");
            }
            else
            {
                description.text="晶脈の標本  ·  6部位    ／    見込み品質 "+session.Quality+"  "+MiningSession.Rank(session.Quality);
                previewLabel.text=session.HasBroken?"破損部位があるため品質は最大39です。続けるか、採取を終了できます。":session.Separated?"全ての部位が分離済み。さらに仕上げるか、今の品質で採取できます。":session.Focus<3||session.Activity==0?"残りの集中力と活性を確認し、調整または「採取する」で終了できます。":"緑の帯を狙い、隣を巻き込む範囲と活性の変化を見比べよう。";
            }
            foreach(var button in menuButtons)button.interactable=!session.Busy;
            RefreshMenuSelection();
        }
        void SetRange(Image image,int lo,int hi)
        {image.gameObject.SetActive(true);image.rectTransform.anchoredPosition=new Vector2(292*Mathf.Clamp01(lo/160f),image.rectTransform.anchoredPosition.y);image.rectTransform.sizeDelta=new Vector2(Mathf.Max(3,292*(Mathf.Clamp01(hi/160f)-Mathf.Clamp01(lo/160f))),4);}
    }
}
