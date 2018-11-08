using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float m_height = 1.0f, m_turnSpeed = 5.0f, m_groundSpeed = 10.0f, m_jumpForce = 10.0f;

    private PlayerInput m_playerInput;

    private Rigidbody m_rigidbody;

    private RaycastHit m_groundAt;
    private Collision m_wallAt;

    private Vector3 m_move;

    private float m_xAng = 0.0f, m_yAng = 0.0f;

    private bool m_grounded = false, m_walled = false, m_jumping = false;


    // Use this for initialization
    void Start ()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        if (m_rigidbody == null)
        {
            Debug.Log("[PlayerController] m_rigidbody not found!");
        }

        m_playerInput = GetComponent<PlayerInput>();
        if (m_playerInput == null)
        {
            Debug.Log("[PlayerController] m_playerInput not found!");
        }

        //Starting orientation (z always 0)
        m_xAng = transform.rotation.eulerAngles.x;
        m_yAng = transform.rotation.eulerAngles.y;
    }

    // Update is called once per frame
    void Update ()
    {
		
	}

    // FixedUpdate called once per physics loop
    private void FixedUpdate()
    {
        CheckGround();
       
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(m_xAng, m_yAng, 0.0f), 0.8f);

        if (m_grounded)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, m_groundAt.point.y + m_height, 0.05f), transform.position.z);
            m_rigidbody.velocity = new Vector3(m_move.x * m_groundSpeed, m_rigidbody.velocity.y, m_move.z * m_groundSpeed);
        }
        else
        {
            //m_rigidbody.velocity = Vector3.Lerp(m_rigidbody.velocity, new Vector3(m_move.x * m_groundSpeed, m_rigidbody.velocity.y, m_move.z * m_groundSpeed), 0.1f);
            m_rigidbody.velocity = new Vector3(Mathf.Lerp(m_rigidbody.velocity.x, m_move.x * m_groundSpeed, 0.1f), m_rigidbody.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 
                Mathf.Lerp(m_rigidbody.velocity.z, m_move.z * m_groundSpeed, 0.1f));
        }
    }

    public void Move(Vector2 move, Vector2 look)
    {
        //Debug.Log("move == " + move);

        m_move = transform.TransformVector(new Vector3(move.normalized.x, 0.0f, move.normalized.y));
        m_move = Vector3.ProjectOnPlane(m_move, Vector3.up).normalized;

        m_xAng += m_turnSpeed * -look.y;
        m_yAng += m_turnSpeed * look.x;
    }

    public void Jump()
    {        
        if(!m_jumping)
        {
            m_jumping = true;
            m_grounded = false;
            m_rigidbody.useGravity = true;
            m_rigidbody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }
    }

    private void CheckGround()
    {
        if (!m_jumping)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out m_groundAt, m_height + 0.1f, LayerMask.GetMask("Default")))
            {
                Debug.Log("Grounded!");
                m_grounded = true;
                m_rigidbody.useGravity = false;
            }
            else
            {
                m_grounded = false;
                m_rigidbody.useGravity = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!m_grounded)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                m_jumping = false;

                if (Vector3.Dot(collision.contacts[0].normal, Vector3.up) < 0.45f)
                {
                    m_walled = true;
                    m_wallAt = collision;
                }
            }
        }        
    }

    private void OnCollisionStay(Collision collision)
    {
        if(!m_grounded)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                if (Vector3.Dot(collision.contacts[0].normal, Vector3.up) < 0.45f)
                {
                    m_walled = true;
                    m_wallAt = collision;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(m_walled && collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {            
            m_walled = false;            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_groundAt.point, 0.3f);

        if(m_walled)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(m_wallAt.contacts[0].point, 0.3f);
        }
    }
}
