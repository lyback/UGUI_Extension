using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;

public class AssetObject
{
    public string path;
    public string name;
    public GameObject gameObject;
    public Texture texture;
    public TextureImporter textureImporter;
    public Sprite sprite;
    public AssetImporter fbxImporter;
    private AssetObject() { }

    public static AssetObject Create<T>(string path)
    {
        var assetObject = new AssetObject();
        assetObject.path = path.Replace("\\", "/");
        string[] pathSplit = assetObject.path.Split('/');
        assetObject.name = pathSplit[pathSplit.Length - 1];
        if (typeof(T) == typeof(GameObject))
        {
            assetObject.gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        if (typeof(T) == typeof(Texture))
        {
            assetObject.textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            assetObject.texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
        }
        if (typeof(T) == typeof(Sprite))
        {
            assetObject.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        if (typeof(T) == typeof(AssetImporter))
        {
            assetObject.fbxImporter = AssetImporter.GetAtPath(path);
        }
        return assetObject;
    }
}
