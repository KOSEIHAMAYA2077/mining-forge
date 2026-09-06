using System;
using System.Reflection;
using MiningForge;

static class Tests
{
    static int checks;
    static void Check(bool ok, string why) { checks++; if (!ok) throw new Exception(why); }
    static MiningSession At(int a, int b, int c, int focus = 32)
    {
        var s = new MiningSession(new MiningConfig(), 17);
        var v = (int[])typeof(MiningSession).GetField("values", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s);
        v[0] = a; v[1] = b; v[2] = c;
        typeof(MiningSession).GetProperty("Focus").SetValue(s, focus);
        return s;
    }
    static void Main()
    {
        BoardTests.Run();
        Check(MiningSession.PartQuality(45,45,55)==100 && MiningSession.PartQuality(55,45,55)==100,"band edges");
        Check(MiningSession.PartQuality(44,45,55)==98 && MiningSession.PartQuality(56,45,55)==95,"outside band");
        Check(MiningSession.PartQuality(200,45,55)==0,"quality floor");
        Check(MiningSession.Rank(80)=="上質" && MiningSession.Rank(79)=="通常" && MiningSession.Rank(40)=="通常" && MiningSession.Rank(39)=="損傷品","rank boundaries");
        Check(At(45,55,36).Quality==100 && At(44,55,36).Quality==99,"quality rounding");
        Check(!At(29,40,20).CanCollect && At(30,40,20).CanCollect,"separation edge");
        Check(At(30,40,20,0).TryCollect() && !At(29,40,20,0).TryCollect(),"zero focus recovery");
        Check(!At(0,0,0).TryCollect(),"no free reward");
        var s=At(30,40,20); Check(s.TryCollect() && !s.TryCollect() && s.RewardId != null,"one reward only");
        Check(!s.CanStrike(0) && !s.TryAbandon(),"collected node closed");
        s=At(0,0,0,1); Check(s.TryStrike(0,0).Length==0 && s.Focus==1,"insufficient focus");
        s=At(0,0,0); var first=s.TryStrike(0,0); int focus=s.Focus, value=s.Value(0);
        Check(first.Length==1 && s.TryStrike(0,0).Length==0 && s.Focus==focus && s.Value(0)==value,"duplicate input");
        Check(!s.TryCollect() && !s.TryAbandon(),"animation lock"); s.FinishAnimation();
        Check(s.CanStrike(0) && s.Focus==focus,"UI reopening preserves session");
        for(int p=0;p<3;p++)
        {
            var cfg=new MiningConfig(); cfg.minimum[2]=cfg.maximum[2]=4;
            s=new MiningSession(cfg,1);
            var v=(int[])typeof(MiningSession).GetField("values",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(s);
            v[p]=s.Fatal(p)-5; s.TryStrike(2,p); s.FinishAnimation();
            Check(s.State==NodeState.Active && s.Value(p)==s.Fatal(p)-1,"fatal minus one");
            v[p]=s.Fatal(p)-4; s.TryStrike(2,p); s.FinishAnimation();
            Check(s.State==NodeState.Destroyed && !s.TryCollect(),"fatal exact boundary");
        }
        Check(MiningSession.ResolveAmount(44,50,12,true)==6,"critical center stop");
        Check(MiningSession.ResolveAmount(50,50,12,true)==12,"no critical at center");
        for(int v=0;v<50;v++) for(int r=10;r<=32;r++)
            Check(MiningSession.PartQuality(v+MiningSession.ResolveAmount(v,50,r,true),45,55)>=MiningSession.PartQuality(v+r,45,55),"critical never worsens quality");
        for(int seed=0;seed<1000;seed++)
        {
            var a=new MiningSession(new MiningConfig(),seed); var b=new MiningSession(new MiningConfig(),seed);
            for(int n=0;n<4;n++) {var x=a.TryStrike(3,0); var y=b.TryStrike(3,0); for(int p=0;p<3;p++) Check(x[p].amount>=10&&x[p].amount<=14&&x[p].amount==y[p].amount&&!x[p].critical,"seed and uniform range"); a.FinishAnimation();b.FinishAnimation();}
        }
        s=At(0,0,0); Check(s.TryAbandon() && !s.TryAbandon() && !s.TryCollect() && s.RewardId==null,"abandon terminal");
        Console.WriteLine($"PASS: {checks} assertions (boundaries, critical safety, randomness, transactions)");
    }
}
