using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildMiningAssets
{
    const string Root="Assets/Resources/MiningPrototype/";
    static Material Resolve(string name)
    {
        string key=name.Replace(" (Instance)","");string path=Root+"Materials/"+key+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m)return m;
        Color c=key.Contains("Crystal")||key.Contains("Mineral")?new Color(.025f,.26f,.3f):key.Contains("Handle")||key.Contains("Wood")?new Color(.16f,.063f,.018f):key.Contains("Grip")||key.Contains("Leather")?new Color(.055f,.025f,.013f):key.Contains("Steel")?new Color(.16f,.20f,.25f):key.Contains("Crack")?new Color(.025f,.035f,.045f):new Color(.1f,.14f,.18f);
        m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=key,color=c};m.SetFloat("_Smoothness",key.Contains("Crystal")?.55f:.25f);m.SetFloat("_Metallic",key.Contains("Steel")?.65f:0);
        if(key.Contains("Crystal")){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*.25f);}AssetDatabase.CreateAsset(m,path);return m;
    }
    static void Import(string name,bool collider)
    {
        string path=Root+"Models/"+name+".fbx";if(!File.Exists(path))return;
        var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;importer.importAnimation=false;importer.isReadable=true;importer.SaveAndReimport();
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);var go=(GameObject)PrefabUtility.InstantiatePrefab(model);go.name=name;
        foreach(var r in go.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)mats[i]=Resolve(mats[i]?mats[i].name:"MP_Basalt");r.sharedMaterials=mats;}
        var renderers=go.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        if(collider){var box=go.AddComponent<BoxCollider>();box.center=bounds.center;box.size=bounds.size;}
        PrefabUtility.SaveAsPrefabAsset(go,Root+"Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(go);Debug.Log("ASSET_IMPORT "+name+" bounds="+bounds);
    }
    public static void Build()
    {
        BuildPrototype.Prepare();Directory.CreateDirectory(Root+"Materials");Directory.CreateDirectory(Root+"Prefabs");AssetDatabase.Refresh();
        foreach(var name in new[]{"OreNode","Pickaxe","BrokenRock","RockChip","CrystalDrop","Cracks"})Import(name,name=="OreNode"||name=="CrystalDrop");
        foreach(var name in new[]{"ring","blade","shield","axe","robe","cross"})Import("Board_"+name,true);
        if(Directory.Exists(Root+"BoardArt"))foreach(string path in Directory.GetFiles(Root+"BoardArt","*.png")){var tex=(TextureImporter)AssetImporter.GetAtPath(path);tex.textureType=TextureImporterType.Sprite;tex.spriteImportMode=SpriteImportMode.Single;tex.alphaIsTransparency=true;tex.mipmapEnabled=false;tex.npotScale=TextureImporterNPOTScale.None;tex.maxTextureSize=2048;tex.textureCompression=TextureImporterCompression.Uncompressed;tex.SaveAndReimport();}
        foreach(var name in new[]{"crystal","pickaxe"})
        {
            string path=Root+"Icons/"+name+".png";if(!File.Exists(path))continue;var tex=(TextureImporter)AssetImporter.GetAtPath(path);tex.textureType=TextureImporterType.Sprite;tex.spriteImportMode=SpriteImportMode.Single;tex.alphaIsTransparency=true;tex.mipmapEnabled=false;tex.SaveAndReimport();
        }
        var args=Environment.GetCommandLineArgs();bool probe=Array.IndexOf(args,"-probeOnly")>=0;
        string scene="Assets/Scenes/"+(probe?"MiningAssetImport":"ForgeAssetStudy")+".unity";
        if(!File.Exists(scene))
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            if(probe)new GameObject("Import verification",typeof(MiningForge.MiningAssetImport));
            else {var game=new GameObject("Mining art on forge board",typeof(MiningForge.ForgeGame)).GetComponent<MiningForge.ForgeGame>();game.prototypeAssets=true;}
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),scene);
        }
        PlayerSettings.productName="Mining Forge Asset Study";PlayerSettings.bundleVersion="0.3.1";
        int i=Array.IndexOf(args,"-buildOutput");string output=Path.GetFullPath(args[i+1]);Directory.CreateDirectory(Path.GetDirectoryName(output));AssetDatabase.SaveAssets();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scene},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Asset build failed");Debug.Log("MINING_ASSET_BUILD_OK");
    }
}
