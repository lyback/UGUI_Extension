using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AtlasManager
{
    static AtlasManager ms_Instance;
    public static AtlasManager Instance
    {
        get
        {
            if (ms_Instance == null)
            {
                ms_Instance = new AtlasManager();
            }
#if UNITY_EDITOR
            Init();
#endif
            return ms_Instance;
        }
    }
    static AtlasMap m_AtlasMap;
    static Dictionary<string, AtlasInfo> m_AtlasInfoDic;
    static Dictionary<string, Vector4> m_PaddingDic;
    static Dictionary<string, bool> m_IsPolyDic;

    static void Init()
    {
        m_AtlasInfoDic = new Dictionary<string, AtlasInfo>();
        m_PaddingDic = new Dictionary<string, Vector4>();
        m_IsPolyDic = new Dictionary<string, bool>();
        string pathAtlasMap = string.Format("{0}/{1}.asset", UIConfig.PATH_ATLAS_TP, UIConfig.ATLAS_MAP_NAME);
        //加载,todo:多平台加载
        m_AtlasMap = AssetDatabase.LoadAssetAtPath<AtlasMap>(pathAtlasMap);
        m_AtlasMap.Init();
    }
    public bool GetIsPolyAtlas(string spriteName)
    {
        bool isPoly;
        if (m_IsPolyDic.TryGetValue(spriteName, out isPoly))
        {
            return isPoly;
        }
        string atlasName = m_AtlasMap.GetAtlasNameBySpriteName(spriteName);
        AtlasInfo atlasInfo = GetAtlasInfo(atlasName);
        isPoly = atlasInfo.m_IsPoly;
        m_IsPolyDic.Add(spriteName, isPoly);
        return isPoly;
    }
    public Vector4 GetPadding(string spriteName)
    {
        Vector4 padding = Vector4.zero;
        if (m_PaddingDic.TryGetValue(spriteName, out padding))
        {
            return padding;
        }
        string atlasName = m_AtlasMap.GetAtlasNameBySpriteName(spriteName);
        AtlasInfo atlasInfo = GetAtlasInfo(atlasName);
        padding = atlasInfo.GetPadding(spriteName);
        m_PaddingDic.Add(spriteName, padding);
        return padding;
    }
    public AtlasInfo GetAtlasInfo(string atlasName)
    {
        AtlasInfo atlasInfo;
        if (!m_AtlasInfoDic.TryGetValue(atlasName, out atlasInfo))
        {
            string pathAtlas = string.Format("{0}/{1}/{2}.asset", UIConfig.PATH_ATLAS_TP, atlasName, atlasName);
            //加载,todo:多平台加载
            atlasInfo = AssetDatabase.LoadAssetAtPath<AtlasInfo>(pathAtlas);
            if (atlasInfo != null)
            {
                atlasInfo.Init();
                m_AtlasInfoDic.Add(atlasName, atlasInfo);
            }
        }
        return atlasInfo;
    }
    public Sprite GetSprite(string sprName)
    {
        string atlasName = m_AtlasMap.GetAtlasNameBySpriteName(sprName);
        if (string.IsNullOrEmpty(atlasName))
        {
            return null;
        }
        AtlasInfo atlasInfo = GetAtlasInfo(atlasName);
        return atlasInfo.GetSprite(sprName);
    }
    public void GetSpriteAndMat(string sprName, out Sprite spr, out Material mat)
    {
        string atlasName = m_AtlasMap.GetAtlasNameBySpriteName(sprName);
        if (string.IsNullOrEmpty(atlasName))
        {
            mat = null;
            spr = null;
            return;
        }
        AtlasInfo atlasInfo = GetAtlasInfo(atlasName);
        mat = atlasInfo.m_Mat;
        spr = atlasInfo.GetSprite(sprName);
    }
}
