using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace WoolGame.Editor
{
    // Original synthesized toy/percussion sounds; no external sound downloads.
    public static class WoolRescueAudioAuthoring
    {
        const int Rate=22050;
        const string Root="Assets/WoolGame/Resources/WoolAudio/Rescue/";
        static float[] New(float seconds)=>new float[(int)(seconds*Rate)];
        static void Note(float[] a,float at,float duration,float frequency,float gain)
        {
            int start=(int)(at*Rate),count=(int)(duration*Rate);
            for(int j=0;j<count&&start+j<a.Length;j++)
            {
                double t=(double)j/Rate;
                double env=Math.Min(1,t/.006)*Math.Exp(-t*7/duration)*Math.Min(1,(duration-t)/.015);
                a[start+j]+=(float)((Math.Sin(2*Math.PI*frequency*t)+.22*Math.Sin(4*Math.PI*frequency*t))*env*gain);
            }
        }
        static void Sweep(float[] a,float at,float duration,float from,float to,float gain,bool noisy=false)
        {
            int start=(int)(at*Rate),count=(int)(duration*Rate);double phase=0,filtered=0;
            var random=new System.Random(42);
            for(int j=0;j<count&&start+j<a.Length;j++)
            {
                double t=(double)j/count,frequency=from+(to-from)*t;
                phase+=2*Math.PI*frequency/Rate;
                filtered=.75*filtered+.25*(random.NextDouble()*2-1);
                double signal=noisy?filtered:Math.Sin(phase)+.18*Math.Sin(phase*2);
                double env=Math.Sin(Math.PI*t)*Math.Exp(-t*2);
                a[start+j]+=(float)(signal*env*gain);
            }
        }
        static AudioClip Save(string name,float[] a)
        {
            Directory.CreateDirectory(Root);
            string path=Root+name+".wav";
            using(var w=new BinaryWriter(File.Create(path)))
            {
                w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+a.Length*2);
                w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);
                w.Write(Rate);w.Write(Rate*2);w.Write((short)2);w.Write((short)16);
                w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(a.Length*2);
                for(int i=0;i<a.Length;i++)
                {
                    float edge=Mathf.Min(1,Mathf.Min(i/100f,(a.Length-1-i)/100f));
                    w.Write((short)(Mathf.Clamp(a[i]*edge,-.85f,.85f)*32767));
                }
            }
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;
            settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;
            settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;settings.preloadAudioData=true;
            importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.loadInBackground=false;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        public static string Apply()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var lib=Resources.Load<WoolSoundLibrary>("WoolSoundLibrary");
            if(!lib)throw new InvalidOperationException("Missing sound library");
            var backup="Docs/Backups/20261027_RescueAudio";Directory.CreateDirectory(backup);
            var libraryPath=AssetDatabase.GetAssetPath(lib);
            if(!File.Exists(backup+"/WoolSoundLibrary.asset.txt"))File.Copy(libraryPath,backup+"/WoolSoundLibrary.asset.txt");
            Undo.RecordObject(lib,"Apply rescue sound effects");
            var a=New(.10f);Note(a,0,.09f,840,.34f);lib.uiClick=Save("Click",a);
            a=New(.19f);Note(a,0,.16f,180,.40f);Note(a,.035f,.10f,135,.14f);Save("Blocked",a);
            a=New(.20f);Note(a,0,.16f,460,.26f);Note(a,.045f,.13f,690,.22f);Save("Dock",a);
            a=New(.17f);Sweep(a,0,.15f,1400,380,.55f,true);Save("Shot",a);
            a=New(.15f);Sweep(a,0,.11f,620,150,.38f);Note(a,.012f,.11f,1100,.10f);Save("ImpactPop",a);
            a=New(.22f);Sweep(a,0,.19f,300,700,.30f,true);Save("Move",a);
            a=New(.55f);Note(a,0,.28f,523.25f,.28f);Note(a,.10f,.28f,659.25f,.26f);Note(a,.20f,.30f,783.99f,.25f);Save("Start",a);
            a=New(.55f);Note(a,0,.23f,1046.5f,.29f);Note(a,.09f,.30f,1568,.23f);Note(a,.20f,.30f,2093,.13f);lib.coin=Save("Coin",a);
            a=New(.65f);Note(a,0,.25f,392,.27f);Note(a,.12f,.25f,523.25f,.27f);Note(a,.25f,.35f,784,.24f);lib.unlockSlot=Save("UnlockSlot",a);
            a=New(.35f);Sweep(a,0,.25f,900,200,.55f,true);Note(a,.17f,.15f,660,.20f);lib.removeVehicle=Save("RemoveVehicle",a);
            a=New(.70f);float[] notes={523.25f,659.25f,784,1046.5f};
            for(int i=0;i<notes.Length;i++)Note(a,i*.10f,.24f,notes[i],.24f);
            lib.sortColors=Save("SortColors",a);
            a=New(.80f);Sweep(a,0,.6f,200,1300,.43f,true);Note(a,.20f,.4f,1174.66f,.20f);Note(a,.32f,.40f,1568,.15f);lib.pierceFog=Save("PierceFog",a);
            lib.bonus=lib.unlockSlot;
            a=New(1.65f);Note(a,0,.45f,523.25f,.25f);Note(a,.15f,.45f,659.25f,.25f);Note(a,.30f,.45f,784,.25f);
            Note(a,.55f,.75f,1046.5f,.23f);Note(a,.55f,.75f,659.25f,.15f);
            Sweep(a,1.05f,.18f,650,390,.28f);Sweep(a,1.29f,.20f,760,440,.25f);
            lib.victory=Save("Win",a);
            a=New(1.05f);Note(a,0,.35f,392,.24f);Note(a,.18f,.35f,329.63f,.22f);Note(a,.40f,.55f,261.63f,.23f);lib.defeat=Save("Lose",a);
            EditorUtility.SetDirty(lib);AssetDatabase.SaveAssets();
            return "14 short original sound assets generated and library assigned; existing music and fire retained.";
        }
    }
}