using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetAIMatchRate : MonoBehaviour {

	// Use this for initialization
	void Start () {
        int numPlayerAndAIMovementMatchingUpdates = PlayerPrefs.GetInt("numPlayerAndAIMovementMatchingUpdates", 0);
        int numPlayerAndAIShootingMatchingUpdates = PlayerPrefs.GetInt("numPlayerAndAIShootingMatchingUpdates", 0);
        int numPlayerAndAIMovingAndShootingMatchingUpdates = PlayerPrefs.GetInt("numPlayerAndAIMovingAndShootingMatchingUpdates", 0);
        int numPlayerAndAITotalTestingUpdates = PlayerPrefs.GetInt("numPlayerAndAITotalTestingUpdates", 0);
        string AIMatchText = "";
        if (numPlayerAndAITotalTestingUpdates != 0)
        {
        AIMatchText = "Player-Shadow Match Rate: \r\n" +
                        "Movement: "+numPlayerAndAIMovementMatchingUpdates+" ("+ ((int)((float)numPlayerAndAIMovementMatchingUpdates / numPlayerAndAITotalTestingUpdates * 100)) + "%)\r\n" +
                        "Shooting: " + numPlayerAndAIShootingMatchingUpdates + " ("+ ((int)((float)numPlayerAndAIShootingMatchingUpdates / numPlayerAndAITotalTestingUpdates * 100)) + "%)\r\n" +
                        "Both: " + numPlayerAndAIMovingAndShootingMatchingUpdates +"("+ ((int)((float)numPlayerAndAIMovingAndShootingMatchingUpdates / numPlayerAndAITotalTestingUpdates * 100))+"%)\r\n" + 
                        "Total: "+numPlayerAndAITotalTestingUpdates;
        }
        GetComponentInChildren<Text>().text = AIMatchText;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
