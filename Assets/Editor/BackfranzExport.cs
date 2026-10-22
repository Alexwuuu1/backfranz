using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BackfranzExport
{
    static readonly string[] Scenes={"Assets/Scenes/MenuPrincipal.unity","Assets/Scenes/Nivel01.unity"};
    [MenuItem("Tools/Backfranz/Exportar/Windows x64")]
    public static void Windows(){Build(BuildTarget.StandaloneWindows64,"../Builds/Backfranz-Jugable/Backfranz.exe");}
    [MenuItem("Tools/Backfranz/Exportar/Android APK")]
    public static void Android(){Build(BuildTarget.Android,"../Builds/Android/Backfranz.apk");}
    [MenuItem("Tools/Backfranz/Exportar/Verificar soporte Android")]
    public static void VerifyAndroid()
    {
        if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
            throw new System.InvalidOperationException("Unity no reconoce Android Build Support.");
        string module=Path.Combine(EditorApplication.applicationContentsPath,"PlaybackEngines/AndroidPlayer");
        foreach(string file in new[]{"OpenJDK/bin/java.exe","SDK/platform-tools/adb.exe","SDK/platforms/android-36/android.jar","SDK/build-tools/36.0.0/aapt2.exe","SDK/cmdline-tools/16.0/bin/sdkmanager.bat","NDK/source.properties"})
            if(!File.Exists(Path.Combine(module,file)))throw new FileNotFoundException("Dependencia Android incompleta: "+file);
        Debug.Log("BACKFRANZ_ANDROID_SUPPORT_OK: módulo, JDK, SDK y NDK disponibles. APK y prueba en dispositivo pendientes.");
    }
    static void Build(BuildTarget target,string path)
    {
        var group=BuildPipeline.GetBuildTargetGroup(target);
        if(!BuildPipeline.IsBuildTargetSupported(group,target))throw new System.InvalidOperationException("Instalar el módulo de la plataforma en Unity Hub, con SDK/NDK/OpenJDK para Android.");
        PlayerSettings.productName="Backfranz";PlayerSettings.companyName="Grupo 64";
        PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.resizableWindow=true;
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"com.grupo64.backfranz");
        PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
        EditorUserBuildSettings.buildAppBundle=false;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=Scenes,locationPathName=path,target=target,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Exportación fallida: "+report.summary.result);
        Debug.Log("BACKFRANZ_BUILD_OK: "+Path.GetFullPath(path));
    }
}
