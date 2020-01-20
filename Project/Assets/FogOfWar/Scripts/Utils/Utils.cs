using UnityEngine;
using System.Collections.Generic;

public static class Utils
{
    /// <summary>
    /// 利用bresenhams直线算法找到两点间的所有格子
    /// </summary>
    /// <param name="start">直线起点</param>
    /// <param name="end">直线终点</param>
    /// <returns>直线覆盖的格子</returns>
    public static List<Vector2Int> LineByBresenhams(Vector2Int start, Vector2Int end)
    {
        //GC：实际项目使用时最好用Pool来存取
        List<Vector2Int> result = new List<Vector2Int>();

        int dx = end.x - start.x;
        int dy = end.y - start.y;
        int ux = dx > 0 ? 1 : -1;
        int uy = dy > 0 ? 1 : -1;
        int x = start.x;
        int y = start.y;
        int eps = 0;
        dx = Mathf.Abs(dx);
        dy = Mathf.Abs(dy);

        if (dx > dy)
        {
            for (x = start.x; x != end.x; x += ux)
            {
                result.Add(new Vector2Int(x, y));

                eps += dy;
                if ((eps << 1) >= dx)
                {
                    y += uy;
                    eps -= dx;
                }
            }
        }
        else
        {
            for (y = start.y; y != end.y; y += uy)
            {
                result.Add(new Vector2Int(x, y));

                eps += dx;
                if ((eps << 1) >= dy)
                {
                    x += ux;
                    eps -= dy;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 找到圆形覆盖的格子
    /// </summary>
    /// <param name="centerTile">圆心的格子坐标</param>
    /// <param name="radius">圆的半径</param>
    /// <param name="tileSize">一格的大小</param>
    /// <returns>圆形覆盖的格子</returns>
    public static List<Vector2Int> CircleByBoundingCircle(Vector2Int centerTile, float radius, float tileSize)
    {
        //GC：实际项目使用时最好用Pool来存取
        List<Vector2Int> result = new List<Vector2Int>();

        int radiusCount = Mathf.CeilToInt(radius / tileSize);
        int sqr = radiusCount * radiusCount;
        int top = centerTile.y + radiusCount;
        int bottom = centerTile.y - radiusCount;

        for (int y = bottom; y <= top; y++)
        {
            int dy = y - centerTile.y;
            int dx = Mathf.FloorToInt(Mathf.Sqrt(sqr - dy * dy));
            int left = centerTile.x - dx;
            int right = centerTile.x + dx;
            for (int x = left; x <= right; x++)
                result.Add(new Vector2Int(x, y));
        }

        return result;
    }

    public static Vector2 XZ(this Vector3 vec)
    {
        return new Vector2(vec.x, vec.z);
    }
}