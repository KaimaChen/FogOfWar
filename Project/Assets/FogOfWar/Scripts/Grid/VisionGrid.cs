using UnityEngine;

/// <summary>
/// 存放整个地图的视野信息
/// </summary>
public class VisionGrid : BaseGrid
{
    /// <summary>
    /// 存放所有格子数据，表示当前哪些玩家有视野
    /// </summary>
    public int[] m_values;

    /// <summary>
    /// 存放所有格子数据，表示玩家对应访问过的视野
    /// </summary>
    public int[] m_visited;

    public VisionGrid(int w, int h) : base(w, h)
    {
        m_width = w;
        m_height = h;
        m_values = new int[w * h];
        m_visited = new int[w * h];
    }

    public void SetVisible(int x, int y, int entityMask)
    {
        m_values[Index(x, y)] |= entityMask;
        m_visited[Index(x, y)] |= entityMask;
    }

    public void Clear()
    {
        for(int i = 0; i < m_values.Length; i++)
        {
            m_values[i] = 0;
        }
    }

    public bool IsVisible(int x, int y, int entityMask)
    {
        return (m_values[Index(x, y)] & entityMask) > 0;
    }

    public bool IsVisible(Vector2Int pos, int entityMask)
    {
        return IsVisible(pos.x, pos.y, entityMask);
    }

    public bool WasVisible(int x, int y, int entityMask)
    {
        return (m_visited[Index(x, y)] & entityMask) > 0;
    }
}