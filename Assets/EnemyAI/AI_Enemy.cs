using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(Animator))]
public class AI_Enemy : MonoBehaviour {

	[System.Serializable]
	public class WaypointBase 
	{
		public Transform destination;
		public float waitTime;
		public Transform lookAtTarget;
	}

	private NavMeshAgent navmesh;
	private CharacterMovement characterMove { get { return GetComponent<CharacterMovement> (); } set { characterMove = value; } }
	private Animator animator { get { return GetComponent<Animator> (); } set { animator = value; } }
	public enum AIState { Patrol, Attack, FindCover }
	public AIState aiState;

	[System.Serializable]
	public class PatrolSettings
	{
		public WaypointBase[] waypoints;
	}
	public PatrolSettings patrolSettings;

	private float currentWaitTime;
	private int waypointIndex;
	private Transform currentLookTransform;
	private bool walkingToDest;


	private float forward;

	// Use this for initialization
	void Start () {

		navmesh = GetComponentInChildren<NavMeshAgent> ();

		if (navmesh == null) {
			Debug.LogError ("We need a navmesh to traverse the world with.");
			enabled = false;
			return;
		}

		if (navmesh.transform == this.transform) {
			Debug.LogError ("The navmesh agent should be a child of the character: " + gameObject.name);
			enabled = false;
			return;
		}


		navmesh.speed = 0;
		navmesh.acceleration = 0;
		navmesh.autoBraking = false;

		if (navmesh.stoppingDistance == 0) {
			Debug.Log ("Auto settings stopping distance to 1.3f");
			navmesh.stoppingDistance = 1.3f;
		}

	}
	
	// Update is called once per frame
	void Update () {

		//TODO: Animate the strafe when the enemy is trying to shoot us.
		characterMove.Animate (forward, 0);
		navmesh.transform.position = transform.position;

		switch (aiState) {
		case AIState.Patrol:
			Patrol ();
			break;
		}

	}

	void Patrol () {

		if (!navmesh.isOnNavMesh) {
			Debug.Log ("We're off the navmesh");
			return;
		}

		if (patrolSettings.waypoints.Length == 0) {
			return;
		}

		navmesh.SetDestination (patrolSettings.waypoints [waypointIndex].destination.position);

		if (navmesh.remainingDistance <= navmesh.stoppingDistance) {

			forward = LerpSpeed (forward, 0, 5);
			waypointIndex = (waypointIndex + 1) % patrolSettings.waypoints.Length;
			currentWaitTime -= Time.deltaTime;

			if (patrolSettings.waypoints [waypointIndex].lookAtTarget != null)
			currentLookTransform = patrolSettings.waypoints [waypointIndex].lookAtTarget;
			walkingToDest = false;
			if (currentWaitTime <= 0) {
				waypointIndex = (waypointIndex + 1) % patrolSettings.waypoints.Length;
			}

			}

		else {
			walkingToDest = true;
			forward = LerpSpeed (forward, 0.5f, 5);
			//currentWaitTime = patrolSettings.waypoints [waypointIndex].waitTime;
			//currentLookTransform = null;
			LookAtPosition(navmesh.steeringTarget);
			currentWaitTime = patrolSettings.waypoints [waypointIndex].waitTime;
		}


	}


	float LerpSpeed (float curSpeed, float destSpeed, float time) {
		curSpeed = Mathf.Lerp (curSpeed, destSpeed, Time.deltaTime * time);
		return curSpeed;
	}

	void LookAtPosition (Vector3 pos) {
		Vector3 dir = pos - transform.position;
		Quaternion lookRot = Quaternion.LookRotation (dir);
		lookRot.x = 0;
		lookRot.z = 0;
		transform.rotation = Quaternion.Lerp (transform.rotation, lookRot, Time.deltaTime * 5);
	}

	void OnAnimatorIK () {
		if (currentLookTransform != null && !walkingToDest) {
			animator.SetLookAtPosition (currentLookTransform.position);
			animator.SetLookAtWeight (1, 0, 0.5f, 0.7f);
		} else {
			animator.SetLookAtWeight (0);
		}
	}

}
