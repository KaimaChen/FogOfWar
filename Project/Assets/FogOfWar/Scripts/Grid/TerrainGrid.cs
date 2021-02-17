using UnityEngine;

/// <summary>
/// 格子存放地形相关的信息
/// </summary>
public class TerrainGrid : BaseGrid
{
    /// <summary>
    /// 海拔高度
    /// </summary>
    readonly short[] m_altitudes;

    /// <summary>
    /// 表示所在的草丛（没草丛则是0）
    /// </summary>
    readonly short[] m_grassIds;

    public TerrainGrid(int w, int h) : base(w, h)
    {
        m_width = w;
        m_height = h;
        m_altitudes = new short[w * h];
        m_grassIds = new short[w * h];
    }

    public void SetAltitude(int tileX, int tileY, short a)
    {
        int index = Index(tileX, tileY);
        m_altitudes[index] = a;
    }

    public short GetAltitude(int tileX, int tileY)
    {
        return m_altitudes[Index(tileX, tileY)];
    }

    public short GetAltitude(Vector2Int tilePos)
    {
        return GetAltitude(tilePos.x, tilePos.y);
    }

    public void SetGrass(int tileX, int tileY, short id)
    {
        m_grassIds[Index(tileX, tileY)] = id;
    }

    public void GetData(int tileX, int tileY, out short altitude, out short grassId)
    {
        int index = Index(tileX, tileY);
        altitude = m_altitudes[index];
        grassId = m_grassIds[index];
    }

    public void GetData(Vector2Int tilePos, out short altitude, out short grassId)
    {
        GetData(tilePos.x, tilePos.y, out altitude, out grassId);
    }

    public void GetData(Vector3 worldPos, out short altitude, out short grassId)
    {
        Vector2Int tilePos = FogOfWarMgr.Instance.WorldPosToTilePos(worldPos);
        GetData(tilePos.x, tilePos.y, out altitude, out grassId);
    }
}