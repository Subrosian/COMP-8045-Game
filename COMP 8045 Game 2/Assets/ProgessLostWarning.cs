using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgessLostWarning : MonoBehaviour {
    public Button NormalModeButton, ShadowModeButton;

	// Use this for initialization
	void Start () {
		if(PlayerPrefs.HasKey("currentLevel"))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y-31, transform.position.z);
            Vector3 NormalModeBtnPosn = NormalModeButton.transform.position;
            Vector3 ShadowModeBtnPosn = ShadowModeButton.transform.position;
            NormalModeButton.transform.position = new Vector3(NormalModeBtnPosn.x, NormalModeBtnPosn.y - 41, NormalModeBtnPosn.z);
            ShadowModeButton.transform.position = new Vector3(ShadowModeBtnPosn.x, ShadowModeBtnPosn.y - 41, ShadowModeBtnPosn.z);
        }
        else
        {
            transform.localScale = Vector3.zero;
        }
        //PlayerPrefs.DeleteKey("currentLevel"); //as debug code
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
