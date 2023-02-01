using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        //textureImporter.textureType = TextureImporterType.Sprite;
        //textureImporter.textureShape = TextureImporterShape.Texture2D;
        textureImporter.spritePixelsPerUnit = 1;
        textureImporter.filterMode = FilterMode.Point;
        //textureImporter.sRGBTexture = true;
        //textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
    }
}
