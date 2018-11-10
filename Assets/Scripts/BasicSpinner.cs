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
}
