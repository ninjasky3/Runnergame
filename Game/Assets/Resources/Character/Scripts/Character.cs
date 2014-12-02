using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	[SerializeField] float jumpPower = 12;								// determines the jump force applied when jumping (and therefore the jump height)
	[SerializeField] float airSpeed = 6;								// determines the max speed of the character while airborne
	//[SerializeField] float airControl = 2;								// determines the response speed of controlling the character while airborne
	[Range(1,4)] [SerializeField] public float gravityMultiplier = 2;	// gravity modifier - often higher than natural gravity feels right for game characters
	//[SerializeField][Range(0.1f,3f)] float moveSpeedMultiplier = 1;	    // how much the move speed of the character will be multiplied by
	//[SerializeField][Range(0.1f,3f)] float animSpeedMultiplier = 1;	    // how much the animation of the character will be multiplied by
	[SerializeField] AdvancedSettings advancedSettings;
	
	public static Character Instance;									// Creating an instance of the Character script to be accesed from other points
	public Vector3 MoveModifier { get; set;}
	public Vector3 MoveVector { get; set;}
	public float VerticalVel { get; set;}

	[System.Serializable]


	public class AdvancedSettings
	{

		public float stationaryTurnSpeed = 180;				// additional turn speed added when the player is stationary (added to animation root rotation)
		public float movingTurnSpeed = 360;					// additional turn speed added when the player is moving (added to animation root rotation)
		public float headLookResponseSpeed = 2;				// speed at which head look follows its target
		public float crouchHeightFactor = 0.6f; 			// collider height is multiplied by this when crouching
		public float crouchChangeSpeed = 4;					// speed at which capsule changes height when crouching/standing
		public float autoTurnThresholdAngle = 100;			// character auto turns towards camera direction if facing away by more than this angle
		public float autoTurnSpeed = 2;						// speed at which character auto-turns towards cam direction
		public PhysicMaterial zeroFrictionMaterial;			// used when in motion to enable smooth movement
		public PhysicMaterial highFrictionMaterial;			// used when stationary to avoid sliding down slopes
		public float jumpRepeatDelayTime = 0.25f;			// amount of time that must elapse between landing and being able to jump again
		public float runCycleLegOffset = 0.2f;				// animation cycle offset (0-1) used for determining correct leg to jump off
		public float groundStickyEffect = 5f;				// power of 'stick to ground' effect - prevents bumping down slopes.
	}

	bool onGround;                                          // Is the character on the ground
	Vector3 currentLookPos;                                 // The current position where the character is looking
	float originalHeight;                                   // Used for tracking the original height of the characters capsule collider
	float lastAirTime;                                      // USed for checking when the character was last in the air for controlling jumps
	Animator animator;                                      // The animator for the character
	CapsuleCollider capsule;                                // The collider for the character
	const float half = 0.5f;                                // whats it says, it's a constant for a half
	Vector3 moveInput;
	bool crouchInput;
	bool jumpInput;
	//float turnAmount;
	//float forwardAmount;
	Vector3 velocity;
	IComparer rayHitComparer;

	void OnAwake(){

		Instance = this;
	}

	void Start(){
		
		animator = GetComponentInChildren<Animator>();
		capsule = collider as CapsuleCollider;
		
		// as can return null so we need to make sure thats its not before assigning to it
		if (capsule != null) {
			originalHeight = capsule.height;
			capsule.center = Vector3.up * originalHeight * half;
		} else {
			Debug.LogError(" collider cannot be cast to CapsuleCollider");
		}
		
		rayHitComparer = new RayHitComparer ();
		
		SetUpAnimator();
	}
	public void Move(Vector3 move, bool jump, bool crouch){
		
		if (move.magnitude > 1) move.Normalize();
		Debug.Log(move);
		this.moveInput = move;
		this.crouchInput = crouch;
		this.jumpInput = jump;

		velocity = rigidbody.velocity;

		ConvertMoveInput ();
		
		PreventStandingInLowHeadroom ();
		
		ScaleCapsuleForCrouching ();
		
		GroundCheck ();

		HandleGroundedVelocities();

		rigidbody.velocity = velocity;	

	}
	
	void ConvertMoveInput ()
	{
		//Vector3 localMove = transform.InverseTransformDirection (moveInput);
		//turnAmount = Mathf.Atan2 (localMove.x, localMove.z);
		//forwardAmount = localMove.z;
	}

	void PreventStandingInLowHeadroom ()
	{
		if (!crouchInput) {
			Ray crouchRay = new Ray (rigidbody.position + Vector3.up * capsule.radius * half, Vector3.up);
			float crouchRayLength = originalHeight - capsule.radius * half;
			if (Physics.SphereCast (crouchRay, capsule.radius * half, crouchRayLength)) {
				crouchInput = true;
			}
		}
	}	

	void ScaleCapsuleForCrouching ()
	{
		// scale the capsule collider according to
		// if crouching ...
		if (onGround && crouchInput && (capsule.height != originalHeight * advancedSettings.crouchHeightFactor)) {
			capsule.height = Mathf.MoveTowards (capsule.height, originalHeight * advancedSettings.crouchHeightFactor, Time.deltaTime * 4);
			capsule.center = Vector3.MoveTowards (capsule.center, Vector3.up * originalHeight * advancedSettings.crouchHeightFactor * half, Time.deltaTime * 2);
		}
		// ... everything else 
		else
		if (capsule.height != originalHeight && capsule.center != Vector3.up * originalHeight * half) {
			capsule.height = Mathf.MoveTowards (capsule.height, originalHeight, Time.deltaTime * 4);
			capsule.center = Vector3.MoveTowards (capsule.center, Vector3.up * originalHeight * half, Time.deltaTime * 2);
		}
	}
	
	void GroundCheck ()
	{
		Ray ray = new Ray (transform.position + Vector3.up * .1f, -Vector3.up);
		RaycastHit[] hits = Physics.RaycastAll (ray, .5f);
		System.Array.Sort (hits, rayHitComparer);
		
		if (velocity.y < jumpPower * .5f) {
			onGround = false;
			rigidbody.useGravity = true;
			foreach (var hit in hits) {
				if (!hit.collider.isTrigger) {
					if (velocity.y <= 0) {
						rigidbody.position = Vector3.MoveTowards (rigidbody.position, hit.point, Time.deltaTime * 5);
					}
					
					onGround = true;
					rigidbody.useGravity = false;
					break;
				}
			}
		}

		if (!onGround) lastAirTime = Time.time;
	}
	void HandleGroundedVelocities()
	{
		
		velocity.y = 0;
		
		if (moveInput.magnitude == 0) {
			velocity.x = 0;
			velocity.z = 0;
		}
		bool animationGrounded = animator.GetCurrentAnimatorStateInfo (0).IsName ("Grounded");
		bool okToRepeatJump = Time.time > lastAirTime + advancedSettings.jumpRepeatDelayTime;
		
		if (jumpInput && !crouchInput && okToRepeatJump && animationGrounded) {
			// jump!
			onGround = false;
			velocity = moveInput * airSpeed;
			velocity.y = jumpPower;
		}
	}
	
	void SetUpAnimator()
	{
		animator = GetComponent<Animator>();

		foreach (var childAnimator in GetComponentsInChildren<Animator>()) {
			if (childAnimator != animator) {
				animator.avatar = childAnimator.avatar;
				Destroy (childAnimator);
				break;
			}
		}
	}
	
	class RayHitComparer: IComparer
	{
		public int Compare(object x, object y)
		{
			return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
		}	
	}

}