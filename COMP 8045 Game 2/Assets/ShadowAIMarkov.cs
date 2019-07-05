using Markov;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Pathfinding;
using UnityEngine;

//Note 3/2/19: This class is not intended to be used; ShadowPM.cs is to be used instead.

public class ShadowAIMarkov : MonoBehaviour {

    public bool isTraining;
    public bool isUsing; //whether the Markov AI is to be used

    public enum EnemyDistance {FAR, /*MIDRANGE, */NEAR}
    
    public enum Action { Attack, Wander, Retreat}


    public MarkovChain<MarkovUnit> AIData;
    public List<List<int>> items_weights;
    public List<string> items_weights_alltogether;

    MarkovUnit CurrAIPrevMarkovUnit;
    public string markovSaveFilePath;

    public bool savingData;
    public bool loadingData;

    Queue<float> avgClosest2eDistances;

    [Serializable()]
    public struct MarkovUnit : IEquatable<MarkovUnit>
    {
        //features: enemy distance, behaviour between wander, attack, retreat ...
        //noting of eg. 'using some number of bits' regarding data 
        //bit packing to be space efficient? Could maybe see how the COMP3900 project and\or such was done? With Acho, Brian, Drew, William and me ...
        //Noting of whether such could be done <for such of efficiency> and such in noting how such data would be represented and used, noting how <such would work for the project regarding time efficiency>
        //noting of 200k frames of data, where each frame would have 2 byte-sized enums, with such being 400KB of data - so, space as not an issue anyway for 200k frames
        //though if eg. regarding 30 playthroughs with each having 100k frames, then 6MB?

        //features of this state
        public EnemyDistance eDistance; //very basic feature for initial testing
        public Action AIAction;

        //tentative features of this state
        //public char neCount; //nearby enemy count
        public char maxSpeed; //speed of enemies
        //public char weaponChoice; //predominant weapon choice, with proportion of ammo-consuming weapon relative to amount of ammo

        //classification of this state ...well, noting of weights and such and how such would be relative to another state in noting eg. state transitions and\or such, with <<how the Markov chain><YKWIM>> would work ... such and eg. a la <<n-grams and such><YKWIM>> - adding weight for the <<distribution><YKWIM>> of what would be given the state<< and\or 'sub-states' and\or such with noting of such of the number of such 'sub-states' and such matching the 'order' of the Markov chain><YKWIM>> <leading up to the last state>
        /// <summary>
        /// For boxing for MarkovChain
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(MarkovUnit other)
        {
            return eDistance == other.eDistance && AIAction == other.AIAction;
        }
    }

    public MarkovUnit prevMarkovData1;

    // Use this for initialization
    void Start() {
        //initial, default values of the first prevMarkovData1
        prevMarkovData1 = new MarkovUnit {
            eDistance = EnemyDistance.FAR,
            AIAction = Action.Wander,
            //tentative <removing text here: values>feature
            //neCount = (char)1,
            maxSpeed = (char)2, 
            //weaponChoice = (char)0
        };

        //storing each frame in training
        isUsing = false;
        isTraining = true;

        AIData = new MarkovChain<MarkovUnit>(1);
        Queue<MarkovUnit> markovUnitEnumerable = new Queue<MarkovUnit>();
        CurrAIPrevMarkovUnit = prevMarkovData1; //set the initial default value of CurrAIPrevMarkovUnit
        markovUnitEnumerable.Enqueue(CurrAIPrevMarkovUnit);
        AIData.Add(markovUnitEnumerable);

        avgClosest2eDistances = new Queue<float>();
    }

    void SaveData()
    {
        SerializableSaveData data = new SerializableSaveData();
        data.AIMarkovChain = AIData;
        //edit the data at this point here, and\or otherwise with basically this kind of block of code on saving data?
        data.AIMarkovChain = AIData;

        Stream stream = File.Open(/*removed here of path string: markovSaveFilePath*/SaveLoad.currentFilePath, FileMode.Create);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        bformatter.Serialize(stream, data);
        stream.Close();
        savingData = false;
    }

    SerializableSaveData LoadData()
    {
        SerializableSaveData data = new SerializableSaveData();
        Stream stream = File.Open(/*removed here of path string: markovSaveFilePath*/SaveLoad.currentFilePath, FileMode.Open);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        data = (SerializableSaveData)bformatter.Deserialize(stream);
        stream.Close();

        // Now use "data" to access your Values
        return data;
    }

