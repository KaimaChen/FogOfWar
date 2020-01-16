using UnityEngine;

/// <summary>
/// 单位视野
/// 通常每个单位有一个对应的单位视野
/// 部分复杂单位，如建筑，会有多个单位视野
/// </summary>
public struct UnitVision
{
    /// <summary>
    /// 表示玩家分组的位掩码
    /// 如玩家0是0001，玩家1是0010，则这两个玩家共同的视野是0011
    /// </summary>
    public int m_mask;

    /// <summary>
    /// 视野范围
    /// </summary>
    public float m_range;
    
    /// <summary>
    /// 单位的所在海拔，用于阻挡视线
    /// </summary>
    public short m_terrainHeight;

    public GameObject m_gameObject;

    #region get-set
    public Vector2 WorldPos
    {
        get { return m_gameObject.transform.position.XZ(); }
    }
    #endregion

    public UnitVision(GameObject go, int mask, float range, short height)
    {
        m_gameObject = go;
        m_mask = mask;
        m_range = range;
        m_terrainHeight = height;
    }
}