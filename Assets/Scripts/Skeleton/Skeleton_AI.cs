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
    public float attackRange = 1.0f;

    /******************************************/
    /*-------------- Animation ---------------*/
    public bool isChasing = false;
    public bool isReacting = false;
    public bool isIdle = false;
    public bool isAttacking = false;
    public bool isWalking = false;

    public Vector2 destDistance = new Vector2(5f, 0);

    // local variables
    [SerializeField]
    private Rigidbody2D rb2d;
    [SerializeField]
    private bool movingForward = true;
    [SerializeField]
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
        Vector2 localScale = rb2d.transform.localScale;
        localScale.x = 1;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (isIdle || animator.GetBool("Is_Dead"))
            return;
        step = speed * Time.deltaTime;
        playerSeen = CanSeePlayer(viewRange);

        if (playerSeen && (!isChasing) && (!isReacting))
        {
            animator.SetTrigger("React");
        }
        else
        {
            if (!isChasing)
            {
                if (isWalking)
                {
                    if (!movingForward)
                    {
                        rb2d.position = Vector2.MoveTowards(rb2d.position, rb2d.position + destDistance, step);
                        Debug.Log("rb2d.position = " + rb2d.transform.position + "  rb2d.position + destDistance = " + (rb2d.position + destDistance));
                        if (rb2d.position.x >= (initialPosition + destDistance).x)
                        {
                            Flip();
                            movingForward = true;
                        }
                    }
                    else
                    {
                        rb2d.position = Vector2.MoveTowards(rb2d.position, initialPosition, step);
                        if (rb2d.position.x <= initialPosition.x)
                        {
                            Flip();
                            movingForward = false;
                        }
                    }
                }
            }
            else
            {
                Chase();

                // escape chase if player too far away
                distance = Vector2.Distance(transform.position, player.transform.position);
                //the player has fleen
                if (Mathf.Abs(distance) > escapeDistance)
                {
                    isChasing = false;
                }
                //attack player
                else if (distance <= attackRange)
                {
                    animator.SetTrigger("Attack");
                }
            }
        }
    }

    public void Chase ()
    {
        if (!isAttacking)
        {
            rb2d.position = Vector2.MoveTowards(rb2d.position, new Vector2(player.position.x, transform.position.y), step);
        }
        if (transform.position.x < player.position.x)
        {
            transform.localScale = new Vector2(1, 1);
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
        }
        
    }
    public bool CanSeePlayer(float distance)
    {
        float castdistance = distance;
        Vector2 localScale = rb2d.transform.localScale;
        if (localScale.x != 1)
        {
            castdistance = -distance;
        }
        Vector2 endPos = rb2d.transform.position + Vector3.right * castdistance;

        RaycastHit2D hit = Physics2D.Linecast(rb2d.transform.position, endPos, 1 << LayerMask.NameToLayer("Action"));
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
