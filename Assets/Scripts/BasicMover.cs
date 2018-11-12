using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMover : MonoBehaviour
{
    [SerializeField]
    private List<Vector3> m_wayPoints = new List<Vector3>();

    [SerializeField]
    private float m_moveSpeed = 5.0f;

    private int m_point = 0;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void FixedUpdate()
    {
        float distToWaypoint = Vector3.Distance(transform.position, m_wayPoints[m_point]);
        if (distToWaypoint > 0.01f)
        {
            //determine direction, calculate next position, no overshooting...
            Vector3 dir = m_wayPoints[m_point] - transform.position;
            transform.position += dir.normalized * Mathf.Min(m_moveSpeed * Time.fixedDeltaTime, distToWaypoint);
        }
        else
        {
            m_point += 1;
            if (m_point == m_wayPoints.Count)
            {
                m_point = 0;
            }
        }
    }
}
