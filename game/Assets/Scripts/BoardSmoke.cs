using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MiningForge.Board;

namespace MiningForge
{
    public partial class BoardGame
    {
        void Check(bool pass,string label){if(!pass)throw new System.Exception("BOARD SMOKE FAIL: "+label);Debug.Log("BOARD SMOKE PASS: "+label);}
        IEnumerator Shot(string folder,string name)
        {
            yield return null;Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
            foreach(var text in FindObjectsByType<Text>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
                Check(text.preferredHeight<=text.rectTransform.rect.height+2,"layout "+name+" / "+text.text.Replace('\n',' '));
            var capture=ScreenCapture.CaptureScreenshotAsTexture();
            var sample=capture.GetPixel(capture.width/2,capture.height/2);Check(sample.r+sample.g+sample.b>.01f,"rendered frame "+name);Destroy(capture);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSeconds(.35f);
        }
        IEnumerator Smoke()
        {
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../..","artifacts",Version));Directory.CreateDirectory(folder);
            yield return new WaitForSeconds(6);Screen.SetResolution(1279,719,FullScreenMode.Windowed);yield return new WaitForSeconds(1);Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSeconds(1);
            yield return Shot(folder,"01-commands-1280");
            Open(BoardScreen.Skills);yield return Shot(folder,"01b-skills-1280");Back();
            int f=session.Focus,t=session.Activity;
            Open(BoardScreen.Skills);Choose(Skill.Square);SelectCell(0,1);Check(!session.Forecast(skill,row,column).Valid,"invalid anchor rejected");Confirm();Check(session.Focus==f,"invalid confirm preserves focus");
            SelectCell(0,0);yield return Shot(folder,"02-range-preview-1280");Back();Open(BoardScreen.Details);Back();Open(BoardScreen.Paused);Back();Check(session.Focus==f&&session.Activity==t,"menus and pause are free");
            Choose(Skill.Square);Confirm();Confirm();Check(session.Actions==1&&session.Focus==106&&session.Activity==950,"double confirm and shared activity");yield return new WaitForSeconds(.6f);
            var control=new BoardSession(config,seed);control.Execute(Skill.Square,0,0);control.FinishAnimation();for(int i=0;i<6;i++)Check(session.Value(i)==control.Value(i),"preview RNG cell "+i);
            Choose(Skill.Raise);Confirm();yield return new WaitForSeconds(.6f);Check(session.Activity==1250,"activity adjustment");
            Choose(Skill.Calm);Confirm();yield return new WaitForSeconds(.6f);Check(session.Activity==1050,"calming adjustment");
            NewSession(seed);
            Choose(Skill.Square);SelectCell(0,0);Confirm();yield return new WaitForSeconds(.45f);Choose(Skill.Square);Confirm();yield return new WaitForSeconds(.45f);
            for(int n=0;n<4;n++){Choose(Skill.Vertical);SelectCell(2,0);Confirm();yield return new WaitForSeconds(.45f);}
            Choose(Skill.Raise);Confirm();yield return new WaitForSeconds(.45f);
            for(int step=0;step<35;step++)
            {
                int target=-1,gap=0;for(int i=0;i<6;i++){int g=session.Lower(i)-session.Value(i);if(g>gap){gap=g;target=i;}}
                if(target<0)break;var c=config.cells[target];Skill chosen=Skill.Gentle;
                foreach(var candidate in new[]{Skill.Strong,Skill.Tap,Skill.Gentle}){var p=session.Forecast(candidate,c.row,c.column);if(p.Valid&&p.cells[0].maximum<=session.Upper(target)){chosen=candidate;break;}}
                if(!session.Forecast(chosen,c.row,c.column).Valid)break;
                Choose(chosen);SelectCell(c.row,c.column);Confirm();yield return new WaitForSeconds(.45f);
            }
            Check(session.Separated,"range and activity play sequence recovers");
            Screen.SetResolution(1920,1080,FullScreenMode.Windowed);yield return new WaitForSeconds(1);Open(BoardScreen.Details);yield return Shot(folder,"03-assessment-1920");Back();yield return Shot(folder,"04-exposed-1920");
            Open(BoardScreen.Collect);yield return null;menuButtons[0].onClick.Invoke();yield return new WaitForSeconds(.5f);Check(session.RewardId!=null&&!session.End(),"one reward");yield return Shot(folder,"05-result-1920");
            NewSession(seed);Open(BoardScreen.Collect);yield return null;menuButtons[0].onClick.Invoke();Check(session.Ended&&session.RewardId==null,"unworked no reward");
            NewSession(seed);for(int i=0;i<4;i++){Choose(Skill.Strong);SelectCell(0,0);Confirm();yield return new WaitForSeconds(.45f);}
            Check(session.Broken(0)&&!session.Ended,"broken local cell continues");yield return Shot(folder,"06-broken-1920");
            Check(audioPeak>.001f,"audio output");
            NewSession(seed);Open(BoardScreen.Settings);yield return null;volume=0;source.volume=0;shake=0;Check(source.volume==0&&shake==0,"mute and shake off");
            volume=.65f;source.volume=volume;Open(BoardScreen.Commands);yield return null;
            int overflow=0;foreach(var text in FindObjectsByType<Text>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))if(text.preferredHeight>text.rectTransform.rect.height+2){Debug.LogWarning("BOARD TEXT OVERFLOW: "+text.text+" "+text.preferredHeight+" > "+text.rectTransform.rect.height);overflow++;}
            Check(overflow==0,"Japanese text layout");Debug.Log("P0A02_SMOKE_OK peak="+audioPeak);File.WriteAllText(Path.Combine(folder,"smoke-result.txt"),"PASS audio="+audioPeak);
            Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSeconds(.3f);Application.Quit();
        }
    }
}
