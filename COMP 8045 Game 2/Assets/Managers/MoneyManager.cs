using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MoneyManager : MonoBehaviour {
    
    public static int money //redundant with PlayerWeapons.GlobalScore
    {
        get
        {
            return PlayerWeapons.GlobalScore;
        }
        set
        {
            PlayerWeapons.GlobalScore = value;
        }
    }
    public static int TotalKillScore
    {
        get
        {
            if (PlayerPrefs.HasKey("TotalKillScore"))
                return PlayerPrefs.GetInt("TotalKillScore");
            else
            {
                return 0;
            }
        }
        set
        {
            PlayerPrefs.SetInt("TotalKillScore", value);
        }
    }
    public static int TotalKills
    {
        get
        {
            if (PlayerPrefs.HasKey("TotalKills"))
                return PlayerPrefs.GetInt("TotalKills");
            else
            {
                return 0;
            }
        }
        set
        {
            PlayerPrefs.SetInt("TotalKills", value);
        }

    }
    public Text text;

    void Start()
    {
    }

	void Awake () {
        text = GetComponent<Text> (); //get the UI text
        money = PlayerPrefs.GetInt("score");
	}
	
	void Update () {
        text.text = money.ToString() + " pts";
    }
}