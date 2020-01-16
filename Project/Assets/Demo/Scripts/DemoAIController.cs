using System.Collections.Generic;
using UnityEngine;

public class DemoAIController : MonoBehaviour 
{
    public List<Vector3> m_patrolPoints = new List<Vector3>();
    public float m_speed = 5;

    int m_curtIndex;

    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        if (m_patrolPoints.Count <= 0)
            return;

        if (m_curtIndex >= m_patrolPoints.Count)
            m_curtIndex = 0;

        Vector3 curtPos = transform.position;
        Vector3 target = m_patrolPoints[m_curtIndex];
        Vector3 toTarget = target - curtPos;
        float dist = toTarget.magnitude;
        if(dist < m_speed * Time.deltaTime)
        {
            transform.position = target;

            m_curtIndex++;
            if (m_curtIndex >= m_patrolPoints.Count)
                m_curtIndex = 0;
        }
        else
        {
            transform.position += toTarget.normalized * m_speed * Time.deltaTime;
        }
    }
}
