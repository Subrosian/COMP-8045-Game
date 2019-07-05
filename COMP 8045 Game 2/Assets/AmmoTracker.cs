using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AmmoTracker : MonoBehaviour
{
    Text text;
    Vector3 infinityPositionOffset = new Vector3(11, 14, 0);

    //initializations
    Vector3 infinityPosition;
    Vector3 origPosition;

    // Use this for initialization
    void Start()
    {
        text = GetComponent<Text>();
        origPosition = transform.position;
        infinityPosition = origPosition + infinityPositionOffset;
    }

    // Update is called once per frame
    void Update()
    {
        //set as defaults
        text.fontSize = 80;
        transform.position = origPosition;

        switch (PlayerWeapons.weaponState)
        {
            case 0:
                text.text = "∞";
                text.fontSize = 120;
                transform.position = infinityPosition;
                break;
            case 1:
                text.text = PlayerPrefs.GetInt("gun1Ammo").ToString();
                break;
            case 2:
                text.text = PlayerPrefs.GetInt("gun2Ammo").ToString();
                break;
            default:
                text.text = "";
                break;
        }
    }
}
