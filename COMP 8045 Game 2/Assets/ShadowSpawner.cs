using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowSpawner : MonoBehaviour {

    GameObject spawnedShadow;
    public GameObject shadowPrefab;
    public int shadowNum; //used for saving Shadow healths as PlayerPrefs data

	// Use this for initialization
	void Start () {
        if (WaveManager.isShadowMode)
        {
            spawnedShadow = Instantiate(shadowPrefab, transform.position, transform.rotation);
            ShadowHealth sHealth = spawnedShadow.GetComponent<ShadowHealth>();
            sHealth.shadowNum = shadowNum;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PerLevelUpdate()
    {
        if(!WaveManager.isShadowMode)
        {
            return;
        }
        //spawn Shadow if it died; otherwise, refill health of Shadow by the amount that a player's health would be refilled
        if (spawnedShadow == null || spawnedShadow.GetComponent<ShadowHealth>().isDead) //if Shadow is dead
        {
            spawnedShadow = Instantiate(shadowPrefab, transform.position, transform.rotation);
            ShadowHealth sHealth = spawnedShadow.GetComponent<ShadowHealth>();
            sHealth.shadowNum = shadowNum; //before the currHealth assignment so that the corresponding PlayerPrefs would be set
            sHealth.currHealth = WaveManager.healthRecoveryPerWave;
        }
        else
        {
            ShadowHealth sHealth = spawnedShadow.GetComponent<ShadowHealth>();
            sHealth.currHealth = (WaveManager.healthRecoveryPerWave + sHealth.currHealth) > sHealth.maxHealth ? sHealth.maxHealth : (WaveManager.healthRecoveryPerWave + sHealth.currHealth);
        }
    }
}
