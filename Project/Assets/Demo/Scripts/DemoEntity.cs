using UnityEngine;

public class DemoEntity : MonoBehaviour
{
    public int m_mask;
    public float m_radius;

    void OnDisable()
    {
        Demo.Instance.UnRegister(GetInstanceID());
    }

    void Update()
    {
        FogOfWarMgr.Instance.TerrainGrid.GetData(transform.position, out short altitude, out short grassId);
        UnitVision unit = new UnitVision(gameObject, m_mask, m_radius, altitude, grassId);
        Demo.Instance.UpdateUnit(GetInstanceID(), unit);
    }
}