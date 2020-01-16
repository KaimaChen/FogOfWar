using UnityEngine;

public class DemoEntity : MonoBehaviour
{
    public int m_mask;
    public float m_radius;
    public short m_height;

    void OnDisable()
    {
        Demo.Instance.UnRegister(GetInstanceID());
    }

    void Update()
    {
        UnitVision unit = new UnitVision(gameObject, m_mask, m_radius, m_height);
        Demo.Instance.UpdateUnit(GetInstanceID(), unit);
    }
}