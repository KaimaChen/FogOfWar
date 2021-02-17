using UnityEngine;
using System.Collections.Generic;

public class Demo : MonoBehaviour
{
    private static Demo m_instance;
    public static Demo Instance
    {
        get
        {
            return m_instance;
        }
    }

    public GameObject m_grassPrefab;

    readonly Dictionary<int, UnitVision> m_unitDict = new Dictionary<int, UnitVision>();

    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            m_instance = this;
    }

    void Start()
    {
        var mgr = FogOfWarMgr.Instance;
        mgr.GetUnitVisionsHandler += GetUnitVisions;

        ///实践使用中，设置障碍物和草丛都应该由地图编辑器完成
        
        //创建障碍物，用于阻挡视线
        Transform container = new GameObject("Obstacles").transform;
        container.SetParent(transform);
        int x = 91;
        for(int z = 40; z < 60; z++)
        {
            mgr.TerrainGrid.SetAltitude(x, z, 2);

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Obstacle";
            go.transform.position = new Vector3(x, 0, z);
            go.transform.SetParent(container);
        }

        //设置草丛
        container = new GameObject("Grass").transform;
        container.SetParent(transform);
        for(x = 75; x < 85; x++)
        {
            for(int z = 30; z < 40; z++)
            {
                mgr.TerrainGrid.SetGrass(x, z, 1);

                GameObject go = Instantiate(m_grassPrefab);
                go.SetActive(true);
                go.name = "Grass1";
                go.transform.position = new Vector3(x, 0, z);
                go.transform.SetParent(container);
            }
        }

        for (x = 90; x < 95; x++)
        {
            for (int z = 30; z < 40; z++)
            {
                mgr.TerrainGrid.SetGrass(x, z, 2);

                GameObject go = Instantiate(m_grassPrefab);
                go.SetActive(true);
                go.name = "Grass2";
                go.transform.position = new Vector3(x, 0, z);
                go.transform.SetParent(container);
            }
        }
    }

    public void UpdateUnit(int id, UnitVision unit)
    {
        m_unitDict[id] = unit;
    }

    public void UnRegister(int id)
    {
        m_unitDict.Remove(id);
    }

    List<UnitVision> GetUnitVisions()
    {
        return new List<UnitVision>(m_unitDict.Values);
    }
}