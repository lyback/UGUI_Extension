using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;

public class FindBase
{
    public delegate bool GameObjectDelegate(GameObject go);
    protected static void FindDic(string path, string suffixs, Action<string> action)
    {
        if (Directory.Exists(path))
        {
            string[] dirs = Directory.GetDirectories(path);
            foreach (string t in dirs)
            {
                FindDic(t, suffixs, action);
            }
        }
        string[] filePaths = Directory.GetFiles(path);
        foreach (var filePath in filePaths)
        {
            string[] arrSuffix = suffixs.Split(';');
            foreach (var suffix in arrSuffix)
            {
                if (filePath.ToLower().EndsWith(suffix.ToLower()))
                {
                    action(filePath);
                }
            }
        }
    }

    public static string GetFullPath(string relativelyPath)
    {
        string[] arrPath = Application.dataPath.Split('/');
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < arrPath.Length; i++)
        {
            if (i < arrPath.Length - 1)
            {
                sb.Append(arrPath[i] + "/");
            }
        }
        sb.Append(relativelyPath);
        return sb.ToString();
    }

    public static List<AssetObject> GetPrefabs(string rootPath)
    {
        List<AssetObject> list = new List<AssetObject>();
        FindDic(rootPath, ".prefab", delegate (string path)
        {
            var assetObj = AssetObject.Create<GameObject>(path);
            list.Add(assetObj);
        });
        return list;
    }

    public static List<AssetObject> GetSourceSprites(string rootPath, string[] banDic = null)
    {
        List<AssetObject> list = new List<AssetObject>();
        FindDic(rootPath, ".png", delegate (string path)
        {
            if (!InCludePath(path, banDic))
            {
                var assetObj = AssetObject.Create<Sprite>(path);
                list.Add(assetObj);
            }
        });
        return list;
    }
    public static List<AssetObject> GetSourceTexture(string rootPath, string[] banDic = null)
    {
        List<AssetObject> list = new List<AssetObject>();
        FindDic(rootPath, ".png", delegate (string path)
        {
            if (!InCludePath(path, banDic))
            {
                var assetObj = AssetObject.Create<Texture>(path);
                list.Add(assetObj);
            }
        });
        return list;
    }

    public static List<AssetObject> GetTextureImporters(string rootPath, string[] banDic = null)
    {
        List<AssetObject> list = new List<AssetObject>();
        FindDic(rootPath, ".png;.tga;.psd", delegate (string path)
        {
            if (!InCludePath(path, banDic))
            {
                var assetObj = AssetObject.Create<Texture>(path);
                list.Add(assetObj);
            }
        });
        return list;
    }

    public static List<AssetObject> GetFbxImporters(string rootPath)
    {
        List<AssetObject> list = new List<AssetObject>();
        FindDic(rootPath, ".fbx", delegate (string path)
        {
            var assetObj = AssetObject.Create<AssetImporter>(path);
            list.Add(assetObj);
        });
        return list;
    }
    public static bool InCludePath(string path, string dic)
    {
        return InCludePath(path, new[] { dic });
    }

    public static bool InCludePath(string path, string[] dics)
    {
        if (dics == null) return false;
        path = path.Replace("\\", "/").ToLower();
        foreach (var s in dics)
        {
            string dicPath = s.Replace("\\", "/").ToLower();
            if (path.IndexOf(string.Format("/{0}/", dicPath), StringComparison.Ordinal) != -1)
            {
                return true;
            }
        }
        return false;
    }
}
