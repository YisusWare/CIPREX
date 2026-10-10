using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Configuración de importación de audio (sección 15.5), solo en la primera importación:
    /// - Assets/_Project/Audio/SFX/   → Decompress On Load, Force To Mono.
    /// - Assets/_Project/Audio/Music/ → Streaming, sin Preload Audio Data.
    /// </summary>
    public class AudioImportPostprocessor : AssetPostprocessor
    {
        const string SfxFolder = "Assets/_Project/Audio/SFX/";
        const string MusicFolder = "Assets/_Project/Audio/Music/";

        void OnPreprocessAudio()
        {
            if (!assetImporter.importSettingsMissing) return;
            var importer = (AudioImporter)assetImporter;

            if (assetPath.StartsWith(SfxFolder))
            {
                importer.forceToMono = true;
                var settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                importer.defaultSampleSettings = settings;
            }
            else if (assetPath.StartsWith(MusicFolder))
            {
                var settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.Streaming;
                settings.preloadAudioData = false;
                importer.defaultSampleSettings = settings;
            }
        }
    }
}
