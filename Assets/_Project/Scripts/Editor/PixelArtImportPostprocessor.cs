using UnityEditor;

namespace Game.EditorTools
{
    /// <summary>
    /// Configuración de pixel art (sección 2.2) para todo PNG nuevo dentro de Assets/_Project/Art:
    /// PPU 16, Filter Point, sin compresión, sin mipmaps y Sprite Mode Multiple.
    /// Solo actúa en la primera importación, así que después se puede cambiar a mano.
    /// Reemplaza al Preset por carpeta: hace lo mismo sin configurar el Preset Manager.
    /// </summary>
    public class PixelArtImportPostprocessor : AssetPostprocessor
    {
        const string ArtFolder = "Assets/_Project/Art/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder)) return;
            if (!assetImporter.importSettingsMissing) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = UnityEngine.FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
        }
    }
}
