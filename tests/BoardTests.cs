using System;
using System.Reflection;
using MiningForge.Board;

static class BoardTests
{
    static int count;
    static void Assert(bool pass,string what){count++;if(!pass)throw new Exception("Board: "+what);}
    static void Values(BoardSession s,params int[] v){Array.Copy(v,(int[])typeof(BoardSession).GetField("values",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(s),v.Length);}
    static void Set(BoardSession s,string property,int value)=>typeof(BoardSession).GetProperty(property).SetValue(s,value);
    static void Act(BoardSession s,Skill skill,int r=0,int c=0){if(s.Execute(skill,r,c)==null)throw new Exception("Rejected "+skill);s.FinishAnimation();}
    public static void Run()
    {
        var cfg=new BoardConfig();var s=new BoardSession(cfg,1701);
        Assert(s.At(2,1)==-1&&s.At(3,1)==-1&&s.At(1,1)==3,"coordinate map");
        foreach(Skill sk in new[]{Skill.Horizontal,Skill.Vertical,Skill.Square})
        {int valid=0;for(int r=0;r<4;r++)for(int c=0;c<2;c++)if(s.Forecast(sk,r,c).Valid)valid++;Assert(valid==(sk==Skill.Horizontal?2:sk==Skill.Vertical?4:1),"valid anchors "+sk);}
        int initial=s.Focus;Assert(s.Execute(Skill.Horizontal,0,1)==null&&s.Focus==initial&&s.Activity==1000,"invalid does not consume");
        for(int n=0;n<20;n++)s.Forecast(Skill.Square,0,0);
        var control=new BoardSession(cfg,1701);Act(s,Skill.Square);Act(control,Skill.Square);
        for(int i=0;i<6;i++)Assert(s.Value(i)==control.Value(i),"preview preserves RNG");
        Assert(s.Activity==950&&s.Focus==106,"square drops activity once");
        Assert(BoardSession.Scaled(19,950)==18&&BoardSession.Scaled(26,2000)==39,"floor and multiplier");
        Assert(BoardSession.CriticalAfter(45,50,18)==50&&BoardSession.CriticalAfter(50,50,18)==68,"critical center rule");
        Set(s,"Activity",1900);int before=s.Value(0);Act(s,Skill.Raise);Assert(s.Activity==2000&&s.Value(0)==before,"raise clamps and no strike");
        Assert(!s.Forecast(Skill.Raise,0,0).Valid,"upper rejection");Set(s,"Activity",100);Act(s,Skill.Calm);Assert(s.Activity==0,"calm clamps");
        Assert(!s.Forecast(Skill.Calm,0,0).Valid&&!s.Forecast(Skill.Tap,0,0).Valid,"zero rejection");Act(s,Skill.Raise);Assert(s.Activity==300,"zero rescue");
        Set(s,"Focus",0);Assert(!s.Forecast(Skill.Raise,0,0).Valid&&s.End()&&s.RewardId==null,"zero focus unseparated ends empty");
        s=new BoardSession(cfg,7);Assert(s.End()&&s.RewardId==null&&!s.End(),"untouched ends once without reward");
        s=new BoardSession(cfg,7);Values(s,30,60,60,30,90,89);Assert(!s.Separated,"separation just before");Values(s,30,60,60,30,90,90);Set(s,"Focus",0);Assert(s.Separated&&s.End()&&s.RewardId!=null&&!s.End(),"boundary reward once with zero focus");
        cfg=new BoardConfig();cfg.minimum[2]=cfg.maximum[2]=9;
        s=new BoardSession(cfg,4);Values(s,65,80,80,50,110,110);Act(s,Skill.Gentle);Assert(s.Value(0)==74&&!s.Broken(0),"break minus one");
        Set(s,"Activity",1000);Values(s,66,80,80,50,110,110);Act(s,Skill.Gentle);Assert(s.Value(0)==75&&s.Broken(0)&&!s.Ended&&s.Quality<=39,"break exact not terminal");
        Assert(s.Forecast(Skill.Horizontal,0,0).Valid,"broken remains affected");Values(s,50,80,80,50,110,110);Assert(s.Quality==39,"broken cap persists");
        Act(s,Skill.Horizontal);Assert(s.Value(1)>80,"finished cells still affected");Assert(s.End()&&s.RewardId!=null,"damaged reward allowed");
        s=new BoardSession(new BoardConfig(),1);s.Execute(Skill.Tap,0,0);Assert(s.Execute(Skill.Tap,0,0)==null&&!s.End()&&s.Actions==1,"animation transaction lock");s.FinishAnimation();
        s=new BoardSession(new BoardConfig(),9,true);Act(s,Skill.Square);Assert(s.Activity==1000&&!s.Forecast(Skill.Raise,0,0).Valid,"fixed comparison");
        // Guaranteed critical rolls verify independent cell eligibility and post-multiplier application.
        cfg=new BoardConfig();cfg.criticalPercent=100;cfg.minimum[5]=cfg.maximum[5]=19;
        s=new BoardSession(cfg,1);Set(s,"Activity",950);Values(s,49,80,0,0,0,0);var hits=s.Execute(Skill.Square,0,0);
        Assert(hits[0].critical&&hits[0].after==50&&!hits[1].critical&&hits[1].after==98&&hits[2].after==36,"row order and critical after floor");
        Assert(hits[0].cell==0&&hits[1].cell==1&&hits[2].cell==2&&hits[3].cell==3,"row-major");
        Console.WriteLine("PASS: Board transaction and boundary checks "+count);
        Balance(false);Balance(true);
    }
    static void Balance(bool ranges)
    {
        int success=0,high=0;double quality=0,focus=0,actions=0,adjustments=0;
        for(int seed=1701;seed<=1720;seed++)
        {
            var s=new BoardSession(new BoardConfig(),seed);
            if(ranges){Act(s,Skill.Square);Act(s,Skill.Square);for(int i=0;i<4;i++)Act(s,Skill.Vertical,2,0);}
            // A simple, visible-information policy: largest safe action at the most unfinished cell.
            // Raise once as a controlled comparison of activity adjustment, then finish with singles.
            Act(s,Skill.Raise);
            for(int step=0;step<50;step++)
            {
                int target=-1,gap=0;for(int i=0;i<6;i++){int g=s.Lower(i)-s.Value(i);if(g>gap){gap=g;target=i;}}
                if(target<0)break;
                var c=s.Config.cells[target];Skill chosen=Skill.Gentle;
                foreach(var candidate in new[]{Skill.Strong,Skill.Tap,Skill.Gentle})
                {var p=s.Forecast(candidate,c.row,c.column);if(p.Valid&&p.cells[0].maximum<=s.Upper(target)){chosen=candidate;break;}}
                if(s.Execute(chosen,c.row,c.column)==null)break;s.FinishAnimation();
            }
            bool separated=s.Separated;s.End();if(separated)success++;if(separated&&s.Quality>=80)high++;quality+=s.Quality;focus+=s.Focus;actions+=s.Actions;adjustments+=s.Usage[6]+s.Usage[7];
            Console.WriteLine($"BALANCE seed={seed} mode={(ranges?"ranges":"singles")} reward={separated} Q={s.Quality} focus={s.Focus} T={s.Activity} actions={s.Actions} usage=[{string.Join(',',s.Usage)}]");
        }
        Console.WriteLine($"BALANCE SUMMARY {(ranges?"ranges":"singles")}: recovery={success}/20 high={high}/20 meanQ={quality/20:F1} focus={focus/20:F1} actions={actions/20:F1} adjustments={adjustments/20:F1}");
    }
}
