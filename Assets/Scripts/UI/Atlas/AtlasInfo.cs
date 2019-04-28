using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtlasInfo : ScriptableObject
{
    public Material m_Mat;
    public bool m_IsPoly;
	[SerializeField]
    private List<SpriteInfo> m_SpriteInfoList = new List<SpriteInfo>();
    public Dictionary<string, SpriteInfo> m_SpriteInfoDic = null;

    public void Init()
    {
        if (m_SpriteInfoDic != null)
        {
            return;
        }
        m_SpriteInfoDic = new Dictionary<string, SpriteInfo>();
        for (int i = 0; i < m_SpriteInfoList.Count; i++)
        {
            m_SpriteInfoDic.Add(m_SpriteInfoList[i].m_Sprite.name, m_SpriteInfoList[i]);
        }
    }
    public Vector4 GetPadding(string sprName)
    {
        SpriteInfo sprInfo;
        if (m_SpriteInfoDic.TryGetValue(sprName, out sprInfo))
        {
            return sprInfo.m_Padding;
        }
        return Vector4.zero;
    }
    public Sprite GetSprite(string sprName)
    {
        SpriteInfo sprInfo;
        if (!m_SpriteInfoDic.TryGetValue(sprName, out sprInfo))
        {
            Debug.LogError(string.Format("Dont find sprite:{0} in AtlasInfo",sprName));
            return null;
        }
        return sprInfo.m_Sprite;
    }
#if UNITY_EDITOR
    public void SetSpriteInfoList(List<SpriteInfo> spriteInfoList)
    {
        m_SpriteInfoList = spriteInfoList;
    }
#endif
}
[System.Serializable]
public struct SpriteInfo
{
    public Sprite m_Sprite;
    public Vector4 m_Padding;

    public SpriteInfo(Sprite spr, Vector4 padding)
    {
        m_Sprite = spr;
        m_Padding = padding;
    }

}
