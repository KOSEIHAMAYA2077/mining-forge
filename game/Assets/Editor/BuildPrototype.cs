using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using MiningForge;

public static class BuildPrototype
{
    [MenuItem("Mining Forge/Prepare scene")]
    public static void Prepare()
    {
        Directory.CreateDirectory("Assets/Settings"); Directory.CreateDirectory("Assets/Scenes");
        var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/MiningPipeline.asset");
        if(!pipeline)
        {
            var renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,"Assets/Settings/MiningRenderer.asset");
            pipeline=ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
            var so=new SerializedObject(pipeline);var renderers=so.FindProperty("m_RendererDataList");renderers.arraySize=1;renderers.GetArrayElementAtIndex(0).objectReferenceValue=renderer;so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(pipeline,"Assets/Settings/MiningPipeline.asset");
        }
        GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
        var graphics=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        var shaders=graphics.FindProperty("m_AlwaysIncludedShaders");
        shaders.ClearArray();
        foreach(string shader in new[]{"UI/Default"})
        {var found=Shader.Find(shader);if(!found)throw new Exception("Missing shader "+shader);int index=shaders.arraySize;shaders.InsertArrayElementAtIndex(index);shaders.GetArrayElementAtIndex(index).objectReferenceValue=found;}
        graphics.ApplyModifiedPropertiesWithoutUndo();
        Directory.CreateDirectory("Assets/Resources/Materials");
        foreach(string name in new[]{"Rock","Crystal","Particle"})
        {
            string materialPath="Assets/Resources/Materials/"+name+".mat";
            if(AssetDatabase.LoadAssetAtPath<Material>(materialPath))continue;
            var m=new Material(Shader.Find(name=="Particle"?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));
            if(name=="Crystal"){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",Color.white);}
            if(name=="Particle"){m.SetFloat("_Surface",1);m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;}
            AssetDatabase.CreateAsset(m,materialPath);
        }
        if(!File.Exists("Assets/Scenes/BoardStudy.unity"))
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("Mining Forge Board",typeof(BoardGame));
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/BoardStudy.unity");
        }
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/BoardStudy.unity",true)};
        PlayerSettings.companyName="MiningForge";PlayerSettings.productName="Mining Forge P0a-02";
        PlayerSettings.bundleVersion="0.2.0";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        AssetDatabase.SaveAssets();
    }
    [MenuItem("Mining Forge/Build Windows")]
    public static void Build()
    {
        Prepare();AssetDatabase.Refresh();
        var args=Environment.GetCommandLineArgs();int outputIndex=Array.IndexOf(args,"-buildOutput");
        string path=outputIndex>=0&&outputIndex+1<args.Length?Path.GetFullPath(args[outputIndex+1]):Path.GetFullPath("../builds/P0a-02/MiningForge.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/BoardStudy.unity"},locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
        Debug.Log("P0A_BUILD_OK "+path);
    }
}
