using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class TPAtlasHelper
{
    static string PATH_ATLAS = "Assets/Art/ui";
    static string PATH_TP_TEMP = "Assets/Temp/TP/ui";
    static string PATH_ATLAS_TP = "Assets/Data/ui/atlas_tp";
    public static void MakePolyAtlas(Object obj, bool isCompress, bool isSplitChannel)
    {
        string srcPath = AssetDatabase.GetAssetPath(obj);
        string name = Path.GetFileNameWithoutExtension(srcPath);

        int totalCount = 5;
        int current = 0;
        try
        {
            EditorUtility.DisplayProgressBar(name, "MAKE_POLY_ATLAS", (++current) / totalCount);
            TPAtlasPacker.MakeAtlas_Polygon(name, srcPath, PATH_TP_TEMP);

            EditorUtility.DisplayProgressBar(name, "SPLIT_CHANNEL_UI", (++current) / totalCount);
            TPAtlasPacker.SpiltChannel_UI(name, PATH_TP_TEMP, PATH_ATLAS_TP, isCompress, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "CREATE_MATERIAL_UI", (++current) / totalCount);
            TPAtlasPacker.CreateMaterial_UI(name, PATH_ATLAS_TP, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "IMPORT_SPRITE_INFO", (++current) / totalCount);
            TPAtlasPacker.ImportSpriteInfo(name, srcPath, PATH_ATLAS_TP, isCompress, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "CREATE_POLY_PREFAB", (++current) / totalCount);

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

        int totalCount = 5;
        int current = 0;
        try
        {
            EditorUtility.DisplayProgressBar(name, "MAKE_ATLAS_NONE", (++current) / totalCount);
            TPAtlasPacker.MakeAtlas_None(name, srcPath, PATH_TP_TEMP);

            EditorUtility.DisplayProgressBar(name, "SPLIT_CHANNEL_UI", (++current) / totalCount);
            TPAtlasPacker.SpiltChannel_UI(name, PATH_TP_TEMP, PATH_ATLAS_TP, isCompress, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "CREATE_MATERIAL_UI", (++current) / totalCount);
            TPAtlasPacker.CreateMaterial_UI(name, PATH_ATLAS_TP, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "IMPORT_SPRITE_INFO", (++current) / totalCount);
            TPAtlasPacker.ImportSpriteInfo(name, srcPath, PATH_ATLAS_TP, isCompress, isSplitChannel);

            EditorUtility.DisplayProgressBar(name, "CREATE_POLY_PREFAB", (++current) / totalCount);

            EditorUtility.ClearProgressBar();
        }
        catch (System.Exception)
        {
            EditorUtility.ClearProgressBar();
            throw;
        }
    }
}