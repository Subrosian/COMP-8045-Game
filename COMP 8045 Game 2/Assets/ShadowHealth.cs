using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ShadowHealth : MonoBehaviour {
    public int maxHealth = 100;
    public int shadowNum;
    //private int currHealth_internal
    public int currHealth
    {
        set
        {
            HealthBarAboveObj hbao = GetComponent<HealthBarAboveObj>();
            if (hbao != null && hbao.HealthBar != null) //where such could be null on Start ...
            {
                hbao.HealthBar.GetComponent<Slider>().value = value;
            }
            PlayerPrefs.SetInt("shadow" + shadowNum + "Health", value);
        }
        get
        {
            return PlayerPrefs.GetInt("shadow" + shadowNum + "Health");
        }
    }

    //numTimesShadowTakenDamage - as one means of assessing playstyle differences - where this can be reset with debug
    public int numTimesShadowTakenDamage
    {
        set
        {
            PlayerPrefs.SetInt("numTimesShadow" + shadowNum + "TakenDamage", value);
        }
        get
        {
            if (PlayerPrefs.HasKey("numTimesShadow" + shadowNum + "TakenDamage"))
            {
                return PlayerPrefs.GetInt("numTimesShadow" + shadowNum + "TakenDamage");
            }
            else
            {
                PlayerPrefs.SetInt("numTimesShadow" + shadowNum + "TakenDamage", 0);
                return PlayerPrefs.GetInt("numTimesShadow" + shadowNum + "TakenDamage");
            }
        }
    }

    //amt. time that the Shadow would have been with an active ShadowHealth since resetting damage taken
    public float amtTimeSinceResetShadowResults
    {
        set
        {
            PlayerPrefs.SetFloat("amtTimeSinceResetShadow" + shadowNum + "Results", value);
        }
        get
        {
            if (PlayerPrefs.HasKey("amtTimeSinceResetShadow" + shadowNum + "Results"))
            {
                return PlayerPrefs.GetFloat("amtTimeSinceResetShadow" + shadowNum + "Results");
            }
            else
            {
                PlayerPrefs.SetFloat("amtTimeSinceResetShadow" + shadowNum + "Results", 0);
                return PlayerPrefs.GetFloat("amtTimeSinceResetShadow" + shadowNum + "Results");
            }
        }
    }

    //Something tentative, as of 3/19/19 - where YKWIM by this
    //public int shadowKillsScore
    //{
    //    set
    //    {
    //        PlayerPrefs.SetInt("shadow" + shadowNum + "KillsScore", value);
    //    }
    //    get
    //    {
    //        return PlayerPrefs.GetInt("shadow" + shadowNum + "KillsScore");
    //    }
    //}

    //public bool isGettingDamaged;
    public float damagedTimer;

    //From EnemyHealth
    Rigidbody rigidBody;
    public bool isDead;
    float deadFadeTimer;
    public const float deadFadeDuration = 0.75f;

    private float prevHitColorTimer;
    public float hitColorTimer;
    private const float hitColorDuration = 0.2f; //the duration that a hit color would be to be
    private static Color hitColor //made to be with values identical to those of freezeColor for familiarity
    {
        get { return new Color(241f / 255f, 255f / 255f, 0, initAlpha); }
    }
    private const float initAlpha = 0.75f;
    ShadowPM PM;

    // Use this for initialization

    void Awake()
    {
    }

    void Start () {
        PM = GetComponent<ShadowPM>();
        if (!(WaveManager.isShadowMode) && !PM.usedForLoggingTraining) //make Shadow cease to exist on normal mode, unless it is the single Shadow used for training whose data would be saved
        {
            Destroy(gameObject);
        }
        rigidBody = GetComponent<Rigidbody>();
        deadFadeTimer = 0;
        hitColorTimer = 0;

        //noting of this code as to be executed before the HealthBarAboveObj component's Start code
        if (/*!WaveManager.isNewGame && */PlayerPrefs.HasKey("shadow" + shadowNum + "Health"))
        {
            //Debug.Log("Retrieved health for shadow " + shadowNum);
            currHealth = PlayerPrefs.GetInt("shadow" + shadowNum + "Health");
        }
        else
        {
            currHealth = maxHealth;
            //Debug.Log("Set health to maxHealth for shadow " + shadowNum);
        }
    }
	
	// Update is called once per frame
	void Update () {
        //die and take damage as an enemy would
        damagedTimer += Time.deltaTime; //increment according to time here
        //J8045: 2D Hitflash - setting color of SpriteRenderer instead of the shader color?
        if (isDead)
        {
            GameObject rootParent = gameObject;
            while (rootParent.transform.parent != null)
            {
                rootParent = rootParent.transform.parent.gameObject;
            }
            //Could set alpha somehow here as well
            Color currColor = GetComponentInChildren<SpriteRenderer>().color;
            deadFadeTimer += Time.deltaTime;
            GetComponentInChildren<SpriteRenderer>().color = new Color(currColor.r, currColor.g, currColor.b, (1f - (deadFadeTimer / deadFadeDuration)) * initAlpha);
            
            if (deadFadeTimer >= deadFadeDuration)
            {
                Destroy(GetComponent<HealthBarAboveObj>().HealthBar);
                Destroy(rootParent);
            }
        }

        if (!WaveManager.fadeScreenIsActive)
        {
            amtTimeSinceResetShadowResults += Time.deltaTime;
        }

        if (hitColorTimer < 0)
        {
            hitColorTimer = 0;
        }
        if (hitColorTimer > 0) /*apparently, component is still present if unchecked in Unity, and can note the variable in such a component*/ //color being active (used for hit mechanic) - though is overridden by freeze color
        {
            float hitColorFraction = hitColorTimer / hitColorDuration;
            float nonHitColorFraction = (hitColorDuration - hitColorTimer) / hitColorDuration;
            GetComponentInChildren<SpriteRenderer>().color = new Color(hitColorFraction * hitColor.r + nonHitColorFraction * 1f,
                                                                       hitColorFraction * hitColor.g + nonHitColorFraction * 1f,
                                                                       hitColorFraction * hitColor.b + nonHitColorFraction * 1f, initAlpha); //blend hitColor and the default pure color (aside from being frozen)
            hitColorTimer -= Time.deltaTime;
        }
        prevHitColorTimer = hitColorTimer;
    }



    public void TakeDamage(int hitPoints/*, Vector3 hitPoint */)
    {
        if (isDead)
            return;

        //Todo: Any damage effects
        ////hitflash code - apply color to each renderer object
        hitColorTimer = hitColorDuration;

        currHealth -= hitPoints;
        numTimesShadowTakenDamage++;
        if (currHealth <= 0)
        {
            isDead = true;
            Die();
        }
    }

    public void Die()
    {
        Destroy(rigidBody);
        Debug.Log("shadowCharacters contains rootParent before removal: " + new List<GameObject>(LocateSeekPursueTarget.shadowCharacters).Contains(gameObject));
        LocateSeekPursueTarget.shadowCharacters.Remove(gameObject); //update the shadowCharacters variable
        Debug.Log("shadowCharacters contains rootParent after removal: " + new List<GameObject>(LocateSeekPursueTarget.shadowCharacters).Contains(gameObject));

        GameObject rootParent = gameObject;
        while (rootParent.transform.parent != null)
        {
            rootParent = rootParent.transform.parent.gameObject;
        }
        Debug.Log("rootParent: " + rootParent.name);
        if (rootParent.GetComponentsInChildren<Collider2D>() != null)
        {
            //destroy all colliders of Shadow
            foreach (Collider2D col in rootParent.GetComponentsInChildren<Collider2D>())
            {
                //noting of error on removing collider in PlayerWeapons, which would rely on a collider - so disabling collider instead
                col.enabled = false;
                //Destroy(col);
            }
        }
    }
}
