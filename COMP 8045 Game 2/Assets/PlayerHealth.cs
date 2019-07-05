using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{

    public static int maxHealth = 100; //static to be used for recovering health from game over scene; there is only one max health for the player
    public int currentHealth //property to synchronize currentHealth with the slider
    {
        set
        {
            healthSlider.value = value;
            PlayerPrefs.SetInt("playerCurrentHealth", value);
        }
        get
        {
            return PlayerPrefs.GetInt("playerCurrentHealth");
        }
    }
    public static int continueCount
    {
        set
        {
            PlayerPrefs.SetInt("playthroughContinueCount", value);
        }
        get
        {
            if (PlayerPrefs.HasKey("playthroughContinueCount"))
            {
                return PlayerPrefs.GetInt("playthroughContinueCount");
            }
            else
            {
                return 0; //default is 0 if no key exists for this <<as I would note><YKWIM>>
            }
        }
    }

    //numTimesPlayerTakenDamage - as one means of assessing differences
    public int numTimesPlayerTakenDamage
    {
        set
        {
            PlayerPrefs.SetInt("numTimesPlayerTakenDamage", value);
        }
        get
        {
            if (PlayerPrefs.HasKey("numTimesPlayerTakenDamage"))
            {
                return PlayerPrefs.GetInt("numTimesPlayerTakenDamage");
            }
            else
            {
                PlayerPrefs.SetInt("numTimesPlayerTakenDamage", 0);
                return PlayerPrefs.GetInt("numTimesPlayerTakenDamage");
            }
        }
    }
    public float amtTimeSinceResetPlayerResults
    {
        set
        {
            PlayerPrefs.SetFloat("amtTimeSinceResetPlayerResults", value);
        }
        get
        {
            if (PlayerPrefs.HasKey("amtTimeSinceResetPlayerResults"))
            {
                return PlayerPrefs.GetFloat("amtTimeSinceResetPlayerResults");
            }
            else
            {
                PlayerPrefs.SetFloat("amtTimeSinceResetPlayerResults", 0);
                return PlayerPrefs.GetFloat("amtTimeSinceResetPlayerResults");
            }
        }
    }

    public Slider healthSlider;

    AudioSource playerAudio;
    public AudioClip TakeDamageSound;

    public float currDeadTimeTowardsGameOver = 0f;
    float deadFadeToBlackDuration = 1.5f;
    public GameObject FadeToBlackShade;
    public GameObject TakeDamageShade;
    CanvasRenderer TakeDamageShadeCR;
    public static bool takeDamageFadeOutFinished;
    public const float takeDamageShadeActiveDuration = 5f;
    public const float takeDamageShadeFullAlpha = 0.25f;

    public static bool playerIsDead;
    bool damaged;
    public static float damagedTimer;

    //public Text characterState;

    // Use this for initialization
    void Start()
    {
        playerAudio = GetComponent<AudioSource>();
        ////playerMovement = GetComponent<PlayerMovement>();
        playerIsDead = false;
        if (WaveManager.isNewGame)
        {
            currentHealth = maxHealth;
            continueCount = 0; //reset continue count
        }

        //fixing any errors from the PlayerPrefs value for currentHealth would not have been set
        if (!PlayerPrefs.HasKey("playerCurrentHealth"))
        {
            currentHealth = (int)healthSlider.value;
        }
        if (!PlayerPrefs.HasKey("playthroughContinueCount"))
        {
            continueCount = 0;
        }
        healthSlider.value = currentHealth;
        damagedTimer = 0;
        FadeToBlackShade.GetComponent<CanvasRenderer>().SetAlpha(0); //setting alpha of this instead of the Image
        TakeDamageShadeCR = TakeDamageShade.GetComponent<CanvasRenderer>();
        TakeDamageShadeCR.SetAlpha(0);
        takeDamageFadeOutFinished = true;
    }

    // Update is called once per frame
    void Update()
    {
        damagedTimer += Time.deltaTime; //increment according to time here

        if(playerIsDead) //during which wave time would not continue, shop could not be entered, and player could not move, shoot, nor use skills, and player's sprite animation would be paused - or, could keep it animating, consistent with the shop animation, though noting of eg. enemies dying and such as well and such animations, with eg. freezing of enemies as well
        {
            currDeadTimeTowardsGameOver += PlayerWeapons.pDeltaTime;
            GetComponentInChildren<Animator>().enabled = false;

            //do fade out and corresponding loading of game over scene
            Shop.IncrementImgAlphaProportionalToDuration(FadeToBlackShade, deadFadeToBlackDuration); //commented out as it would be used for images; renamed since commenting this out
            float transitionDuration = deadFadeToBlackDuration + 0.5f;

            WaveManager wManager = GameObject.FindGameObjectWithTag("WaveManager").GetComponent<WaveManager>();
            float fadingMusicVol = (1 - currDeadTimeTowardsGameOver / transitionDuration) * MusicSFXVolChange.MusicVol;
            wManager.BGMusicSource.volume = fadingMusicVol; //fade music
            wManager.ShadowModeMusicSource.volume = fadingMusicVol;
            
            if (currDeadTimeTowardsGameOver >= transitionDuration) //switch scenes 0.5 sec. after the fade to black would be done
            {
                SaveQuit.resetStaticVarsOnThisSceneChange();
                SceneManager.LoadScene("GameOverScene");
            }
        }
        else if (!takeDamageFadeOutFinished) //for the 'take damage' shade: fade out where the period of the entire fadeout would be to be takeDamageShadeActiveDuration
        {
            Shop.DecrementImgAlphaProportionalToDuration(TakeDamageShadeCR, takeDamageShadeActiveDuration, takeDamageShadeFullAlpha);
            if(TakeDamageShadeCR.GetAlpha() <= 0)
            {
                takeDamageFadeOutFinished = true;
            }
        }
        if (!playerIsDead && !WaveManager.fadeScreenIsActive)
        {
            amtTimeSinceResetPlayerResults += Time.deltaTime;
        }
    }

    public void TakeDamage(int damageAmount)
    {

        damaged = true;
        damageAmount /= 1; //halve damage taken, as a way to lower difficulty //2/5/19: halve damage taken again, so now damage would be a quarter of what it would have been; 2/6/19: revert damage taken to original levels, but add continuing

        if (!WaveManager.isShadowMode)
        {
            currentHealth -= damageAmount;
        }
        else
        {
            currentHealth -= (int)(damageAmount * 1.3f); //Shadow Mode difference - enemies do 30% more damage
        }

        if (!playerIsDead) //play sounds only if player isn't dead
        {
            playerAudio.clip = TakeDamageSound;
            playerAudio.PlayOneShot(TakeDamageSound);

            //take damage tint
            takeDamageFadeOutFinished = false;
            TakeDamageShadeCR.SetAlpha(takeDamageShadeFullAlpha);

            numTimesPlayerTakenDamage++;
        }

        if (currentHealth <= 0 && !playerIsDead)
        {
            playerIsDead = true;
            //PlayerPrefs.DeleteKey("currentLevel"); //delete progress of level here, though as of 2/6/19, such would be done on pressing "End Game" on the Game Over screen
            ShadowPM.savingData = true; //save PM data here - though requires a ShadowCharacter somewhere; using a 'training' Shadow
            ShadowPM.savingMovementData = true; //save movement data here
            return;
        }
    }
}
