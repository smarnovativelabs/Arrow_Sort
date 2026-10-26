using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WoolGame.Editor
{
    public static class WoolAudioAuthoring
    {
        const string Root="Assets/Sounds/Pixabay/";
        static AudioClip Clip(string name,bool music=false)
        {
            string path=Root+name+".mp3";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=AssetImporter.GetAtPath(path) as AudioImporter;
            if(!importer)throw new InvalidOperationException("Missing audio: "+path);
            var settings=importer.defaultSampleSettings;
            settings.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat=AudioCompressionFormat.Vorbis;
            settings.quality=music?.75f:.85f;
            settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData=!music;
            importer.defaultSampleSettings=settings;importer.forceToMono=!music;
            importer.loadInBackground=music;
            importer.SaveAndReimport();
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if(!clip||clip.length<=0)throw new InvalidOperationException("Audio import failed: "+path);
            return music?clip:TrimForResponsivePlayback(clip,name);
        }
        static AudioClip TrimForResponsivePlayback(AudioClip source,string name)
        {
            source.LoadAudioData();var samples=new float[source.samples*source.channels];
            if(!source.GetData(samples,0))throw new InvalidOperationException("Cannot decode "+source.name);
            int first=Array.FindIndex(samples,s=>Mathf.Abs(s)>.003f),last=Array.FindLastIndex(samples,s=>Mathf.Abs(s)>.003f);
            if(first<0)throw new InvalidOperationException("Silent source: "+source.name);
            int start=Mathf.Max(0,first/source.channels-Mathf.RoundToInt(source.frequency*.008f));
            int end=Mathf.Min(source.samples,last/source.channels+Mathf.RoundToInt(source.frequency*.035f));
            int frames=end-start,channels=source.channels,bytes=frames*channels*2;
            string directory=Root+"Processed";Directory.CreateDirectory(directory);string path=directory+"/"+name+".wav";
            using(var writer=new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+bytes);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16);writer.Write((short)1);writer.Write((short)channels);writer.Write(source.frequency);writer.Write(source.frequency*channels*2);writer.Write((short)(channels*2));writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(bytes);
                float fadeFrames=Mathf.Max(1,source.frequency*.004f);
                for(int frame=0;frame<frames;frame++)
                {
                    float fade=Mathf.Min(1,Mathf.Min(frame/fadeFrames,(frames-1-frame)/fadeFrames));
                    for(int channel=0;channel<channels;channel++)writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(samples[(start+frame)*channels+channel]*fade,-1,1)*32767));
                }
            }
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;
            settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.preloadAudioData=true;
            settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.85f;
            importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.loadInBackground=false;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Author audio outside Play Mode.");
            var library=Resources.Load<WoolSoundLibrary>("WoolSoundLibrary");
            if(!library)throw new InvalidOperationException("Existing WoolSoundLibrary is required.");
            Undo.RecordObject(library,"Add Pixabay game audio");
            library.uiClick=Clip("UiClick");library.menuMusic=Clip("MenuHappyRelaxing",true);
            library.victory=Clip("Victory");library.defeat=Clip("Defeat");
            library.unlockSlot=Clip("AbilityUnlock");library.removeVehicle=Clip("AbilityRemove");
            library.sortColors=Clip("AbilitySort");library.pierceFog=Clip("AbilityFog");
            // The supplied background track remains the level music, with streaming for mobile memory.
            if(library.background)
            {
                var importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(library.background)) as AudioImporter;
                if(importer){var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.Streaming;settings.preloadAudioData=false;importer.defaultSampleSettings=settings;importer.loadInBackground=true;importer.SaveAndReimport();}
            }
            EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();
            var hud=UnityEngine.Object.FindFirstObjectByType<WoolHud>(FindObjectsInactive.Include);
            int count=0;
            if(hud)
            {
                foreach(var button in hud.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                {
                    if(!button.GetComponent<WoolButtonAudio>())Undo.AddComponent<WoolButtonAudio>(button.gameObject);
                    count++;
                }
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(hud.gameObject.scene);
            }
            var clips=new[]{library.uiClick,library.menuMusic,library.victory,library.defeat,library.unlockSlot,library.removeVehicle,library.sortColors,library.pierceFog};
            string report="Bound "+count+" authored buttons; original level music: "+AssetDatabase.GetAssetPath(library.background)+"\n"+
                string.Join("\n",clips.Select(c=>c.name+": "+c.length.ToString("F3")+"s; "+c.channels+" channel(s); "+c.loadType));
            Directory.CreateDirectory("Docs/AudioRelease20261025");File.WriteAllText("Docs/AudioRelease20261025/import-report.txt",report);
            return report;
        }
    }
}
