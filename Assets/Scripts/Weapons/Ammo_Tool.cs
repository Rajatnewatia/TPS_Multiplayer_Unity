using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Ammo_Tool : MonoBehaviour {

	Collider col;
	Rigidbody rigidBody;
	Animator animator;
	SoundController sc;

	public static Camera ammo_cam;
	public GameObject blood;

	public enum WeaponType
	{
		Pistol,Rifle 
	}

	public WeaponType weaponType;

	[System.Serializable]
	public class UserSettings
	{
		public Transform leftHandIKTarget;
		public Vector3 spineRotation;
	}

	[SerializeField]
	public UserSettings userSettings;

	[System.Serializable]
	public class WeaponSettings
	{
		[Header(" -Bullet Options- ")]
		public Transform bulletSpwan;
		public float fireRate = 0.2f;
		public float damage = 5.0f;
		public float bulletSpread = 5.0f;
		public float range = 200.0f;
		public LayerMask bulletLayers;

		[Header("-Effects-")]
		public GameObject muzzleFlash ;
		public GameObject decal;
		public GameObject clip;
		public GameObject shell;

		[Header(" -Other- ")]
		public GameObject crosshairPrefab;
		public Transform shellEjectSpot;
		public GameObject clipGO;
		public float reloadDuration = 2.0f;
		public Transform clipEjectPos;
		public float  shellEjectSpeed = 7.5f;

		[Header("-Positioning-")]
		public Vector3 equipPosition ;
		public Vector3 equipRotation ;
		public Vector3 unequipPosition ;
		public Vector3 unequipRotation ;

		[Header(" -Animations-")]
		public bool useAnimation;
		public int fireAnimationLayer ;
		public string fireAnimationName = "fire" ;


	}

	[SerializeField]
	public WeaponSettings weaponSettings;

	[System.Serializable]
	public class Ammunition
	{
		public int carryingAmmo;
		public int clipAmmo;
		public int maxClipAmmo;
	}
	[SerializeField]
	public Ammunition ammo;

	public Ray shootRay{ protected get ; set ; } 
	public bool ownerAiming{  get ; set ; } 

	Ammo_Tool_Handler owner;
	bool equipped;
	bool pullingTrigger;
	bool resetingCartridge;

	[System.Serializable]
	public class SoundSettings {
		public AudioClip[] gunshotSounds;
		public AudioClip reloadSound;
		[Range(0, 3)] public float pitchMin = 1;
		[Range(0, 3)] public float pitchMax = 1.2f;
		public AudioSource audioS;
	}
	[SerializeField]
	public SoundSettings sounds;

	// Use this for initialization
	void Start () {
		//GameObject  cam = GameObject.FindGameObjectWithTag ("TPSCamera");
		GameObject check = GameObject.FindGameObjectWithTag ("Sound Controller");



		if (check != null) {
			sc = check.GetComponent<SoundController> ();
		}

		col = GetComponent<Collider>();
		rigidBody = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();

		if (weaponSettings.crosshairPrefab != null) 
		{
			weaponSettings.crosshairPrefab = Instantiate (weaponSettings.crosshairPrefab);
			ToggleCrosshair (false);
		}

	}

	// Update is called once per frame
	void Update () 
	{

		if (owner) 
		{
			DisableEnableComponents(false);
			if (equipped)
			{
				if (owner.userSettings.rightHand)
				{
					Equip();

					if (pullingTrigger) 
					{
						Fire ( shootRay);
					}

					if (ownerAiming) {
						PositionCrosshair (shootRay);
					}
					else
					{
						ToggleCrosshair (false);
					}
				}
			} 
			else
			{
				/*if (weaponSettings.bulletSpawn.childCount > 0)
				{
					foreach (Transform t in weaponSettings.bulletSpawn.GetComponentsInChildren<Transform>())
					{
						if (t != weaponSettings.bulletSpawn)
						{
							Destroy (t.gameObject);
						}
					}
				} */
				Unequip(weaponType);
			}

		}

		else 
		{ // If owner is null
			DisableEnableComponents(true);
			transform.SetParent(null);
			ownerAiming = false;
		}
	}

	//fires the weapon

	void Fire( Ray ray)
	{
		if (ammo.clipAmmo <= 0 || resetingCartridge || !weaponSettings.bulletSpwan)
			return;
		RaycastHit hit;
		Transform bSpawn  = weaponSettings.bulletSpwan ;
		Vector3 bspawnPoint = bSpawn.position;
		Vector3 dir = ray.GetPoint(weaponSettings.range) - bspawnPoint;

		//dir += (Vector3)Random.insideUnitCircle * weaponSettings.bulletSpread;

		if (Physics.Raycast(bspawnPoint, dir, out hit , weaponSettings.range, weaponSettings.bulletLayers)) 
		{
			hitEffects (hit);
		}
	
		#region muzzle flash
		if(weaponSettings.muzzleFlash)
		{
			Vector3 bulletSpawnPos = weaponSettings.bulletSpwan.position;
			GameObject muzzleFlash = Instantiate( weaponSettings.muzzleFlash , bulletSpawnPos , Quaternion.identity) as GameObject;
			Transform muzzleT = muzzleFlash.transform;
			muzzleT.SetParent(weaponSettings.bulletSpwan);
			Destroy(muzzleFlash , 1.0f);
		}
		#endregion

		#region shell
		if(weaponSettings.shell)
		{
			Vector3 shellEjectPos = weaponSettings.shellEjectSpot.position;
			Quaternion shellEjectRot = weaponSettings.shellEjectSpot.rotation;
			GameObject shell = Instantiate(weaponSettings.shell  , shellEjectPos ,shellEjectRot ) as GameObject;

			if(shell.GetComponent<Rigidbody>())
			{
				Rigidbody rigidB = shell.GetComponent<Rigidbody>();
				rigidB.AddForce( weaponSettings.shellEjectSpot.forward * weaponSettings.shellEjectSpeed , ForceMode.Impulse);

			}
			Destroy(shell , Random.Range(30.0f , 45.0f ));
		}
		#endregion

		if (sc == null) {
			return;
		}

		if (sounds.audioS != null) {
			if (sounds.gunshotSounds.Length > 0) {
				sc.InstantiateClip (
					weaponSettings.bulletSpwan.position, // Where we want to play the sound from
					sounds.gunshotSounds [Random.Range (0, sounds.gunshotSounds.Length)],  // What audio clip we will use for this sound
					2, // How long before we destroy the audio
					true, // Do we want to randomize the sound?
					sounds.pitchMin, // The minimum pitch that the sound will use.
					sounds.pitchMax); // The maximum pitch that the sound will use.
			}
		}


		if (weaponSettings.useAnimation)
			animator.Play (weaponSettings.fireAnimationName, weaponSettings.fireAnimationLayer);

		ammo.clipAmmo--;
		resetingCartridge = true;
		StartCoroutine (LoadNextBullet ());
	}

	IEnumerator LoadNextBullet()
	{
		yield return new WaitForSeconds (weaponSettings.fireRate);
		resetingCartridge = false;
	}


	void hitEffects(RaycastHit hit)
	{
		#region decal
		if(hit.collider.gameObject.isStatic)
		{
			if(weaponSettings.decal)
			{
				Vector3 hitpoint = hit.point;
				Quaternion lookRotation = Quaternion.LookRotation(hit.normal);
				GameObject decal = Instantiate( weaponSettings.decal , hitpoint , lookRotation ) as GameObject;
				Transform decalT = decal.transform;
				Transform hitT = hit.transform;
				decalT.SetParent(hitT);
				Destroy(decal , Random.Range(30.0f , 45.0f ));
			}
		}
		else if(!hit.collider.gameObject.isStatic)
		{
			Vector3 hitpoint = hit.point;
			Quaternion lookRotation = Quaternion.LookRotation(hit.normal);
			GameObject bloodsplash  = Instantiate( blood , hitpoint , lookRotation ) as GameObject;
			Destroy( bloodsplash , 5f) ;
		}
			
	    #endregion
	}

	void PositionCrosshair( Ray ray)
	{
		RaycastHit hit;
		Transform bSpawn  = weaponSettings.bulletSpwan ;
		Vector3 bspawnPoint = bSpawn.position;
		Vector3 dir = ray.GetPoint(weaponSettings.range) -  bspawnPoint;

		//dir += (Vector3)Random.insideUnitCircle * weaponSettings.bulletSpread;

		if (Physics.Raycast (bspawnPoint, dir, out hit, weaponSettings.range, weaponSettings.bulletLayers)) 
		{
			if (weaponSettings.crosshairPrefab != null) {
				ToggleCrosshair (true);
				weaponSettings.crosshairPrefab.transform.position = hit.point;
				weaponSettings.crosshairPrefab.transform.LookAt (ammo_cam.transform);
			} 
			else
			{
				ToggleCrosshair (false);
			}
		}
	}

	void ToggleCrosshair(bool enabled)
	{
		if (weaponSettings.crosshairPrefab != null) 
		{
			weaponSettings.crosshairPrefab.SetActive (enabled);
		}
	}




	//Disables or enables collider and rigidbody
	void DisableEnableComponents(bool enabled)
	{
		if(!enabled)
		{
			rigidBody.isKinematic = true;
			col.enabled = false;
		}
		else
		{
			rigidBody.isKinematic = false;
			col.enabled = true;
		}
	}

	void Equip()
	{
		if(!owner)
			return;
		if (!owner.userSettings.rightHand)
			return;
		transform.SetParent (owner.userSettings.rightHand);
		transform.localPosition = weaponSettings.equipPosition;
		Quaternion equipRot = Quaternion.Euler (weaponSettings.equipRotation);
		transform.localRotation = equipRot;
		
	}
	void  Unequip(WeaponType wpType)
	{
		if (!owner)
			return;
		switch (wpType) {
		case WeaponType.Pistol:
			transform.SetParent (owner.userSettings.pistolUnequipSpot);
			break;

		case WeaponType.Rifle:
			transform.SetParent (owner.userSettings.rifleUnequipSpot);
			break;
		}
		transform.localPosition = weaponSettings.unequipPosition;
		Quaternion uneEquipRot = Quaternion.Euler (weaponSettings.unequipRotation);
		transform.localRotation = uneEquipRot;
	}

	// loads the clip and calculates the ammo
	public void LoadClip()
	{
		int ammoNeeded = ammo.maxClipAmmo - ammo.clipAmmo;
		if (ammoNeeded >= ammo.carryingAmmo) {
			ammo.clipAmmo = ammo.carryingAmmo;
			ammo.carryingAmmo = 0;
		} 
		else
		{
			ammo.carryingAmmo -= ammoNeeded;
			ammo.clipAmmo = ammo.maxClipAmmo;
		}
	}

	public void SetEquipped( bool equip )
	{
		equipped = equip; 
	}
	public void PullTrigger(bool isPulling)
	{
		pullingTrigger = isPulling;

	}
	public void SetOwner(Ammo_Tool_Handler wp)
	{
		owner = wp;
	}
}
