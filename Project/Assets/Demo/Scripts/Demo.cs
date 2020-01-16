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
        mgr.m_getUnitVisionsHandler += GetUnitVisions;

        float x = 90;
        for(int z = 40; z < 60; z++)
        {
            mgr.TerrainGrid.SetAltitude(90, z, 2);

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = new Vector3(x, 0, z);
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