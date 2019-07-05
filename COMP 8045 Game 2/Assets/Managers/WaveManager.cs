using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour {
    
    public static int level = 1;
    public GameObject[] SpawnPoint; //Not used
    public string[] levelName; //to correspond to the enemies spawned in the enemy gameobject in EnemySpawner
    public static float waveTime; //current wave time
    public int[] levelTime; //full time for the wave to finish
    public int waveStartTime = 5;

    public Slider bossHealthSlider;
    public Image BossIconImg;
    public EnemyHealth bossEnemyHealth;
    
    public AudioClip /*NightClip, */NormalModeMusic, ShadowModeMusicAddition;
    public AudioSource BGMusicSource/*, NightSource*/;
    public AudioSource ShadowModeMusicSource;

    public GameObject ShadowModeShade;
    public const int healthRecoveryPerWave = 30; //recovery of player and (if any are present) each Shadow's health at the end of each wave
    
    //note that shadowNumMin and shadowNumMax are hard coded, corresponding to what would be in GameplayScene <<where YKWIM by this><YKWIM>>
    public int shadowNumMin, shadowNumMax;

    public static bool fadeScreenIsActive; //controlled by shop, and tutorial; made it so multiple fade screens would be something that could be done at once while setting this variable
    public static int activeFadeScreens = 0; //to be updated whenever something that would be to change the number of fade screens - such as when the shop or tutorial's isActive variables would be set - would be done

    public static void setIsActive(ref bool isActive_internal, bool value)
    {
        //handle the case of if there would be multiple 'fade screens' - such as of the shop and tutorial - active at once
        if (value && !isActive_internal)
        {
            WaveManager.activeFadeScreens++;
        }
        else if (!value && isActive_internal)
        {
            WaveManager.activeFadeScreens--;
        }
        if (WaveManager.activeFadeScreens >= 1) //where this would be supposed to be always true if value is true
        {
            WaveManager.fadeScreenIsActive = true;
        }
        else
        {
            WaveManager.fadeScreenIsActive = false;
        }
        isActive_internal = value;
    }


    public static bool isNewGame
    {
        get
        {
            if (PlayerPrefs.HasKey("isNewGame"))
            {
                return PlayerPrefs.GetInt("isNewGame") == 1;
            }
            else
            {
                return true; //default to new game if there would not even be a key set for it yet
            }
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("isNewGame", 1);
            }
            else
            {
                PlayerPrefs.SetInt("isNewGame", 0);
            }
        }
    }

    public static bool isShadowMode
    {
        get
        {
            if (PlayerPrefs.HasKey("isShadowMode"))
            {
                return PlayerPrefs.GetInt("isShadowMode") == 1;
            }
            else
            {
                return false;
            }
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("isShadowMode", 1);
            }
            else
            {
                PlayerPrefs.SetInt("isShadowMode", 0);
            }
        }
    }

    public static bool continueFromGameOver
    {
        get
        {
            if (PlayerPrefs.HasKey("continueFromGameOver"))
            {
                return PlayerPrefs.GetInt("continueFromGameOver") == 1;
            }
            else
            {
                return false;
            }
        }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt("continueFromGameOver", 1);
            }
            else
            {
                PlayerPrefs.SetInt("continueFromGameOver", 0);
            }
        }
    }

    public static bool gameFadePause = false;
    public static bool DEBUG_ENABLED = true;
    public const bool DEBUG_SHADOWAITESTING = false;
    public const bool DEBUG_WEAPONS = false;
    public const bool DEBUG_LEVEL = false;
    public const bool DEBUG_FINDLARGESTKEYINPMDATA = false;
    public const bool DEBUG_RESETSHADOWDAMAGETAKEN = true; //for getting stats of Shadow taking damage across time
    public const bool DEBUG_RESETPLAYERDAMAGETAKEN = false; //for getting stats of player taking damage across time

    void NewGame_Waves()
    {
        //reset all values to new game values, except for whether the current game mode is Shadow Mode or not and whether Shadow Mode is unlocked
        level = 1;
        PlayerPrefs.SetInt("currentLevel", level);

        //reset Shadow healths as well by resetting the keys for shadow health if such would exist <<where YKWIM by this><YKWIM>>
        for (int shadowNum = shadowNumMin; shadowNum <= shadowNumMax; ++shadowNum)
        {
            if (PlayerPrefs.HasKey("shadow" + shadowNum + "Health"))
            {
                PlayerPrefs.DeleteKey("shadow" + shadowNum + "Health");
            }
        }
        //reset number of times that the Shadow took damage if the debug boolean is true for such
        if (WaveManager.DEBUG_RESETSHADOWDAMAGETAKEN)
        {
            for (int shadowNum = shadowNumMin; shadowNum <= shadowNumMax; ++shadowNum)
            {
                int timesTakenDamage = 0;
                float timeSinceResetResults = 0;
                if (PlayerPrefs.HasKey("numTimesShadow" + shadowNum + "TakenDamage"))
                {
                    timesTakenDamage = PlayerPrefs.GetInt("numTimesShadow" + shadowNum + "TakenDamage");
                    PlayerPrefs.DeleteKey("numTimesShadow" + shadowNum + "TakenDamage");
                }
                if (PlayerPrefs.HasKey("amtTimeSinceResetShadow" + shadowNum + "Results"))
                {
                    timeSinceResetResults = PlayerPrefs.GetFloat("amtTimeSinceResetShadow" + shadowNum + "Results");
                    PlayerPrefs.DeleteKey("amtTimeSinceResetShadow" + shadowNum + "Results");
                }
                Debug.Log("Shadow " + shadowNum + "'s taken damage has been reset from " + timesTakenDamage + " and results reset from " + timeSinceResetResults + ".");
            }
        }
        if (WaveManager.DEBUG_RESETPLAYERDAMAGETAKEN)
        {
            int timesTakenDamage = 0;
            float timeSinceResetResults = 0;
            if (PlayerPrefs.HasKey("numTimesPlayerTakenDamage"))
            {
                timesTakenDamage = PlayerPrefs.GetInt("numTimesPlayerTakenDamage");
                PlayerPrefs.DeleteKey("numTimesPlayerTakenDamage");
            }
            if (PlayerPrefs.HasKey("amtTimeSinceResetPlayerResults"))
            {
                timeSinceResetResults = PlayerPrefs.GetFloat("amtTimeSinceResetPlayerResults");
                PlayerPrefs.DeleteKey("amtTimeSinceResetPlayerResults");
            }
            Debug.Log("Player's taken damage has been reset from " + timesTakenDamage + " and results reset from " + timeSinceResetResults + ".");
        }
    }

    // Use this for initialization
    // start before any of the SpawnPoint code runs
    void Awake ()
    {
#if !UNITY_EDITOR        
        WaveManager.DEBUG_ENABLED = false; //disallow DEBUG_ENABLED from being true outside of being run in the Unity editor
#endif
        //disable debug logging
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
            Debug.unityLogger.logEnabled = false;
#endif
        if (!DEBUG_ENABLED)
        {
            Debug.unityLogger.logEnabled = false;
        }

        //delete the singleton music and sound objects in this gameplay scene, where such music and sound would be handled in a different way
        GameObject SingletonMusicObj = GameObject.Find("SingletonMusicAudioSource");
        GameObject SingletonSoundObj = GameObject.Find("SingletonSoundAudioSource");
        if (SingletonMusicObj != null)
        {
            Destroy(SingletonMusicObj);
        }
        if (SingletonSoundObj != null)
        {
            Destroy(SingletonSoundObj);
        }
        //isNewGame = false; //debug val
        //isShadowMode = true; //debug val
        if (isNewGame)
        {
            NewGame_Waves();
        }
        else
        {
            //get saved level here if one exists
            if (PlayerPrefs.HasKey("currentLevel")) //new level
            {
                level = PlayerPrefs.GetInt("currentLevel");
                //LevelNameText.text = levelName[level - 1]; //level-1 is the 0-based level
                Debug.Log("Level: " + level);
            }
        }
        if (!PlayerPrefs.HasKey("continueFromGameOver"))
        {
            PlayerPrefs.SetInt("continueFromGameOver", 0);
        }
        if (continueFromGameOver)
        {
            //reset player and Shadow healths here
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponentInChildren<PlayerHealth>().currentHealth = PlayerHealth.maxHealth;
            for (int shadowNum = shadowNumMin; shadowNum <= shadowNumMax; ++shadowNum) //loop to reset Shadow health
            {
                if (PlayerPrefs.HasKey("shadow" + shadowNum + "Health"))
                {
                    PlayerPrefs.DeleteKey("shadow" + shadowNum + "Health");
                }
            }
            continueFromGameOver = false;

        }
        if (WaveManager.DEBUG_LEVEL)
        {
            level = 16; //debug level setting
        }
       
        waveTime = 0;
        EnemySpawner.spawning = true;

        //reset static variables, as their values are otherwise preserved on screen transitions
        EnemyHealth.BossKilled = false;
        EnemyHealth.bossExists = false;
        ShadowPM.clearavgClosest2eDistances_player();
        waveTime = 0;
    }

	void Start () {
        DayNightCycle.SecondsInDay = levelTime[level - 1]; //update time of the day with the wave time.
        if (!isShadowMode)
        {
            Destroy(ShadowModeShade); //the tint would only be part of shadow mode
            ShadowModeMusicSource.Stop();
        }
        if(!DEBUG_SHADOWAITESTING)
        {
            foreach(GameObject debugObj in GameObject.FindGameObjectsWithTag("DebugObj")) //the gameobjects including eg. the training buttons and such
            {
                Destroy(debugObj);
            }
        }
        BGMusicSource.volume = MusicSFXVolChange.MusicVol;
        ShadowModeMusicSource.volume = MusicSFXVolChange.MusicVol * 0.6f;
        BGMusicSource.Play();

        //run tutorial first thing on loading gameplay, whether it be in continuing or starting a new game
        TutorialIntroPauseDisplay.isActive = true; //as debug in removing tutorial for debugging, setting to false, but otherwise have such start as true
        Debug.Log(WaveManager.activeFadeScreens);

        //disable boss health slider visibility if no boss yet
        if (!EnemyHealth.bossExists)
        {
            //Debug.Log("Disable boss health");
            bossHealthSlider.enabled = false;
            bossHealthSlider.transform.localScale = new Vector3(0, 0, 0); //hide the slider - this being one way to do it; another would be to have a CanvasGroup component.
            BossIconImg.transform.localScale = new Vector3(0, 0, 0);
        }

        Debug.Log("Angle diff from (0,0): "+Vector2.SignedAngle(new Vector2(0, 0), new Vector2(0, 0)));
        //Debug.Log("(0,0) normalized: " + new Vector2(0, 0).normalized);
    }

    // Update is called once per frame
    void Update () {
        //prevent enemies from moving if shop is currently active

        //prevent wave time from continuing and wave progression if shop is currently active
        if (WaveManager.fadeScreenIsActive)
        {
            //call for each SpawnPoint in the children of WaveManager
            EnemySpawner.spawning = false; //though set to true later in the Update loop when after the waveStartTime if Shop.isActive is false
            return;
        }

        if (!PlayerHealth.playerIsDead) //increment wave time only if player is not dead
        {
            waveTime += Time.deltaTime;
        }

        if (!EnemyHealth.bossExists)
        {
            //Debug.Log("Disable boss health");
            bossHealthSlider.enabled = false;
            bossHealthSlider.transform.localScale = new Vector3(0, 0, 0); //hide the slider - this being one way to do it; another would be to have a CanvasGroup component.
            BossIconImg.transform.localScale = new Vector3(0, 0, 0);
            //bossHealthSlider.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            bossHealthSlider.enabled = true;
            bossHealthSlider.transform.localScale = new Vector3(1, 1, 1); //hide the slider - this being one way to do it; another would be to have a CanvasGroup component.
            BossIconImg.transform.localScale = new Vector3(1, 1, 1);
            bossHealthSlider.value = bossEnemyHealth.currHealth;
        }
        //advance to next level when time of wave elapses, as long as more levels exist
        //or, advance when a boss is killed
        if (EnemyHealth.BossKilled || waveTime >= levelTime[level-1] && level < levelName.Length)
        {
            EnemyHealth.BossKilled = false;

            //LevelNameText.text = levelName[level]; //level-1 is the 0-based level
            EnemySpawner.spawning = false; //pause for the period of waveStartTime

            level++;
            if (level-1 >= levelName.Length) //if getting past the last level
            {
                OnLoadTransition.LoadScene("WinCongratulationsScene");
                return;
            }
            DayNightCycle.SecondsInDay = levelTime[level - 1];
            waveTime = 0;
            Shop.ShopAccessedThisWave = false;
            Shop.ShopNotifiedThisWave = false;

            PlayerPrefs.SetInt("currentLevel", level);
            

            Debug.Log("Now at level " + level);

            EnemyHealth.bossExists = false; //no more boss at the end of the level
                                            //Application.LoadLevel("Shop");

            //reset values and such for the next level, which would have otherwise been done only on Awake and\or otherwise initialization that would be desired for the next level
            //clear remaining enemies - of which there could be eg. especially wandering enemies
            //has bug with still disabling colliders of newly made enemies
            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                EnemyHealth currEnemyHealth = enemy.GetComponentInChildren<EnemyHealth>();
                currEnemyHealth.isDead = true; //triggers before the Start() of such enemies apparently?
                currEnemyHealth.Die(false);
            }

            PlayerHealth playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerHealth>();
            playerHealth.currentHealth = (healthRecoveryPerWave + playerHealth.currentHealth) > PlayerHealth.maxHealth ? PlayerHealth.maxHealth : (healthRecoveryPerWave + playerHealth.currentHealth); //increase health by 30, where if it would overfill player's health, set 
            
            //call the PerLevelUpdate method in each SpawnPoint in the children of WaveManager
            //Note: the SpawnPoint tag for GameObjects is an alternate way of getting each SpawnPoint, and if it's redundant then it is
            foreach (Transform child in transform)
            {
                if (child.gameObject.GetComponent<EnemySpawner>() != null)
                {
                    child.gameObject.GetComponent<EnemySpawner>().PerLevelUpdate(); //could use polymorphism for EnemySpawner and ShadowSpawner, though there would be just two types as of 10/23/18, 11:35PM
                }
                if (child.gameObject.GetComponent<ShadowSpawner>() != null)
                {
                    child.gameObject.GetComponent<ShadowSpawner>().PerLevelUpdate();
                }

                //update Shadows here
                LocateSeekPursueTarget.shadowCharacters = new List<GameObject>(GameObject.FindGameObjectsWithTag("ShadowCharacter"));
            }
        }

       

        //start spawning of the current wave after waveStartTime
        if (waveTime >= waveStartTime && !EnemySpawner.spawning)
        {
            EnemySpawner.spawning = true;
        }
	}
}
