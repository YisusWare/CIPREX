using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// Configuración de pixel art (sección 2.2) para todo PNG nuevo dentro de Assets/_Project/Art:
    /// PPU 16, Filter Point, sin compresión, sin mipmaps y Sprite Mode Multiple.
    /// Solo actúa en la primera importación, así que después se puede cambiar a mano.
    /// Reemplaza al Preset por carpeta: hace lo mismo sin configurar el Preset Manager.
    /// También avisa si el spritesheet del personaje se cortó a mano (Sprite Editor → Slice): los
    /// clips dependen de los sprites que crea Game → Setup → 4. Personaje animado.
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
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
        }

        void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {
            if (PlayerArtSetup.IsSlicing || assetPath != PlayerArtSetup.SheetPath || sprites == null || sprites.Length == 0) return;

            // Los sprites que crea la herramienta se llaman como su clip (Torso_Run_0, Legs_Idle_2…).
            bool cutByTool = sprites.Any(s => s.name.StartsWith("Legs_") || s.name.StartsWith("Torso_") ||
                                              s.name.StartsWith("FullBody_"));
            if (cutByTool) return;

            Debug.LogError(
                $"[Personaje animado] {assetPath} se cortó a mano ({sprites.Length} sprites como '{sprites[0].name}'). " +
                "Los clips del personaje apuntan a los sprites que crea la herramienta, así que las animaciones se rompen. " +
                "Ejecuta Game → Setup → 4. Personaje animado para volver a cortarla. " +
                "No uses Slice en el Sprite Editor con esta hoja: para agregar clips, edita el SpritesheetLayout.");
        }
    }
}
