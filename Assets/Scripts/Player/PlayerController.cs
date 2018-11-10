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
    private List<Collision> m_levelAt = new List<Collision>();

    private Vector3 m_move;

    private float m_xAng = 0.0f, m_yAng = 0.0f;

    private int m_lastWallID = 0;
    
    private bool m_grounded = false, m_walled = false, m_levelTouch = false, m_jumping = false;

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

        if(m_grounded)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, m_groundAt.point.y + m_height, 0.05f), transform.position.z);
            m_rigidbody.velocity = new Vector3(m_move.x * m_groundSpeed, m_rigidbody.velocity.y, m_move.z * m_groundSpeed);
        }
        else if(m_walled)
        {
            m_rigidbody.velocity = new Vector3(Mathf.Lerp(m_rigidbody.velocity.x, m_move.x * m_groundSpeed, 0.01f), m_rigidbody.velocity.y + Physics.gravity.y *  0.25f * Time.fixedDeltaTime,
                Mathf.Lerp(m_rigidbody.velocity.z, m_move.z * m_groundSpeed, 0.01f));
        }
        else
        {
            m_rigidbody.velocity = new Vector3(Mathf.Lerp(m_rigidbody.velocity.x, m_move.x * m_groundSpeed, 0.05f), m_rigidbody.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 
                Mathf.Lerp(m_rigidbody.velocity.z, m_move.z * m_groundSpeed, 0.05f));
        }
    }

    public void Move(Vector2 move, Vector2 look)
    {
        m_move = transform.TransformVector(new Vector3(move.normalized.x, 0.0f, move.normalized.y));
        
        //No sticking to level with air control
        if(m_levelTouch)
        {
            Vector3 intoLevel = Vector3.Project(m_move, m_levelAt[m_levelAt.Count - 1].contacts[0].normal);
            m_move += m_levelAt[m_levelAt.Count - 1].contacts[0].normal * intoLevel.magnitude;
        }

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
            transform.parent = null;
            if (m_walled)
            {                
                m_rigidbody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
                m_rigidbody.AddForce(m_wallAt.contacts[0].normal * m_jumpForce * 2.0f, ForceMode.Impulse);                
            }
            else
            {
                m_rigidbody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
            }
        }
    }

    private void CheckGround()
    {
        if(!m_jumping)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out m_groundAt, m_height + 0.1f, LayerMask.GetMask("Default")))
            {
                //Debug.Log("Grounded!");
                m_grounded = true;
                m_rigidbody.useGravity = false;

                m_lastWallID = 0; //Last wall is last wall until grounded!

                if (m_groundAt.collider.gameObject.tag == "MovingPlatform")
                {
                    transform.parent = m_groundAt.collider.gameObject.transform.parent;                    
                }
                else
                {
                    transform.parent = null;
                }
            }
            else
            {
                m_grounded = false;         

                if(!m_walled)
                {
                    m_rigidbody.useGravity = true;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Check if level collision...
        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            m_levelTouch = true;
            bool newTouch = true;
            foreach(Collision col in m_levelAt)
            {
                if(col.gameObject.GetInstanceID() == collision.gameObject.GetInstanceID())
                {
                    newTouch = false;
                }
            }
            if(newTouch)
            {
                m_levelAt.Add(collision);
            }

            //Check if wall collision...
            if (Vector3.Dot(collision.contacts[0].normal, Vector3.up) < 0.45f)
            {
                //Check if new wall...
                if(m_lastWallID != collision.gameObject.GetInstanceID())
                {
                    m_lastWallID = collision.gameObject.GetInstanceID();

                    m_jumping = false;
                    m_walled = true;
                    m_wallAt = collision;

                    m_rigidbody.useGravity = false;
                }
            }
            else
            {
                //Collided with flat enough ground (CheckGround() manages "bounce")
                m_jumping = false;
            }
        }                
    }

    private void OnCollisionStay(Collision collision)
    {   
        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            if(m_levelAt.Count != 0)
            {
                m_levelAt[m_levelAt.Count - 1] = collision;
            }
            
            if (Vector3.Dot(collision.contacts[0].normal, Vector3.up) < 0.45f)
            {
                m_wallAt = collision;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            if(m_levelAt.Count != 0)
            {
                //Debug.Log("manage touch list!");
                //Debug.Log("m_levelAt.Count == " + m_levelAt.Count.ToString());
                for(int i = 0; i <= m_levelAt.Count - 1; i++)
                {
                    //Debug.Log("i == " + i.ToString());
                    if(m_levelAt[i].gameObject.GetInstanceID() == collision.gameObject.GetInstanceID())
                    {
                        //Debug.Log("removing touch!");
                        m_levelAt.RemoveAt(i);
                    }

                    //Debug.Log("m_levelAt[" + i.ToString() + "].gameObject.GetInstanceID() == " + m_levelAt[i].gameObject.GetInstanceID().ToString());
                    //Debug.Log("collision.gameObject.GetInstanceID() == " + collision.gameObject.GetInstanceID().ToString());
                }
            }
            if(m_levelAt.Count == 0)
            {
                m_levelTouch = false;
            }

            if (m_walled)
            {
                m_walled = false;
                m_wallAt = null;
            }

            if (!m_grounded)
            {
                m_rigidbody.useGravity = true;
            }
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

        if(m_levelTouch)
        {
            Gizmos.color = Color.magenta;

            foreach (Collision col in m_levelAt)
            {
                Gizmos.DrawWireSphere(col.contacts[0].point, 0.3f);
            }
        }
    }
}
