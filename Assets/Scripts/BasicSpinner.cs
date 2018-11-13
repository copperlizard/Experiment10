using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSpinner : MonoBehaviour
{
    [SerializeField]
    private Vector3 m_rotate = Vector3.zero; //degrees to rotate in each axis in seconds...

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
        transform.Rotate(m_rotate * Time.fixedDeltaTime);
    }

    public Vector3 GetVelocityAt(Vector3 point)
    {
        Vector3 velocityAtPoint = Vector3.zero;

        Vector3 xzPoint = new Vector3(point.x, transform.position.y, point.z);
        Vector3 xyPoint = new Vector3(point.x, point.y, transform.position.z);
        Vector3 yzPoint = new Vector3(transform.position.x, point.y, point.z);

        float xW = Mathf.Abs(m_rotate.x) * Mathf.Deg2Rad;
        float yW = Mathf.Abs(m_rotate.y) * Mathf.Deg2Rad;
        float zW = Mathf.Abs(m_rotate.z) * Mathf.Deg2Rad;

        Vector3 xzR = xzPoint - transform.position;
        Vector3 xyR = xyPoint - transform.position;
        Vector3 yzR = yzPoint - transform.position;

        float xzV = yW * xzR.magnitude;
        float xyV = zW * xyR.magnitude;
        float yzV = xW * yzR.magnitude;

        Vector3 xzP = Quaternion.Euler(0.0f, (m_rotate.y < 0.0f)? -90.0f : 90.0f, 0.0f) * xzR;
        Vector3 xyP = Quaternion.Euler(0.0f, 0.0f, (m_rotate.z < 0.0f) ? -90.0f : 90.0f) * xyR;
        Vector3 yzP = Quaternion.Euler((m_rotate.x < 0.0f) ? -90.0f : 90.0f, 0.0f, 0.0f) * yzR;

        velocityAtPoint += xzP.normalized * xzV;
        velocityAtPoint += xyP.normalized * xyV;
        velocityAtPoint += yzP.normalized * yzV;

        return velocityAtPoint;
    }
}
