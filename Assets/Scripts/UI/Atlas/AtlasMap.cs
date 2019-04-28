using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AtlasMap : ScriptableObject
{
    [SerializeField]
    private List<Atlas_SpriteNameList> m_AtlasMapList = new List<Atlas_SpriteNameList>();
    private Dictionary<string, string> m_AtlasMapDic = null;

    public void Init()
    {
        m_AtlasMapDic = new Dictionary<string, string>();
        for (int i = 0; i < m_AtlasMapList.Count; i++)
        {
            var atlasMap = m_AtlasMapList[i];
            var atlasName = atlasMap.name;
            foreach (var sprName in atlasMap.spriteNameList)
            {
                m_AtlasMapDic[sprName] = atlasName;
            }
        }
    }

    public string GetAtlasNameBySpriteName(string sprName){
        string atlasName;
        if (!m_AtlasMapDic.TryGetValue(sprName, out atlasName))
        {
            Debug.LogError(string.Format("Dont find sprite:{0} in AtlasMap",sprName));
            return null;
        }
        return atlasName;
    }

#if UNITY_EDITOR
    public void AddAtlas_SpriteName(Atlas_SpriteNameList atlas)
    {
        for (int i = 0; i < m_AtlasMapList.Count; i++)
        {
            if (m_AtlasMapList[i].name == atlas.name)
            {
                m_AtlasMapList[i] = atlas;
                CheckAtlasMap();
                return;
            }
        }
        m_AtlasMapList.Add(atlas);
        CheckAtlasMap();
    }
    void CheckAtlasMap()
    {
        for (int i = m_AtlasMapList.Count - 1; i >= 0; i--)
        {
            string name = m_AtlasMapList[i].name;
            var pathAtlasInfo = string.Format("{0}/{1}/{2}.asset", UIConfig.PATH_ATLAS_TP, name, name);
            var asset = AssetDatabase.LoadAssetAtPath<AtlasInfo>(pathAtlasInfo);
            if (asset == null)
            {
                m_AtlasMapList.Remove(m_AtlasMapList[i]);
            }
        }
    }
#endif
}

[System.Serializable]
public struct Atlas_SpriteNameList
{
    public string name;
    [SerializeField]
    public List<string> spriteNameList;
}