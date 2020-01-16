//参考：https://www.gameres.com/856904.html

//TODO: 现在敌人出现在视野的表现很突兀

using UnityEngine;
using System;
using System.Collections.Generic;

public class FogOfWarMgr : MonoBehaviour
{
    /// <summary>
    /// 多久更新一遍视野（单位秒）
    /// </summary>
    const float c_updateDuration = 0.1f;

    /// <summary>
    /// 迷雾的平滑速度
    /// </summary>
    const float c_smoothSpeed = 10f;
    public Func<List<UnitVision>> m_getUnitVisionsHandler;

    /// <summary>
    /// 地图宽度（格子数）
    /// </summary>
    [SerializeField]
    int m_width = 200;

    /// <summary>
    /// 地图高度（格子数）
    /// </summary>
    [SerializeField]
    int m_height = 200;

    /// <summary>
    /// 格子大小
    /// </summary>
    [SerializeField]
    float m_tileSize = 1f;

    [SerializeField]
    MeshRenderer m_rawRenderer;

    [SerializeField]
    MeshRenderer m_finalRenderer;

    /// <summary>
    /// 当前以什么Mask来查看视野
    /// </summary>
    [SerializeField]
    int m_visionMask = 1;

    Texture2D m_fowTex;

    VisionGrid m_visionGrid;
    TerrainGrid m_terrainGrid;

    Color[] m_curtColors;
    Color[] m_targetColors;
    
    float m_nextUpdateTime;

    #region get-set
    public static FogOfWarMgr Instance { get; private set; }

    public int Width { get { return m_width; } }

    public int Height { get { return m_height; } }

    public TerrainGrid TerrainGrid { get { return m_terrainGrid; } }
    #endregion

    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        m_visionGrid = new VisionGrid(m_width, m_height);
        m_terrainGrid = new TerrainGrid(m_width, m_height);
        m_targetColors = new Color[m_width * m_height];
        m_curtColors = new Color[m_width * m_height];
        m_fowTex = new Texture2D(m_width, m_height, TextureFormat.Alpha8, false);
        m_rawRenderer.sharedMaterial.mainTexture = m_fowTex;

        InitCamera();
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void Update()
    {
        List<UnitVision> units = m_getUnitVisionsHandler.Invoke();

        if (Time.time >= m_nextUpdateTime)
        {
            m_nextUpdateTime = Time.time + c_updateDuration;
            CalculateVision(units);
            UpdateTextures(m_visionMask);
        }

        //判断单位的可见性
        UpdateVisibles(units);

        //平滑颜色
        SmoothColor();
    }

    void InitCamera()
    {
        GameObject go = new GameObject("FogOfWarCamera");
        go.transform.SetParent(transform);
        go.AddComponent<Blur>();

        Camera cam = go.AddComponent<Camera>();
        cam.cullingMask = LayerMask.GetMask(Defines.c_LayerFogOfWar);
        cam.clearFlags = CameraClearFlags.Depth;
        cam.depth = Camera.main.depth + 1;
        
        int width = m_width * 4;
        int height = m_height * 4;
        RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.R8);

        cam.targetTexture = rt;
        m_finalRenderer.sharedMaterial.mainTexture = rt;

