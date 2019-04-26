using UnityEditor;

public class UIMenuItem {
	[MenuItem("Assets/UI/打压缩Polygon图集", false, 1)]
    static void ExportCompressAtlas_Polygon()
    {
        EditorHelper.ExcuteAssetOperat(obj =>
        {
            bool isCompress = true;
            bool isSplitChannel = true;
            TPAtlasHelper.MakePolyAtlas(obj, isCompress, isSplitChannel);
        }, SelectionMode.Assets);
    }
    [MenuItem("Assets/UI/打压缩Nor图集", false, 2)]
    static void ExportCompressAtlas_None()
    {
        EditorHelper.ExcuteAssetOperat(obj =>
        {
            bool isCompress = true;
            bool isSplitChannel = true;
            TPAtlasHelper.MakeNoneAtlas(obj, isCompress, isSplitChannel);
        }, SelectionMode.Assets);
    }
}
