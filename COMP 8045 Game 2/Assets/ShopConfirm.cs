using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ShopConfirm : MonoBehaviour, IPointerClickHandler
{

    public AudioSource ShopConfirmAudioSource;
    public AudioClip ShopConfirmSound;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(Shop.isActive)
        {
            GetComponent<Image>().raycastTarget = true;
        }
        else
        {
            GetComponent<Image>().raycastTarget = false;
        }
	}

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Shop.isActive)
        {
            return;
        }

        //play click sound for shop
        ShopConfirmAudioSource.clip = ShopConfirmSound;
        ShopConfirmAudioSource.PlayOneShot(ShopConfirmSound);

        //Debug.Log("ShopConfirm triggered");
        GameObject shopObj = Shop.currSelectedShopItemGameObject;
        
        //do confirm action if gun is selected
        if (!(shopObj.GetComponent<SelectGun>() == null)) //set to null as .Equals() would return an error apparently in trying what was claimed here of using .Equals() - https://forum.unity.com/threads/find-out-if-gameobject-has-a-compenent.38524/ - and can note of such of if there would be an issue with using == null
        {
            shopObj.GetComponent<SelectGun>().OnConfirmPurchase();
        }
        if (!(shopObj.GetComponent<SelectSkill>() == null))
        {
            shopObj.GetComponent<SelectSkill>().OnConfirmPurchase();
        }
        if (!(shopObj.GetComponent<SelectNextWave>() == null))
        {
            Shop.isActive = false;
            WaveManager.gameFadePause = false;
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerWeapons>().InitializeNonShopPeriod();
        }
        if (!(shopObj.GetComponent<SaveQuit>() == null))
        {
            //load the title screen
            SaveQuit.resetStaticVarsOnThisSceneChange();

            
            ShadowPM.savingData = true; //save PM data here - though requires a ShadowCharacter somewhere; using a 'training' Shadow
            ShadowPM.savingMovementData = true; //save movement data here
            
            StartCoroutine("LoadTitleWhenDoneSavingData");
        }
    }

    IEnumerator LoadTitleWhenDoneSavingData()
    {
        while(ShadowPM.savingData || ShadowPM.savingData_nextFrame || ShadowPM.savingMovementData)
        {
            Debug.Log("in the while loop; "+ShadowPM.savingData);
            yield return null;
        }
        Debug.Log("after the while loop");
        if (!(ShadowPM.savingData || ShadowPM.savingData_nextFrame || ShadowPM.savingMovementData))
        {
            OnLoadTransition.LoadScene("TitleScreenScene");
        }
    }
}
