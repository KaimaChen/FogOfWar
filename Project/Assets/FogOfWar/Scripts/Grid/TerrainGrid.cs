using UnityEngine;

public class TerrainGrid : BaseGrid
{
    readonly short[] m_values;

    public TerrainGrid(int w, int h) : base(w, h)
    {
        m_width = w;
        m_height = h;
        m_values = new short[w * h];
    }

    public void SetAltitude(int x, int y, short a)
    {
        int index = Index(x, y);
        m_values[index] = a;
    }

    public short GetAltitude(int x, int y)
    {
        return m_values[Index(x, y)];
    }

    public short GetAltitude(Vector2Int pos)
    {
        return GetAltitude(pos.x, pos.y);
    }
}