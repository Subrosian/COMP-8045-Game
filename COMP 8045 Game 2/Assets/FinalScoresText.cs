using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalScoresText : MonoBehaviour {

    public bool continuesText;

	// Use this for initialization
	void Start () {
        switch(name)
        {
            case "FinalContinuesText":
                if(PlayerHealth.continueCount <= 0)
                {
                    GetComponent<Text>().text = "WOW! NO CONTINUES!" + PlayerHealth.continueCount;
                    GetComponent<Text>().color = new Color(234f / 255, 234f / 255, 90f / 255); //'gold' color
                }
                else
                {
                    GetComponent<Text>().text = "USED " + PlayerHealth.continueCount + " CONTINUES";
                    if (PlayerHealth.continueCount <= 2)
                    {
                        GetComponent<Text>().color = new Color(1f, 1f, 1f);
                    }
                    else if(PlayerHealth.continueCount <= 5)
                    {
                        GetComponent<Text>().color = new Color(0.75f, 0.75f, 0.75f);
                    }
                    else
                    {
                        GetComponent<Text>().color = new Color(0.45f, 0.45f, 0.45f);
                    }
                }
                break;
            case "FinalKillsKillScoreText":
                GetComponent<Text>().text = "Total Kills: " + MoneyManager.TotalKills
                                  + "\r\nFinal Kill Score: " + MoneyManager.TotalKillScore;
                break;
            default: break;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
