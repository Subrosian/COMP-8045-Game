using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour {

    public static bool isActive/* - no longer setting to true due to alpha being kept in the WeaponSelected function in PlayerWeapons as of 8/21/18, 1:51PM*/
    {
        get { return isActive_internal; }
        set
        {
            WaveManager.setIsActive(ref isActive_internal, value);
        }
    }
    private static bool isActive_internal;
    public Image PistolImg, LaserGunImg, CrossbowImg, PsiBurstImg, DeltaPrisonImg, PsiLeapImg, TemporalInhibitionImg;
    public Text NextWaveButtonText, SaveQuitButtonText;
    public static bool ShopAccessedThisWave;
    public static bool ShopNotifiedThisWave;

    public static GameObject currSelectedShopItemGameObject;
    
    public const float shopFadeTransitionDuration = 1f; //amount of time for the shop to transition to fading in or out
    public CanvasGroup[] shopExclusiveUICanvasGroups;
    public GameObject[] shopObjsToLookAtRegardingFadingInAndOut;

    //some redundancy and with if statements, but for this project, it is not necessary to make less redundant given the project's scope and intended scalability
    Dictionary<Image, int> ImgAndItem;
    public static bool shopObjsFadeOutFinished/*, shopObjsFadeOutStart*/;

    public static void SetTextObjText(string ObjName, string ObjValue)
    {
        GameObject TextObj = GameObject.Find(ObjName);
        Text TextComponent = TextObj.GetComponent<Text>();
        TextComponent.text = ObjValue;
        
        //adjust width of all texts except for the description and current money
        switch(ObjName)
        {
            case "PurchaseItemNameDescrip":
                break;
            case "CurrentMoney":
                break;
            default:
                TextObj.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TextComponent.preferredWidth); //update width of obj
                break;
        }
    }

    public static Color darkUnselectedSkillColor = new Color(0.2f, 0.2f, 0.2f);
    public static Color shopUnselectedUnobtainedWeaponColor = Color.black;

    // Use this for initialization
    void Start () {
        isActive = false;
        //WaveManager.gameFadePause = false; //not with such a pause occurring here
        //isActive = true; //debug code
        ShopAccessedThisWave = false;
        ShopNotifiedThisWave = false;
        currSelectedShopItemGameObject = PistolImg.gameObject;
        ImgAndItem = new Dictionary<Image, int>{ //noting of keys and being unique, but values and being what can be the same
            {LaserGunImg, PlayerWeapons.gun1}, //maybe already processed into a value, where this would have to be re-updated each time
            {CrossbowImg, PlayerWeapons.gun2},
            {PsiBurstImg, PlayerWeapons.sk_PsiBurst},
            {DeltaPrisonImg, PlayerWeapons.sk_DeltaPrison},
            {PsiLeapImg, PlayerWeapons.sk_PsiLeap},
            {TemporalInhibitionImg, PlayerWeapons.sk_TemporalInhibition}
        };
        shopObjsFadeOutFinished = false;
        darkUnselectedSkillColor = new Color(0.2f, 0.2f, 0.2f);
        shopUnselectedUnobtainedWeaponColor = Color.black;
    }
	
    public static Color colorWithCurrAlpha(Color currColor, Color newColorWhoseAlphaIsDiscarded)
    {
        return new Color(newColorWhoseAlphaIsDiscarded.r, newColorWhoseAlphaIsDiscarded.g, newColorWhoseAlphaIsDiscarded.b, currColor.a);
    }

	// Update is called once per frame
	void Update ()
    {

        //text values would also be set on when a button would be clicked? Or maybe on each Update? Noting such an implementation in the Testosterone Jones game
        //noting of how such could be done on each Update, actually - so, not just initialization of the Shop objects' text, but also setting as done here
        if (isActive)
        {
            //set texts
            if (PlayerWeapons.gun1 == 1)
            {
                SetTextObjText("LaserGunPriceText", PlayerWeapons.gunAmmoPrice(1).ToString()); //corrected "CrossbowPriceText" to "LaserGunPriceText"
            }
            else
            {
                SetTextObjText("LaserGunPriceText", PlayerWeapons.gunPrice(1).ToString()); //corrected "CrossbowPriceText" to "LaserGunPriceText"
            }
            if (PlayerWeapons.gun2 == 1)
            {
                SetTextObjText("CrossbowPriceText", PlayerWeapons.gunAmmoPrice(2).ToString());
            }
            else
            {
                SetTextObjText("CrossbowPriceText", PlayerWeapons.gunPrice(2).ToString());
            }
            SetTextObjText("Sk-PsiBurstPriceText", PlayerWeapons.sk_Price(PlayerWeapons.PSIBURST).ToString());
            SetTextObjText("Sk-DeltaPrisonPriceText", PlayerWeapons.sk_Price(PlayerWeapons.DELTAPRISON).ToString());
            SetTextObjText("Sk-PsiLeapPriceText", PlayerWeapons.sk_Price(PlayerWeapons.PSILEAP).ToString());
            SetTextObjText("Sk-TemporalInhibitionPriceText", PlayerWeapons.sk_Price(PlayerWeapons.TEMPORALINHIBITION).ToString());

            //switch()

            //set UI images and corresponding price colors
            //if not yet obtained, make faded/darker - maybe black while not selected and gray when selected
            Color unselectedItemPriceColor = new Color(0.5f, 0.5f, 0.5f);
            LaserGunImg.raycastTarget = true;
            CrossbowImg.raycastTarget = true;
            if (PlayerWeapons.gun1 == 1)
            {
                LaserGunImg.color = Color.white;
                GameObject.Find("LaserGunPriceText").GetComponent<Text>().color = Color.white;
            }
            else
            {
                LaserGunImg.color = colorWithCurrAlpha(LaserGunImg.color, shopUnselectedUnobtainedWeaponColor);
                GameObject.Find("LaserGunPriceText").GetComponent<Text>().color = unselectedItemPriceColor;
            }
            if (PlayerWeapons.gun2 == 1)
            {
                CrossbowImg.color = Color.white;
                GameObject.Find("CrossbowPriceText").GetComponent<Text>().color = Color.white;
            }
            else
            {
                CrossbowImg.color = colorWithCurrAlpha(CrossbowImg.color, shopUnselectedUnobtainedWeaponColor);
                GameObject.Find("CrossbowPriceText").GetComponent<Text>().color = unselectedItemPriceColor;
            }
            PsiBurstImg.raycastTarget = true;
            DeltaPrisonImg.raycastTarget = true;
            PsiLeapImg.raycastTarget = true;
            TemporalInhibitionImg.raycastTarget = true;

            if( PlayerWeapons.sk_PsiBurst == 0)
            {
                PsiBurstImg.color = colorWithCurrAlpha(PsiBurstImg.color, darkUnselectedSkillColor);
                GameObject.Find("Sk-PsiBurstPriceText").GetComponent<Text>().color = unselectedItemPriceColor;
            }
            else
            {
                PsiBurstImg.color = Color.white;
                GameObject.Find("Sk-PsiBurstPriceText").GetComponent<Text>().color = Color.white;
            }
            if (PlayerWeapons.sk_DeltaPrison == 0)
            {
                DeltaPrisonImg.color = colorWithCurrAlpha(DeltaPrisonImg.color, darkUnselectedSkillColor);
                GameObject.Find("Sk-DeltaPrisonPriceText").GetComponent<Text>().color = unselectedItemPriceColor;
            }
            else
            {
                DeltaPrisonImg.color = Color.white;
                GameObject.Find("Sk-DeltaPrisonPriceText").GetComponent<Text>().color = Color.white;
            }
            if (PlayerWeapons.sk_PsiLeap == 0)
            {
                PsiLeapImg.color = colorWithCurrAlpha(PsiLeapImg.color, darkUnselectedSkillColor);
                GameObject.Find("Sk-PsiLeapPriceText").GetComponent<Text>().color = unselectedItemPriceColor;
            }
            else
            {
                PsiLeapImg.color = Color.white;
                GameObject.Find("Sk-PsiLeapPriceText").GetComponent<Text>().color = Color.white;
            }
            if (PlayerWeapons.sk_TemporalInhibition == 0)
            {
                TemporalInhibitionImg.color = colorWithCurrAlpha(TemporalInhibitionImg.color, darkUnselectedSkillColor);
                GameObject.Find("Sk-TemporalInhibitionPriceText").GetComponent<Text>().color = unselectedItemPriceColor;
            }
            else
            {
                TemporalInhibitionImg.color = Color.white;
                GameObject.Find("Sk-TemporalInhibitionPriceText").GetComponent<Text>().color = Color.white;
            }

            //default unselected color of the next wave button's text
            float unselectedWaveColorGrayscaleVal = 194 / 255f;
            Color darkUnselectedWaveColor = new Color(unselectedWaveColorGrayscaleVal, unselectedWaveColorGrayscaleVal, unselectedWaveColorGrayscaleVal);
            Text NextWaveButtonTextComponent = NextWaveButtonText.GetComponent<Text>();
            NextWaveButtonTextComponent.color = darkUnselectedWaveColor;
            Text SaveQuitButtonTextComponent = SaveQuitButtonText.GetComponent<Text>();
            SaveQuitButtonText.color = darkUnselectedWaveColor;

            GameObject shopObj = Shop.currSelectedShopItemGameObject;
            GameObject selectedPriceTextObj = GameObject.Find("PurchaseItemNameDescrip"); //placeholder initialization, which would be set to the currently selected item's name/description text object, which would be with text set to white regardless

            //set description text of the currently selected item if such would exist
            if (!(shopObj.GetComponent<SelectGun>() == null)) { //gun description text
                int gunNum = shopObj.GetComponent<SelectGun>().gunNum;
                switch (gunNum)
                {
                    case 1: //Laser gun
                        if (PlayerWeapons.gun1 == 1)
                        {
                            SetTextObjText("PurchaseItemNameDescrip",
                                            "Laser Gun Ammo\r\n" +
                                            PlayerWeapons.gunAmmoBundleQty(gunNum)+" Ammo\r\n" +
                                            "Ammo: " + PlayerWeapons.gun1Ammo.ToString() + "\r\n" +
                                            "Cost: " + PlayerWeapons.gunAmmoPrice(gunNum));
                        }
                        else
                        {
                            SetTextObjText("PurchaseItemNameDescrip",
                                            "Laser Gun\r\n" +
                                            "Cost: " + PlayerWeapons.gunPrice(gunNum));
                        }
                        selectedPriceTextObj = GameObject.Find("LaserGunPriceText");
                        break;
                    case 2: //Crossbow
                        if (PlayerWeapons.gun2 == 1)
                        {
                            SetTextObjText("PurchaseItemNameDescrip",
                                            "Crossbow Ammo\r\n" +
                                            PlayerWeapons.gunAmmoBundleQty(gunNum)+" Ammo\r\n" +
                                            "Ammo: " + PlayerWeapons.gun2Ammo.ToString() + "\r\n" +
                                            "Cost: " + PlayerWeapons.gunAmmoPrice(gunNum));
                        }
                        else
                        {
                            SetTextObjText("PurchaseItemNameDescrip",
                                            "Crossbow\r\n" +
                                            "Cost: " + PlayerWeapons.gunPrice(gunNum));
                        }
                        selectedPriceTextObj = GameObject.Find("CrossbowPriceText");
                        break;
                }
            }
            if (!(shopObj.GetComponent<SelectSkill>() == null)) { //skill description text
                int skillNum = shopObj.GetComponent<SelectSkill>().skillNum;

                string skillName = "";
                string skillDescription = "";
                switch (skillNum)
                {
                    case PlayerWeapons.PSIBURST:
                        skillName = "PSI Burst";
                        skillDescription = "Ripple shockwave of psionic energy";
                        selectedPriceTextObj = GameObject.Find("Sk-PsiBurstPriceText");
                        break;
                    case PlayerWeapons.DELTAPRISON:
                        skillName = "Delta Prison";
                        skillDescription = "Stasis prison surrounding nearby enemies";
                        selectedPriceTextObj = GameObject.Find("Sk-DeltaPrisonPriceText");
                        break;
                    case PlayerWeapons.PSILEAP:
                        skillName = "PSI Leap";
                        skillDescription = "Quick temporal leap over a distance";
                        selectedPriceTextObj = GameObject.Find("Sk-PsiLeapPriceText");
                        break;
                    case PlayerWeapons.TEMPORALINHIBITION:
                        skillName = "Temporal Inhibition";
                        skillDescription = "Inhibits the surrounding passage of time";
                        selectedPriceTextObj = GameObject.Find("Sk-TemporalInhibitionPriceText");
                        break;
                }
                SetTextObjText("PurchaseItemNameDescrip",
                                skillName+" Lv"+(PlayerWeapons.sk_Level(skillNum)+1)+"\r\n" +
                                "\r\n" +
                                skillDescription + "\r\n" +
                                "Cost: " + PlayerWeapons.sk_Price(skillNum));
            }
            SetTextObjText("CurrentMoney",
                PlayerWeapons.GlobalScore + " Pts");
            if (!(shopObj.GetComponent<SelectNextWave>() == null))
            {
                SetTextObjText("PurchaseItemNameDescrip",
                                "Wave " + (WaveManager.level) + "\r\n" +
                                "\r\n" +
                                "Continue to Wave " + (WaveManager.level));
                NextWaveButtonTextComponent.color = Color.white;
            }
            else
            if (!(shopObj.GetComponent<SaveQuit>() == null))
            {
                SetTextObjText("PurchaseItemNameDescrip",
                                "Save & Quit\r\n" +
                                "\r\n" +
                                "Save progress at current wave and return to the title screen");
                SaveQuitButtonTextComponent.color = Color.white;
            }
            else //if the currently selected item would not be the "Next Wave" button
            {
                //override the prior img set color for the corresponding selected item
                if (shopObj.GetComponent<Image>().color == colorWithCurrAlpha(shopObj.GetComponent<Image>().color, Color.black)) //for guns, which would have such a black color if unselected
                {
                    shopObj.GetComponent<Image>().color = colorWithCurrAlpha(shopObj.GetComponent<Image>().color, Color.gray);
                    selectedPriceTextObj.GetComponent<Text>().color = Color.white;
                }
                Color darkYellowSelectedSkillColor = new Color(0.4f, 0.4f, 0f); //matching the 'PSI' color and meter
                if (shopObj.GetComponent<Image>().color == colorWithCurrAlpha(shopObj.GetComponent<Image>().color, darkUnselectedSkillColor)) //for skills, which would have this unselected color
                {
                    shopObj.GetComponent<Image>().color = colorWithCurrAlpha(shopObj.GetComponent<Image>().color, darkYellowSelectedSkillColor);
                    selectedPriceTextObj.GetComponent<Text>().color = Color.white;
                }
                //GameObject.Find("ShootButton").transform.localScale = Vector3.zero; 
            }

            //fade in shop if not completed fading
            foreach (CanvasGroup cGroup in shopExclusiveUICanvasGroups)
            {
                if (cGroup.alpha < 1) {
                    cGroup.alpha += Time.deltaTime / shopFadeTransitionDuration; //add the alpha proportional to the fraction of the set transition duration
                }
                if(cGroup.alpha > 1)
                {
                    cGroup.alpha = 1; //if gone to greater than 1, then set to 1
                }
            }

            DoFadeInForCorrespondingShopObjects();
            if (shopObjsFadeOutFinished) //cease fadeout finished
            {
                shopObjsFadeOutFinished = false;
            }

            //make interactable
            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        else
        {
            //fade out shop if not completed fading
            foreach (CanvasGroup cGroup in shopExclusiveUICanvasGroups)
            {
                if (cGroup.alpha > 0)
                {
                    cGroup.alpha -= Time.deltaTime / shopFadeTransitionDuration; //subtract the alpha proportional to the fraction of the set transition duration
                }
                if(cGroup.alpha < 0)
                {
                    cGroup.alpha = 0; //if gone to less than 0, then set to 0
                }
            }
            if (!shopObjsFadeOutFinished)
            {
                //re-assign in case there would be any newly purchased weapons / skills
                //could be skipped with some code regarding when the fadeout would have been started, and keeping track of such
                ImgAndItem = new Dictionary<Image, int>{ //noting of keys and being unique, but values and being what can be the same
                    {LaserGunImg, PlayerWeapons.gun1}, //maybe already processed into a value, where this would have to be re-updated each time
                    {CrossbowImg, PlayerWeapons.gun2},
                    {PsiBurstImg, PlayerWeapons.sk_PsiBurst},
                    {DeltaPrisonImg, PlayerWeapons.sk_DeltaPrison},
                    {PsiLeapImg, PlayerWeapons.sk_PsiLeap},
                    {TemporalInhibitionImg, PlayerWeapons.sk_TemporalInhibition}
                };
            
                DoFadeOutForCorrespondingShopObjects();
            }

            //make uninteractable
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }
    void DoFadeInForCorrespondingShopObjects()
    {
        foreach (KeyValuePair<Image, int> pair in ImgAndItem)
        {
            if (pair.Value == 0) //if the current gun or skill would not be in possession
            {
                IncrementImgAlphaProportionalToDuration(pair.Key.gameObject, shopFadeTransitionDuration);
            }
        }
    }

    void DoFadeOutForCorrespondingShopObjects()
    {
        //cease doing this if the fade out process is finished
        shopObjsFadeOutFinished = true; //made false when no more imgs have GetAlpha() > 0

        foreach (KeyValuePair<Image, int> pair in ImgAndItem)
        {
            if (pair.Value == 0) //if the current gun or skill would not be in possession
            {
                DecrementImgAlphaProportionalToDuration(pair.Key.gameObject, shopFadeTransitionDuration);
                if(pair.Key.gameObject.GetComponent<CanvasRenderer>().GetAlpha() > 0)
                {
                    shopObjsFadeOutFinished = false;
                }
            }
        }
    }

    //decrement alpha value proportional to the fraction of the fade transition duration, if not already <=0
    public static void DecrementImgAlphaProportionalToDuration(GameObject gameObj, float duration)    {
        if (!(gameObj.GetComponent<Image>() == null))
        {
            float oldAlpha = gameObj.GetComponent<CanvasRenderer>().GetAlpha();
            if (oldAlpha <= 0)
            {
                return;
            }
            float newAlpha = oldAlpha - Time.deltaTime / duration;
            if (newAlpha < 0)
            {
                newAlpha = 0;
            }
            gameObj.GetComponent<CanvasRenderer>().SetAlpha(0);
            gameObj.GetComponent<CanvasRenderer>().SetAlpha(newAlpha);
        }
    }

    //decrement alpha value proportional to the fraction of the fade transition duration, if not already <=0
    public static void DecrementImgAlphaProportionalToDuration(CanvasRenderer gameObjCR, float duration, float totalAlpha)
    {
            float oldAlpha = gameObjCR.GetAlpha();
            if (oldAlpha <= 0)
            {
                return;
            }
            float newAlpha = oldAlpha - Time.deltaTime / duration / totalAlpha;
            if (newAlpha < 0)
            {
                newAlpha = 0;
            }
            gameObjCR.SetAlpha(newAlpha);
    }

    //increment alpha value proportional to the fraction of the fade transition duration, if not already >=1
    public static void IncrementImgAlphaProportionalToDuration(GameObject gameObj, float duration)
    {
        if (!(gameObj.GetComponent<Image>() == null))
        {
            float oldAlpha = gameObj.GetComponent<CanvasRenderer>().GetAlpha();
            if (oldAlpha >= 1)
            {
                return;
            }
            float newAlpha = oldAlpha + Time.deltaTime / duration;
            if (newAlpha > 1)
            {
                newAlpha = 1;
            }
            gameObj.GetComponent<CanvasRenderer>().SetAlpha(newAlpha);
        }
    }

    void StartShop()
    {
    }
}
