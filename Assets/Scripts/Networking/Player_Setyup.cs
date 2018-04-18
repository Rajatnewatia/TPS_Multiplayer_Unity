using UnityEngine;
using UnityEngine.Networking;

public class Player_Setyup : NetworkBehaviour {

	[SerializeField]
	Behaviour[] componentstoDisable;

	Camera Scenecamera; 

	public GameObject cameraprefab;

	CameraRig cameraRig;

	[SerializeField]
	string remoteLayerName = "RemotePlayer";

	void Start()
	{
		if (isLocalPlayer) 
		{
			GameObject camprefab = Instantiate (cameraprefab, transform.position, transform.rotation) as GameObject;

			cameraRig = camprefab.GetComponent<CameraRig> ();

			camprefab.gameObject.tag = "PlayerCam";

			cameraRig.target = transform;

			Player_Input.TPScamera = cameraRig.mainCamera;
			Ammo_Tool.ammo_cam = cameraRig.mainCamera;
		}
		if (!isLocalPlayer) 
		{
			DisableComponents ();
			AssignRemoteLayer ();
	     	DisableCam ();
		}
		else
		{
			Scenecamera = Camera.main;
			if (Scenecamera != null) 
			{
				Scenecamera.gameObject.SetActive (false);

			}

		}
			
    }
	/* public override void OnStartClient()
	{

		string _netID = GetComponent<NetworkIdentity>().netId.ToString();
		Player _player = GetComponent<Player>();

		base.OnStartClient ();

		GameManager.RegisterPlayer(_netID, _player);
	} */

	void AssignRemoteLayer()
	{
		gameObject.layer = LayerMask.NameToLayer (remoteLayerName);
	}
		


	void DisableComponents()
	{
		for (int i = 0; i < componentstoDisable.Length; i++) 
		{
			componentstoDisable [i].enabled = false;
		}
	}

	void OnDisable()
	{

		if (Scenecamera != null) 
		{
			Scenecamera.gameObject.SetActive (true);
		}
		//GameManager.UnRegisterPlayer (transform.name);
	}
   	void DisableCam()
	{
		GameObject[] disablecam = GameObject.FindGameObjectsWithTag ("TPSCamera");

		for (int i = 0; i < disablecam.Length ; i++) 
		{
			if (!disablecam[i].CompareTag("PlayerCam")) 
			{
				disablecam[i].SetActive (false);
			}
		}
	} 
}