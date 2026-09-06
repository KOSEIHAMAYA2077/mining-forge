using System;
using System.Collections.Generic;

namespace MiningForge.Reference
{
    public enum Technique { Tap, Vertical, Gentle, Double, Heat, Quad, RandomFour, Triple, Aim, SuperQuad, Cool, Diagonal, Ultimate, HotWind }
    public enum MaterialEffect { None, Revert, Power, Focus, Single }
    public sealed class TechniqueInfo
    {
        public readonly string Name, Description;
        public readonly int Level, Cost;
        public readonly double Power;
        public TechniqueInfo(string name, int level, int cost, double power, string description)
        { Name=name; Level=level; Cost=cost; Power=power; Description=description; }
    }
    public static class ForgeTechniques
    {
        public static readonly TechniqueInfo[] All = {
            new TechniqueInfo("たたく",1,5,1,"ひとつの部位を ふつうの力でたたく"),
            new TechniqueInfo("上下打ち",10,8,1.2,"上下に並ぶ2部位を 少し強くたたく"),
            new TechniqueInfo("てかげん打ち",12,10,.5,"ひとつの部位を 半分の力でたたく"),
            new TechniqueInfo("2倍打ち",17,8,2,"ひとつの部位を 2倍の力でたたく"),
            new TechniqueInfo("火力上げ",23,10,0,"活性度を300上げる"),
            new TechniqueInfo("4連打ち",26,12,1.2,"2×2の4部位を 少し強くたたく"),
            new TechniqueInfo("みだれ打ち",31,7,.8,"場所を選ばず 軽めに4回たたく"),
            new TechniqueInfo("3倍打ち",35,11,3,"ひとつの部位を 3倍の力でたたく"),
            new TechniqueInfo("ねらい打ち",38,16,1,"ひとつの部位を 会心を狙ってたたく"),
            new TechniqueInfo("超4連打ち",44,18,2,"2×2の4部位を 2倍の力でたたく"),
            new TechniqueInfo("冷やし込み",46,12,0,"活性度を300下げる"),
            new TechniqueInfo("ななめ打ち",50,7,1.2,"右上と左下の2部位を 少し強くたたく"),
            new TechniqueInfo("ヘパイトスの炎",55,0,0,"次の行動が 必ず会心になる"),
            new TechniqueInfo("熱風おろし",60,6,2.5,"ひとつの部位を2.5倍でたたき 活性度を150下げる")
        };
        public static bool Strikes(Technique t) => t!=Technique.Heat && t!=Technique.Cool && t!=Technique.Ultimate;
        public static bool Targets(Technique t) => Strikes(t) && t!=Technique.RandomFour;
    }
    [Serializable] public sealed class ForgeCalibration
    {
        // These parameters are isolated because the public reference does not establish exact PS4 values.
        public int baseMinimum=12, baseMaximum=18, revertMinimum=12, revertMaximum=16;
        public double criticalAtLevel1=.01, criticalPerLevel=.001, difficultyPenalty=.003;
        public double chargeChance=.06, eventCriticalMultiplier=2;
        public bool requireFullFootprint=true, eventsAtStart=false, hotWindUsesCooledValue=false;
        public int exactBonus=5, plus3Error=0, plus2Error=12, plus1Error=28, normalError=50;
        public string status="仮係数あり・原作との数値照合は未完了";
    }
    public sealed class ForgeCell
    {
        public readonly int Row, Column, Target, Band;
        public ForgeCell(int row,int column,int target,int band=5) { Row=row; Column=column; Target=target; Band=band; }
    }
    public sealed class ForgeRecipe
    {
        public readonly string Id, Name;
        public readonly ForgeCell[] Cells;
        public readonly MaterialEffect Effect;
        public readonly int Difficulty, MaterialCost, PearlCost, PearlReward;
        public ForgeRecipe(string id,string name,MaterialEffect effect,int difficulty,params ForgeCell[] cells)
        { Id=id; Name=name; Effect=effect; Difficulty=difficulty; Cells=cells; MaterialCost=2+difficulty; PearlCost=2+difficulty*2; PearlReward=2+difficulty; }
        public static readonly ForgeRecipe[] Samples = {
            new ForgeRecipe("ring","環の地金",MaterialEffect.None,1,new ForgeCell(1,0,45),new ForgeCell(1,1,45)),
            new ForgeRecipe("blade","短剣の地金",MaterialEffect.None,1,new ForgeCell(0,0,37),new ForgeCell(1,0,54),new ForgeCell(2,0,37)),
            new ForgeRecipe("shield","盾の地金",MaterialEffect.Power,2,new ForgeCell(0,0,100),new ForgeCell(0,1,100),new ForgeCell(1,0,120),new ForgeCell(1,1,120)),
            new ForgeRecipe("axe","斧の地金",MaterialEffect.Focus,3,new ForgeCell(0,0,160),new ForgeCell(0,1,140),new ForgeCell(1,0,180),new ForgeCell(1,1,140),new ForgeCell(2,0,100),new ForgeCell(3,0,100)),
            new ForgeRecipe("robe","大板の地金",MaterialEffect.Revert,4,new ForgeCell(0,0,170),new ForgeCell(0,1,170),new ForgeCell(1,0,190),new ForgeCell(1,1,190),new ForgeCell(2,0,160),new ForgeCell(2,1,160),new ForgeCell(3,0,140),new ForgeCell(3,1,140)),
            new ForgeRecipe("cross","飾りの地金",MaterialEffect.Single,3,new ForgeCell(0,0,140),new ForgeCell(0,1,170),new ForgeCell(1,0,120),new ForgeCell(2,0,120))
        };
    }
    public interface IForgeRandom { int Next(int minimum,int exclusiveMaximum); double Unit(); }
    public sealed class ForgeRandom : IForgeRandom
    {
        readonly Random random;
        public ForgeRandom(int seed) { random=new Random(seed); }
        public int Next(int a,int b)=>random.Next(a,b);
        public double Unit()=>random.NextDouble();
    }
    public sealed class ForgeHit { public int Cell, Before, After; public bool Critical, Miss; }
    public sealed class ForgeAction
    {
        public string Error, EventMessage;
        public int Cost, BeforeActivity, AfterActivity;
        public bool Charged;
        public readonly List<ForgeHit> Hits=new List<ForgeHit>();
        public bool Valid=>Error==null;
    }
    public sealed class ForgeAssessment
    {
        public int Error, Plus, Tier;
        public string Label;
    }
    public sealed class ForgeSession
    {
        public readonly ForgeRecipe Recipe;
        public readonly ForgeCalibration Calibration;
        public readonly int Level, MaximumFocus;
        readonly IForgeRandom random;
        readonly int[] values;
        public int Focus { get; private set; }
        public int Activity { get; private set; }=1000;
        public int Actions { get; private set; }
        public bool Ended { get; private set; }
        public bool Charged { get; private set; }
        public bool UltimateUsed { get; private set; }
        public bool Guaranteed { get; private set; }
        public double PowerEffect { get; private set; }=1;
        public double FocusEffect { get; private set; }=1;
        public double CriticalEffect { get; private set; }=1;
        public int BoostedCell { get; private set; }=-1;
        public string EventLabel { get; private set; }="";
        public ForgeSession(ForgeRecipe recipe,int level,ForgeCalibration calibration,int seed=1701,IForgeRandom randomSource=null)
        {
            if(recipe==null||recipe.Cells.Length<2||recipe.Cells.Length>8)throw new ArgumentException("Invalid recipe");
            Recipe=recipe; Level=Math.Max(1,Math.Min(99,level)); Calibration=calibration??throw new ArgumentNullException(nameof(calibration));
            random=randomSource??new ForgeRandom(seed); values=new int[recipe.Cells.Length];
            Focus=MaximumFocus=FocusForLevel(Level);
            if(calibration.eventsAtStart)ResolveEvent();
        }
        public static int FocusForLevel(int level)
        {
            // Published DQ11 table at levels 5–60; 1–4 extrapolated, used only by test setup.
            int[] table={0,53,54,55,56,57,58,59,60,61,62,63,64,65,66,68,69,71,72,73,74,75,76,80,81,82,85,86,87,89,90,91,92,93,94,95,96,97,98,99,100,101,106,107,108,112,113,114,115,116,116,117,118,119,120,122,124,126,128,130,132};
            level=Math.Max(1,Math.Min(99,level)); return level<=60?table[level]:132+(level-60)*2;
        }
        public int Value(int i)=>values[i];
        public int At(int r,int c) { for(int i=0;i<values.Length;i++)if(Recipe.Cells[i].Row==r&&Recipe.Cells[i].Column==c)return i; return -1; }
        public static double ActivityMultiplier(int value)=>.5+Math.Min(2000,Math.Max(0,value))/2000.0;
        public int Cost(Technique t)=> (int)Math.Ceiling(ForgeTechniques.All[(int)t].Cost*FocusEffect);
        public double CriticalChance(Technique t,int cell)
        {
            double rate=Math.Max(0,Calibration.criticalAtLevel1+(Level-1)*Calibration.criticalPerLevel-Recipe.Difficulty*Calibration.difficultyPenalty);
            return Math.Min(1,rate*(t==Technique.Aim?7:1)*CriticalEffect*(cell==BoostedCell?Calibration.eventCriticalMultiplier:1));
        }
        public List<int> Footprint(Technique t,int row,int column)
        {
            var result=new List<int>();
            if(!ForgeTechniques.Strikes(t))return result;
            if(t==Technique.RandomFour){for(int i=0;i<values.Length;i++)result.Add(i);return result;}
            var offsets=new List<int[]>{new[]{0,0}};
            if(t==Technique.Vertical)offsets.Add(new[]{1,0});
            if(t==Technique.Quad||t==Technique.SuperQuad){offsets.Add(new[]{0,1});offsets.Add(new[]{1,0});offsets.Add(new[]{1,1});}
            if(t==Technique.Diagonal){offsets.Clear();offsets.Add(new[]{0,1});offsets.Add(new[]{1,0});}
            foreach(var offset in offsets) result.Add(At(row+offset[0],column+offset[1]));
            return result;
        }
        public string CanUse(Technique t,int row=0,int column=0)
        {
            int index=(int)t;
            if(index<0||index>=ForgeTechniques.All.Length)return "技が選ばれていません";
            if(Ended)return "すでに仕上げています";
            if(Level<ForgeTechniques.All[index].Level)return "まだ覚えていない技です";
            if(Focus<Cost(t))return "集中力が足りません";
            if(t==Technique.Ultimate && (!Charged||UltimateUsed))return "ひっさつがチャージされていません";
            if(ForgeTechniques.Strikes(t)&&Activity==0)return "活性度が0です";
            if(t==Technique.Cool&&Activity<300)return "活性度が300必要です";
            if(t==Technique.HotWind&&Activity<150)return "活性度が150必要です";
            var cells=Footprint(t,row,column);
            if(ForgeTechniques.Targets(t))
            {
                if(Calibration.requireFullFootprint && cells.Contains(-1))return "範囲に地金のない場所が含まれています";
                if(cells.TrueForAll(i=>i<0))return "地金のある場所を選んでください";
            }
            return null;
        }
        public ForgeAction Execute(Technique t,int row=0,int column=0)
        {
            var result=new ForgeAction{Error=CanUse(t,row,column),BeforeActivity=Activity,AfterActivity=Activity};
            if(!result.Valid)return result;
            result.Cost=Cost(t); Focus-=result.Cost; Actions++;
            bool guaranteed=Guaranteed; Guaranteed=false;
            int hitActivity=t==Technique.HotWind&&Calibration.hotWindUsesCooledValue?Activity-150:Activity;
            var targets=Footprint(t,row,column);
            if(t==Technique.RandomFour)
            {
                targets.Clear();var counts=new int[values.Length];
                for(int n=0;n<4;n++){var eligible=new List<int>();for(int i=0;i<values.Length;i++)if(counts[i]<3)eligible.Add(i);int chosen=eligible[random.Next(0,eligible.Count)];counts[chosen]++;targets.Add(chosen);}
            }
            foreach(int cell in targets)
            {
                if(cell<0)continue;
                int before=values[cell],target=Recipe.Cells[cell].Target;
                bool critical=guaranteed||random.Unit()<CriticalChance(t,cell);
                // U01: rounding and discrete base values are calibration pending, not claimed exact.
                int amount=(int)Math.Ceiling(random.Next(Calibration.baseMinimum,Calibration.baseMaximum+1)*ForgeTechniques.All[(int)t].Power*ActivityMultiplier(hitActivity)*PowerEffect*(cell==BoostedCell?2:1));
                bool miss=critical&&before>=target;
                int after=miss?before:critical?Math.Min(target,before+amount*2):before+amount;
                values[cell]=after;
                result.Hits.Add(new ForgeHit{Cell=cell,Before=before,After=after,Critical=critical,Miss=miss});
            }
            if(t==Technique.Heat)Activity+=300;
            else if(t==Technique.Cool)Activity-=300;
            else if(t==Technique.HotWind)Activity-=150;
            else Activity=Math.Max(0,Activity-50);
            if(t==Technique.Ultimate){UltimateUsed=true;Charged=false;Guaranteed=true;}
            PowerEffect=FocusEffect=CriticalEffect=1;BoostedCell=-1;EventLabel="";
            result.EventMessage=ResolveEvent();
            if(Level>=55&&!UltimateUsed&&!Charged && random.Unit()<Calibration.chargeChance){Charged=true;result.Charged=true;}
            result.AfterActivity=Activity;return result;
        }
        string ResolveEvent()
        {
            if(Activity<=0||Activity%200!=0)return "";
            bool alternate=Activity%400==0;
            switch(Recipe.Effect)
            {
                case MaterialEffect.Revert:
                    var eligible=new List<int>();for(int i=0;i<values.Length;i++)if(values[i]>0&&values[i]!=Recipe.Cells[i].Target)eligible.Add(i);
                    if(eligible.Count>0){int cell=eligible[random.Next(0,eligible.Count)];int before=values[cell];values[cell]=Math.Max(0,before-random.Next(Calibration.revertMinimum,Calibration.revertMaximum+1));EventLabel="部位 "+(cell+1)+" が "+(before-values[cell])+" 戻った";}
                    break;
                case MaterialEffect.Power:PowerEffect=alternate?2:.5;EventLabel=alternate?"次のひと打ち 威力2倍":"次のひと打ち 威力半減";break;
                case MaterialEffect.Focus:FocusEffect=alternate?.5:1.5;CriticalEffect=alternate?1:Calibration.eventCriticalMultiplier;EventLabel=alternate?"次の行動 集中力の消費が半分":"次の行動 消費1.5倍・会心率上昇";break;
                case MaterialEffect.Single:BoostedCell=random.Next(0,values.Length);EventLabel="部位 "+(BoostedCell+1)+" の威力・会心率上昇";break;
            }
            return EventLabel;
        }
        public ForgeAssessment Assess()
        {
            int error=0;
            for(int i=0;i<values.Length;i++)error+=values[i]==Recipe.Cells[i].Target?-Calibration.exactBonus:Math.Abs(values[i]-Recipe.Cells[i].Target);
            int tier=error<=Calibration.plus3Error?4:error<=Calibration.plus2Error?3:error<=Calibration.plus1Error?2:error<=Calibration.normalError?1:0;
            string[] labels={"悪い","ふつう","ちょっといい","なかなかいい","とてもいい"};
            return new ForgeAssessment{Error=error,Tier=tier,Plus=Math.Max(0,tier-1),Label=labels[tier]};
        }
        public bool Finish(){if(Ended)return false;Ended=true;return true;}
    }
    public sealed class ForgedItem
    {
        public readonly string Id=Guid.NewGuid().ToString("N");
        public readonly ForgeRecipe Recipe;
        public int Plus { get; internal set; }
        public ForgedItem(ForgeRecipe recipe,int plus=0){Recipe=recipe;Plus=plus;}
    }
    public sealed class ForgeWorkshop
    {
        public int Materials { get; private set; }=80;
        public int Pearls { get; private set; }=30;
        public readonly List<ForgedItem> Items=new List<ForgedItem>();
        public ForgeSession Active { get; private set; }
        ForgedItem rework;
        public bool Begin(ForgeRecipe recipe,int level,ForgeCalibration calibration,int seed,ForgedItem item=null)
        {
            if(Active!=null&&!Active.Ended)return false;
            if(item!=null&&(!Items.Contains(item)||item.Recipe!=recipe||item.Plus>=3||Pearls<recipe.PearlCost))return false;
            if(item==null&&Materials<recipe.MaterialCost)return false;
            var next=new ForgeSession(recipe,level,calibration,seed);
            if(item==null)Materials-=recipe.MaterialCost;else Pearls-=recipe.PearlCost;
            rework=item;Active=next;return true;
        }
        public ForgedItem Finish()
        {
            if(Active==null||!Active.Finish())return null;
            var assessment=Active.Assess();
            if(rework!=null){rework.Plus=Math.Min(3,rework.Plus+assessment.Plus);return rework;}
            var item=new ForgedItem(Active.Recipe,assessment.Plus);Items.Add(item);
            if(assessment.Tier>=1)Pearls+=Active.Recipe.PearlReward;
            return item;
        }
    }
}
