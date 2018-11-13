using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float m_height = 1.0f, m_turnSpeed = 5.0f, m_groundSpeed = 10.0f, m_airSpeed = 10.0f, m_jumpForce = 10.0f;

    private PlayerInput m_playerInput;

    private Rigidbody m_rigidbody;

    private RaycastHit m_groundAt = new RaycastHit();
    private Collision m_wallAt = new Collision();
    private List<Collision> m_levelAt = new List<Collision>();

    private Vector3 m_move, m_moveInput;

    private float m_xAng = 0.0f, m_yAng = 0.0f;

    private int m_lastWallID = 0;
    
    private bool m_grounded = false, m_walled = false, m_levelTouch = false, m_jumping = false;

    //Utility functions
    private float smoothstep(float edge0, float edge1, float x)
    {
        // Scale, bias and saturate x to 0..1 range
        x = clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
        // Evaluate polynomial
        return x * x * (3 - 2 * x);
    }

    private float clamp(float x, float lowerlimit, float upperlimit)
    {
        if (x < lowerlimit)
            x = lowerlimit;
        if (x > upperlimit)
            x = upperlimit;
        return x;
    }

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
            m_rigidbody.velocity = new Vector3(Mathf.Lerp(m_rigidbody.velocity.x, m_move.x * m_groundSpeed, 0.01f), m_rigidbody.velocity.y + Physics.gravity.y *  0.5f * Time.fixedDeltaTime,
                Mathf.Lerp(m_rigidbody.velocity.z, m_move.z * m_groundSpeed, 0.01f));
        }
        else
        {
            //m_rigidbody.velocity = new Vector3(Mathf.Lerp(m_rigidbody.velocity.x, m_move.x * m_airSpeed, 0.05f), m_rigidbody.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 
            //    Mathf.Lerp(m_rigidbody.velocity.z, m_move.z * m_airSpeed, 0.05f));

            Vector3 tarVel = new Vector3(m_move.x * m_airSpeed, m_rigidbody.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, m_move.z * m_airSpeed);
            m_rigidbody.velocity = Vector3.Lerp(new Vector3(m_rigidbody.velocity.x, m_rigidbody.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, m_rigidbody.velocity.z),
                tarVel, 0.05f * m_move.magnitude);
        }
    }

    public void Move(Vector2 move, Vector2 look)
    {
        m_move = transform.TransformVector(new Vector3(move.normalized.x, 0.0f, move.normalized.y));
        m_moveInput = m_move; //Inteded move direction
        
        //No sticking to level with air control
        if(m_levelTouch)
        {
            Vector3 intoLevel = Vector3.Project(m_move, m_levelAt[m_levelAt.Count - 1].contacts[0].normal);
            m_move += m_levelAt[m_levelAt.Count - 1].contacts[0].normal * intoLevel.magnitude;
        }

        //Make wall running "sticky" (make it easier to wall jump/bounce)...
        if(m_walled)
        {
            Vector3 awayFromWall = Vector3.Project(m_move, m_wallAt.contacts[0].normal);
            m_move -= m_wallAt.contacts[0].normal * awayFromWall.magnitude * 0.75f;
        }

        m_xAng += m_turnSpeed * -look.y;
        m_yAng += m_turnSpeed * look.x;
    }

    public void Jump()
    {        
        if(!m_jumping)
        {
            m_rigidbody.useGravity = true;
            m_jumping = true;
            m_grounded = false;            
            if (m_walled)
            {
                //Debug.Log("wall jump!");
                m_rigidbody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);

                Vector3 intoWall = Vector3.Project(m_moveInput, m_wallAt.contacts[0].normal);
                float wallJump = 1.0f - smoothstep(0.5f, 0.9f, intoWall.magnitude * (1.0f - clamp(Vector3.Dot(intoWall, m_wallAt.contacts[0].normal), 0.0f, 1.0f)));

                m_rigidbody.AddForce(m_wallAt.contacts[0].normal * m_jumpForce * wallJump, ForceMode.Impulse);                
            }
            else
            {
                //Debug.Log("ground jump!");
                m_rigidbody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
            }

            //Jumping off moving platform; maintain momementum
            if(transform.parent != null)
            {
                BasicSpinner spinner = transform.parent.GetComponent<BasicSpinner>();
                if(spinner != null)
                {
                    m_rigidbody.velocity += spinner.GetVelocityAt(m_groundAt.point);
                }

                BasicMover mover = transform.parent.GetComponent<BasicMover>();
                if(mover != null)
                {
                    m_rigidbody.velocity += mover.GetVelocity();
                }

                if(m_groundAt.collider != null)
                {
                    Rigidbody body = m_groundAt.collider.GetComponent<Rigidbody>();
                    if (body != null)
                    {
                        m_rigidbody.velocity += body.velocity;
                    }
                }

                transform.parent = null;
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

                //Land on moving platform
                if (m_groundAt.collider.gameObject.tag == "MovingPlatform")
                {
                    transform.parent = m_groundAt.transform.parent;                    
                }
                //Step off moving platform (does not maintain momentum from moving platform...)
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
