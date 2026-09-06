using System;
using System.Collections.Generic;

namespace MiningForge.Board
{
    [Serializable] public class CellConfig { public string id; public int row, column, center; }
    [Serializable] public class BoardConfig
    {
        public int focus = 120, initialActivity = 1000, activityDrop = 50, band = 5, separationMargin = 15, damageMargin = 20;
        public int criticalPercent = 10;
        public CellConfig[] cells = {
            new CellConfig{id="A",row=0,column=0,center=50}, new CellConfig{id="B",row=0,column=1,center=80},
            new CellConfig{id="C",row=1,column=0,center=80}, new CellConfig{id="D",row=1,column=1,center=50},
            new CellConfig{id="E",row=2,column=0,center=110}, new CellConfig{id="F",row=3,column=0,center=110}};
        public int[] minimum = {18,36,9,18,18,18,0,0};
        public int[] maximum = {26,52,13,26,26,26,0,0};
        public int[] costs = {5,8,3,8,8,14,8,4};
    }
    public enum Skill { Tap, Strong, Gentle, Horizontal, Vertical, Square, Raise, Calm }
    public struct CellForecast
    {
        public int cell, minimum, maximum, criticalMinimum, criticalMaximum;
        public bool canCritical;
    }
    public sealed class Preview
    {
        public string error;
        public int nextActivity, cost;
        public CellForecast[] cells = Array.Empty<CellForecast>();
        public bool Valid => error == null;
    }
    public struct CellHit
    {
        public int cell, before, after;
        public bool critical, enteredBand, exact, over, newlyBroken;
    }
    public sealed class BoardSession
    {
        public readonly BoardConfig Config;
        public readonly bool FixedActivity;
        readonly Random random;
        readonly int[] values;
        readonly bool[] broken;
        public readonly int Seed;
        public int Focus {get; private set;}
        public int Activity {get; private set;}
        public int Actions {get; private set;}
        public bool Busy {get; private set;}
        public bool Ended {get; private set;}
        public string RewardId {get; private set;}
        public int[] Usage {get;} = new int[8];
        public BoardSession(BoardConfig config, int seed, bool fixedActivity = false)
        {
            Config=config; Seed=seed; FixedActivity=fixedActivity; random=new Random(seed);
            values=new int[config.cells.Length]; broken=new bool[values.Length]; Focus=config.focus; Activity=fixedActivity?1000:config.initialActivity;
        }
        public int Value(int i)=>values[i];
        public bool Broken(int i)=>broken[i];
        public int Lower(int i)=>Config.cells[i].center-Config.band;
        public int Upper(int i)=>Config.cells[i].center+Config.band;
        public int Separation(int i)=>Lower(i)-Config.separationMargin;
        public int Damage(int i)=>Upper(i)+Config.damageMargin;
        public bool InBand(int i)=>values[i]>=Lower(i)&&values[i]<=Upper(i);
        public int At(int row,int column)
        {for(int i=0;i<values.Length;i++)if(Config.cells[i].row==row&&Config.cells[i].column==column)return i;return -1;}
        public static double Multiplier(int activity)=>.5+activity/2000.0;
        public static int Scaled(int basic,int activity)=>(int)Math.Floor(basic*Multiplier(activity));
        public static int CriticalAfter(int current,int center,int amount)=>current<center?Math.Min(center,current+amount*2):current+amount;
        public int Quality
        {
            get {double sum=0;bool damaged=false;for(int i=0;i<values.Length;i++){sum+=MiningSession.PartQuality(values[i],Lower(i),Upper(i));damaged|=broken[i];}
                int q=(int)Math.Floor(sum/values.Length+.5);return damaged?Math.Min(39,q):q;}
        }
        public bool Separated {get {for(int i=0;i<values.Length;i++)if(values[i]<Separation(i))return false;return true;}}
        public bool HasBroken {get {foreach(bool b in broken)if(b)return true;return false;}}
        public Preview Forecast(Skill skill,int row,int column)
        {
            int sk=(int)skill;var p=new Preview{nextActivity=Activity};
            if(sk<0||sk>7){p.error="技を選んでください";return p;}
            p.cost=Config.costs[sk];
            if(skill==Skill.Raise||skill==Skill.Calm)
            {
                p.nextActivity=FixedActivity?1000:Math.Max(0,Math.Min(2000,Activity+(skill==Skill.Raise?300:-200)));
                if(FixedActivity)p.error="比較モードでは活性を1000に固定しています";
                else if(p.nextActivity==Activity)p.error="活性はこれ以上変わりません";
            }
            else
            {
                int width=skill==Skill.Horizontal||skill==Skill.Square?2:1, height=skill==Skill.Vertical||skill==Skill.Square?2:1;
                var cells=new List<CellForecast>();
                for(int r=row;r<row+height;r++)for(int c=column;c<column+width;c++)
                {
                    int i=At(r,c);if(i<0){p.error=r<0||r>=4||c<0||c>=2?"範囲が盤面の外に出ています":"範囲に鉱石のない位置を含みます";continue;}
                    int lo=Scaled(Config.minimum[sk],Activity),hi=Scaled(Config.maximum[sk],Activity);
                    cells.Add(new CellForecast{cell=i,minimum=values[i]+lo,maximum=values[i]+hi,canCritical=skill!=Skill.Gentle&&values[i]<Config.cells[i].center,
                        criticalMinimum=CriticalAfter(values[i],Config.cells[i].center,lo),criticalMaximum=CriticalAfter(values[i],Config.cells[i].center,hi)});
                }
                p.cells=cells.ToArray();p.nextActivity=FixedActivity?1000:Math.Max(0,Activity-Config.activityDrop);
                if(Activity==0)p.error="活性が0です。活性を上げるか、採取を終了してください";
            }
            if(Focus<p.cost)p.error="集中力が足りません";
            if(Ended)p.error="この結晶の採取は終了しています";
            if(Busy)p.error="演出中です";
            return p;
        }
        public CellHit[] Execute(Skill skill,int row,int column)
        {
            var p=Forecast(skill,row,column);if(!p.Valid)return null;
            Busy=true;Focus-=p.cost;Actions++;Usage[(int)skill]++;
            var hits=new CellHit[p.cells.Length];
            for(int k=0;k<p.cells.Length;k++)
            {
                int i=p.cells[k].cell;
                int amount=Scaled(random.Next(Config.minimum[(int)skill],Config.maximum[(int)skill]+1),Activity);
                bool crit=p.cells[k].canCritical&&random.Next(100)<Config.criticalPercent;
                int after=crit?CriticalAfter(values[i],Config.cells[i].center,amount):values[i]+amount;
                hits[k]=new CellHit{cell=i,before=values[i],after=after,critical=crit,enteredBand=!InBand(i)&&after>=Lower(i)&&after<=Upper(i),
                    exact=after==Config.cells[i].center,over=after>Upper(i),newlyBroken=!broken[i]&&after>=Damage(i)};
            }
            foreach(var h in hits){values[h.cell]=h.after;broken[h.cell]|=h.newlyBroken;}
            Activity=p.nextActivity;return hits;
        }
        public void FinishAnimation(){Busy=false;}
        public bool End(bool abandon=false)
        {
            if(Busy||Ended)return false;
            if(!abandon&&Separated)RewardId=Guid.NewGuid().ToString("N")+":crystal";
            Ended=true;return true;
        }
    }
}
