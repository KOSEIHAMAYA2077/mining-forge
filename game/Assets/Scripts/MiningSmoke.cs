using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace MiningForge
{
    public partial class MiningGame
    {
        volatile float audioPeak;
        readonly float[] audioSamples = new float[256];
        void SampleAudio()
        {AudioListener.GetOutputData(audioSamples,0);foreach(float sample in audioSamples)audioPeak=Math.Max(audioPeak,Math.Abs(sample));}
        void OnAudioFilterRead(float[] data,int channels)
        {for(int i=0;i<data.Length;i++) if(Math.Abs(data[i])>audioPeak) audioPeak=Math.Abs(data[i]);}
        void Verify(bool condition,string name)
        {if(!condition)throw new Exception("SMOKE FAIL: "+name);Debug.Log("SMOKE PASS: "+name);}
        IEnumerator Snapshot(string folder,string name)
        {yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return new WaitForSeconds(.3f);}
        IEnumerator SmokeTest()
        {
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../..","artifacts","P0a-01"));Directory.CreateDirectory(folder);
            yield return new WaitForSeconds(6);
            Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSeconds(1);
            Verify(font!=null&&impacts[0]!=null&&Resources.Load<GameObject>("Art/rock_largeA")!=null,"bundled assets loaded");
            Verify(!session.TryCollect(),"no unworked reward");
            yield return Snapshot(folder,"01-start-1280");
            skills[3].onClick.Invoke();skills[3].onClick.Invoke();
            Verify(session.Focus==27 && session.Actions==1,"UI repeated strike guarded");
            yield return new WaitForSeconds(.6f);
            int oldFocus=session.Focus,oldValue=session.Value(0);
            closeButton.onClick.Invoke();yield return null;
            Verify(!miningPanel.activeSelf&&closedPanel.activeSelf,"close panel");
            yield return Snapshot(folder,"02-closed");
            closedPanel.transform.Find("Reopen").GetComponent<Button>().onClick.Invoke();
            Verify(session.Focus==oldFocus&&session.Value(0)==oldValue,"reopen retains focus and gauge");
            var reference=new MiningSession(config,seed);reference.TryStrike(3,0);reference.FinishAnimation();var expected=reference.TryStrike(0,0);
            skills[0].onClick.Invoke();yield return new WaitForSeconds(.6f);
            Verify(session.Value(0)==reference.Value(0),"reopen retains random state");
            NewSample(seed);
            for(int attempt=0;attempt<20;attempt++)
            {
                int target=-1;for(int p=0;p<3;p++)if(session.Value(p)<session.Lower(p)){target=p;break;}
                if(target<0 || session.Focus<2)break;
                parts[target].onClick.Invoke();int gap=config.centers[target]-session.Value(target);
                int skill=gap>=32?1:gap>=14?0:2;
                if(!session.CanStrike(skill)) skill=2;
                skills[skill].onClick.Invoke();yield return new WaitForSeconds(.45f);
            }
            Verify(session.CanCollect,"play sequence reaches recoverable crystal");
            Screen.SetResolution(1920,1080,FullScreenMode.Windowed);yield return new WaitForSeconds(1);
            yield return Snapshot(folder,"03-ready-1920");
            collectButton.onClick.Invoke();collectButton.onClick.Invoke();yield return new WaitForSeconds(.4f);
            Verify(session.State==NodeState.Collected&&session.RewardId!=null&&!session.TryCollect(),"one collection and result screen");
            yield return Snapshot(folder,"04-recovered-1920");
            Verify(audioPeak>.001f,"nonzero audio output from playback");
            resultPanel.transform.Find("Retry").GetComponent<Button>().onClick.Invoke();
            Verify(session.Focus==32&&session.Value(0)==0&&session.RewardId==null,"explicit new sample resets");
            for(int i=0;i<5&&session.State==NodeState.Active;i++){skills[1].onClick.Invoke();yield return new WaitForSeconds(.45f);}
            Verify(session.State==NodeState.Destroyed&&!session.CanCollect,"fatal damage ends sample");
            yield return Snapshot(folder,"05-destroyed");
            NewSample(seed);abandonButton.onClick.Invoke();Verify(session.State==NodeState.Active,"abandon confirmation");abandonButton.onClick.Invoke();Verify(session.State==NodeState.Abandoned,"abandon ends without reward");
            NewSample(seed);
            int overflows=0;foreach(var label in FindObjectsByType<Text>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
                if(label.preferredHeight>label.rectTransform.rect.height+2){Debug.LogWarning("TEXT_OVERFLOW "+label.text+" preferred="+label.preferredHeight+" available="+label.rectTransform.rect.height);overflows++;}
            Verify(overflows==0,"visible Japanese text fits layout");
            Debug.Log("P0A_SMOKE_OK peak="+audioPeak);File.WriteAllText(Path.Combine(folder,"smoke-result.txt"),"PASS\nAudio peak: "+audioPeak+"\nResolution: "+Screen.width+"x"+Screen.height);
            Application.Quit(0);
        }
    }
}