        //TODO: 让rawRenderer自己渲染到整个屏幕
        var pos = m_rawRenderer.transform.position;
        cam.transform.position = new Vector3(pos.x, pos.y, pos.z - 1);
        cam.orthographicSize = 0.5f;
        cam.orthographic = true;
    }

    void CalculateVision(List<UnitVision> units)
    {
        if (m_getUnitVisionsHandler == null)
            return;

        m_visionGrid.Clear();

        for(int unitIndex = 0; unitIndex < units.Count; unitIndex++)
        {
            UnitVision unit = units[unitIndex];
            Vector2Int startTile = WorldPosToTilePos(unit.WorldPos);
            List<Vector2Int> tiles = GetCircleCoverTiles(unit.WorldPos, unit.m_range);
            for(int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].x < 0 || tiles[i].x >= m_width || tiles[i].y < 0 || tiles[i].y >= m_height)
                    Debug.LogError(tiles[i]);

                if (!IsBlocked(startTile, tiles[i], unit))
                    m_visionGrid.SetVisible(tiles[i].x, tiles[i].y, unit.m_mask);
            }
        }
    }

    /// <summary>
    /// 两点间的视野是否因为地形被阻挡了
    /// </summary>
    bool IsBlocked(Vector2Int startTile, Vector2Int targetTile, UnitVision unit)
    {
        List<Vector2Int> points = Line(startTile, targetTile);
        for(int i = 0; i < points.Count; i++)
        {
            short altitude = m_terrainGrid.GetAltitude(points[i]);
            if (altitude > unit.m_terrainHeight)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 利用bresenhams直线算法找到两点间的所有格子
    /// </summary>
    /// <param name="start">直线起点</param>
    /// <param name="end">直线终点</param>
    /// <returns>直线覆盖的格子</returns>
    List<Vector2Int> Line(Vector2Int start, Vector2Int end)
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

        if(dx > dy)
        {
            for(x = start.x; x != end.x; x += ux)
            {
                eps += dy;
                if((eps << 1) >= dx)
                {
                    y += uy;
                    eps -= dx;
                }

                result.Add(new Vector2Int(x, y));
            }
        }
        else
        {
            for(y = start.y; y != end.y; y += uy)
            {
                eps += dx;
                if((eps << 1) >= dy)
                {
                    x += ux;
                    eps -= dy;
                }

                result.Add(new Vector2Int(x, y));
            }
        }

        return result;
    }

    void UpdateTextures(int playerMask)
    {
        for(int x = 0; x < m_width; x++)
        {
            for(int y = 0; y < m_height; y++)
            {
                int index = x + y * m_width;
                m_targetColors[index] = new Color(0, 0, 0, 1);

                if (m_visionGrid.IsVisible(x, y, playerMask))
                    m_targetColors[index] = new Color(0, 0, 0, 0);
                else if (m_visionGrid.WasVisible(x, y, playerMask))
                    m_targetColors[index] = new Color(0, 0, 0, 0.5f);
            }
        }

        //如果在这里直接设置颜色，则移动时会很明显发现迷雾是一顿一顿的，不平滑
        //m_fowTex.SetPixels(m_targetColors);
        //m_fowTex.Apply(false);
    }

    void SmoothColor()
    {
        for (int i = 0; i < m_targetColors.Length; i++)
        {
            Color target = m_targetColors[i];
            Color curt = m_curtColors[i];
            m_curtColors[i] = Color.Lerp(curt, target, c_smoothSpeed * Time.deltaTime);
        }

        m_fowTex.SetPixels(m_curtColors);
        m_fowTex.Apply(false);
    }

    void UpdateVisibles(List<UnitVision> units)
    {
        for(int i = 0; i < units.Count; i++)
        {
            string layerName = IsVisible(m_visionMask, units[i]) ? Defines.c_LayerDefault : Defines.c_LayerHidden;
            units[i].m_gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

    bool IsVisible(int curtMask, UnitVision unit)
    {
        if ((curtMask & unit.m_mask) > 0)
            return true;

        Vector2Int tilePos = WorldPosToTilePos(unit.WorldPos);
        if (m_visionGrid.IsVisible(tilePos, curtMask))
            return true;

        return false;
    }

    List<Vector2Int> GetCircleCoverTiles(Vector2 worldPos, float radius)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        Vector2Int tilePos = WorldPosToTilePos(worldPos);
        int tileCountOnRadius = (int)(radius / m_tileSize);
        float sqrRadius = radius * radius;

        for(int x = -tileCountOnRadius; x <= tileCountOnRadius; x++)
        {
            for(int y = -tileCountOnRadius; y <= tileCountOnRadius; y++)
            {
                Vector2Int relativeTilePos = new Vector2Int(x, y);
                Vector2 relativeWorldPos = TilePosToWorldPos(relativeTilePos);
                if(relativeWorldPos.sqrMagnitude <= sqrRadius)
                {
                    result.Add(tilePos + relativeTilePos);
                }
            }
        }

        return result;
    }

    #region 转换
    Vector2Int WorldPosToTilePos(Vector2 worldPos)
    {
        int x = (int)(worldPos.x / m_tileSize);
        int y = (int)(worldPos.y / m_tileSize);
        return new Vector2Int(x, y);
    }

    Vector2 TilePosToWorldPos(Vector2Int tilePos)
    {
        float half = m_tileSize / 2f;
        float x = tilePos.x * m_tileSize + half;
        float y = tilePos.y * m_tileSize + half;
        return new Vector2(x, y);
    }
    #endregion
}