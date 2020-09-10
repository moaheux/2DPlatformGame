using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Skeleton_AI : MonoBehaviour
{
    //Scripts
    public EnemyWeapon enemyWeapon;

    //Extern Reference
    public Rigidbody2D player;
     
    public float viewRange = 1.0f;
    public float speed = 0.5f;
    public float step = 0.0f;
    public float escapeDistance = 2.0f;
    public bool isChasing = false;
    public bool isReacting = false;

    public Vector2 destDistance = new Vector2(5f, 0);


    // local variables
    [SerializeField]
    private Rigidbody2D rb2d;
    [SerializeField]
    private bool movingForward;

    private bool playerSeen = false;

    [SerializeField]
    private Vector2 initialPosition;
    private Animator animator;
    private float distance;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        initialPosition = rb2d.position;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        step = speed * Time.deltaTime;
        playerSeen = CanSeePlayer(viewRange);
        if (playerSeen && (isChasing == false) && (isReacting == false))
        {
            animator.SetTrigger("React");
            rb2d.velocity = Vector2.zero;
        }
        else
        { 
            if ((isChasing == false))
            {
                if (isReacting == false)
                {
                    if (movingForward)
                    {
                        rb2d.transform.position = Vector2.MoveTowards(rb2d.position, rb2d.position + destDistance, step);
                        if (rb2d.position.x >= (initialPosition + destDistance).x)
                        {
                            movingForward = false;
                            Flip();
                        }
                    }
                    else
                    {
                        rb2d.transform.position = Vector2.MoveTowards(rb2d.position, initialPosition, step);
                        if (rb2d.position.x <= initialPosition.x)
                        {
                            movingForward = true;
                            Flip();
                        }
                    }
                }
            }
            else
            {
                Chase();
                // escape chase if player too far away
                distance = Vector2.Distance(transform.position, player.transform.position);
                if (Mathf.Abs(distance) > escapeDistance)
                {
                    isChasing = false;
                }
            }
        }
    }

    public void Chase ()
    {
        if(transform.position.x < player.position.x)
        {
            rb2d.velocity = new Vector2(speed, 0);
            transform.localScale = new Vector2(1, 1);
        }
        else
        {
            rb2d.velocity = new Vector2(-speed, 0);
            transform.localScale = new Vector2(-1, 1);
        }
    }
    public bool CanSeePlayer(float distance)
    {
        float castdistance = distance;
        Vector2 localScale = rb2d.transform.localScale;
        if (localScale.x == 1)
            movingForward = true;
        else
            movingForward = false;

        if (!movingForward)
        {
            castdistance = -distance;
        }
        Vector2 endPos = rb2d.transform.position + Vector3.right * castdistance;

        RaycastHit2D hit = Physics2D.Linecast(rb2d.transform.position, endPos, 1 << LayerMask.NameToLayer("Action"));
        Debug.DrawRay(transform.position, endPos, Color.green);
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }
    private void Flip()
    {
        // Multiply the player's x local scale by -1.
        Vector3 theScale = rb2d.transform.localScale;
        theScale.x *= -1;
        rb2d.transform.localScale = theScale;
    }
}
