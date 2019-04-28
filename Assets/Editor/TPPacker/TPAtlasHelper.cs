using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class TPAtlasHelper
{
    public const string PATH_ATLAS = UIConfig.PATH_ATLAS;
    public const string PATH_TP_TEMP = UIConfig.PATH_TP_TEMP;
    public const string PATH_ATLAS_TP = UIConfig.PATH_ATLAS_TP;
    public static void MakePolyAtlas(Object obj, bool isCompress, bool isSplitChannel)
    {
        string srcPath = AssetDatabase.GetAssetPath(obj);
        string name = Path.GetFileNameWithoutExtension(srcPath);
        string pathAtlasTP = string.Format("{0}/{1}",PATH_ATLAS_TP,name);
        if(!Directory.Exists(pathAtlasTP)){
            Directory.CreateDirectory(pathAtlasTP);
        }
        int totalCount = 5;
        int current = 0;
        try
        {
            EditorUtility.DisplayProgressBar(name, "MAKE_POLY_ATLAS", (++current) / totalCount);
            TPAtlasPacker.MakeAtlas_Polygon(name, srcPath, PATH_TP_TEMP);

            EditorUtility.DisplayProgressBar(name, "SPLIT_CHANNEL_UI", (++current) / totalCount);
            TPAtlasPacker.SpiltChannel_UI(name, PATH_TP_TEMP, pathAtlasTP, isCompress, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "CREATE_MATERIAL_UI", (++current) / totalCount);
            TPAtlasPacker.CreateMaterial_UI(name, pathAtlasTP, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "IMPORT_SPRITE_INFO", (++current) / totalCount);
            TPAtlasPacker.ImportSpriteInfo(name, srcPath, pathAtlasTP, isCompress, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "CREATE_ATLAS_ASSET", (++current) / totalCount);
            TPAtlasPacker.CreateSpriteAsset(name, true, srcPath, pathAtlasTP);
            EditorUtility.ClearProgressBar();
        }
        catch (System.Exception)
        {
            EditorUtility.ClearProgressBar();
            throw;
        }
    }
    public static void MakeNoneAtlas(Object obj, bool isCompress, bool isSplitChannel)
    {
        string srcPath = AssetDatabase.GetAssetPath(obj);
        string name = Path.GetFileNameWithoutExtension(srcPath);
        string pathAtlasTP = string.Format("{0}/{1}",PATH_ATLAS_TP,name);
        if(!Directory.Exists(pathAtlasTP)){
            Directory.CreateDirectory(pathAtlasTP);
        }       
        int totalCount = 5;
        int current = 0;
        try
        {
            EditorUtility.DisplayProgressBar(name, "MAKE_ATLAS_NONE", (++current) / totalCount);
            TPAtlasPacker.MakeAtlas_None(name, srcPath, PATH_TP_TEMP);

            EditorUtility.DisplayProgressBar(name, "SPLIT_CHANNEL_UI", (++current) / totalCount);
            TPAtlasPacker.SpiltChannel_UI(name, PATH_TP_TEMP, pathAtlasTP, isCompress, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "CREATE_MATERIAL_UI", (++current) / totalCount);
            TPAtlasPacker.CreateMaterial_UI(name, pathAtlasTP, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "IMPORT_SPRITE_INFO", (++current) / totalCount);
            TPAtlasPacker.ImportSpriteInfo(name, srcPath, pathAtlasTP, isCompress, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "CREATE_ATLAS_ASSET", (++current) / totalCount);
            TPAtlasPacker.CreateSpriteAsset(name, false, srcPath, pathAtlasTP);
            EditorUtility.ClearProgressBar();
        }
        catch (System.Exception)
        {
            EditorUtility.ClearProgressBar();
            throw;
        }
    }
}