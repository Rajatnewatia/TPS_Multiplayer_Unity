using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public static GameController GC;

	private Player_Input player { get { return FindObjectOfType<Player_Input> (); } set { player = value; } }

    private PlayerUI playerUI { get { return FindObjectOfType<PlayerUI> (); } set { playerUI = value; } }

	private Ammo_Tool_Handler wp { get { return player.GetComponent<Ammo_Tool_Handler> (); } set { wp = value; } }

	void Awake () 
	{
		if (GC == null) 
		{
			GC = this;
		} 
		else 
		{
			if (GC != this) 
			{
				Destroy (gameObject);
			}
		}
	}

	void Update ()
	{
		UpdateUI ();
	}

	void UpdateUI()
	{
		if (player) 
		{
			if (playerUI) 
			{
				if (wp) 
				{
					if (playerUI.ammoText) {
						if (wp.currentWeapon == null) {
							playerUI.ammoText.text = "Unarmed.";
						} else {
							playerUI.ammoText.text = wp.currentWeapon.ammo.clipAmmo + "//" + wp.currentWeapon.ammo.carryingAmmo; 
						}
					}
				}

				if (playerUI.healthBar && playerUI.healthText) 
				{
					playerUI.healthText.text = Mathf.Round(playerUI.healthBar.value).ToString();
				}
			}
		}
	}
}
