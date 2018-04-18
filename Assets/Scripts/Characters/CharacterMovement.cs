using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]

public class CharacterMovement : MonoBehaviour {


	Animator animator;
	CharacterController charactercontroller;
	bool Jumping;
	bool resetGravity;
	float gravity;
	bool isGrounded = true;
	[System.Serializable]
	public class AnimationSettings
	{
		public string verticalVelocityfloat = "Forward";
		public string horizontalVelocityfloat = "Strafe";
		public string groundedBool = "isGrounded";
		public string jumpBool = "isJumping";
	}
	[SerializeField]
	public AnimationSettings animations;

	[System.Serializable]
	public class PhysicsSettings
	{
		public float gravityModifier = 9.8f;
		public float baseGravity = 50.0f;
		public float resetGravityValue = 1.2f;
	}
	[SerializeField]
	public PhysicsSettings physics;

	[System.Serializable]
	public class MovementSettings
	{
		public float jumpSpeed = 6f;
		public float jumpTime = 0.25f;

	}
	[SerializeField]
	public MovementSettings movements;

	/* void Awake()
	{
		animator = GetComponent<Animator> ();
		SetupAnimator ();

	} */


	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		charactercontroller = GetComponent<CharacterController> ();
		SetupAnimator ();
	}

	// setup animator with the child avatar
	void SetupAnimator()
	{
		Animator wantedAnim = GetComponentsInChildren<Animator>()[1];

		Avatar wantedAvatar = wantedAnim.avatar;

		animator.avatar = wantedAvatar;

		Destroy (wantedAnim);
	}
	// Update is called once per frame
	void Update () 
	{
		ApplyGravity();
		isGrounded = charactercontroller.isGrounded;
	}


	//Animate the character and root motion controls the character
	public void Animate( float forward , float strafe)
	{
		animator.SetFloat (animations.verticalVelocityfloat, forward);
		animator.SetFloat (animations.horizontalVelocityfloat, strafe);
		animator.SetBool (animations.groundedBool, isGrounded);
		animator.SetBool (animations.jumpBool, Jumping);
	}

	 public void ApplyGravity()
	{
		if (!charactercontroller.isGrounded)
		{
		
			if (!resetGravity) {
				gravity = physics.resetGravityValue;
				resetGravity = true;
			}
			gravity += physics.gravityModifier * Time.deltaTime; 
		}
		else
		{
			gravity = physics.baseGravity;  
			resetGravity = false;	
		}
		Vector3 gravityvector = new Vector3 ();
		if (!Jumping) {
			gravityvector.y -= gravity;
		}
		else
		{
			gravityvector.y = movements.jumpSpeed;
		}
		charactercontroller.Move (gravityvector * Time.deltaTime);
	}
	public void Jump()
	{
		if (Jumping)
			return;
		if (isGrounded) 
		{
			Jumping = true;
			StartCoroutine (StopJump ());
		}
	}
	IEnumerator StopJump()
	{
		yield return new WaitForSeconds (movements.jumpTime);
		Jumping = false;
	}
}
