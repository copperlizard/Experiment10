using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private GameObject m_player;

	// Use this for initialization
	void Start ()
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
        if(m_player == null)
        {
            Debug.Log("[PlayerCamera] m_player not found!");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = Vector3.Lerp(transform.position, m_player.transform.position, 80.0f * Time.deltaTime);
        transform.rotation = m_player.transform.rotation;
	}
}
