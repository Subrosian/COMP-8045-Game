using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterShop : MonoBehaviour, IPointerEnterHandler
{

    public AudioSource ShopAudioSource;
    public AudioClip ShopEnterSound, ShopAppearNotificationSound;
    public GameObject EnterShopTutorialTxt;
    public bool firstTimeTutorialTxtDisplay = true;

    // Use this for initialization
    void Start () {
        firstTimeTutorialTxtDisplay = true;

        //initialize alpha to 0 for what would be starting the scene, in which there would be any tutorials if so
        Image ButtonImageComponent = GetComponent<Image>();
        Text ButtonTextComponent = GetComponentInChildren<Text>();

        Color objprevColor = ButtonImageComponent.color;
        ButtonImageComponent.color = new Color(objprevColor.r, objprevColor.g, objprevColor.b, 0);
        Color textPrevColor = ButtonTextComponent.color;
        ButtonTextComponent.color = new Color(textPrevColor.r, textPrevColor.g, textPrevColor.b, 0);
        ButtonImageComponent.raycastTarget = false;
        ButtonTextComponent.raycastTarget = false;
        EnterShopTutorialTxt.GetComponent<CanvasRenderer>().SetAlpha(0);

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("pointer enter");
        if (!WaveManager.fadeScreenIsActive/* - disallowing entering shop if 'any fade screen is active, not just the shop' -*/ && WaveManager.waveTime < 8 && !PlayerHealth.playerIsDead) //first 8 seconds of a wave, and when player would not be dead, as when this button could be used
        {
            //play button sound for shop enter
            ShopAudioSource.clip = ShopEnterSound;
            ShopAudioSource.PlayOneShot(ShopEnterSound);

            Shop.isActive = true; //set shop to active
            WaveManager.gameFadePause = true;
            Shop.ShopAccessedThisWave = true;
        }
    }

    // Update is called once per frame
    void Update () {
        Image ButtonImageComponent = GetComponent<Image>();
        Text ButtonTextComponent = GetComponentInChildren<Text>();
        if (WaveManager.waveTime < 8 && !WaveManager.fadeScreenIsActive/* - disallowing entering shop if 'any fade screen is active, not just the shop' -*/ && !Shop.ShopAccessedThisWave)
        {
            if (ShopAudioSource.clip != ShopAppearNotificationSound) //play button sound for shop notification
            {
                ShopAudioSource.clip = ShopAppearNotificationSound;
            }
            if (!Shop.ShopNotifiedThisWave)
            {
                ShopAudioSource.PlayOneShot(ShopAppearNotificationSound);
                Shop.ShopNotifiedThisWave = true;
            }

            //transform.localScale = Vector3.one;
            GetComponent<Image>().raycastTarget = true;
            GetComponentInChildren<Text>().raycastTarget = true;

            //set alpha immediately to 1 for button
            Color objprevColor = ButtonImageComponent.color;
            ButtonImageComponent.color = new Color(objprevColor.r, objprevColor.g, objprevColor.b, 1);
            Color textPrevColor = ButtonTextComponent.color;
            ButtonTextComponent.color = new Color(textPrevColor.r, textPrevColor.g, textPrevColor.b, 1);
            if (firstTimeTutorialTxtDisplay)
            {
                EnterShopTutorialTxt.GetComponent<CanvasRenderer>().SetAlpha(1);
            }
        }
        else //WaveManager.waveTime >= 8 || WaveManager.fadeScreenIsActive
        {
            //transform.localScale = Vector3.zero;
            //GetComponent<Image>().raycastTarget = false;
            //GetComponentInChildren<Text>().raycastTarget = false;

            //fade out image and text concurrently
            Color objprevColor = ButtonImageComponent.color;
            if (!(objprevColor.a <= 0))
            {
                float newAlpha = objprevColor.a - Time.deltaTime / Shop.shopFadeTransitionDuration;
                if (newAlpha < 0)
                {
                    newAlpha = 0;
                }
                ButtonImageComponent.color = new Color(objprevColor.r, objprevColor.g, objprevColor.b, newAlpha);
                Color textPrevColor = ButtonTextComponent.color;
                ButtonTextComponent.color = new Color(textPrevColor.r, textPrevColor.g, textPrevColor.b, newAlpha);
                if (firstTimeTutorialTxtDisplay)
                {
                    EnterShopTutorialTxt.GetComponent<CanvasRenderer>().SetAlpha(newAlpha);
                }
            }
            else if(WaveManager.waveTime > 0) //end of fade, after tutorial
            {
                firstTimeTutorialTxtDisplay = false;
            }
            ButtonImageComponent.raycastTarget = false;
            ButtonTextComponent.raycastTarget = false;
        }
    }
}
