using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SelectSkill : MonoBehaviour, IPointerClickHandler
{
    public int skillNum;
    public GameObject redirectObj; //object that would be redirected to for the selected game object if such a redirection would exist - e.g. Sk-PsiBurstPriceText redirecting to Sk-PsiBurstImg
    GameObject player;

    // Use this for initialization
    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
	}

    public void OnConfirmPurchase()
    {
        //skill confirmation code
        //level up skill
        if (PlayerWeapons.GlobalScore >= PlayerWeapons.sk_Price(skillNum))
        {
            PlayerWeapons.GlobalScore -= PlayerWeapons.sk_Price(skillNum);
            if (skillNum == PlayerWeapons.DELTAPRISON) //due to this skill level updating specifically having an implementation, making an exception for this
            {
                PlayerWeapons.sk_DeltaPrison += 1;
            }
            else
            {
                PlayerPrefs.SetInt("skill" + skillNum, PlayerWeapons.sk_Level(skillNum) + 1);
            }
            Debug.Log("Level up for skill" + skillNum);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!WaveManager.fadeScreenIsActive)
        {
            player.GetComponent<PlayerWeapons>().SkillSelected(skillNum);
        }
        else if(Shop.isActive)
        {
            if (redirectObj != null)
            {
                Shop.currSelectedShopItemGameObject = redirectObj;
            }
            else
            {
                Shop.currSelectedShopItemGameObject = gameObject;
            }
        }
        Debug.Log("selected skill " + skillNum);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
