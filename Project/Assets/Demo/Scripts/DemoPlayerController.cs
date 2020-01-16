using UnityEngine;

public class DemoPlayerController : MonoBehaviour
{
    public float m_speed = 3;

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(h * m_speed * Time.deltaTime, 0, v * m_speed * Time.deltaTime);
        transform.Translate(movement);
    }
}