using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildForge
{
    public static void Build()
    {
        BuildPrototype.Prepare();
        foreach(string path in new[]{"Assets/Resources/ForgeArt/workbench.png","Assets/Resources/ForgeArt/hot-metal.png"})
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }
        if(!File.Exists("Assets/Scenes/ForgeStudy.unity"))
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Forge reference",typeof(MiningForge.ForgeGame));
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/ForgeStudy.unity");
        }
        PlayerSettings.productName="Mining Forge P0a-03";PlayerSettings.bundleVersion="0.3.0";
        var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-buildOutput");if(i<0)throw new ArgumentException("Missing buildOutput");
        var pathOut=Path.GetFullPath(args[i+1]);Directory.CreateDirectory(Path.GetDirectoryName(pathOut));
        AssetDatabase.SaveAssets();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/ForgeStudy.unity"},locationPathName=pathOut,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Forge build failed");Debug.Log("FORGE_BUILD_OK "+pathOut);
    }
}