    // Update is called once per frame
    void Update () {
        if (isTraining)
        {// Feature and classification<<\generation\action><YKWIM>> <<definitions are implemented here><YKWIM>>


            //Get state of enemy positions and 'player action'<< - with 'player action' being just the difference in distance in frames><YKWIM>>
            MarkovUnit currData;
            //compute median distance of enemies ... or basically determine whether there would be at least 2 enemies within a certain distance

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            //get num. near enemies
            float nearRadius = 2.5f;
            float nearRadiusSqr = nearRadius * nearRadius;
            int nearEnemyCount = 0;
            float avgEnemyDistanceOfClosest2 = 0;


            float minDist = 99999f;
            float minDist_2nd = 100000f;
            GameObject minEnemy = gameObject;
            float maxNearbyEnemySpeed = 0f;

            foreach (GameObject enemy in enemies)
            {
                Vector2 enemyPos = enemy.transform.position;
                Vector2 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;

                float sqrDistance = (enemyPos - playerPos).sqrMagnitude;
                if (sqrDistance <= nearRadiusSqr) //if enemy is nearby
                {
                    nearEnemyCount++;

                    //get max speed of nearby enemies
                    //quantize speeds into tiers: [0,1.5), [1.5,3), 3 or higher as something that could be done for the 3 tiers of max enemy speeds
                    if(enemy.GetComponent<AIPath>().maxSpeed > maxNearbyEnemySpeed)
                    {
                        maxNearbyEnemySpeed = enemy.GetComponent<AIPath>().maxSpeed;
                    }
                }

                float currEnemyDist = (enemyPos - playerPos).magnitude;

                if (currEnemyDist < minDist)
                {
                    //update the closest 2 enemies' distances
                    minDist_2nd = minDist;
                    minDist = currEnemyDist;
                    minEnemy = enemy;
                }
                else if (currEnemyDist < minDist_2nd)
                {
                    //update the 2nd closest enemy's distance if only <<closer than what was the 2nd closest enemy distance found at this point in code running><YKWIM>>
                    minDist_2nd = currEnemyDist;
                }
            }

            avgEnemyDistanceOfClosest2 = (minDist_2nd + minDist) / 2; //to be compared with that 180 frames before and\or such

            //currData.neCount = (char)(nearEnemyCount>=9?9:nearEnemyCount); //number of enemies, as a char, with any being 9 or greater <<as 9><YKWIM>> <<so 9 would basically be <<9+\8+ depending on what such would mean><YKWIM>>><YKWIM>>

            //quantize enemy speed into tiers as <<mentioned before><YKWIM>> and have that as part of the PM MarkovUnit
            if(maxNearbyEnemySpeed < 1.5f)
            {
                currData.maxSpeed = (char)0;
            }
            else if(maxNearbyEnemySpeed < 3f)
            {
                currData.maxSpeed = (char)1;
            }
            else
            {
                currData.maxSpeed = (char)2;
            }

            if (nearEnemyCount >= 2)
            {
                currData.eDistance = EnemyDistance.NEAR;
            }
            else
            {
                currData.eDistance = EnemyDistance.FAR;
            }

            //quantize distance and classify it into NEAR, FAR
            //get attack and classify it ...

            //classification of whether the AI would be doing an "attacking" behaviour - which would just be shooting and maybe approaching, "Wander" behaviour - which would be having been moving sideways towards enemies <<if not alternatingly towards and away from enemies, noting of how such would be done in terms of such, noting of eg. <<rather large><YKWIM>> <<amounts of><YKWIM>> movement that would yield a <<low><YKWIM>> difference in distance from enemies><YKWIM>>, or "Retreat" behaviour - which would be moving away from enemies
            //where such of moving away from enemies would be in increasing the distance between the closest enemy, or increasing the average distance from the closest 2 enemies
            //with comparison with 2 seconds before, or 120 frames before, with noting of eg. positions tracked of eg. enemies in the past 120 frames and\or distances of such enemies from the player in the past 180 frames ... with eg. a queue storing such distances of each frame ... or keeping track of the past 180 frames ...
            //Queue<float> avgClosest2eDistances = new Queue<float>(); //across 180F

            //though could note of how the difference could be due to just the enemies moving and not necessarily the player moving; could note this as a <<super simplification><YKWIM>>
            currData.AIAction = Action.Attack; //set in order to avoid Visual Studio <<from ><YKWIM>>displaying an "error" due to lack of initialization
            float distanceDiff = 0;
            float prevDistance = 0; //made for debugging; otherwise could do without this var
            if (avgClosest2eDistances.Count >= 180)
            {
                //can compare the 180f prior with the curr.
                prevDistance = avgClosest2eDistances.Dequeue();
                distanceDiff = avgEnemyDistanceOfClosest2 - prevDistance;
            }
            else
            {
                if(avgClosest2eDistances.Count > 0)
                {
                    prevDistance = avgClosest2eDistances.Peek();
                }
                distanceDiff = avgEnemyDistanceOfClosest2 - /*((avgClosest2eDistances.Count > 0)?avgClosest2eDistances.Peek() : 0f)*/prevDistance; //get the earliestmost unit of data
            }
            if (distanceDiff > 1)
            {
                //consider such an amount of positive difference as retreat
                currData.AIAction = Action.Retreat;
            }
            else if (distanceDiff > -1)
            {
                //consider such a small \ nonexistent difference as wander
                currData.AIAction = Action.Wander;
            }
            else
            {
                //consider such an amount of negative difference as attack
                currData.AIAction = Action.Attack;
            }
            avgClosest2eDistances.Enqueue(avgEnemyDistanceOfClosest2);

            List<MarkovUnit> dataList = new List<MarkovUnit>(); //<<PM transition><YKWIM>>
            
            //noting of only adding transitions that would not <<lead to different features><YKWIM>>, being <<basically with each set of features being a state, with a probability distribution of outputs that would be the generation><YKWIM>>
            dataList.Add(prevMarkovData1);
            dataList.Add(currData);
            AIData.Add(dataList, 1); //requires an IEnumerable regarding data as the Markov chain would be <<an n-order Markov chain><YKWIM>>
            //Noting of weights and basically adding a weight where the fraction of the weight would be its probability of being triggered by the AI and\or such as a transition ...
            //noting of how eg. such would preserve the Markov data in a lot of frames ... noting of eg. how such would be probabilistic rather than eg. <<moreso periodic and\or such><YKWIM>>
            prevMarkovData1 = currData;
        }
        //else is being used and/or testing

        //if saving MarkovChain - save on the frame that "m" is <<first pressed><YKWIM>>
        if(Input.GetKeyDown("m"))
        {
            savingData = true;
        }

        if (Input.GetKeyDown("u")) //toggle training and using
        {
            isUsing = !isUsing;
            isTraining = !isTraining;
        }
        if (savingData) //<<set this><YKWIM>> to true for one data save
        {
            SaveData();
            Debug.Log("Saving data of the Markov chain into serializable data");
        }

        
        if(loadingData)
        {
            SerializableSaveData data;
            data = LoadData();
            AIData = data.AIMarkovChain;
        }

        if (isUsing) //'action' mode as opposed to 'training'
        {
            // Randomly generate words that resemble the words in the dictionary.
            var rand = new System.Random();

            Queue<MarkovUnit> stateEnumerable = new Queue<MarkovUnit>();
            stateEnumerable.Enqueue(CurrAIPrevMarkovUnit); //noting of 1 MarkovUnit for each state ... so moving between MarkovUnits as to be equivalent to moving between states ...

            IEnumerable<MarkovUnit> nextItem = AIData.NextItem(stateEnumerable, rand); //NOTE: randomly select the next state ... noting of if the features would be selected at random or <<if they would be retrieved from the game><YKWIM>>, with noting of <<eg. what distance would be retrieved and such
            var nextItemQueue = nextItem.Last();
            MarkovUnit newAIMarkovUnit = nextItemQueue;


            if(!newAIMarkovUnit.Equals(CurrAIPrevMarkovUnit)) //<wl: corrected from >if(!newAIState.Equals(nextItemQueue)) //if a state change occured
            {// display new feature\action
                
                string somethingStr = "1";
                //somethingStr += (EnemyDistance.NEAR + '0'); //would return <<the number as <<maybe an int, the entire int made as a string><YKWIM>> rather than a char ascii value><YKWIM>>
                int somethingStrInt = somethingStr[0]; //<<prints ><YKWIM>>49
                int somethingStrInt2 = somethingStr[0] - '0'; //<<prints ><YKWIM>>1
                Debug.Log(somethingStr+"; Int: "+somethingStrInt+"; Int2: "+somethingStrInt2); //getting somethingStr
                CurrAIPrevMarkovUnit = newAIMarkovUnit; //corrected from "nextItemQueue = newAIState;"
                switch(newAIMarkovUnit.AIAction)
                {
                    case Action.Retreat:
                        Debug.Log("Shadow AI now on Retreat");
                        break;
                    case Action.Attack:
                        Debug.Log("Shadow AI now on Attack");
                        break;
                    case Action.Wander:
                        Debug.Log("Shadow AI now on Wander");
                        break;
                }
                //Debug.Log("Shadow AI nearby enemy count: " + (int)newAIMarkovUnit.neCount);
                Debug.Log("Shadow AI nearby enemy max speed: " + (int)newAIMarkovUnit.maxSpeed);
                switch (newAIMarkovUnit.eDistance)
                {
                    case EnemyDistance.NEAR:
                        Debug.Log("Shadow AI now with enemy distance as NEAR");
                        break;
                    case EnemyDistance.FAR:
                        Debug.Log("Shadow AI now with enemy distance as FAR");
                        break;
                }
            }
            ////behaviour of AI based on the action ... of higher level ...
            ////'retreat' behaviour, noting of how such would be hardcoded ...
            //Debug.Log("Shadow AI now on Retreat");
            
            ////'attack' behaviour ...
            //Debug.Log("Shadow AI now on Attack");
            ////'wander' behaviour ...
            //Debug.Log("Shadow AI now on Wander");
        }
        Dictionary<ChainState<MarkovUnit>, Dictionary<MarkovUnit, int>> items = AIData.items;

        //create a List with the current weights
        items_weights_alltogether = new List<string>();
        foreach (var item in items) //current item ...
        {
            if (item.Key.items.Length >= 1)
            {
                MarkovUnit currItemMarkov = item.Key.items.Last(); //get the last item in the state, if there is one
                string AIActionText = "";
                switch (currItemMarkov.AIAction)
                {
                    case Action.Attack:
                        AIActionText = "Attack";
                        break;
                    case Action.Retreat:
                        AIActionText = "Retreat";
                        break;
                    case Action.Wander:
                        AIActionText = "Wander";
                        break;
                }
                //string nearEnemyCountText = "" + (int)(currItemMarkov.neCount);
                string nearEnemyMaxSpeedText = "" + (int)(currItemMarkov.maxSpeed);
                string eDistanceText = "";
                switch (currItemMarkov.eDistance)
                {
                    case EnemyDistance.FAR:
                        eDistanceText = "FAR";
                        break;
                    case EnemyDistance.NEAR:
                        eDistanceText = "NEAR";
                        break;
                }
                items_weights_alltogether.Add("Unit "+ AIActionText + ", "/* +nearEnemyCountText+ ", "*/+nearEnemyMaxSpeedText+", "+ eDistanceText+": "); //MarkovUnit based on 
            }
            else
            {
                items_weights_alltogether.Add("empty ChainState");
            }
            foreach (var transitionItem in item.Value)
            {
                MarkovUnit ctiMarkov = transitionItem.Key; //current transition item Markov
                string AIActionText = "";
                switch (ctiMarkov.AIAction)
                {
                    case Action.Attack:
                        AIActionText = "Attack";
                        break;
                    case Action.Retreat:
                        AIActionText = "Retreat";
                        break;
                    case Action.Wander:
                        AIActionText = "Wander";
                        break;
                }
                //string nearEnemyCountText = ""+(int)(ctiMarkov.neCount);
                string nearEnemyMaxSpeedText = "" + (int)(ctiMarkov.maxSpeed);
                string eDistanceText = "";
                switch (ctiMarkov.eDistance)
                {
                    case EnemyDistance.FAR:
                        eDistanceText = "FAR";
                        break;
                    case EnemyDistance.NEAR:
                        eDistanceText = "NEAR";
                        break;
                }
                items_weights_alltogether.Add(AIActionText + ", "/* + nearEnemyCountText + ", "*/ +nearEnemyMaxSpeedText + ", " + eDistanceText + ": "+transitionItem.Value.ToString()); //weight of the MarkovUnit
            }
        }
    }
}
