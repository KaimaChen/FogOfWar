//参考：https://www.gameres.com/856904.html

using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 统一管理战争迷雾
/// </summary>
public class FogOfWarMgr : MonoBehaviour
{
    /// <summary>
    /// 多久更新一遍视野（单位秒）
    /// </summary>
    const float c_updateDuration = 0.1f;

    /// <summary>
    /// 迷雾的平滑速度
    /// </summary>
    const float c_smoothSpeed = 5f;
    
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

    /// <summary>
    /// 当前以什么Mask来查看视野
    /// </summary>
    [SerializeField]
    int m_visionMask = 1;

    /// <summary>
    /// 存放最开始的迷雾信息，用于给摄像机进行二次处理
    /// </summary>
    MeshRenderer m_rawRenderer;

    /// <summary>
    /// 渲染最终的迷雾
    /// 注意要覆盖整个场景
    /// </summary>
    MeshRenderer m_finalRenderer;

    /// <summary>
    /// 存放原始迷雾的贴图
    /// </summary>
    Texture2D m_fowTex;

    /// <summary>
    /// 存放视野的具体数据
    /// </summary>
    VisionGrid m_visionGrid;

    /// <summary>
    /// 存放地形高度等信息
    /// </summary>
    TerrainGrid m_terrainGrid;

    /// <summary>
    /// 当前战争迷雾贴图的颜色信息
    /// </summary>
    Color[] m_curtColors;

    /// <summary>
    /// 战争迷雾想要过渡到的目标颜色（不直接设置目标颜色是为了有渐变的效果）
    /// </summary>
    Color[] m_targetColors;
    
    float m_nextUpdateTime;

    #region get-set
    public static FogOfWarMgr Instance { get; private set; }

    public int Width { get { return m_width; } }

    public int Height { get { return m_height; } }

    public TerrainGrid TerrainGrid { get { return m_terrainGrid; } }

    public Func<List<UnitVision>> GetUnitVisionsHandler { get; set; }
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

        InitRawRenderer();
        InitFinalRenderer();
        InitCamera();
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void Update()
    {
        if (GetUnitVisionsHandler == null)
            return;

        List<UnitVision> units = GetUnitVisionsHandler.Invoke();

        //定时更新视野数据
        if (Time.time >= m_nextUpdateTime)
        {
            m_nextUpdateTime = Time.time + c_updateDuration;
            CalculateVision(units);
            UpdateTargetColors(m_visionMask);
        }

        //判断单位的可见性
        UpdateVisibles(units);

        //平滑颜色
        SmoothColor();
    }

    /// <summary>
    /// 计算所有单位的视野数据
    /// </summary>
    /// <param name="units">所有单位的列表</param>
    void CalculateVision(List<UnitVision> units)
    {
        m_visionGrid.Clear();

        for(int unitIndex = 0; unitIndex < units.Count; unitIndex++)
        {
            UnitVision unit = units[unitIndex];
            Vector2Int centerTile = WorldPosToTilePos(unit.WorldPos);

            if(IsOutsideMap(centerTile))
            {
                Debug.LogError($"单位{unit.m_gameObject.name}的格子位置{centerTile}超出了地图范围");
                continue;
            }

            List<Vector2Int> tiles = Utils.CircleByBoundingCircle(centerTile, unit.m_range, m_tileSize);
            for(int i = 0; i < tiles.Count; i++)
            {
                if (!IsBlocked(centerTile, tiles[i], unit))
                    m_visionGrid.SetVisible(tiles[i].x, tiles[i].y, unit.m_mask);
            }
        }
    }

    /// <summary>
    /// 判断格子位置是否超过地图范围
    /// </summary>
    bool IsOutsideMap(Vector2Int tilePos)
    {
        return tilePos.x < 0 || tilePos.x >= m_width ||
                    tilePos.y < 0 || tilePos.y > m_height;
    }

    /// <summary>
    /// 两点间的视野是否因为地形被阻挡了
    /// </summary>
    bool IsBlocked(Vector2Int startTile, Vector2Int targetTile, UnitVision unit)
    {
        List<Vector2Int> points = Utils.LineByBresenhams(startTile, targetTile);
        for(int i = 0; i < points.Count; i++)
        {
            short altitude = m_terrainGrid.GetAltitude(points[i]);
            if (altitude > unit.m_terrainHeight)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 更新原始迷雾贴图的目标颜色
    /// </summary>
    void UpdateTargetColors(int entityMask)
    {
        for(int x = 0; x < m_width; x++)
        {
            for(int y = 0; y < m_height; y++)
            {
                int index = x + y * m_width;
                m_targetColors[index] = new Color(0, 0, 0, 1);

                if (m_visionGrid.IsVisible(x, y, entityMask))
                    m_targetColors[index] = new Color(0, 0, 0, 0);
                else if (m_visionGrid.WasVisible(x, y, entityMask))
                    m_targetColors[index] = new Color(0, 0, 0, 0.6f);
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

    /// <summary>
    /// 根据可见性设置渲染层级
    /// </summary>
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

    #region 初始化
    void InitRawRenderer()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "RawRenderer";
        go.layer = LayerMask.NameToLayer(Defines.c_LayerFogOfWar);
        go.transform.SetParent(transform);
        go.transform.position = new Vector3(1000, 0, 0);

        m_rawRenderer = go.GetComponent<MeshRenderer>();
        m_rawRenderer.sharedMaterial = new Material(Shader.Find("KaimaChen/RawFogOfWar"))
        {
            mainTexture = m_fowTex
        };
    }

    void InitFinalRenderer()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "FinalRenderer";

        Transform trans = go.transform;
        trans.SetParent(transform);
        m_finalRenderer = trans.GetComponent<MeshRenderer>();
        m_finalRenderer.sharedMaterial = new Material(Shader.Find("KaimaChen/FinalFogOfWar"));

        //将最终的迷雾覆盖到整张地图（这里假设地图左下角是原点，如果有任何不一样，则需要自己另外设置）
        trans.localScale = new Vector3(m_width * m_tileSize, m_height * m_tileSize, 1);
        trans.rotation = Quaternion.Euler(90, 0, 0);
        trans.position = new Vector3(trans.localScale.x / 2, 1, trans.localScale.y / 2);
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

        var pos = m_rawRenderer.transform.position;
        cam.transform.position = new Vector3(pos.x, pos.y, pos.z - 1);
        cam.orthographicSize = 0.5f;
        cam.orthographic = true;
    }
    #endregion

    #region 转换
    Vector2Int WorldPosToTilePos(Vector2 worldPos)
    {
        int x = (int)(worldPos.x / m_tileSize);
        int y = (int)(worldPos.y / m_tileSize);
        return new Vector2Int(x, y);
    }
    #endregion
}