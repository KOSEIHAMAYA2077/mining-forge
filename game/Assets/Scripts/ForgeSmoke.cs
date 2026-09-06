using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MiningForge.Reference;

namespace MiningForge
{
    public partial class ForgeGame
    {
        string smokeFolder;
        int smokeChecks;
        void Check(bool condition,string message){smokeChecks++;if(!condition)throw new Exception("FORGE_SMOKE_FAIL "+message);Debug.Log("FORGE_CHECK "+message);}
        IEnumerator Capture(string name)
        {
            yield return new WaitForSeconds(.2f);yield return new WaitForEndOfFrame();
            var image=ScreenCapture.CaptureScreenshotAsTexture();var bytes=image.EncodeToPNG();Check(bytes.Length>50000,"nonblank capture "+name);File.WriteAllBytes(Path.Combine(smokeFolder,name+".png"),bytes);Destroy(image);
        }
        IEnumerator Smoke()
        {
            smokeFolder=Path.GetFullPath(Path.Combine(Application.dataPath,prototypeAssets?"../../../artifacts/ForgeAssets":"../../../artifacts/P0a-03"));Directory.CreateDirectory(smokeFolder);
            Application.logMessageReceived+=(message,stack,type)=>{if(type==LogType.Exception){File.WriteAllText(Path.Combine(smokeFolder,"FAILED.txt"),message+"\n"+stack);Application.Quit(2);}};
            Screen.SetResolution(1281,720,false);yield return new WaitForSeconds(.4f);Screen.SetResolution(1280,720,false);yield return new WaitForSeconds(.5f);
            yield return Capture("01-preparation");
            Check(font!=null&&strike!=null,"font and strike audio loaded");
            // Test-only forced charge is reported; it does not alter the shipped JSON.
            calibration.chargeChance=1;
            Begin(2);Check(Session!=null&&Session.Focus==132,"level60 focus132");
            yield return Capture("02-commands");
            Show(View.Skills);yield return Capture("03-skills-page1");page=1;Show(View.Skills);yield return Capture("04-skills-page2");page=2;Show(View.Skills);yield return Capture("05-skills-page3");
            Choose(Technique.Quad);row=column=0;Refresh();yield return Capture("06-range-target");
            Confirm();float peak=0;var audio=new float[256];while(busy){AudioListener.GetOutputData(audio,0);foreach(float v in audio)peak=Mathf.Max(peak,Mathf.Abs(v));yield return null;}
            Check(Session.Actions==1&&Session.Focus==120,"quad action and cost12");Check(Session.Charged,"forced charge path");Check(peak>0,"audio output present; listening not asserted");
            yield return Capture("07-after-hit");
            Choose(Technique.Ultimate);Confirm();while(busy)yield return null;Check(Session.Guaranteed&&Session.Activity==900,"ultimate consumes50 and grants critical");
            yield return Capture("08-ultimate-ready");
            Choose(Technique.SuperQuad);row=column=0;Confirm();while(busy)yield return null;Check(Session.UltimateUsed&&!Session.Guaranteed,"guarantee consumed by range strike");
            Choose(Technique.Gentle);row=column=0;Confirm();while(busy)yield return null;Check(Session.Activity==800&&Session.PowerEffect==2,"power event at800");
            yield return Capture("09-material-effect");
            int focus=Session.Focus,activity=Session.Activity;Show(View.Details);Check(Session.Focus==focus&&Session.Activity==activity,"assessment free");yield return Capture("10-assessment");
            Show(View.Finish);yield return Capture("11-finish-confirmation");Finish();Check(resultItem!=null&&workshop.Items.Count==1,"finish produces exactly one item");Check(workshop.Finish()==null,"duplicate finish rejected");
            if(prototypeAssets){while(!artWaiting)yield return null;Check(artDrop&&artDrop.raycastTarget&&artHits>0&&artContactError<.01f,"rendered drop, aligned tool and pointer target");yield return Capture("12a-drop");CollectArt();CollectArt();while(busy)yield return null;Check(artCollections==1&&workshop.Items.Count==1,"visual pickup cannot duplicate item");}
            yield return Capture("12-result");
            Show(View.Inventory);yield return Capture("13-inventory");
            Begin(2,resultItem);Check(Session.Actions==0,"rework begins");Show(View.Finish);Finish();Check(workshop.Items.Count==1,"rework retains same item");
            if(prototypeAssets){while(!artWaiting)yield return null;CollectArt();while(busy)yield return null;Check(artCollections==2&&workshop.Items.Count==1,"rework pickup retains same inventory item");}
            Screen.SetResolution(1920,1080,false);yield return new WaitForSeconds(.7f);Begin(3);yield return Capture("14-six-cell-1920");
            Show(View.Finish);Finish();if(prototypeAssets){while(!artWaiting)yield return null;CollectArt();while(busy)yield return null;}Begin(4);yield return Capture("15-eight-cell-1920");
            if(prototypeAssets){foreach(string shape in new[]{"ring","blade","shield","axe","robe","cross"})Check(ArtSprite(shape)!=null,"Blender board image "+shape);Check(Resources.Load<GameObject>("MiningPrototype/Prefabs/Pickaxe")!=null&&Resources.Load<GameObject>("MiningPrototype/Prefabs/Board_axe").GetComponent<Collider>()!=null,"FBX prefabs and board collider saved");}
            File.WriteAllText(Path.Combine(smokeFolder,"report.txt"),"FORGE_SMOKE_OK checks="+smokeChecks+"\nUI method-driven test, not OS-input test.\nForced charge chance in smoke only.\nAudio peak="+peak+"; subjective audio not reviewed.\nNo claim of exact original mechanics calibration.\n");
            Debug.Log("FORGE_SMOKE_OK "+smokeChecks);Application.Quit(0);
        }
    }
}
