using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
	// Movements
	 public float m_JumpForce = 400f;                          // Amount of force added when the player jumps.
	[Range(0, 1)]  public float m_CrouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, .3f)]  public float m_MovementSmoothing = .05f;  // How much to smooth out the movement
	public bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
	public LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
	public Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
	public Transform m_CeilingCheck;                          // A position marking where to check for ceilings
	[SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching
	public Transform m_FrontCheck;                           // A position marking where to check if the player is touching the wall.

	// Jump and wall jump part
	public float m_frontPointRadius;
	public float m_xWallForce;
	public float m_yWallForce;
	public float m_WallJumpTime;
	private bool m_TouchingFront;
	public bool m_WallJumping;


	const float k_CheckBoxRadius = 0.05f; // Radius of the overlap circle to determine if grounded, touching wall and touching ceil
	private bool m_Grounded;            // Whether or not the player is grounded.
	private Rigidbody2D m_Rigidbody2D;
	private Animator animator;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 velocity = Vector3.zero;

	private float speed;
	private bool m_Crouched;            // Whether or not the player is grounded.

	//Attack part
	public Transform attackPoint;
	public float attackRange = 0.5f;
	public LayerMask enemyLayers;
	public int attackDamage = 40;
	public float attackRate = 2f;
	float nextAttackTime = 0f;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		m_Grounded = false;
	}

	private void Update()
	{
		
		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_CheckBoxRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
				m_Grounded = true;
		}
		if (Time.time >= nextAttackTime)
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				animator.SetTrigger("Attack");
				nextAttackTime = Time.time + 1f / attackRate;
			}
		}


		SetAnimation();
	}

	public void Attack()
    {
		Collider2D[] hitEnemy =  Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
		foreach (Collider2D enemy in hitEnemy)
        {
			Debug.Log("Enemy hit");
			enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
		if(attackPoint == null)
        {
			return;
        }
		Gizmos.DrawWireSphere(attackPoint.position, attackRange);
		if (m_GroundCheck == null)
		{
			return;
		}
		Gizmos.DrawWireSphere(m_GroundCheck.position, k_CheckBoxRadius);
	}
    public void Move(float move, bool crouch, bool jump)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CheckBoxRadius, m_WhatIsGround))
			{
				crouch = true;
			}
		}

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{

			// If crouching
			if (crouch)
			{
				// Reduce the speed by the crouchSpeed multiplier
				move *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			}
			else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;
			}

			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, m_MovementSmoothing);

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}
		// If the player should jump...
		if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
		}

		// wall jump
		m_TouchingFront = Physics2D.OverlapCircle(m_FrontCheck.position, k_CheckBoxRadius, m_WhatIsGround);
		if (m_TouchingFront == true && m_Grounded == false && jump == true)
		{
			m_WallJumping = true;
			Invoke("SetWallJumpingToFalse", m_WallJumpTime);
		}
		if (m_WallJumping == true)
        {
			m_Rigidbody2D.velocity = new Vector2(m_xWallForce * -move, m_yWallForce);

		}
		m_Crouched = crouch;
	}

	private void SetWallJumpingToFalse()
    {
		m_WallJumping = false;
	}
	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	private void SetAnimation ()
    {
		//Debug.Log(m_Grounded);
		if (m_Grounded == true)
		{
            if (m_Crouched)
            {
				animator.SetBool("Is_Crouched", true);
			}
            else
            {
				speed = Mathf.Abs(m_Rigidbody2D.velocity.x);
				animator.SetFloat("Speed", speed);
				animator.SetBool("Is_Crouched", false);
			}
			animator.SetBool("Is_Falling", false);
			animator.SetBool("Is_Jumping", false);
		}
		else
		{
			if (m_Rigidbody2D.velocity.y > 0.1)
			{
				animator.SetBool("Is_Jumping", true);
			}
			else if (m_Rigidbody2D.velocity.y < -0.1)
			{
				animator.SetBool("Is_Falling", true);
			}
            else
            {
				animator.SetBool("Is_Jumping", false);
				animator.SetBool("Is_Falling", false);
			}
		}
	}
}