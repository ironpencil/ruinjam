using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {

	public float crawlModifier = 0.5f;
	public float walkSpeed = 2;
	public float runSpeed = 6;
	public float initialJumpForce = 5;
	public float holdJumpForce = 0.2f;
	public float maxJumpTime = 0.5f;
	public float jumpDecay = 0.25f;
	public Transform[] groundChecks;

	public Animator animator;

	public PhysicsMaterial2D noFrictionMaterial;

	//public Transform lookUpTarget;
	//public Transform lookDownTarget;
	//public float lookDelay = 2.0f;

	private bool beginJump = false;
	private bool jumping = false;
	private bool running = false;
	private bool crawling = false;
	private float jumpTime = 0.0f;
	private float currentJumpForce = 0.0f;

	private bool grounded = false;
	private PhysicsMaterial2D originalMaterial;

	//private bool lookingDown = false;
	//private bool lookingUp = false;

	//private float lookTime = 0.0f;


	// Use this for initialization
	void Start () {

		//groundCheck = transform.Find ("groundCheck");
		originalMaterial = collider2D.sharedMaterial;
		//noFrictionMaterial = Resources.Load ("Materials/NoFriction");
	
	}
	
	// Update is called once per frame
	void Update () {

		grounded = false;

		float groundCheckModifier = 1.0f;

		foreach (var groundCheck in groundChecks) {
			if (crawling) {
				groundCheck.localPosition = new Vector3(groundCheck.localPosition.x, -0.22f, 0f);
			} else {
				groundCheck.localPosition = new Vector3(groundCheck.localPosition.x, -0.337758f, 0f);
			}

			if (Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"))) {
				grounded = true;
				break;
			}
		}
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		//grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  

		if (Input.GetButton ("Run")) {
			running = true;
		} else {
			running = false;
		}

		// If the jump button is pressed and the player is grounded then the player should jump.
		if(Input.GetButtonDown("Jump") && grounded) {
		//if(Input.GetAxis("Jump") > 0.0f && grounded) {
			beginJump = true;
		}

		if (grounded && !beginJump && !jumping) {
			//we're on the ground, we're not jumping, make sure our collision material is correct
			if (collider2D.sharedMaterial != originalMaterial) {
				collider2D.sharedMaterial = originalMaterial;

				// seems to be a bug that currently requires the collider to be reset to refresh material
				collider2D.enabled = !collider2D.enabled;
				collider2D.enabled = !collider2D.enabled;
			}
		}

	}

	void FixedUpdate () {

		float directionY = Input.GetAxis ("Vertical");

		if (directionY <= -0.5f) {
			crawling = true;
		} else {
			crawling = false;
		}
	
		float directionX = Input.GetAxis ("Horizontal");

		float desiredVelocityX = 0.0f;

		float moveSpeed;

		if (running) {
			moveSpeed = runSpeed;
		} else {
			moveSpeed = walkSpeed;
		}

		if (crawling) {
			//account for joystick being less horizontal if crouching to get max speed
			directionX = Mathf.Min(1.0f, Mathf.Abs(directionX) * 2) * Mathf.Sign(directionX);

			//modify move speed by crawling factor
			moveSpeed = moveSpeed * crawlModifier;
			((BoxCollider2D)collider2D).size = new Vector2(0.22f, 0.13f);
		} else {
			((BoxCollider2D)collider2D).size = new Vector2(0.22f, 0.4f);
		}


			desiredVelocityX = moveSpeed * directionX;

		if (animator) {

			animator.SetBool("isCrawling", crawling);

			if (desiredVelocityX != 0.0f) {
				animator.SetBool("isWalking", true);
				animator.speed = Mathf.Abs(desiredVelocityX * 0.5f);

				float velocityDirection = Mathf.Sign(desiredVelocityX);
				if (velocityDirection != Mathf.Sign(this.transform.localScale.x)) {
					this.transform.localScale = new Vector3(this.transform.localScale.x * -1, this.transform.localScale.y);
				}
			} else {
				animator.SetBool("isWalking", false);
			}

		}
		//}

		float velocityX = Mathf.Lerp(rigidbody2D.velocity.x, desiredVelocityX, Time.fixedDeltaTime * 10);
		float velocityY = rigidbody2D.velocity.y;	
		
		//rigidbody2D.velocity = new Vector2(velocityX, velocityY);

		if (beginJump) {
			jumpTime = Time.time;
			jumping = true;
			currentJumpForce = holdJumpForce;
			Debug.Log("Jump! Adding force: " + initialJumpForce);

			velocityY += initialJumpForce;
			//currentJumpForce = currentJumpForce * 0.1f;

			collider2D.sharedMaterial = noFrictionMaterial;
			
			// seems to be a bug that currently requires the collider to be reset to refresh material
			collider2D.enabled = !collider2D.enabled;
			collider2D.enabled = !collider2D.enabled;
		} else if (jumping) {
			//bool addJumpForce = CanAddJumpForce ();

//			if (!Input.GetButton ("Jump") ||
//		    (Time.time - jumpTime > maxJumpTime)) {
//
//				addJumpForce = false;
//			}

			float jumpDuration = Time.time - jumpTime;
				//if (rigidbody2D

			if (CanAddJumpForce(jumpDuration) && currentJumpForce > 0.0f) {

				velocityY += currentJumpForce;
				Debug.Log ("Jumping... Adding force: " + currentJumpForce);

				//currentJumpForce = currentJumpForce * 0.5f * Time.fixedDeltaTime;
				//currentJumpForce = Mathf.Lerp(currentJumpForce, 0.0f, Time.fixedDeltaTime * rigidbody2D.gravityScale);
				currentJumpForce = currentJumpForce - (Time.fixedDeltaTime * rigidbody2D.gravityScale * rigidbody2D.mass * jumpDecay);
				//currentJumpForce = Mathf.Lerp (currentJumpForce, 0.0f, Time.fixedDeltaTime * rigidbody2D.gravityScale * jumpDuration);
			//rigidbody2D.AddForce(new Vector2(0.0f, jumpForce));

			} else {
				jumping = false;
				currentJumpForce = 0.0f;
			}

			//beginJump = false;
		}

		beginJump = false;


		rigidbody2D.velocity = new Vector2(velocityX, velocityY);

		Debug.Log("Velocity = " + rigidbody2D.velocity.ToString());


	}

	private bool CanAddJumpForce(float jumpDuration) {

		//if they stopped pressing Jump, stop jumping
		if (!Input.GetButton ("Jump")) {
			return false;
		}

		//if they are at max air time, stop jumping
		if (jumpDuration > maxJumpTime) {			
			return false;
		}

		//if their jump was stopped (due to colliding with something)
		if ( (rigidbody2D.velocity.y <= 0) && !beginJump) {
			return false;
		}

		return true;
	}

//	void OnCollisionEnter2D(Collision2D other) {
//		if (jumping) {
//			jumping = false;
//		}
//	}



}
