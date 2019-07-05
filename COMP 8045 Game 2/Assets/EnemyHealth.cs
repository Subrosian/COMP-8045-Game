using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{

    public int maxHealth;
    public int currHealth;
    public int killScore;

    Rigidbody rigidBody;
    public bool isDead;
    float deadFadeTimer;
    public static float deadFadeDuration = 0.75f;
    
    public bool isBoss;
    private float prevHitColorTimer;
    public float hitColorTimer;
    private static float hitColorDuration = 0.2f; //the duration that a hit color would be to be
    private static Color hitColor //made to be with values identical to those of freezeColor for familiarity
    {
        get { return new Color(241f / 255f, 255f / 255f, 0); }
    }

    public static bool BossKilled = false;

    //   //boss health - handled in PlayerHealth
       public static bool bossExists = false;
       public Slider bossHealthSlider;
       public WaveManager waveManager;
    public EnemySpawner creationSpawnPoint; //tracking for removing the enemy from the respective EnemySpawner's enemyCount when the enemy is destroyed

    void Awake()
    {
    }

    //// Use this for initialization
    void Start()
    {
        currHealth = maxHealth;
        rigidBody = GetComponent<Rigidbody>();
        deadFadeTimer = 0;
        hitColorTimer = 0;

        if (isBoss)
        {
            bossExists = true;
            waveManager = GameObject.FindGameObjectWithTag("WaveManager").GetComponent<WaveManager>();
            bossHealthSlider = waveManager.bossHealthSlider;
            bossHealthSlider.maxValue = maxHealth;
            bossHealthSlider.value = currHealth;
            bossHealthSlider.minValue = 0;
            waveManager.bossEnemyHealth = this; //handle health updating in WaveManager, where the updates would only occur once per Update there
        }
    }


    //// Update is called once per frame
    void Update()
    {
        //J8045: 2D Hitflash - setting color of SpriteRenderer instead of the shader color?

        if(isDead)
        {
            GameObject rootParent = gameObject;
            while (rootParent.transform.parent != null)
            {
                rootParent = rootParent.transform.parent.gameObject;
            }
            //Could set alpha somehow here as well
            Color currColor = GetComponentInChildren<SpriteRenderer>().color;
            deadFadeTimer += Time.deltaTime;
            GetComponentInChildren<SpriteRenderer>().color = new Color(currColor.r, currColor.g, currColor.b, (1f - (deadFadeTimer / deadFadeDuration)));
            //Debug.Log("dead alpha: "+currColor.a+"; deadFadeTimer: "+deadFadeTimer+"; deadFadeDuration: "+deadFadeDuration+"; expression: "+ (1f - (deadFadeTimer / deadFadeDuration)));
            if (deadFadeTimer >= deadFadeDuration)
            {
                if (rootParent.GetComponentInChildren<AnimateToMovement>() != null)
                {
                    rootParent.GetComponentInChildren<AnimationToMovement>().enabled = false;
                }
                Destroy(rootParent);
            }
        }

        if (hitColorTimer < 0)
        {
            hitColorTimer = 0;
        }
        if (hitColorTimer > 0 && !(GetComponent<EnemyMovement>().freezeColorTimer > 0)) /*apparently, component is still present if unchecked in Unity, and can note the variable in such a component*/ //color being active (used for hit mechanic) - though is overridden by freeze color
        {
            float hitColorFraction = hitColorTimer / hitColorDuration;
            float nonHitColorFraction = (hitColorDuration - hitColorTimer) / hitColorDuration;
            GetComponentInChildren<SpriteRenderer>().color = new Color(hitColorFraction * hitColor.r + nonHitColorFraction * 1f, 
                                                                       hitColorFraction * hitColor.g + nonHitColorFraction * 1f, 
                                                                       hitColorFraction * hitColor.b + nonHitColorFraction * 1f); //blend hitColor and the default pure color (aside from being frozen)
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

        if (!WaveManager.isShadowMode)
        {
            currHealth -= hitPoints;
        }
        else
        {
            if (!isBoss)
            {
                currHealth -= (int)(hitPoints / 2.0f); //lessen the health in order to make the duration of the boss not that much longer than what would be on the Normal Mode, with where enemies' healths apart from the boss's health would still be present in adding difficulty to the wave with such in balancing against Shadows, and with noting of how long that Shadows would be e.g. normally expected to survive on such a wave and such of such a balance on such a boss wave on the Shadow mode - where YKWIM by this
            }
            else
            {
                currHealth -= (int)(hitPoints / 2.8f);//Shadow mode difference - damage taken divided by 2.8f, with noting of whatever would be expected presence and firepower of the Shadows and any rough 'guesstimate' of such
            }
        }
        if (currHealth <= 0)
        {
            isDead = true;
            Die(true);
        }
    }

    public void Die(bool giveScore)
    {
        Destroy(rigidBody);
        GameObject rootParent = gameObject;
        while (rootParent.transform.parent != null)
        {
            rootParent = rootParent.transform.parent.gameObject;
        }
        //Debug.Log("rootParent: " + rootParent.name);
        if (rootParent.GetComponentsInChildren<Collider2D>() != null)
        {
            //destroy all colliders of enemy
            foreach (Collider2D col in rootParent.GetComponentsInChildren<Collider2D>())
            {
                //noting of error on removing collider in PlayerWeapons, which would rely on a collider - so disabling collider instead
                col.enabled = false;
                //Destroy(col);
            }
        }

        if (giveScore)
        {
            MoneyManager.money += killScore;
            MoneyManager.TotalKillScore += killScore; //add kill score to a stored value that would not be subtracted, as a tally for end game results
            MoneyManager.TotalKills++;
        }
    }

    void OnDestroy() //noting that this would be called regardless of whether an enemy dies or not on scene transitions, as I would note - where YKWIM by this
    {
        if (isBoss && isDead)
        {
            //could do some death animation
            BossKilled = true;
        }
        creationSpawnPoint.enemyCount--; //reduce from the tracked enemy count
    }
}
