using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class BackfranzAudioImport : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        const string folder = "Assets/Resources/Backfranz/Audio/";
        if(!assetPath.StartsWith(folder,StringComparison.Ordinal))return;
        string name=Path.GetFileName(assetPath);
        bool ambience=name.StartsWith("ambiente-",StringComparison.Ordinal)||
            name.StartsWith("musica-",StringComparison.Ordinal);
        var importer=(AudioImporter)assetImporter;
        var settings=importer.defaultSampleSettings;
        settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
        settings.loadType=ambience?AudioClipLoadType.CompressedInMemory:AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat=ambience?AudioCompressionFormat.Vorbis:AudioCompressionFormat.PCM;
        settings.quality=.84f;
        importer.defaultSampleSettings=settings;
        importer.forceToMono=false;
    }
}
