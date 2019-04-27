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
        string args = "\"--max-size 2048 --force-squared --size-constraints POT --disable-rotation --trim-mode CropKeepPos --algorithm MaxRects --extrude 1 --border-padding 0 --shape-padding 0 --multipack\"";
        MakeAtlas(name, pathSrc, pathDst, args);
    }
    public static void MakeAtlas_Polygon(string name, string pathSrc, string pathDst)
    {
        string args = "\"--max-size 2048 --force-squared --size-constraints POT --disable-rotation --trim-mode Polygon --algorithm Polygon --extrude 1 --border-padding 0 --shape-padding 0 --multipack\"";
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
        RewriteSpriteMetaData(sprites, pathSrc);
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
    private static void RewriteSpriteMetaData(List<SpriteMetaData> sprites, string pathSrc)
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
    #region 创建图集Asset
    public static void CreateSpriteAsset(string name, string pathScr, string pathDst)
    {
        string pathAsset = string.Format("{0}/{1}.asset", pathDst, name);
        string pathColor = string.Format("{0}/{1}.png", pathDst, name);
        string pathMaterial = string.Format("{0}/{1}_mat.mat", pathDst, name);

        var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(pathColor);
        var mat = AssetDatabase.LoadAssetAtPath<Material>(pathMaterial);
        RewriteAtlasInfoAsset(sprites, pathScr, pathAsset, mat);
        RewriteAtlasMapAsset(name, sprites);
    }
    private static void RewriteAtlasInfoAsset(Object[] sprites, string pathSrc, string pathAsset, Material mat)
    {
        var atlasInfo = ScriptableObject.CreateInstance<AtlasInfo>();
        atlasInfo.m_Mat = mat;
        List<SpriteInfo> spriteInfo = new List<SpriteInfo>();
        var assetObjects = FindBase.GetSourceSprites(pathSrc);
        for (int i = 0; i < sprites.Length; ++i)
        {
            Sprite packSprite = sprites[i] as Sprite;
            for (int j = 0; j < assetObjects.Count; j++)
            {
                Sprite srcSprite = assetObjects[j].sprite;
                if (Path.GetFileNameWithoutExtension(assetObjects[j].name) == packSprite.name)
                {
                    var src_Padding = UnityEngine.Sprites.DataUtility.GetPadding(srcSprite);

                    float pack_PivotLeft = packSprite.pivot.x;
                    float pack_PivotRight = packSprite.rect.width - pack_PivotLeft;
                    float pack_PivotBottom = packSprite.pivot.y;
                    float pack_PivotTop = packSprite.rect.height - pack_PivotBottom;

                    float src_PivotLeft = srcSprite.pivot.x - src_Padding.x;
                    float src_PivotRight = (srcSprite.rect.width - src_Padding.x - src_Padding.z) - src_PivotLeft;
                    float src_PivotBottom = srcSprite.pivot.y - src_Padding.y;
                    float src_PivotTop = (srcSprite.rect.height - src_Padding.y - src_Padding.w) - src_PivotBottom;

                    float d_PivotLeft = pack_PivotLeft - src_PivotLeft;
                    float d_PivotRight = pack_PivotRight - src_PivotRight;
                    float d_PivotTop = pack_PivotTop - src_PivotTop;
                    float d_PivotBottom = pack_PivotBottom - src_PivotBottom;

                    var padding = new Vector4(src_Padding.x - d_PivotLeft, src_Padding.y - d_PivotBottom, src_Padding.z - d_PivotRight, src_Padding.w - d_PivotTop);

                    spriteInfo.Add(new SpriteInfo(sprites[i] as Sprite, padding));
                    break;
                }
            }
        }
        atlasInfo.SetSpriteInfoList(spriteInfo);
        AssetDatabase.CreateAsset(atlasInfo, pathAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return;
    }
    private static void RewriteAtlasMapAsset(string name, Object[] sprites)
    {
        Atlas_SpriteNameList atlas_SpriteName = new Atlas_SpriteNameList();
        atlas_SpriteName.name = name;
        atlas_SpriteName.spriteNameList = new List<string>();
        for (int i = 0; i < sprites.Length; i++)
        {
            atlas_SpriteName.spriteNameList.Add(sprites[i].name);
        }
        string pathAtlasMap = string.Format("{0}/{1}.asset", UIConfig.PATH_ATLAS_TP, UIConfig.ATLAS_MAP_NAME);
        var atlasMap = AssetDatabase.LoadAssetAtPath<AtlasMap>(pathAtlasMap);
        if (atlasMap == null)
        {
            atlasMap = ScriptableObject.CreateInstance<AtlasMap>();
            AssetDatabase.CreateAsset(atlasMap, pathAtlasMap);
            AssetDatabase.Refresh();
        }
        atlasMap.AddAtlas_SpriteName(atlas_SpriteName);
        EditorUtility.SetDirty(atlasMap);
        AssetDatabase.SaveAssets();
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
