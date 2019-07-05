using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class SelectGun : MonoBehaviour, IPointerClickHandler
{
    public int gunNum;
    public GameObject redirectObj; //object that would be redirected to for the selected game object if such a redirection would exist - e.g. LaserGunPriceText redirecting to LaserWeaponImg
    public GameObject player;

	// Use this for initialization
	void Start () {
	
	}

    public void OnConfirmPurchase()
    {
        //gun confirmation code
        //do nothing with starting gun
        if(gunNum == 0)
        {
            return;
        }

        //purchase gun if not already obtained; otherwise, purchase gun ammo of the corresponding quantity
        bool hasGun = PlayerPrefs.GetInt("gun" + gunNum) == 1;
        if (hasGun)
        {
            //purchase gunAmmoBundleQty of gun ammo
            if (PlayerWeapons.GlobalScore >= PlayerWeapons.gunAmmoPrice(gunNum))
            {
                PlayerWeapons.GlobalScore -= PlayerWeapons.gunAmmoPrice(gunNum);
                int prevGunAmmo = PlayerPrefs.GetInt("gun" + gunNum + "Ammo");
                PlayerPrefs.SetInt("gun" + gunNum + "Ammo", (prevGunAmmo + PlayerWeapons.gunAmmoBundleQty(gunNum)));
            }
        }
        else
        {
            //purchase gun
            if (PlayerWeapons.GlobalScore >= PlayerWeapons.gunPrice(gunNum))
            {
                PlayerWeapons.GlobalScore -= PlayerWeapons.gunPrice(gunNum);
                PlayerPrefs.SetInt("gun" + gunNum, 1);
                PlayerPrefs.SetInt("gun" + gunNum + "Ammo", (PlayerWeapons.gunAmmoBundleQty(gunNum))); //start with initial ammo as well
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData) //select gun with this event
    {
        if (!WaveManager.fadeScreenIsActive)
        {   //during non-fade screen phases, select gun as in gameplay
            bool hasAmmo = true;
            bool hasGun = PlayerPrefs.GetInt("gun" + gunNum) == 1;

            if (gunNum != 0)
            { //if not the starting gun, which would be the only gun with infinite ammo as of 8/17/18
                int gunAmmo = PlayerPrefs.GetInt("gun" + gunNum + "Ammo"); //get ammo of gun
                if (gunAmmo <= 0)
                {
                    hasAmmo = false;
                }
            }
            if (hasAmmo && (gunNum == 0 || hasGun)) //only allow selecting guns with ammo
            {
                player.GetComponent<PlayerWeapons>().WeaponSelected(gunNum);
            }
        }
        else if(Shop.isActive) //select gun as in shop if shop is active
        {
            if (redirectObj != null)
            {
                Shop.currSelectedShopItemGameObject = redirectObj;
            }
            else
            {
                //select gun as in shop - with the GameObject matched as what would determine the current selection
                Shop.currSelectedShopItemGameObject = gameObject;
            }
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
