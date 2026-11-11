using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
namespace WoolGame.Editor
{
    // Native build completion produces the unsigned deliverable without another agent step.
    public sealed class WoolUnsignedApk:IPostprocessBuildWithReport
    {
        public int callbackOrder=>1000000;
        public static string Prepare()
        {
            SessionState.SetBool("WoolUnsignedRequested",true);
            SessionState.SetBool("WoolOriginalCustomSigning",PlayerSettings.Android.useCustomKeystore);
            PlayerSettings.Android.useCustomKeystore=false;
            EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
            return "Unsigned APK postprocessing armed";
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            if(!SessionState.GetBool("WoolUnsignedRequested",false)||report.summary.platform!=BuildTarget.Android)return;
            string output=Path.GetFullPath(report.summary.outputPath),temporary=output+".unsigned-temp.apk";
            try
            {
                using(var input=ZipFile.OpenRead(output))
                using(var stream=File.Create(temporary))
                using(var unsigned=new ZipArchive(stream,ZipArchiveMode.Create))
                {
                    foreach(var entry in input.Entries)
                    {
                        string name=entry.FullName.ToUpperInvariant();
                        if(name.StartsWith("META-INF/")&&(name.EndsWith(".RSA")||name.EndsWith(".SF")||name.EndsWith(".DSA")||name.EndsWith(".EC")||name=="META-INF/MANIFEST.MF"))continue;
                        var copy=unsigned.CreateEntry(entry.FullName,entry.CompressedLength==entry.Length?CompressionLevel.NoCompression:CompressionLevel.Optimal);
                        copy.LastWriteTime=entry.LastWriteTime;
                        using(var source=entry.Open())using(var destination=copy.Open())source.CopyTo(destination);
                    }
                }
                string sdk=Path.Combine(EditorApplication.applicationContentsPath,"PlaybackEngines/AndroidPlayer/SDK/build-tools");
                string tool=Directory.GetDirectories(sdk).OrderByDescending(p=>p).Select(p=>Path.Combine(p,"zipalign.exe")).First(File.Exists);
                var start=new System.Diagnostics.ProcessStartInfo(tool,"-P 16 -f 4 \""+temporary+"\" \""+output+"\"")
                {UseShellExecute=false,CreateNoWindow=true,WindowStyle=System.Diagnostics.ProcessWindowStyle.Hidden,RedirectStandardError=true};
                using(var process=System.Diagnostics.Process.Start(start)){string error=process.StandardError.ReadToEnd();process.WaitForExit();if(process.ExitCode!=0)throw new BuildFailedException("APK alignment failed: "+error);}
                using(var archive=ZipFile.OpenRead(output)){if(archive.GetEntry("AndroidManifest.xml")==null)throw new BuildFailedException("APK manifest missing");}
                File.Delete(temporary);Directory.CreateDirectory("Builds/QA");
                File.WriteAllText("Builds/QA/unsigned-apk-result.txt","SUCCESS unsigned, 16KB native-library alignment\n"+output+"\nBytes="+new FileInfo(output).Length);
                UnityEngine.Debug.Log("Unsigned APK completed: "+output);
            }
            finally
            {
                PlayerSettings.Android.useCustomKeystore=SessionState.GetBool("WoolOriginalCustomSigning",false);
                SessionState.SetBool("WoolUnsignedRequested",false);
            }
        }
    }
}
