using System;
using System.Linq;
using System.Reflection;
using MiningForge.Reference;

static class ForgeTests
{
    static int checks;
    static void Check(bool condition,string why){checks++;if(!condition)throw new Exception(why);}
    sealed class Fixed : IForgeRandom
    {
        public double UnitValue=.99;
        public int Next(int min,int max)=>min;
        public double Unit()=>UnitValue;
    }
    static ForgeSession Session(int recipe=2,Fixed random=null,ForgeCalibration config=null,int level=99)
        =>new ForgeSession(ForgeRecipe.Samples[recipe],level,config??new ForgeCalibration{chargeChance=0},42,random??new Fixed());
    static void Set(ForgeSession s,string property,object value)=>typeof(ForgeSession).GetProperty(property).SetValue(s,value);
    static void Values(ForgeSession s,params int[] values){var target=(int[])typeof(ForgeSession).GetField("values",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(s);Array.Copy(values,target,values.Length);}
    static void Main()
    {
        int[] costs={5,8,10,8,10,12,7,11,16,18,12,7,0,6};
        for(int i=0;i<costs.Length;i++)Check(ForgeTechniques.All[i].Cost==costs[i],"source technique cost "+i);
        Check(ForgeSession.FocusForLevel(5)==57&&ForgeSession.FocusForLevel(23)==80&&ForgeSession.FocusForLevel(60)==132&&ForgeSession.FocusForLevel(99)==210,"focus reference anchors");
        var s=Session();Check(s.Activity==1000,"initial activity");
        var a=s.Execute(Technique.Heat);Check(a.Valid&&s.Activity==1300&&s.Focus==200,"heat +300 cost10");
        for(int i=0;i<4;i++)s.Execute(Technique.Heat);
        Check(s.Activity==2500&&ForgeSession.ActivityMultiplier(s.Activity)==1.5,"activity may exceed2000; effect capped");
        s=Session();Set(s,"Activity",200);int focus=s.Focus;var denied=s.Execute(Technique.Cool);Check(!denied.Valid&&s.Focus==focus&&s.Activity==200&&s.Actions==0,"invalid cool atomic");
        Set(s,"Activity",149);Check(!s.Execute(Technique.HotWind).Valid,"hotwind cannot cool belowzero");Set(s,"Activity",150);s.Execute(Technique.HotWind);Check(s.Activity==0,"hotwind consumes150 only");Check(!s.Execute(Technique.Tap).Valid,"zero activity stops hit");Check(s.Execute(Technique.Heat).Valid&&s.Activity==300,"heat restores fromzero");
        s=Session();Set(s,"Focus",4);Check(!s.Execute(Technique.Tap).Valid&&s.Actions==0,"insufficient focus atomic");
        s=Session(level:11);Check(s.CanUse(Technique.Gentle)!=null,"locked skill");
        s=Session();focus=s.Focus;Check(!s.Execute(Technique.Quad,3,1).Valid&&s.Focus==focus,"invalid footprint no cost");
        s=Session();a=s.Execute(Technique.Diagonal,0,0);Check(a.Hits.Select(h=>h.Cell).SequenceEqual(new[]{1,2}),"diagonal top-right bottom-left");
        s=Session();a=s.Execute(Technique.Vertical);Check(a.Hits.Select(h=>h.Cell).SequenceEqual(new[]{0,2}),"vertical targets");
        var rng=new Fixed{UnitValue=0};s=Session(random:rng);Values(s,95,100,110,0);a=s.Execute(Technique.Quad);
        Check(a.Hits[0].Critical&&a.Hits[0].After==100,"critical stops at center");
        Check(a.Hits[1].Miss&&a.Hits[1].After==100,"center critical misses");
        Check(a.Hits[2].After==120,"critical reaches other target");
        Check(a.Hits[3].After>0&&a.Hits[3].After<120,"critical does not teleport to center");
        s=Session(random:rng);Values(s,105,0,0,0);a=s.Execute(Technique.Tap);Check(a.Hits[0].Miss&&s.Value(0)==105,"over-center critical misses");
        s=Session(random:rng);a=s.Execute(Technique.Gentle);Check(a.Hits[0].Critical,"gentle can critical");
        s=Session();Values(s,99,0,0,0);a=s.Execute(Technique.Tap);Check(s.Value(0)>100,"normal can overshoot");
        s=Session(recipe:0);a=s.Execute(Technique.RandomFour);Check(a.Hits.Count==4&&a.Hits.Count(h=>h.Cell==0)==3&&a.Hits.Count(h=>h.Cell==1)==1,"randomfour max3 percell");
        Check(a.Hits[1].Before==a.Hits[0].After&&a.Hits[2].Before==a.Hits[1].After,"randomfour sequential hits");
        s=Session();double normal=s.CriticalChance(Technique.Tap,0);Check(Math.Abs(s.CriticalChance(Technique.Aim,0)-normal*7)<.00001,"aim7x reference multiplier");
        s=Session(config:new ForgeCalibration{chargeChance=1});s.Execute(Technique.Tap);Check(s.Charged,"charge path");s.Execute(Technique.Ultimate);Check(s.Guaranteed&&s.UltimateUsed&&s.Activity==900,"ultimate costs50 and primes guarantee");
        a=s.Execute(Technique.Quad);Check(a.Hits.All(h=>h.Critical)&&!s.Guaranteed,"ultimate applies full next technique");Check(!s.Execute(Technique.Ultimate).Valid,"ultimate only once");
        s=Session(config:new ForgeCalibration{chargeChance=1});s.Execute(Technique.Tap);s.Execute(Technique.Ultimate);s.Execute(Technique.Heat);Check(!s.Guaranteed,"PS4 heat consumes guarantee");
        s=Session(config:new ForgeCalibration{chargeChance=1});s.Execute(Technique.Tap);s.Execute(Technique.Ultimate);s.Execute(Technique.Cool);Check(!s.Guaranteed,"PS4 cool consumes guarantee");
        s=Session();Set(s,"Activity",850);s.Execute(Technique.Tap);Check(s.PowerEffect==2,"power event800");s.Execute(Technique.Heat);Check(s.PowerEffect==1,"heat passes power event");Set(s,"Activity",650);s.Execute(Technique.Tap);Check(s.PowerEffect==.5,"power event600");
        s=Session(recipe:3);Set(s,"Activity",850);s.Execute(Technique.Tap);Check(s.Cost(Technique.RandomFour)==4,"provisional odd focus rounds up under half");Set(s,"Activity",650);s.Execute(Technique.Tap);Check(s.Cost(Technique.Tap)==8&&s.CriticalEffect>1,"focus1.5 and critical effect");
        s=Session(recipe:4);Values(s,170,185,0,0,0,0,0,0);Set(s,"Activity",850);s.Execute(Technique.Heat);Check(s.Value(0)==170,"no event when not a multiple200");Set(s,"Activity",850);s.Execute(Technique.Tap,3,0);Check(s.Value(0)==170&&s.Value(1)<185,"revert excludes exact target");
        s=Session(recipe:5);Set(s,"Activity",850);s.Execute(Technique.Tap);Check(s.BoostedCell>=0,"singlecell effect");
        s=Session();Values(s,100,100,120,120);var exact=s.Assess();Check(exact.Plus==3,"all exact top quality");Values(s,105,105,125,125);Check(s.Assess().Error>exact.Error&&s.Assess().Plus<3,"band not flat maximum");
        focus=s.Focus;int activity=s.Activity;s.Assess();s.Assess();Check(s.Focus==focus&&s.Activity==activity,"assessment free");
        var work=new ForgeWorkshop();var recipe=ForgeRecipe.Samples[2];int initial=work.Materials;Check(work.Begin(recipe,60,new ForgeCalibration(),3),"begin new");Check(work.Materials==initial-recipe.MaterialCost,"materials charged at start");Check(!work.Begin(recipe,60,new ForgeCalibration(),3),"cannot cancel active session by begin");
        int pearls=work.Pearls;var item=work.Finish();Check(item!=null&&item.Plus==0&&work.Items.Count==1,"untouched still produces item");Check(work.Pearls==pearls,"bad new gives no pearls");Check(work.Finish()==null&&work.Items.Count==1,"no duplicate reward");
        Check(work.Begin(recipe,60,new ForgeCalibration(),4,item),"begin rework");Check(work.Pearls==pearls-recipe.PearlCost,"rework consumes pearls");Values(work.Active,100,100,120,120);var refinal=work.Finish();Check(object.ReferenceEquals(item,refinal)&&item.Plus==3&&work.Items.Count==1,"rework upgrades same item");Check(work.Pearls==pearls-recipe.PearlCost,"rework produces no pearls");Check(!work.Begin(recipe,60,new ForgeCalibration(),4,item),"plus3 cannot rework");
        Check(!work.Begin(recipe,60,new ForgeCalibration(),4,new ForgedItem(recipe)),"foreign item rejected");
        work=new ForgeWorkshop();work.Begin(recipe,60,new ForgeCalibration(),3);Values(work.Active,100,100,120,120);work.Finish();Check(work.Pearls==30+recipe.PearlReward,"good new gives pearls once");
        Console.WriteLine("FORGE_RULES_OK checks="+checks+"; implementation checks, not proof of original numerical calibration");
    }
}
