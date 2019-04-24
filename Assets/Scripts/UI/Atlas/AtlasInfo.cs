using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtlasInfo : ScriptableObject {
	public Material m_Mat;
	public List<SpriteInfo> m_SpriteInfoList = new List<SpriteInfo>();
	public Dictionary<string, SpriteInfo> m_SpriteInfoDic = null;
	public void InitAtlasInfo(){
		if (m_SpriteInfoDic != null)
		{
			return;
		}
		m_SpriteInfoDic = new Dictionary<string, SpriteInfo>();
		for (int i = 0; i < m_SpriteInfoList.Count; i++)
		{
			m_SpriteInfoDic.Add(m_SpriteInfoList[i].m_Sprite.name,m_SpriteInfoList[i]);
		}
	}
}
[System.Serializable]
public struct SpriteInfo {
	public Sprite m_Sprite;
	public Vector4 m_Padding;

	public SpriteInfo(Sprite spr, Vector4 padding){
		m_Sprite = spr;
		m_Padding = padding;
	}
	
}
