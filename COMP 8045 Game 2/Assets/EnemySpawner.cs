using UnityEngine;
using System.Collections;
using Markov;

public class EnemySpawner : MonoBehaviour {

    public GameObject[] enemy;
    public static bool spawning; //global condition on whether to spawn
    public static int level {
        get {return WaveManager.level;}
    } //what level, or wave, this would be, which determines which enemy is to spawn.
    public float[] spawnInterval; //'global' interval shared across all spawners; can separate them as well.
    private int spawnLimit;
    const int DEFAUlT_SPAWNLIMIT = 20;
    const int DEFAULT_SPAWNRATE = 3;
    public int enemyCount;
    public int[] customSpawnLimit;
    public bool isBossSpawner;

    public void ResetSpawnInvoke()
    {
        if (isBossSpawner)
        {
            return; //handle boss spawns in Update instead
        }

        if (customSpawnLimit.Length >= level-1+1 && customSpawnLimit[level-1] != 0)
        {
            spawnLimit = customSpawnLimit[level-1];
        }
        else
        {
            spawnLimit = (int)(DEFAUlT_SPAWNLIMIT * DEFAULT_SPAWNRATE * 1f / spawnInterval[level - 1]); //make the spawn limit for the enemies from this SpawnPoint where the default would be 20 for a spawn interval of 3, where such a spawn limit would be inversely proportional to the spawn interval
        }
        Debug.Log("spawnLimit: " + spawnLimit);
        CancelInvoke("Spawn");
        InvokeRepeating("Spawn", 0, spawnInterval[level - 1]);
    }

	// Use this for initialization
	void Start () {
        enemyCount = 0;

        if(isBossSpawner)
        {
            spawnLimit = 1;
            return; //handle boss spawns in Update instead
        }

        // Repeatedly call Spawn at intervals of spawnInterval.
        if (enemy[level - 1] != null)
        {
            
            if (customSpawnLimit.Length >= level - 1 + 1 && customSpawnLimit[level - 1] != 0)
            {
                spawnLimit = customSpawnLimit[level - 1];
            }
            else
            {
                spawnLimit = (int)(DEFAUlT_SPAWNLIMIT * DEFAULT_SPAWNRATE * 1f / spawnInterval[level - 1]); //make the spawn limit for the enemies from this SpawnPoint where the default would be 20 for a spawn interval of 3, where such a spawn limit would be inversely proportional to the spawn interval
            }
            //Debug.Log("spawnLimit: " + spawnLimit);
            InvokeRepeating("Spawn", 0, spawnInterval[level - 1]); //recreated each scene, so just refer to this ... //2/19/19: maybe this might behave differently depending on how the scene loads? Maybe if it takes longer then it might be different? So, could just ensure that bosses spawn by checking such in order to be safe? And have bosses as exclusive to BossSpawnPoint objects?
        }
       
    }

    void Spawn ()
    {
        if (spawning && enemyCount <= spawnLimit-1 && enemy[level - 1] != null)
        { //check global condition, in addition to if there would be an enemy
            GameObject newEnemy = Instantiate(enemy[level - 1], transform.position, transform.rotation);
            enemyCount++; //increment enemy count for this enemy spawner
            newEnemy.GetComponentInChildren<EnemyHealth>().creationSpawnPoint = this;
        }
    }

	// Update is called once per frame
	void Update () {
        if(isBossSpawner && enemy[level - 1] != null && !EnemyHealth.bossExists && !EnemyHealth.BossKilled) //if this is a boss spawner, a boss is on this wave but not a boss has not yet spawned and a boss has not yet been killed on this wave
        {
            Debug.Log("isBossSpawner: " + isBossSpawner + ", enemy[level - 1]: " + enemy[level - 1] + "; EnemyHealth.bossExists: " + EnemyHealth.bossExists + "; EnemyHealth.bossKilled: " + EnemyHealth.BossKilled);
            Spawn();
        }
    }
    public void PerLevelUpdate()
    {
        ResetSpawnInvoke();
    }
}
