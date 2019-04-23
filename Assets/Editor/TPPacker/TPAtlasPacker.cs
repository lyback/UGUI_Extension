using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using TPImporter;

public class TPAtlasPacker
{
    public static SpritesheetCollection spriteSheets = new SpritesheetCollection();
    #region 打TP图集
    static void MakeAtlas(string name, string pathSrc, string pathDst, string args)
    {
        pathSrc = string.Format("\"{0}\"", pathSrc);
        string fullPathDst = string.Format("\"{0}/{1}\"", pathDst, name);
        string TexturePackerBatPath = GetTexturePackerBatPath("maker");
        if (string.IsNullOrEmpty(TexturePackerBatPath))
        {
            return;
        }
        args = string.Format("{0} {1} {2}", pathSrc, fullPathDst, args);
        string error = CMDHelper.ProcessCommand(TexturePackerBatPath, args);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError(string.Format("Make Atlas Error. name:{0}, msg:{1}", name, error));
        }
        AssetDatabase.Refresh();
    }
    public static void MakeAtlas_None(string name, string pathSrc, string pathDst)
    {
        string args = "\"--max-size 2048 --force-squared --size-constraints POT --disable-rotation --trim-mode None --algorithm MaxRects --extrude 1 --border-padding 0 --shape-padding 0\"";
        MakeAtlas(name, pathSrc, pathDst, args);
    }
    public static void MakeAtlas_Polygon(string name, string pathSrc, string pathDst)
    {
        string args = "\"--max-size 2048 --force-squared --size-constraints POT --disable-rotation --trim-mode Polygon --algorithm Polygon --extrude 1 --border-padding 0 --shape-padding 0\"";
        MakeAtlas(name, pathSrc, pathDst, args);
    }
    #endregion
    #region 剥离透明通道
    public static void SplitChannel(string name, string pathSrc, string pathDst, bool isCompress, bool isSplitChannel)
    {
        string path = string.Format("{0}/{1}.png", pathSrc, name);
        string pathColor = string.Format("{0}/{1}.png", pathDst, name);
        string pathAlpha = string.Format("{0}/{1}_a.png", pathDst, name);
        if (!Directory.Exists(pathDst))
        {
            Directory.CreateDirectory(pathDst);
        }
        Texture2D srcTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        TextureImporter srcImportTex = (TextureImporter)AssetImporter.GetAtPath(path);
        srcImportTex.isReadable = true;
        srcImportTex.alphaSource = TextureImporterAlphaSource.FromInput;
        srcImportTex.SaveAndReimport();
        if (isSplitChannel)
        {
            Texture2D textureColor = new Texture2D(srcTex.width, srcTex.height);
            Texture2D textureAlpha = new Texture2D(srcTex.width, srcTex.height);

            for (int i = 0; i < srcTex.width; ++i)
            {
                for (int j = 0; j < srcTex.height; ++j)
                {
                    Color color = srcTex.GetPixel(i, j);
                    textureAlpha.SetPixel(i, j, new Color(color.a, color.a, color.a));
                    textureColor.SetPixel(i, j, new Color(color.r, color.g, color.b));
                }
            }

            File.WriteAllBytes(pathColor, textureColor.EncodeToPNG());
            File.WriteAllBytes(pathAlpha, textureAlpha.EncodeToPNG());
        }
        else
        {
            Texture2D textureColor = new Texture2D(srcTex.width, srcTex.height);

            for (int i = 0; i < srcTex.width; ++i)
            {
                for (int j = 0; j < srcTex.height; ++j)
                {
                    Color color = srcTex.GetPixel(i, j);
                    textureColor.SetPixel(i, j, new Color(color.r, color.g, color.b, color.a));
                }
            }

            File.WriteAllBytes(pathColor, textureColor.EncodeToPNG());
            if (File.Exists(pathAlpha))
            {
                File.Delete(pathAlpha);
            }
        }
        AssetDatabase.Refresh();
        if (isSplitChannel)
        {
            TextureImporter importAlpha = (TextureImporter)AssetImporter.GetAtPath(pathAlpha);
            importAlpha.textureType = TextureImporterType.Default;
            importAlpha.mipmapEnabled = false;
            importAlpha.isReadable = false;
            importAlpha.alphaSource = TextureImporterAlphaSource.None;
            importAlpha.textureCompression = isCompress ? TextureImporterCompression.Compressed : TextureImporterCompression.Uncompressed;
            int scale = 1; //todo:配置透明通道压缩比例
            importAlpha.maxTextureSize = (srcTex.width > srcTex.height ? srcTex.width : srcTex.height) / scale;
            importAlpha.filterMode = FilterMode.Bilinear;
            importAlpha.SaveAndReimport();
        }
        else
        {
            TextureImporter importColor = (TextureImporter)AssetImporter.GetAtPath(pathColor);
            importColor.textureType = TextureImporterType.Default;
            importColor.mipmapEnabled = false;
            importColor.isReadable = false;
            importColor.alphaSource = TextureImporterAlphaSource.FromInput;
            importColor.textureCompression = isCompress ? TextureImporterCompression.Compressed : TextureImporterCompression.Uncompressed;
            importColor.maxTextureSize = srcTex.width > srcTex.height ? srcTex.width : srcTex.height;
            importColor.filterMode = FilterMode.Bilinear;
            importColor.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }
    public static void SpiltChannel_UI(string name, string pathSrc, string pathDst, bool isCompress, bool isSplitChannel)
    {
        string pathSrcTpSheet = string.Format("{0}/{1}.tpsheet", pathSrc, name);
        string pathTpSheet = string.Format("{0}/{1}.tpsheet", pathDst, name);
        File.Copy(pathSrcTpSheet, pathTpSheet, true);
        SplitChannel(name, pathSrc, pathDst, isCompress, isSplitChannel);
        File.Delete(pathTpSheet);
    }
    #endregion
    #region 创建材质球
    public static void CreateMaterial(string name, string pathDst, string shaderName, string[] shaderProName)
    {
        string pathColor = string.Format("{0}/{1}.png", pathDst, name);
        string pathAlpha = string.Format("{0}/{1}_a.png", pathDst, name);
        string pathMaterial = string.Format("{0}/{1}_mat.mat", pathDst, name);

        Texture2D textureColor = AssetDatabase.LoadAssetAtPath<Texture2D>(pathColor);
        Texture2D textureAlpha = AssetDatabase.LoadAssetAtPath<Texture2D>(pathAlpha);

        Shader shader = Shader.Find(shaderName);
        Material material = null;
        if (File.Exists(pathMaterial))
        {
            material = AssetDatabase.LoadAssetAtPath<Material>(pathMaterial);
            material.shader = shader;
        }
        else
        {
            material = new Material(shader);
        }

        if (shaderProName[0] != null)
        {
            material.SetTexture(shaderProName[0], textureColor);
        }
        if (shaderProName[1] != null)
        {
            material.SetTexture(shaderProName[1], textureAlpha);
        }

        if (File.Exists(pathMaterial))
        {
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }
        else
        {
            AssetDatabase.CreateAsset(material, pathMaterial);
        }
    }
    public static void CreateMaterial_UI(string name, string pathDst, bool isSplitChannel)
    {
        string shaderName = "UI/Default(RGB+A)";
        string[] shaderProName = new string[2];
        if (isSplitChannel)
        {
            shaderProName[0] = "_MainTex";
            shaderProName[1] = "_AlphaTex";
        }
        else
        {
            shaderProName[0] = "_MainTex";
        }
        CreateMaterial(name, pathDst, shaderName, shaderProName);
    }
    #endregion
    #region 导入sprite信息
    public static void ImportSpriteInfo(string name, string pathSrc, string pathDst, bool isCompress, bool isSplitChannel)
    {
        string pathTexture = string.Format("{0}/{1}.png", pathDst, name);
        TextureImporter importer = AssetImporter.GetAtPath(pathTexture) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError(pathTexture + ":is null");
            return;
        }
        List<SpriteMetaData> sprites = new List<SpriteMetaData>(importer.spritesheet);
        RewriteSpriteInfo(sprites, pathSrc);
        importer.spritesheet = sprites.ToArray();
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.sRGBTexture = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.spritePackingTag = null;
        importer.mipmapEnabled = false;
        importer.isReadable = false;
        importer.maxTextureSize = 2048;
        if (isCompress)
            importer.textureCompression = TextureImporterCompression.Compressed;
        else
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        if (isSplitChannel)
            importer.alphaSource = TextureImporterAlphaSource.None;
        else
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.SaveAndReimport();
        AssetDatabase.ImportAsset(pathTexture, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.Refresh();
    }
    private static void RewriteSpriteInfo(List<SpriteMetaData> sprites, string pathSrc)
    {
        var assetObjects = FindBase.GetSourceTexture(pathSrc);
        for (int i = 0; i < sprites.Count; ++i)
        {
            SpriteMetaData data = sprites[i];
            for (int j = 0; j < assetObjects.Count; j++)
            {
                if (Path.GetFileNameWithoutExtension(assetObjects[j].name) == sprites[i].name)
                {
                    data.border = assetObjects[j].textureImporter.spriteBorder;
                    if (data.alignment != 9)
                    {
                        data.pivot = assetObjects[j].textureImporter.spritePivot;
                    }
                    sprites[i] = data;
                    break;
                }
            }
        }
    }
    #endregion
    #region 创建图集Prefab
    static void CreatePolySpritePrefab(string name, string pathScr, string pathDst, string pathTP)
    {
        string pathPrefab = string.Format("{0}/{1}.prefab", pathDst, name);
        string pathColor = string.Format("{0}/{1}.png", pathDst, name);
        string pathMaterial = string.Format("{0}/{1}_mat.mat", pathDst, name);
        string pathTpScr = string.Format("{0}/{1}.png", pathTP, name);

    }
    #endregion
    static string GetTexturePackerBatPath(string name)
    {
        string stuffix;
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
                stuffix = ".sh";
                break;
            case RuntimePlatform.WindowsEditor:
                stuffix = ".bat";
                break;
            default:
                Debug.LogError("错误平台");
                return "";
        }
        string fullpath = FindBase.GetFullPath("Tools/TexturePacker/" + name + stuffix);
        return fullpath;
    }

}
