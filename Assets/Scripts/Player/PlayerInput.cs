using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerInput : MonoBehaviour
{
    private PlayerController m_playerController;

    private Vector2 m_move, m_look;

    private bool m_jumpAvailable = true;

    // Use this for initialization
    void Start ()
    {
        m_playerController = GetComponent<PlayerController>();
        if(m_playerController == null)
        {
            Debug.Log("[PlayerInput] m_playerController not found!");
        }		
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (Input.GetButton("Jump") && m_jumpAvailable)
        {
            m_playerController.Jump();
            m_jumpAvailable = false;
        }
        else if (!m_jumpAvailable && !Input.GetButton("Jump"))
        {
            m_jumpAvailable = true;
        }

        // Get WASD input
        m_move.x = Input.GetAxis("Horizontal");
        m_move.y = Input.GetAxis("Vertical");

        // Get look input
        m_look.x = Input.GetAxisRaw("Mouse X");
        m_look.y = Input.GetAxisRaw("Mouse Y");

        m_playerController.Move(m_move, m_look);
    }
}
