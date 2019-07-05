using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour {

    public float attackFrequency = .5f;
    public int attackDamage = 10;

    GameObject player;

    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;

    bool playerInRange;
    ShadowHealth sHealthOfShadowInRange;
    //float timer;

    // Use this for initialization
    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
        enemyHealth = GetComponent<EnemyHealth>();
        sHealthOfShadowInRange = null;
    }

    // Update is called once per frame
    void Update()
    {
        //timer += Time.deltaTime; //run only concurrently once
        if (PlayerHealth.damagedTimer >= attackFrequency && playerInRange && !enemyHealth.isDead) //whichever enemy runs its update loop first once the damagedTimer of the player would meet the attack frequency would run first - so, the fastest attacking enemy would attack first. Perhaps the highest-damage enemy could do its damage instead if balancing such
        {
            AttackPlayer();
        }
        if(sHealthOfShadowInRange != null && sHealthOfShadowInRange.damagedTimer >= attackFrequency && !enemyHealth.isDead/* though noting of this and being the last to check, in case this component would become null when EnemyAttack would run? Though both would be destroyed together before a cycle would run for each? Though such being usually an unnecessary check anyway*/)
        {
            AttackShadow();
        }
    }

    void AttackPlayer()
    {
        GameObject rootParent = gameObject;
        while (rootParent.transform.parent != null)
        {
            rootParent = rootParent.transform.parent.gameObject;
        }
        if (rootParent.GetComponentInChildren<EnemyMovement>().movementFreezeTimer > 0 || WaveManager.fadeScreenIsActive) //prevent attacking if movement is frozen or shop is active
        {
            return;
        }

            PlayerHealth.damagedTimer = 0;

        if (!PlayerHealth.playerIsDead)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    void AttackShadow()
    {
        GameObject rootParent = gameObject;
        while (rootParent.transform.parent != null)
        {
            rootParent = rootParent.transform.parent.gameObject;
        }
        if (rootParent.GetComponentInChildren<EnemyMovement>().movementFreezeTimer > 0 || WaveManager.fadeScreenIsActive) //prevent attacking if movement is frozen or shop is active
        {
            return;
        }

        sHealthOfShadowInRange.damagedTimer = 0;

        if (!sHealthOfShadowInRange.isDead)
        {
            sHealthOfShadowInRange.TakeDamage(attackDamage);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == player && other.GetType() == typeof(BoxCollider2D)) //on connecting with specifically the BoxCollider2D of the player, all of which would be dedicated to finding if the player would be in range (for taking damage)
        {
            playerInRange = true; //used for taking damage<< of the player><YKWIM>>
        }
        else if (other.gameObject.GetComponent<ShadowHealth>() != null && other.GetType() == typeof(BoxCollider2D))
        {
            ShadowHealth sHealth = other.gameObject.GetComponent<ShadowHealth>();
            sHealthOfShadowInRange = sHealth;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            playerInRange = false;
        }
        else if(other.gameObject.GetComponent<ShadowHealth>() != null && other.GetType() == typeof(BoxCollider2D))
        {
            ShadowHealth sHealth = other.gameObject.GetComponent<ShadowHealth>();
            sHealthOfShadowInRange = null;
        }
    }

}
