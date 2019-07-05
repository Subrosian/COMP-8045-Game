using Markov;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Pathfinding;
using UnityEngine;
using System.Xml.Linq;
using Newtonsoft.Json;
//using MathNet.Numerics.Statistics;

public class ShadowPM : MonoBehaviour {

    //feature combination
    //represented as: digits - where digits would be eg. unpacked with extracting each digit one by one through eg. division by 10, or ints, or a string
    //with noting of how much space that such would take, and how much that such would improve space ... with noting of other things, such as graphics and such, vs. readability and\or such
    //as an int: 
    //as a string: [0-2][0-2][0-255][0-7][0-7]
    //with maybe tokens to indicate <<variable><YKWIM>> numbers of digits for a value<< of a feature><YKWIM>>, such as two '|' chars surrounding such <<variable number digits><YKWIM>>, and either hardcoding the amounts of the rest of the digits to specific <<amounts><YKWIM>> <<or all of such of said rest of the digits being set to 1><YKWIM>>
    //and then: [0-2][0-1][0-8]
    //as actions and\or such from such data, though such not being part of the feature combination string and\or such but as part of a weight in the action probability distribution
    struct Features
    {

    }

    //directions of movement for movement vectors quantized into 45-degree angles for 8 directions of movement used for Shadow AI movement direction and animation
    public float dir_angleRad; //-100f would be neutral
    public int dir_angleDeg;
    public int dir_lastFacedAngleDeg;

    //for usedForLoggingTraining Shadow - where YKWIM by this
    //public float currMoveDirRelativeToSM = 0;
    //public float currMoveDirRelativeToMN = 0;
    public float currMoveDirRelativeToSM_Using = 0;
    public float currMoveDirRelativeToMN_Using = 0;

    //could make the action probability distribution a Dictionary, or something like what would have been seen in the MarkovChain and\or such

    //map of (feature combination) : (action probability distribution) pairs
    Dictionary<string, Dictionary<string, int>> PMData; //PMData
    Dictionary<string, List<KeyValuePair<string, int>>> PMData_maintainOrder; //Part of PMData, as one of the randomly selected outcomes, which would be a cycle of generations - with maintaining order of the generation periods within each cycle
    Dictionary<string, Tuple<int, int>> PMDataOrdered_cyclePosn; //index and current weight value

    #region About PMData_ofmaintainOrderCycles and types with the corresponding implementation of such
    Dictionary<string, List</*Tuple<*/List<KeyValuePair<string, int>>/*, int>*/>> PMData_ofmaintainOrderCycles; //where the cycle would likely stick with having a weight value of 1? And whose length would be from when the player would be with the corresponding environment input to: 
    //  when the player would not be in such an environment OR when some time limit for a cycle would have been reached 
    //      (with the time limit being based on some value relative to what would be maybe a typical length of a period, with any experimentation with data to determine such)
    //noting of - due to unlikeliness of obtaining the same cycle in training as another and 
    //computation of determining whether two Lists - for such cycles - would be Equal? 
    //For:  
    //  such an update to weight in adding a cycle, and in randomly selecting a cycle, 
    //  noting of just adding cycles without weight as what could be done instead of a weighted random selection with such weights
    //thus instead, just adding the cycles to a List instead? With noting of such use of statistics for such ... and such of lack of weighted random selection in terms of using weight values ... and instead just selecting among Lists that could be repeated, with such an expected space cost from such for such saving of expected computational time cost as said
    //and noting of such of machine learning and if what I would have would be considered as such, with noting of the transferrability of such a skill and such
    #endregion

        
    List<KeyValuePair<string, int>> PMDataOrdered2_currTrainingCycle; //current cycle being constructed within training, among the distribution of cycles - to be inserted upon completion
    int PMDataOrdered2_currTrainingCycle_Length;
    int PMDataOrdered2_cycleLengthLimit = 240;
    string PMDataOrdered2_currTrainingCycle_featureStr; //featureStr that PMDataOrdered2_currCycle would be for
    List<KeyValuePair<string, int>> PMDataOrdered2_currUsingCycle;
    Tuple<int, int> PMDataOrdered2_cyclePosn; //index and current <rmtxth: weight value>position-of-actionperiod of PMData_ofmaintainOrderCycles's current cycle given PMDataOrdered2_currUsingCycle
    //bool PMDataOrdered2_toSwapUsingCycleWhenDone;

    //for fixed cycles for the relative movement implementation
    Dictionary<string, List<List<Pair<genBehaviourData, float>>>> PMDataOrdered3_ofFixedLengthCycles = new Dictionary<string, List<List<Pair<genBehaviourData, float>>>>();
    List<Pair<genBehaviourData, float>> PMDataOrdered3_currTrainingCycle;
    string PMDataOrdered3_currTrainingCycle_featureStr;
    float PMDataOrdered3_currTrainingCycle_Length;
    List<Pair<genBehaviourData, float>> PMDataOrdered3_currUsingCycle;
    Tuple<int, float> PMDataOrdered3_cyclePosn; //index and current position-of-movementdir
    public int cyclePosn_Index;
    public float cyclePosn_weight;
    bool resetCurrTrainingCycle_withLoadedDataThisUpdate = false;
    bool resetCurrUsingCycle_withLoadedDataThisUpdate = false;
    bool getCurrMovementAngleAndMag = true;

    public bool SwappedPlacesThisUpdate = false; //for Shadow swapping between player and Shadow

    [Serializable()]
    public struct genBehaviourData : IEquatable<genBehaviourData>
    {
        public float movement_thisUpdate_CCWAngleRelativeToSMPos;
        public float movement_thisUpdate_CCWAngleRelativeToMNPos;
        public float movement_thisUpdate_Magnitude;
        //could add time period of the movement data, if eg. frame rate would vary and such would lead to varying durations between each movement data added and also used
        public bool isFiring_thisUpdate;

        public bool Equals(genBehaviourData other)
        {
            return movement_thisUpdate_CCWAngleRelativeToSMPos == other.movement_thisUpdate_CCWAngleRelativeToSMPos && movement_thisUpdate_CCWAngleRelativeToMNPos == other.movement_thisUpdate_CCWAngleRelativeToMNPos && movement_thisUpdate_Magnitude == other.movement_thisUpdate_Magnitude;
        }
    };

    public bool isTraining;
    public bool isUsing; //whether the Markov AI is to be used
    public bool toggleTrainingAndUsing;

    public enum EnemyDistance { FAR, /*MIDRANGE, */NEAR, FAR_OFFSCREEN }

    public enum Action { Attack, Wander, Retreat }
    public Dictionary<EnemyDistance, string> debugStr_dist;
    public Dictionary<Action, string> debugStr_act;

    string prevGenStr = "9"; //a value that would not be intended to correspond to behaviour - as basically "n/a" behaviour
    
    public List<List<int>> items_weights;
    public List<string> items_weights_alltogether;

    public string markovSaveFilePath;

    //done in order to make saving occur from just one PM among the ShadowCharacters
    public static bool savingData;
    public static bool savingData_nextFrame;

    public bool loadingData;
    public bool loadingData_nextFrame;

    Queue<float> avgClosest2eDistances_nonplayer; //distances for non-player
    static Queue<float> avgClosest2eDistances_player = new Queue<float>(); //distances for player

    public static void clearavgClosest2eDistances_player() //used for resetting from the Awake() method of a Manager object, such as WaveManager
    {
        avgClosest2eDistances_player.Clear();
    }

    //timer for setting prev. movement directions
    float prevMovementDirTimer;
    float prevMovementDirInterval = 0.5f;
    Vector3 prevPosAtStartOfInterval = Vector3.zero;
    Vector3 closestEnemyPosAtStartOfInterval = new Vector3(-9999f, -9999f, -9999f);
    Vector3 medianEnemyPosAtStartOfInterval = Vector3.zero;
    bool resetMovementDirTimerThisUpdate = false;

    const int MAX_PREVDIRS = 1;
    Queue<int> QuantizedDirAngle_RelativeToCM;
    Queue<int> QuantizedDirAngle_RelativeToMN;

    Vector3 prevPlayerPos;
    Vector3 prevClosestEnemyPosRelativeToPlayer;
    Vector3 prevMedianEnemyPosRelativeToPlayer;

    Vector3 prevShadowPos;
    Vector3 prevClosestEnemyPosRelativeToShadow;
    Vector3 prevMedianEnemyPosRelativeToShadow;
    public GameObject minEnemyRelativeToShadow_prevOrNot;
    public GameObject minEnemyToShootRelativeToShadow_prevOrNot;
    public bool isFiringThisUpdate;

    public struct gizmoSphereProperties
    {
        public Vector3 position;
        public Color color;
        public float radius;
    }
    List<gizmoSphereProperties> gizmoSpheresToDraw;
    bool enableOnDrawGizmos = false;
    
    //If this is the single Shadow used for training
    //With noting of this and such an instance also being used as a predictive model if used in such a way, as testing
    public bool usedForLoggingTraining;
    public static bool savingMovementData; //save movement data while it would be being tracked by the ShadowCharacter instance that would be also used for logging training

    public int numPlayerAndAIMovementMatchingUpdates, numPlayerAndAIShootingMatchingUpdates, numPlayerAndAIMovingAndShootingMatchingUpdates, numPlayerAndAITotalTestingUpdates;
    public float PlayerAndAIMovementAngleDiffTotalInAllUpdates;
    public int numPlayerAndAIMovementHavingNoMovementInPlayerOrAIButNotBothUpdates;
    public int numPlayerAndAIMovementHavingMovementInBothPlayerAndAIUpdates;

    public int numPlayerAndAIMovementMatchingUpdates_enemiesOnScreen, numPlayerAndAIShootingMatchingUpdates_enemiesOnScreen, numPlayerAndAIMovingAndShootingMatchingUpdates_enemiesOnScreen, numPlayerAndAITotalTestingUpdates_enemiesOnScreen;
    public float PlayerAndAIMovementAngleDiffTotalInAllUpdates_enemiesOnScreen;
    public int numPlayerAndAIMovementHavingNoMovementInPlayerOrAIButNotBothUpdates_enemiesOnScreen;
    public int numPlayerAndAIMovementHavingMovementInBothPlayerAndAIUpdates_enemiesOnScreen;

    GameObject player;
    public static RuntimeAnimatorController[] shadowAnimations;

    public bool findLargestKey = false;

    //public Bounds cameraTriggerBounds
    //{
    //    get { return GetComponentInChildren<CameraTriggerSet>().gameObject.GetComponent<BoxCollider2D>().bounds; }
    //}

    //From https://stackoverflow.com/questions/7787994/is-there-a-version-of-the-class-tuple-whose-items-properties-are-not-readonly-an
    [Serializable()]
    public class Pair<T1, T2>
    {
        public T1 First { get; set; }
        public T2 Second { get; set; }

        public Pair(T1 f, T2 s) {
            First = f;
            Second = s;
        }
    }

    [Serializable()]
    public struct MarkovUnit_moreData : IEquatable<MarkovUnit_moreData>
    {
    //features of this state
        public EnemyDistance eDistance; //very basic feature for initial testing

        //generations of this state
        public Action AIAction;

        //tentative features of this state
        //public char neCount; //nearby enemy count
        public char maxSpeed; //speed of enemies

        public Vector2 avgClosest2EnemyPosn;

        /// <summary>
        /// For boxing for MarkovChain
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(MarkovUnit_moreData other)
        {
            return eDistance == other.eDistance && AIAction == other.AIAction && maxSpeed == other.maxSpeed && avgClosest2EnemyPosn == other.avgClosest2EnemyPosn;
        }
    }

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

        //generations of this state
        public Action AIAction;

        //tentative features of this state
        public char maxSpeed; //speed of enemies
        public Queue<int> QuantizedDirAngle_RelativeToCMAtData;
        public Queue<int> QuantizedDirAngle_RelativeToMNAtData;

        public float movementDir_thisUpdate_CCWAngleRelativeToSMPos;
        public float movementDir_thisUpdate_CCWAngleRelativeToMNPos;
        public float movementDir_thisUpdate_Magnitude;

        public Vector2 avgClosest2EnemyPosn;
        public Vector2 medianEnemyPosn;

        //classification of this state ...well, noting of weights and such and how such would be relative to another state in noting eg. state transitions and\or such, with <<how the Markov chain><YKWIM>> would work ... such and eg. a la <<n-grams and such><YKWIM>> - adding weight for the <<distribution><YKWIM>> of what would be given the state<< and\or 'sub-states' and\or such with noting of such of the number of such 'sub-states' and such matching the 'order' of the Markov chain><YKWIM>> <leading up to the last state>
        /// <summary>
        /// For boxing for MarkovChain
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(MarkovUnit other)
        {
            return eDistance == other.eDistance && AIAction == other.AIAction && maxSpeed == other.maxSpeed;
        }
    }

    public MarkovUnit prevMarkovData1;

    //variables for collision handling
    public Vector3 prevMovePosition; //public in order to be used e.g. as a variable for enemy's Pursuit behaviour
    public Vector3 prevMoveDelta; //last movement before any collision would have been detected yet
    bool isColliding = false;
    bool collisionOccurredThisUpdate = false; //used to handle just one collision per update
    List<Collision2D> collisionsThisUpdate;
    public int clearTrimAccessCount = 0;

    public static GameObject[] enemies;

    //For the getData method
    const float nearRadius = 3.5f;
    const float nearRadiusSqr = nearRadius * nearRadius;
    const float withinRangeRadius = 9.0f;
    const float withinRangeRadiusSqr = withinRangeRadius * withinRangeRadius;

    //For collision handling
    BoxCollider2D thisBoxCollider2D;
    Rigidbody2D thisRigidbody2D;


    public int i_selected;
    private WaveManager WaveManagerObj;

    // Use this for initialization
    void Start()
    {
        thisBoxCollider2D = GetComponent<BoxCollider2D>();
        thisRigidbody2D = GetComponent<Rigidbody2D>();

        PMData = new Dictionary<string, Dictionary<string, int>>();
        PMData_maintainOrder = new Dictionary<string, List<KeyValuePair<string, int>>>();
        PMData_ofmaintainOrderCycles = new Dictionary<string, List</*Tuple<*/List<KeyValuePair<string, int>>/*, int>*/>>();
        avgClosest2eDistances_nonplayer = new Queue<float>();
        PMDataOrdered2_currTrainingCycle_featureStr = "";
        //initialize empty dummy for what can be used as the first 'previous cycle'
        PMDataOrdered2_cyclePosn = Tuple.Create<int, int>(0, 0);
        PMDataOrdered2_currUsingCycle = new List<KeyValuePair<string, int>>();
        PMDataOrdered2_currUsingCycle.Add(new KeyValuePair<string, int>("9", 1)); //default - with 'dummy' generation that would be intended as "n/a" that would be to be replaced

        //Testing the Quaternion.Euler code for rotation of vector
        //Vector2 rotatedVector = Quaternion.Euler(0f, 0f, -180f) * new Vector2(-1f, 0);
        //Debug.Log("Quaternion.Euler test of (-1, 0): ("+rotatedVector.x+", "+rotatedVector.y+")");

        float vectorTurnAngle = (Vector2.SignedAngle((Vector2)(new Vector2(0, 1f)) - (Vector2)(prevPosAtStartOfInterval), new Vector2(-1f, 0f)) + 360) % 360;
        Debug.Log("Vector2.SignedAngle test of (-1f, 0) to (0f, 1f): " + vectorTurnAngle);

        //storing each frame in training - though initializations would be as in the prefab
        //isUsing = false;
        //isTraining = true;

        //start with the loaded data
        //add loading display as well
        loadingData_nextFrame = false;
        loadingData = true;

        //initialize the static variables savingData and savingData_nextFrame
        savingData = false;
        savingData_nextFrame = false;

        player = GameObject.FindGameObjectWithTag("Player");
        shadowAnimations = player.GetComponent<TouchMove>().animations;

        prevPlayerPos = player.transform.position;
        prevShadowPos = transform.position;
        prevMovePosition = transform.position;
        prevPosAtStartOfInterval = transform.position;
        QuantizedDirAngle_RelativeToCM = new Queue<int>();
        QuantizedDirAngle_RelativeToMN = new Queue<int>();

        //for cycles
        PMDataOrdered3_ofFixedLengthCycles = new Dictionary<string, List<List<Pair<genBehaviourData, float>>>>();
        PMDataOrdered3_cyclePosn = Tuple.Create<int, float>(0, 0f);
        PMDataOrdered3_currTrainingCycle_featureStr = "";
        //PMDataOrdered3
        PMDataOrdered3_currUsingCycle = new List<Pair<genBehaviourData, float>>(); //dummy
        PMDataOrdered3_currUsingCycle.Add(new Pair<genBehaviourData, float>(new genBehaviourData(), 0f)); //dummy that would be to be replaced

        resetMovementDirTimerThisUpdate = true;

        //isFiringThisUpdate = false;

        minEnemyRelativeToShadow_prevOrNot = gameObject; //default assignment
        minEnemyToShootRelativeToShadow_prevOrNot = gameObject; //default assignment

        //for debug gizmos
        gizmoSpheresToDraw = new List<gizmoSphereProperties>();
        enableOnDrawGizmos = true;

        //movement collision handling code
        collisionsThisUpdate = new List<Collision2D>();
        //Debug.Log("collisionsThisUpdate: " + collisionsThisUpdate.Count);
        collisionsThisUpdate.Clear();
        collisionsThisUpdate.TrimExcess();

        //debug strings
        debugStr_act = new Dictionary<Action, string>(){
            { Action.Attack, "Attack"},
            { Action.Wander, "Wander"},
            { Action.Retreat, "Retreat"}
                };
        debugStr_dist = new Dictionary<EnemyDistance, string>(){
            { EnemyDistance.FAR, "FAR"},
            { EnemyDistance.NEAR, "NEAR"},
            { EnemyDistance.FAR_OFFSCREEN, "FAR_OFFSCREEN"}
                };
        if(usedForLoggingTraining)
        {
            savingMovementData = false;
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
            findLargestKey = true;
        }
        Debug.Log("Application.persistentDataPath: " + Application.persistentDataPath);

        WaveManagerObj = GameObject.FindGameObjectWithTag("WaveManager").GetComponentInChildren<WaveManager>();
    }


    //Credit to this 'invoke next frame' code goes to https://answers.unity.com/questions/347468/how-to-invoke-one-frame.html
    public delegate void Function();

    public void InvokeNextFrame(Function function)
    {
        try
        {
            StartCoroutine(_InvokeNextFrame(function));
        }
        catch
        {
            Debug.Log("Trying to invoke " + function.ToString() + " but it doesnt seem to exist");
        }
    }

    private IEnumerator _InvokeNextFrame(Function function)
    {
        yield return null;
        function();
    }

    void SaveData()
    {
        //do save data operation on the next update in order to give time for OnLoadTransition to show
        OnLoadTransition.Show("Saving player data...");
        savingData = false;
        savingData_nextFrame = true;
        InvokeNextFrame(SaveData_NextUpdate);
    }
    void SaveData_NextUpdate()
    {
        SerializableSaveDataPM data = new SerializableSaveDataPM();
        data.AIPM = PMData;
        data.AIPM_maintainOrder = PMData_maintainOrder;
        data.AIPM_ofmaintainOrderCycles = PMData_ofmaintainOrderCycles;
        data.AIPMOrdered3_ofFixedLengthCycles = PMDataOrdered3_ofFixedLengthCycles;
        //edit the data at this point here, and\or otherwise with basically this kind of block of code on saving data?
        //data.AIPM = PMData; //commenting out; not sure if this was needed

        Stream stream = File.Open(SaveLoadPM.currentFilePath, FileMode.Create);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        bformatter.Serialize(stream, data);
        stream.Close();
        OnLoadTransition.Hide();
        //Debug.Log("Hiding transition");
        savingData_nextFrame = false;
    }

    void LoadData()
    {
        loadingData = false;
        OnLoadTransition.Show();
        Debug.Log("OnLoadTransition.Show();");
        InvokeNextFrame(LoadData_NextUpdate);
    }

    void LoadData_NextUpdate()
    {
        //only load data from a file if the file exists
        if (File.Exists(SaveLoadPM.currentFilePath))
        {
            SerializableSaveDataPM data;
            data = LoadData_NextUpdate_PMData();
            Debug.Log("Loading data of the PM from serializable data");
            PMData = data.AIPM;
            PMData_maintainOrder = data.AIPM_maintainOrder;
            PMData_ofmaintainOrderCycles = data.AIPM_ofmaintainOrderCycles;
            PMDataOrdered3_ofFixedLengthCycles = data.AIPMOrdered3_ofFixedLengthCycles;
        }
        loadingData_nextFrame = false;

        if (WaveManager.DEBUG_FINDLARGESTKEYINPMDATA && findLargestKey && usedForLoggingTraining)
        {
            //Find key with largest length's worth of actions in the associated value
            float largestLengthOfActionsOfAGivenKey = 0;
            string keyWithLargestLengthOfActions = "";

            Dictionary<string, float> setFeaturesStrAndLength = new Dictionary<string, float>();
            float largestLengthOfActionsOfSetFeatures = 0;
            string setFeaturesWithLargestLengthOfActions = "";

            string setFeaturesBlueprint = "xx x"; //get distributions for the features set at where x would be the positions
            List<int> unsetFeaturePositions = new List<int>();
            for(int i=0; i<setFeaturesBlueprint.Length; i++)
            {
                if(setFeaturesBlueprint[i] == ' ')
                {
                    unsetFeaturePositions.Add(i);
                }
            }
            foreach (KeyValuePair<string, List<List<Pair<genBehaviourData, float>>>> pair in PMDataOrdered3_ofFixedLengthCycles)
            {
                float lengthOfActionsOfCurrentKey = 0;

                //get the featureStr with the corresponding unset and set features
                char[] currSetFeaturesStr_CharArray = pair.Key.ToCharArray();
                for (int i = 0; i < currSetFeaturesStr_CharArray.Length; i++) {
                    foreach (int unsetPosn in unsetFeaturePositions)
                    {
                        currSetFeaturesStr_CharArray[unsetPosn] = ' ';
                    }
                }
                string currSetFeaturesStr = new string(currSetFeaturesStr_CharArray);


                //discard pairs whose featureStr's value is not what is seen in setFeaturesBlueprint (although x is a wildcard)
                bool foundUnmatchingFeatureStr = false;
                for (int i = 0; i < currSetFeaturesStr_CharArray.Length; i++)
                {
                    if (setFeaturesBlueprint[i] != 'x' && setFeaturesBlueprint[i] != ' ' && currSetFeaturesStr_CharArray[i] != setFeaturesBlueprint[i])
                    {
                        foundUnmatchingFeatureStr = true;
                        break;
                    }
                }
                if (foundUnmatchingFeatureStr) //if the pair's featureStr does not match what is seen in setFeaturesBlueprint, then do not add the pair's values to the distributions
                {
                    continue;
                }

                //initialize if the key did not exist in the dictionary
                if (!setFeaturesStrAndLength.ContainsKey(currSetFeaturesStr))
                {
                    setFeaturesStrAndLength[currSetFeaturesStr] = 0;
                }

                float noMovementLength = 0;

                foreach (List<Pair<genBehaviourData, float>> sequence in pair.Value)
                {
                    //add length's worth of actions in the sequence to lengthOfActionsOfCurrentKey
                    foreach (Pair<genBehaviourData, float> actionAndLength in sequence)
                    {
                        if (actionAndLength.First.movement_thisUpdate_Magnitude != 0) //if actionAndLength.First.movement_thisUpdate_Magnitude == 0, noting of such of lack of movement
                        {
                            lengthOfActionsOfCurrentKey += actionAndLength.Second;
                        }
                        else
                        {
                            noMovementLength += actionAndLength.Second;
                            lengthOfActionsOfCurrentKey += actionAndLength.Second; //no movement - still count as an action
                        }
                    }
                }

                //add total length associated with this key to what would be the 'key with specifically set features'
                setFeaturesStrAndLength[currSetFeaturesStr] += lengthOfActionsOfCurrentKey;

                if (lengthOfActionsOfCurrentKey > largestLengthOfActionsOfAGivenKey)
                {
                    largestLengthOfActionsOfAGivenKey = lengthOfActionsOfCurrentKey;
                    keyWithLargestLengthOfActions = pair.Key;
                }
                if(setFeaturesStrAndLength[currSetFeaturesStr] > largestLengthOfActionsOfSetFeatures)
                {
                    largestLengthOfActionsOfSetFeatures = setFeaturesStrAndLength[currSetFeaturesStr];
                    setFeaturesWithLargestLengthOfActions = currSetFeaturesStr;
                }
            }
            float largestFeaturesLength = 0;
            string largestFeaturesStr = "";
            float secondLargestFeaturesLength = 0; //not necessarily the second largest, but would be second largest for what would be up to finding the largest at least, but there could be e.g. larger than what would be in this field after finding the largest - as I would understand such to mean
            string secondLargestFeaturesStr = "";
            //List<string> alreadyTakenIndices = new List<string>(); //i.e. where features would be the indices - as I would understand such to mean
            foreach (KeyValuePair<string, float> pair in setFeaturesStrAndLength)
            {
                if(pair.Value > largestFeaturesLength)
                {
                    secondLargestFeaturesLength = largestFeaturesLength;
                    secondLargestFeaturesStr = largestFeaturesStr;

                    largestFeaturesStr = pair.Key;
                    largestFeaturesLength = pair.Value;
                }
                else if(pair.Value > secondLargestFeaturesLength)
                {
                    secondLargestFeaturesLength = pair.Value;
                    secondLargestFeaturesStr = pair.Key;
                }
            }
            Debug.Log("keyWithLargestLengthOfActions: " + keyWithLargestLengthOfActions + "; largestLengthOfActionsOfAGivenKey: " + largestLengthOfActionsOfAGivenKey);
            Debug.Log("setFeaturesWithLargestLengthOfActions: " + setFeaturesWithLargestLengthOfActions + "; largestLengthOfActionsOfSetFeatures: " + largestLengthOfActionsOfSetFeatures);
            Debug.Log("secondLargestFeaturesStr: " + secondLargestFeaturesStr + "; secondLargestFeaturesLength: " + secondLargestFeaturesLength);
            //noting of the key and what would be not handling previous enemy positions from the training data
            //get distributions of the values of said key
            if (largestLengthOfActionsOfAGivenKey > 0)
            {
                string setFeaturesStr = "01 1";

                //get the unset feature positions that would be indicated by setFeaturesStr
                List<int> unsetFeaturePositions_setFeaturesStr = new List<int>();
                for (int i = 0; i < setFeaturesStr.Length; i++)
                {
                    if (setFeaturesStr[i] == ' ')
                    {
                        unsetFeaturePositions_setFeaturesStr.Add(i);
                    }
                }
                
                Dictionary<int, float> sequenceSMAngleQuantizedBy45DegAndLength = new Dictionary<int, float>();
                Dictionary<int, float> sequenceMNAngleQuantizedBy45DegAndLength = new Dictionary<int, float>();
                float noMovementLength = 0;

                //initialize map values - where YKWIM by this
                for (int quantizedAngle = 0; quantizedAngle < 360; quantizedAngle += 45)
                {
                    sequenceSMAngleQuantizedBy45DegAndLength[quantizedAngle] = 0;
                    sequenceMNAngleQuantizedBy45DegAndLength[quantizedAngle] = 0;
                }

                //for each key's feature string matching the set feature values, add to what would be the distributions
                foreach (KeyValuePair<string, List<List<Pair<genBehaviourData, float>>>> pair in PMDataOrdered3_ofFixedLengthCycles)
                {
                    //get the featureStr with the corresponding unset and set features - where YKWIM by this
                    char[] currSetFeaturesStr_CharArray = pair.Key.ToCharArray();
                    for (int i = 0; i < currSetFeaturesStr_CharArray.Length; i++)
                    {
                        foreach (int unsetPosn in unsetFeaturePositions_setFeaturesStr)
                        {
                            currSetFeaturesStr_CharArray[unsetPosn] = ' ';
                        }
                    }
                    string currSetFeaturesStr = new string(currSetFeaturesStr_CharArray);
                    if(!currSetFeaturesStr.Equals(setFeaturesStr)) //if the key with the unset features omitted does not match setFeaturesStr, then do not add their values to the distributions
                    {
                        continue;
                    }

                    List<List<Pair<genBehaviourData, float>>> distributionOfSequences_ofAKeyWithMatchingSetFeatures = PMDataOrdered3_ofFixedLengthCycles[pair.Key];
                    foreach (List<Pair<genBehaviourData, float>> sequence in distributionOfSequences_ofAKeyWithMatchingSetFeatures)
                    {
                        //add length's worth of actions in the sequence to numActionsOfCurrentKey - where YKWIM by this
                        foreach (Pair<genBehaviourData, float> actionAndLength in sequence)
                        {
                            //noting of the magnitude value, and neglecting such as well
                            //Rotate CCW, as the training data with Vector2.SignedAngle as done in training would find the CCW angle
                            //Vector2 moveVecRelativeToMNPosComponent = Quaternion.Euler(0f, 0f, -actionAndLength.First.movement_thisUpdate_CWAngleRelativeToMNPos) * (currData.avgClosest2EnemyPosn - shadowPos).normalized * actionAndLength.First.movement_thisUpdate_Magnitude;
                            //Vector2 moveVecRelativeToSMPosComponent = Quaternion.Euler(0f, 0f, -actionAndLength.First.movement_thisUpdate_CWAngleRelativeToSMPos) * (currData.medianEnemyPosn - shadowPos).normalized * actionAndLength.First.movement_thisUpdate_Magnitude;

                            //moveData.movement_thisUpdate_CWAngleRelativeToMNPos
                            //Vector2 moveVec = (moveVecRelativeToMNPosComponent + moveVecRelativeToSMPosComponent) / 2;

                            //float avgAngle = actionAndLength.First.movement_thisUpdate_CWAngleRelativeToMNPos;
                            //lengthOfActionsOfCurrentKey += actionAndLength.Second;

                            if (actionAndLength.First.movement_thisUpdate_Magnitude != 0)
                            {
                                int CWAngleRelativeToMNPos_45DegQuantized = (int)(actionAndLength.First.movement_thisUpdate_CCWAngleRelativeToMNPos / 45) * 45; //quantization with [0,45) => 0, [45,90) => 45, etc. - where YKWIM by this
                                int CWAngleRelativeToSMPos_45DegQuantized = (int)(actionAndLength.First.movement_thisUpdate_CCWAngleRelativeToSMPos / 45) * 45;
                                sequenceMNAngleQuantizedBy45DegAndLength[CWAngleRelativeToMNPos_45DegQuantized] += actionAndLength.Second;
                                sequenceSMAngleQuantizedBy45DegAndLength[CWAngleRelativeToSMPos_45DegQuantized] += actionAndLength.Second;
                            }
                            else
                            {
                                noMovementLength += actionAndLength.Second;
                            }
                        }
                    }
                }

                Debug.Log("For " + setFeaturesStr + ": ");

                //Display all of the values for the corresponding angles
                string SMAngleKVpairs = "";
                foreach (KeyValuePair<int, float> pair in sequenceSMAngleQuantizedBy45DegAndLength)
                {
                    SMAngleKVpairs += "(" + pair.Key + "," + pair.Value + ") ";
                }
                Debug.Log("SM angle pairs: " + SMAngleKVpairs);
                string MNAngleKVpairs = "";
                foreach (KeyValuePair<int, float> pair in sequenceMNAngleQuantizedBy45DegAndLength)
                {
                    MNAngleKVpairs += "(" + pair.Key + "," + pair.Value + ") ";
                }
                Debug.Log("MN angle pairs: " + MNAngleKVpairs);
                Debug.Log("no movement length: " + noMovementLength);

                //combined 90 degree directions
                Debug.Log("90-degree SM pairs: ((315, 0)," + (sequenceSMAngleQuantizedBy45DegAndLength[315] + sequenceSMAngleQuantizedBy45DegAndLength[0]) + "), " +
                    "((45,90)," + (sequenceSMAngleQuantizedBy45DegAndLength[45] + sequenceSMAngleQuantizedBy45DegAndLength[90])+"), "+
                    "((135,180)," + (sequenceSMAngleQuantizedBy45DegAndLength[135] + sequenceSMAngleQuantizedBy45DegAndLength[180]) + "), " +
                    "((225,270)," + (sequenceSMAngleQuantizedBy45DegAndLength[225] + sequenceSMAngleQuantizedBy45DegAndLength[270]) + ")"
                    );
                Debug.Log("90-degree SM pairs and no movement length as comma-separated values: " + (sequenceSMAngleQuantizedBy45DegAndLength[315] + sequenceSMAngleQuantizedBy45DegAndLength[0]) + "," +
                    (sequenceSMAngleQuantizedBy45DegAndLength[45] + sequenceSMAngleQuantizedBy45DegAndLength[90]) + "," +
                    (sequenceSMAngleQuantizedBy45DegAndLength[135] + sequenceSMAngleQuantizedBy45DegAndLength[180]) + "," +
                    (sequenceSMAngleQuantizedBy45DegAndLength[225] + sequenceSMAngleQuantizedBy45DegAndLength[270]) + "," +
                    noMovementLength
                    );
                //Debug.Log("90-degree SM pairs and no movement length as normalized, rounded comma-separated values: " + (sequenceSMAngleQuantizedBy45DegAndLength[315] + sequenceSMAngleQuantizedBy45DegAndLength[0]) / totalLength + "," +
                //    (sequenceSMAngleQuantizedBy45DegAndLength[45] + sequenceSMAngleQuantizedBy45DegAndLength[90]) / totalLength + "," +
                //    (sequenceSMAngleQuantizedBy45DegAndLength[135] + sequenceSMAngleQuantizedBy45DegAndLength[180]) / totalLength + "," +
                //    (sequenceSMAngleQuantizedBy45DegAndLength[225] + sequenceSMAngleQuantizedBy45DegAndLength[270]) / totalLength + "," +
                //    noMovementLength
                //    );
                Debug.Log("90-degree MN pairs: ((315, 0)," + (sequenceMNAngleQuantizedBy45DegAndLength[315] + sequenceMNAngleQuantizedBy45DegAndLength[0]) + "), " +
                    "((45,90)," + (sequenceMNAngleQuantizedBy45DegAndLength[45] + sequenceMNAngleQuantizedBy45DegAndLength[90]) + "), " +
                    "((135,180)," + (sequenceMNAngleQuantizedBy45DegAndLength[135] + sequenceMNAngleQuantizedBy45DegAndLength[180]) + "), " +
                    "((225,270)," + (sequenceMNAngleQuantizedBy45DegAndLength[225] + sequenceMNAngleQuantizedBy45DegAndLength[270]) + ")"
                    );
                Debug.Log("90-degree MN pairs and no movement length as comma-separated values: " + (sequenceMNAngleQuantizedBy45DegAndLength[315] + sequenceMNAngleQuantizedBy45DegAndLength[0]) + "," +
                    (sequenceMNAngleQuantizedBy45DegAndLength[45] + sequenceMNAngleQuantizedBy45DegAndLength[90]) + "," +
                    (sequenceMNAngleQuantizedBy45DegAndLength[135] + sequenceMNAngleQuantizedBy45DegAndLength[180]) + "," +
                    (sequenceMNAngleQuantizedBy45DegAndLength[225] + sequenceMNAngleQuantizedBy45DegAndLength[270]) + "," +
                    noMovementLength
                    );

                float totalLength = 0;
                foreach(KeyValuePair<int, float> pair in sequenceSMAngleQuantizedBy45DegAndLength)
                {
                    totalLength += pair.Value;
                }
                totalLength += noMovementLength;
                Debug.Log("Total length: " +totalLength);
            }

            //enemy direction relative to player direction ... noting of such on 3/10/19 and that such would be a feature
            //  -and noting of how such would be what the generation would be doing as well, though such a said generation would be with such of the features and where said features would be assessed at the end of each sequence
            findLargestKey = false;
        }
        OnLoadTransition.Hide();
    }

    SerializableSaveDataPM LoadData_NextUpdate_PMData()
    {
        SerializableSaveDataPM data = new SerializableSaveDataPM();
        Stream stream = File.Open(SaveLoadPM.currentFilePath, FileMode.Open);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        data = (SerializableSaveDataPM)bformatter.Deserialize(stream);
        stream.Close();

        loadingData = false;

        bool doJsonSerializationConversion = false;
        if (doJsonSerializationConversion)
        {
            //string unserializedFilePath = System.IO.Path.Combine(Application.persistentDataPath, "SaveDataPM_unserialized.cjc");
            //string json = JsonConvert.SerializeObject(data);
            //data = JsonConvert.DeserializeObject<SerializableSaveDataPM>(json);
            //Debug.Log("Before trying to write data to file at " + unserializedFilePath);
            ////File.WriteAllText(@unserializedFilePath, ((SerializableSaveDataPM)bformatter.Deserialize(stream)).ToString());
            //File.WriteAllText(@unserializedFilePath, json);
            //Debug.Log("Writing data to file at " + unserializedFilePath);

            //saving JSON-serialized file as serialized via a BinaryFormatter?
            //iterate through all of the files and convert each of them
            string[] jsonFiles = Directory.GetFiles(System.IO.Path.Combine(Application.persistentDataPath, "JSON serialized object data"), "*.cjc*", SearchOption.AllDirectories);

            for (int i = 0; i < jsonFiles.Length; i++)
            {
                string jsonSerializedFilePath = System.IO.Path.Combine(Application.persistentDataPath, jsonFiles[i]);
                string bfSerializedFilePath = System.IO.Path.Combine(System.IO.Path.Combine(Application.persistentDataPath, "non-JSON formatting"), jsonFiles[i]);
                if (File.Exists(jsonSerializedFilePath))
                {
                    string json_read = File.ReadAllText(jsonSerializedFilePath);

                    SerializableSaveDataPM data_fromjson = JsonConvert.DeserializeObject<SerializableSaveDataPM>(json_read);


                    //SerializableSaveDataPM unser_data = new SerializableSaveDataPM();
                    //string unserializedFilePathToSerialize = System.IO.Path.Combine(Application.persistentDataPath, "SaveDataPM_toserialize.cjc");
                    Stream jsontobfstream = File.Open(bfSerializedFilePath, FileMode.Create);
                    BinaryFormatter bformatter_jsontobf = new BinaryFormatter();
                    bformatter_jsontobf.Binder = new VersionDeserializationBinder();
                    //unser_data = (SerializableSaveDataPM)bformatter_unsertoser.;
                    bformatter_jsontobf.Serialize(jsontobfstream, data_fromjson);
                    jsontobfstream.Close();
                    //string filePathForUnserializedFileToSerialize = System.IO.Path.Combine(Application.persistentDataPath, "SaveDataPM_serializedfromunserialized.cjc");


                    //as debug, save the loaded data as unserialized to file

                    //Stream savestream = File.Open(unserializedFilePath, FileMode.Create);
                    //BinaryFormatter bformatter_save = new BinaryFormatter();
                    //bformatter_save.Binder = new VersionDeserializationBinder();
                    //bformatter_save.Deserialize(savestream);
                    //savestream.Close();

                    //saving unserialized file back as serialized?
                    //SerializableSaveDataPM unser_data = new SerializableSaveDataPM();
                    //string unserializedFilePathToSerialize = System.IO.Path.Combine(Application.persistentDataPath, "SaveDataPM_toserialize.cjc");
                    //Stream unsertoserstream = File.Open(unserializedFilePath, FileMode.Create);
                    //BinaryFormatter bformatter_unsertoser = new BinaryFormatter();
                    //bformatter_unsertoser.Binder = new VersionDeserializationBinder();
                    //unser_data = (SerializableSaveDataPM)bformatter_unsertoser.;
                    //bformatter_unsertoser.Serialize(unsertoserstream);

                    //string filePathForUnserializedFileToSerialize = System.IO.Path.Combine(Application.persistentDataPath, "SaveDataPM_serializedfromunserialized.cjc");

                    ////System.IO.Path.Combine(Application.persistentDataPath, "SaveDataPM.cjc")
                    Debug.Log("Conversion of JSON data");
                }
            }
        }

        // Now use "data" to access your Values
        return data;
    }

    MarkovUnit getData(Vector2 characterPos, bool forPlayer) //returns what would be currData
    {
        MarkovUnit currData;
        //compute median distance of enemies ... or basically determine whether there would be at least 2 enemies within a certain distance

        if(usedForLoggingTraining) //done once by this
        {
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
        }

        //get num. near enemies
        //moved to outside the method
        //const float nearRadius = 3.5f;
        //const float nearRadiusSqr = nearRadius * nearRadius;
        //const float withinRangeRadius = 9.0f;
        //const float withinRangeRadiusSqr = withinRangeRadius * withinRangeRadius;
        int nearEnemyCount = 0;
        float avgEnemyTTROfClosest2 = 0;


        float minTimeToReach = 99999f;
        float minTimeToReach_2nd = 100000f;
        GameObject minEnemy = gameObject;
        GameObject minEnemy_2nd = gameObject;

        float minTimeToReach_includingWanderingTimesAdjustedForWandering = 999999f;
        GameObject minEnemy_includingWanderingForShoot = gameObject; //not involved in training data, but is involved in shooting and/or is to be involved in shooting

        float maxOnScreenEnemySpeed = 0f;
        #region Comments on determining median enemy pos
        /* median enemy pos: 
         * -enemy with the lowest sum of divergence in rank from each of the middlemost ranks of X position and Y position
         *  -could get median rank with maybe selection sort \ linear searching half the enemies for the minimum and next-minimum, etc. or
         * -could 
         *      sort enemies by the respective position (X or Y) and get the corresponding rank with eg. Mergesort or List<T>.Sort(), then 
         *      sum up each enemies' ranks in terms of divergence from the middle rank (i.e. the middle index of the List)
         *          -maybe having it as a value within each enemy object, or having a Dictionary mapping each enemy to a rank
         *          with noting of this regarding a Dictionary with using Objects and such of references for fast lookup - https://stackoverflow.com/questions/1266469/what-does-net-dictionaryt-t-use-to-hash-a-reference
         *          and so, doing such of a Dictionary to look up specific references, which would only retrieve the objects' references exclusively 
         *      and then get the minimum of such a sum
         *     -with noting of regarding not doing: eg. assigning ranks during the sort - still such and requiring finding the minimum sum afterwards
         * 
         *  -could note the computational cost in doing this
         */
        #endregion

        Vector3 prevCharacterPosition;
        if(forPlayer)
        {
            prevCharacterPosition = prevPlayerPos;
        }
        else
        {
            prevCharacterPosition = prevShadowPos;
        }

        Queue<float> avgClosest2eTTR; //set to separate queues for player and non-player
        if (forPlayer)
        {
            avgClosest2eTTR = avgClosest2eDistances_player;
        }
        else
        {
            avgClosest2eTTR = avgClosest2eDistances_nonplayer;
        }
        List<GameObject> enemies_notOffscreen = new List<GameObject>();

        foreach (GameObject enemy in enemies)
        {
            Vector2 enemyPos = enemy.transform.position;

            //skip enemy iteration if not visible on camera bounds
            if (forPlayer)
            {
                Bounds playerCameraTriggerBoundsZ0 = player.GetComponentInChildren<CameraTriggerSet>().gameObject.GetComponent<BoxCollider2D>().bounds;
                playerCameraTriggerBoundsZ0.center = new Vector3(playerCameraTriggerBoundsZ0.center.x, playerCameraTriggerBoundsZ0.center.y, 0);
                Bounds EnemyBoundsZ0 = enemy.GetComponentInChildren<BoxCollider2D>().bounds;
                EnemyBoundsZ0.center = new Vector3(EnemyBoundsZ0.center.x, EnemyBoundsZ0.center.y, 0);
                if (!EnemyBoundsZ0.Intersects(playerCameraTriggerBoundsZ0))
                {
                    continue;
                }
            }
            else
            {
                Bounds cameraTriggerBoundsZ0 = GetComponentInChildren<CameraTriggerSet>().gameObject.GetComponent<BoxCollider2D>().bounds;
                cameraTriggerBoundsZ0.center = new Vector3(cameraTriggerBoundsZ0.center.x, cameraTriggerBoundsZ0.center.y, 0);
                Bounds EnemyBoundsZ0 = enemy.GetComponentInChildren<BoxCollider2D>().bounds;
                EnemyBoundsZ0.center = new Vector3(EnemyBoundsZ0.center.x, EnemyBoundsZ0.center.y, 0);
                //Bounds cameraTriggerBounds = GetComponentInChildren<CameraTriggerSet>().gameObject.GetComponent<BoxCollider2D>().bounds;
                if (!EnemyBoundsZ0.Intersects(cameraTriggerBoundsZ0))
                {
                    continue;
                }
            }

            float sqrDistance = (enemyPos - characterPos).sqrMagnitude;
            if (sqrDistance <= nearRadiusSqr) //if enemy is nearby
            {
                nearEnemyCount++;
            }
            
            //add enemy (which is not offscreen)
            enemies_notOffscreen.Add(enemy);

            //if enemy is not aggressive, then not count enemy towards what would be the max speed of an enemy considered in features and not count enemy towards what would be the enemy with the least time to reach such, though still count it towards the 'CM' of enemies and also still shoot said enemies if such enemies would be with TTR as at least 0.75f less as a rough way to handle such where what would be wandering would be treated as with 0.75f more TTR in determining shooting
            bool hasLocateWanderTarget = enemy.GetComponentInChildren<LocateWanderTarget>() != null;
            LocateSeekPursueTarget enemyLSPT = enemy.GetComponentInChildren<LocateSeekPursueTarget>();
            bool enemyIsToBeWandering = hasLocateWanderTarget && (enemyLSPT == null || (enemyLSPT != null && !enemyLSPT.isProvoked && enemyLSPT.seekPursueOnProvoked)); //enemy either does not seek/pursue or seek/pursues when provoked and currently not provoked

            if (!enemyIsToBeWandering)
            {
                //get max speed of aggressive enemies on screen
                //quantize speeds into tiers: [0,2), [2,4.5), 4.5 or higher as something that could be done for the 3 tiers of max enemy speeds
                float enemySpeed = enemy.GetComponent<AILerp>().speed;
                if (enemySpeed > maxOnScreenEnemySpeed) //error on this line occurred. Error occurred on 2/6/19.
                {
                    maxOnScreenEnemySpeed = enemySpeed;
                }

                //get minEnemy and minEnemy_2nd
                float currEnemyTimeToReach = (enemyPos - characterPos).magnitude / enemySpeed;

                if (currEnemyTimeToReach < minTimeToReach)
                {
                    //update the closest 2 enemies' distances and the corresponding 2 enemies
                    minTimeToReach_2nd = minTimeToReach;
                    minEnemy_2nd = minEnemy;

                    minTimeToReach = currEnemyTimeToReach;
                    minEnemy = enemy;
                }
                else if (currEnemyTimeToReach < minTimeToReach_2nd)
                {
                    //update the 2nd closest enemy's distance if only <<closer than what was the 2nd closest enemy distance found at this point in code running><YKWIM>>
                    minTimeToReach_2nd = currEnemyTimeToReach;
                    minEnemy_2nd = enemy;
                }
                
                //get minimum enemy for shooting
                if(currEnemyTimeToReach < minTimeToReach_includingWanderingTimesAdjustedForWandering)
                {
                    minTimeToReach_includingWanderingTimesAdjustedForWandering = currEnemyTimeToReach;
                    minEnemy_includingWanderingForShoot = enemy;
                }
            }
            else
            {
                float enemySpeed = enemy.GetComponent<AILerp>().speed;
                float currEnemyTimeToReach_adjustedForWandering = (enemyPos - characterPos).magnitude / enemySpeed + 0.75f; //add 0.75f so that the TTR would have to be at least 0.75f less for wandering enemies to be selected as the enemy to be shot at - where YKWIM by this
                if (currEnemyTimeToReach_adjustedForWandering < minTimeToReach_includingWanderingTimesAdjustedForWandering)
                {
                    //update the closest enemy's distance and the corresponding enemy to shoot
                    
                    minTimeToReach_includingWanderingTimesAdjustedForWandering = currEnemyTimeToReach_adjustedForWandering;
                    minEnemy_includingWanderingForShoot = enemy;
                }
            }
        }

        
        avgEnemyTTROfClosest2 = (minTimeToReach_2nd + minTimeToReach) / 2; //to be compared with that 180 frames before and\or such

        //currData.neCount = (char)(nearEnemyCount>=9?9:nearEnemyCount); //number of enemies, as a char, with any being 9 or greater as 9, so 9 would basically be 9+/8+ depending on what such would mean

        //quantize enemy speed into tiers as <<mentioned before><YKWIM>> and have that as part of the PM MarkovUnit
        if (maxOnScreenEnemySpeed < 2)
        {
            currData.maxSpeed = (char)0;
        }
        else if (maxOnScreenEnemySpeed < 4.5f)
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
        else if(enemies_notOffscreen.Count >= 2)
        {
            currData.eDistance = EnemyDistance.FAR;
        }
        else
        {
            currData.eDistance = EnemyDistance.FAR_OFFSCREEN; //FAR_OFFSCREEN means both (far and offscreen), or nonexistent
        }
        //default values of these
        Vector3 closestShootingEnemyPos = Vector3.zero;
        Vector3 closestEnemyPos = Vector3.zero; //default - not used
        Vector2 medianEnemyPos = Vector2.zero;

        #region get median enemy pos and closest 2 enemy pos

        //make it so at least the closest 2 enemies - if they exist - would be added to enemies_withinRange, if no other enemies would exist in such as a part of such of enemies_withinRange
        if(minEnemy != gameObject && !(enemies.Length < 1))
        {
            enemies_notOffscreen.Add(minEnemy);
        }
        if(minEnemy_2nd != gameObject && !(enemies.Length < 2))
        {
            enemies_notOffscreen.Add(minEnemy_2nd);
        }

        #region get median enemy pos
        //getting median enemy pos - noting computational cost in doing such
        Dictionary<GameObject, int> EnemyAndDivergenceFromMiddlePosRank = new Dictionary<GameObject, int>();
        List<GameObject> enemiesOrderedByXPos = new List<GameObject>(enemies_notOffscreen);
        List<GameObject> enemiesOrderedByYPos = new List<GameObject>(enemies_notOffscreen);

        //sort lists in ascending order?
        int middleRank = enemiesOrderedByXPos.Count / 2;
        enemiesOrderedByXPos.Sort(new Comparison<GameObject>((x, y) => (int)(Mathf.Floor(x.transform.position.x - y.transform.position.x))));
        enemiesOrderedByYPos.Sort(new Comparison<GameObject>((x, y) => (int)(Mathf.Floor(x.transform.position.y - y.transform.position.y))));
        
        GameObject medianPosEnemy = null;
        if (enemiesOrderedByXPos.Count > 0)
        {
            //add X and Y pos divergence from mid
            for (int i = 0; i < enemiesOrderedByXPos.Count; i++)
            {
                EnemyAndDivergenceFromMiddlePosRank[enemiesOrderedByXPos[i]] = 0;
            }
            for (int i = 0; i < enemiesOrderedByXPos.Count; i++)
            {
                EnemyAndDivergenceFromMiddlePosRank[enemiesOrderedByXPos[i]] += Mathf.Abs(middleRank - i);
                EnemyAndDivergenceFromMiddlePosRank[enemiesOrderedByYPos[i]] += Mathf.Abs(middleRank - i);
            }
            medianPosEnemy = enemiesOrderedByXPos[0];//enemy with minimum divergence from middle rank

            //get enemy with minimum divergence in medianPosEnemy
            foreach (var item in EnemyAndDivergenceFromMiddlePosRank)
            {
                if (item.Value < EnemyAndDivergenceFromMiddlePosRank[medianPosEnemy])
                {
                    medianPosEnemy = item.Key;
                }
            }
            medianEnemyPos = medianPosEnemy.transform.position;
        }
        #endregion
        if (enemiesOrderedByXPos.Count >= 1)
        {
            closestEnemyPos = minEnemy.transform.position;
            closestShootingEnemyPos = minEnemy_includingWanderingForShoot.transform.position;
        }

        #endregion

        if (prevMovementDirTimer >= prevMovementDirInterval || closestEnemyPosAtStartOfInterval == new Vector3(-9999f, -9999f, -9999f)/* - as initialization*/)
        {
            if (closestEnemyPosAtStartOfInterval == new Vector3(-9999f, -9999f, -9999f)) //initialize if not done already
            {
                closestEnemyPosAtStartOfInterval = closestEnemyPos;
                medianEnemyPosAtStartOfInterval = medianEnemyPos;
            }
        }
        if (prevMovementDirTimer >= prevMovementDirInterval)
        {
            resetMovementDirTimerThisUpdate = true;
            //end of getting the median and avg enemy pos at the end? of interval?
            prevMovementDirTimer -= prevMovementDirInterval; //basically reset of the timer

            //determine angles to be added to QuantizedDirAngle as the prev. angles, replacing any after the 3rd
            Vector3 movementDir = (Vector3)characterPos - prevPosAtStartOfInterval;

            //get angle of (position from (the player position at start of interval)) relative to (the enemy positions from (the player position at the start of the interval))
            //positive CW angle
            float movementDirAngle_RelativeToCM = (Vector2.SignedAngle(movementDir, (Vector2)medianEnemyPosAtStartOfInterval - (Vector2)(prevPosAtStartOfInterval)) + 360)%360;
            float movementDirAngle_RelativeToMN = (Vector2.SignedAngle(movementDir, (Vector2)closestEnemyPosAtStartOfInterval - (Vector2)(prevPosAtStartOfInterval)) + 360)%360;
            //quantize the angles
            int dir = 1;
            int degArcLength = 45;
            int currArcStart = -degArcLength / 2;


            //add the corresponding dir to the respective queues
            #region enqueue and dequeue the QuantizedDirAngle queues
            //handle dir that would deal with 'negative' (being at the latter part of the positive) and positive first as a separate case, i.e. when dir is 1
            if (movementDirAngle_RelativeToCM >= currArcStart+360 || movementDirAngle_RelativeToCM < currArcStart + degArcLength)
            {
                QuantizedDirAngle_RelativeToCM.Enqueue(dir);
            }
            if (movementDirAngle_RelativeToMN >= currArcStart+360 || movementDirAngle_RelativeToMN < currArcStart + degArcLength)
            {
                QuantizedDirAngle_RelativeToMN.Enqueue(dir);
            }
            currArcStart += degArcLength;

            //if the angle would be within any of the arcs found, add corresponding dir - for the positive-only angles
            for (dir = 2; dir <= 8; dir++)
            {
                if(movementDirAngle_RelativeToCM >= currArcStart && movementDirAngle_RelativeToCM < currArcStart + degArcLength)
                {
                    QuantizedDirAngle_RelativeToCM.Enqueue(dir);
                }
                if (movementDirAngle_RelativeToMN >= currArcStart && movementDirAngle_RelativeToMN < currArcStart + degArcLength)
                {
                    QuantizedDirAngle_RelativeToMN.Enqueue(dir);
                }
                currArcStart += degArcLength;
            }

            if (QuantizedDirAngle_RelativeToCM.Count > MAX_PREVDIRS)
            {
                QuantizedDirAngle_RelativeToCM.Dequeue();
            }
            if (QuantizedDirAngle_RelativeToMN.Count > MAX_PREVDIRS)
            {
                QuantizedDirAngle_RelativeToMN.Dequeue();
            }
            #endregion
            
            //update the prev. pos, and prev. enemy pos'es that would be used for the curr. movement dir. generation
            prevPosAtStartOfInterval = (Vector3)characterPos;
            closestEnemyPosAtStartOfInterval = closestEnemyPos;
            medianEnemyPosAtStartOfInterval = medianEnemyPos;
        }
#if UNITY_EDITOR
        //Draw debug sphere at positions for SM and MN
        gizmoSpheresToDraw.Clear();
        gizmoSpheresToDraw.Add(new gizmoSphereProperties() { color = Color.red, position = medianEnemyPos, radius = 0.7f });
        gizmoSpheresToDraw.Add(new gizmoSphereProperties() { color = Color.blue, position = closestEnemyPos, radius = 0.5f });
        gizmoSpheresToDraw.Add(new gizmoSphereProperties() { color = Color.magenta, position = closestShootingEnemyPos, radius = 0.3f });
#endif

        prevMovementDirTimer += Time.fixedDeltaTime;

        currData.QuantizedDirAngle_RelativeToCMAtData = new Queue<int>(QuantizedDirAngle_RelativeToCM);
        currData.QuantizedDirAngle_RelativeToMNAtData = new Queue<int>(QuantizedDirAngle_RelativeToMN);
        //get direction relative to prev. position in cycle
        //closest2EnemyAvgPos
        //medianEnemyPos

        #region noting of determining action - with noting of this and not used within Using as the action would be generated (in the Using() method) rather than retrieved


        //**
        // get relative direction relative to the two enemy posns, so two directions that would be vectors whose directions 
        // would be relative to directions of enemy positions each with half the weight
        // though noting of such of the enemy posns and being with current enemy posns compared to the prev character posns ...
        //**
        Vector2 movementDir_thisUpdate;

            movementDir_thisUpdate = characterPos - (Vector2)prevPlayerPos;
            if(movementDir_thisUpdate == Vector2.zero)
            {
                //no movement; count as nothing ...
            }
            currData.movementDir_thisUpdate_CCWAngleRelativeToSMPos = (Vector2.SignedAngle((Vector2)medianEnemyPos - (Vector2)(prevPosAtStartOfInterval), movementDir_thisUpdate) + 360) % 360;
            currData.movementDir_thisUpdate_CCWAngleRelativeToMNPos = (Vector2.SignedAngle((Vector2)closestEnemyPos - (Vector2)(prevPosAtStartOfInterval), movementDir_thisUpdate) + 360) % 360;

            if(isTraining && getCurrMovementAngleAndMag)
            {
                Debug.Log("medianEnemyPos: (" + medianEnemyPos.x + ", " + medianEnemyPos.y + "); closest2EnemyAvgPos: (" + closestEnemyPos.x + ", " + closestEnemyPos.y + "); mag: "+ movementDir_thisUpdate.magnitude);
                Debug.Log("CWAngleRelativeToSMPos: (" + currData.movementDir_thisUpdate_CCWAngleRelativeToSMPos + "; CWAngleRelativeToMNPos: " + currData.movementDir_thisUpdate_CCWAngleRelativeToMNPos + "; eDistance: "+currData.eDistance+"; maxSpeed: "+currData.maxSpeed);
                getCurrMovementAngleAndMag = false;
            }
            if(currData.movementDir_thisUpdate_CCWAngleRelativeToMNPos == 0)
            {
                Debug.Log("CWAngleRelativeToMNPos is 0 right now");
            }


            currData.movementDir_thisUpdate_Magnitude = movementDir_thisUpdate.magnitude;

        //get attack and classify it ...

        currData.AIAction = Action.Attack; //set in order to avoid Visual Studio <<from ><YKWIM>>displaying an "error" due to lack of initialization
        float TTRDiff = 0;
        float prevTTR = 0; //made for debugging; otherwise could do without this var
        if (avgClosest2eTTR.Count >= 150)
        {
            //can compare the 150f prior with the curr.
            prevTTR = avgClosest2eTTR.Dequeue();
            TTRDiff = avgEnemyTTROfClosest2 - prevTTR;
        }
        else
        {
            if (avgClosest2eTTR.Count > 0)
            {
                prevTTR = avgClosest2eTTR.Peek();
            }
            TTRDiff = avgEnemyTTROfClosest2 - /*((avgClosest2eDistances.Count > 0)?avgClosest2eDistances.Peek() : 0f)*/prevTTR; //get the earliestmost unit of data
        }
        //Debug.Log("curr dist: " + avgEnemyDistanceOfClosest2 + ", prev dist: " + prevDistance+"; distanceDiff: "+distanceDiff);
        if (TTRDiff > 1)
        {
            //consider such an amount of positive difference as retreat
            currData.AIAction = Action.Retreat;
        }
        else if (TTRDiff > -1)
        {
            //consider such a small \ nonexistent difference as wander
            currData.AIAction = Action.Wander;
        }
        else
        {
            //consider such an amount of negative difference as attack
            currData.AIAction = Action.Attack;
        }
        avgClosest2eTTR.Enqueue(avgEnemyTTROfClosest2);

        #endregion

        //and noting of a for loop for such if statements

        //Noting of precondition and postcondition: the prev. player values are used exclusively in Using(), and the prev. Shadow values are used exclusively within code in Training() - and both being prior to the execution of this code
        if (forPlayer)
        {
            prevPlayerPos = player.transform.position;
            prevClosestEnemyPosRelativeToPlayer = closestEnemyPos;
            prevMedianEnemyPosRelativeToPlayer = medianEnemyPos;
        }
        else
        {
            prevShadowPos = transform.position;
            prevClosestEnemyPosRelativeToShadow = closestEnemyPos;
            prevMedianEnemyPosRelativeToShadow = medianEnemyPos;
            minEnemyRelativeToShadow_prevOrNot = minEnemy;
            minEnemyToShootRelativeToShadow_prevOrNot = minEnemy_includingWanderingForShoot;
        }
        
        currData.avgClosest2EnemyPosn = closestEnemyPos;
        currData.medianEnemyPosn = medianEnemyPos;

        return currData;
    }

    MarkovUnit_moreData getData_more(Vector2 characterPos, bool forPlayer) //returns what would be currData
    {
        MarkovUnit_moreData currData;
        //compute median distance of enemies ... or basically determine whether there would be at least 2 enemies within a certain distance

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        //get num. near enemies

        //moved to outside the method
        float nearRadius = 3.5f;
        float nearRadiusSqr = nearRadius * nearRadius;
        int nearEnemyCount = 0;
        float avgEnemyDistanceOfClosest2 = 0;
        Vector2 avgEnemyPosOfClosest2 = Vector2.zero;

        float minDist = 99999f;
        float minDist_2nd = 100000f;
        GameObject minEnemy = gameObject; //default value (as I would consider such to be)
        Vector2 closestPos = new Vector2(9999f, 9999f);
        Vector2 closestPos_2nd = new Vector2(10000f, 10000f);
        float maxNearbyEnemySpeed = 0f;

        Queue<float> avgClosest2eDistances; //used for comparison of distances with past distances <<at least><YKWIM>> //set to separate queues for player and non-player
        if (forPlayer)
        {
            avgClosest2eDistances = avgClosest2eDistances_player;
        }
        else
        {
            avgClosest2eDistances = avgClosest2eDistances_nonplayer;
        }

        foreach (GameObject enemy in enemies)
        {
            Vector2 enemyPos = enemy.transform.position;

            //skip enemy iteration if not visible on camera
            Bounds playerCameraTriggerBounds = player.GetComponentInChildren<CameraTriggerSet>().gameObject.GetComponent<BoxCollider2D>().bounds;
            if (enemy.GetComponent<BoxCollider2D>().bounds.Intersects(playerCameraTriggerBounds))
            {
                continue;
            }

            float currEnemyDist = (enemyPos - characterPos).magnitude;

            if (currEnemyDist < minDist)
            {
                //update the closest 2 enemies' distances
                minDist_2nd = minDist;
                minDist = currEnemyDist;

                //update the closest 2 enemies' positions
                closestPos_2nd = closestPos;
                closestPos = enemyPos;

                minEnemy = enemy;
            }
            else if (currEnemyDist < minDist_2nd)
            {
                //update the 2nd closest enemy's distance if only closer than what was the 2nd closest enemy distance found at this point in code running
                minDist_2nd = currEnemyDist;

                closestPos_2nd = enemyPos;
            }
        }
        
        avgEnemyDistanceOfClosest2 = (minDist_2nd + minDist) / 2; //to be compared with that 180 frames before and\or such
        avgEnemyPosOfClosest2 = (closestPos + closestPos_2nd) / 2;

        //quantize enemy speed into tiers as mentioned before and have that as part of the PM MarkovUnit
        if (maxNearbyEnemySpeed < 2)
        {
            currData.maxSpeed = (char)0;
        }
        else if (maxNearbyEnemySpeed < 4.5f)
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

        #region noting of determining action - with noting of this and not used within Using as the action would be generated rather than retrieved

        //get attack and classify it ...

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
            if (avgClosest2eDistances.Count > 0)
            {
                prevDistance = avgClosest2eDistances.Peek();
            }
            distanceDiff = avgEnemyDistanceOfClosest2 - /*((avgClosest2eDistances.Count > 0)?avgClosest2eDistances.Peek() : 0f)*/prevDistance; //get the earliestmost unit of data
        }
        //Debug.Log("curr dist: " + avgEnemyDistanceOfClosest2 + ", prev dist: " + prevDistance+"; distanceDiff: "+distanceDiff);
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
        #endregion
        avgClosest2eDistances.Enqueue(avgEnemyDistanceOfClosest2);

        currData.avgClosest2EnemyPosn = avgEnemyPosOfClosest2;
        return currData;
    }


    void Training()
    {
        //Get state of enemy positions and 'player action'<< - with 'player action' being just the difference in distance in frames><YKWIM>>
        Vector2 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position; //the position of the player for training
        MarkovUnit currData = getData(playerPos, true); //noting of not being able to pass an empty struct to the function, so not being able to pass a null MarkovUnit as an argument to be modified and/or such

        bool isAlsoCycling = false; //made so that such code would not run, in noting of testing game on mobile and such and preventing such computations
        bool isDoingDistributionOfCycles = false;
        bool isDoingDistributionOfFixedRelativeMovementVecCycles = true;

        //PM feature combination and generation data, encoded as strings
        string featureStr = "";
        string genStr = "";

        //convert the data of prev. movement dirs relative to SM and M-N into strings, from oldest movement dirs to newest
        string prevMovementRelativeToCMAndMNStr = "";
        while(currData.QuantizedDirAngle_RelativeToCMAtData.Count > 0/* && (prevMovementRelativeToCMAndMNStr.Length <= 4) in case the string would have copied more than MAX_PREVDIRS * 2 chars somehow*/)
        {
            prevMovementRelativeToCMAndMNStr += currData.QuantizedDirAngle_RelativeToCMAtData.Dequeue(); //getting the oldest prev. data first
            prevMovementRelativeToCMAndMNStr += currData.QuantizedDirAngle_RelativeToMNAtData.Dequeue();
        }
        prevMovementRelativeToCMAndMNStr = prevMovementRelativeToCMAndMNStr.PadLeft(MAX_PREVDIRS * 2, '0'); //padding with the max length for dirs

        featureStr += ((char)(currData.eDistance + '0')) + "" + ((char)(currData.maxSpeed + '0'))/* plus previous 3 movement vectors in terms of the relative direction from the <<'CM' and 'M-N' positions - with such directions as <<quantized angles><YKWIM>> relative to such 'CM' and 'M-N' positions><YKWIM>>*/ + prevMovementRelativeToCMAndMNStr;
        genStr += ((char)(currData.AIAction + '0'));

        //reset the currTrainingCycle if just loaded data, or swapped places with a Shadow
        if(resetCurrTrainingCycle_withLoadedDataThisUpdate || SwappedPlacesThisUpdate)
        {
            PMDataOrdered3_currTrainingCycle_featureStr = featureStr;
            PMDataOrdered3_currTrainingCycle = new List<Pair<genBehaviourData, float>>();
            Debug.Log("training cycle reset");
        }
        if(resetCurrTrainingCycle_withLoadedDataThisUpdate)
        {
            resetCurrTrainingCycle_withLoadedDataThisUpdate = false;
        }
        if (SwappedPlacesThisUpdate)
        {
            //also reset the movement timer for the cycle alongside the prior resetting of the currTrainingCycle (with any TODO as of 12/23/18 of making this resetting work for other types of cycles of such would be TODO), with any
            //and reset whatever else could be based on the previous environment / features and/or such, including the prev dir queues as well and any other queues, such as avgClosest2eDistances
            prevMovementDirTimer = 0f; //not setting prevMovementDirTimer to prevMovementDirInterval because of resetting eg. featureStr and/or such being already done in the prior if statement
            QuantizedDirAngle_RelativeToCM.Clear();
            QuantizedDirAngle_RelativeToMN.Clear();
            avgClosest2eDistances_player.Clear();
            avgClosest2eDistances_nonplayer.Clear();
            SwappedPlacesThisUpdate = false;
        }

        //add to probability distribution map - add weight to the feature and generation combination
        if (!PMData.ContainsKey(featureStr))
        { //initialize if not already done
            PMData[featureStr] = new Dictionary<string, int>();

            if (isAlsoCycling)
            {
                PMData_maintainOrder[featureStr] = new List<KeyValuePair<string, int>>();
            }
            PMData_ofmaintainOrderCycles[featureStr] = new List<List<KeyValuePair<string, int>>>();
        }
        if(!PMDataOrdered3_ofFixedLengthCycles.ContainsKey(featureStr)) //in case that PMData would be having elements that PMDataOrdered3_ofFixedLengthCycles would not, separating these
        {
            PMDataOrdered3_ofFixedLengthCycles[featureStr] = new List<List<Pair<genBehaviourData, float>>>();
        }
        if (!PMData[featureStr].ContainsKey(genStr)) //initialize if not already done
        {
            PMData[featureStr][genStr] = 0;
        }
        PMData[featureStr][genStr] += 1; //add 1 to the weight for the generation found within the generation probability distribution <<given the feature combination><YKWIM>>

        //Code for adding to PMData_maintainOrder
        //create new list item for genStr if not already the last list item; otherwise add 1 to the last list item
        #region isAlsoCycling
        if (isAlsoCycling)
        {
            KeyValuePair<string, int> LastPMData;
            bool PMDistributionIsEmpty = PMData_maintainOrder[featureStr].Count <= 0;
            if (!PMDistributionIsEmpty)
            {
                LastPMData = PMData_maintainOrder[featureStr].Last();
                if (LastPMData.Key == genStr) //just increment weight if last weight
                {
                    PMData_maintainOrder[featureStr][PMData_maintainOrder[featureStr].Count - 1] = new KeyValuePair<string, int>(genStr, LastPMData.Value + 1); //Last() not allowing reassignment
                }
                else //add new list item with different genStr if not
                {
                    PMData_maintainOrder[featureStr].Add(new KeyValuePair<string, int>(genStr, 1)); //add new element
                }
            }
            else
            {
                PMData_maintainOrder[featureStr].Add(new KeyValuePair<string, int>(genStr, 1)); //alongside initializing, adding first element to the List
            }
        }
        #endregion
        //Code for adding to PMData_ofmaintainOrderCycles
        //Add to PMDataOrdered2_currCycle until complete, then add to the distribution for the corresponding featureStr
        bool newCycleOrGenStr = false; //for detecting new feature or generation changes
        #region isDoingDistributionOfCycles
        if (isDoingDistributionOfCycles)
        {
            if (featureStr != PMDataOrdered2_currTrainingCycle_featureStr || PMDataOrdered2_currTrainingCycle_Length >= PMDataOrdered2_cycleLengthLimit) //One cycle for each period of the entirety of while the feature value set would be unchanged; new featureStr - start new cycle
            {
                if(PMDataOrdered2_currTrainingCycle_featureStr != "") //if not the initial str (i.e. before any cycle would have been started being created), then add the currCycle
                {
                    PMData_ofmaintainOrderCycles[PMDataOrdered2_currTrainingCycle_featureStr].Add(PMDataOrdered2_currTrainingCycle);
                    //Debug.Log("Adding new training cycle; last gen before addition w: " + PMDataOrdered2_currTrainingCycle.Last().Value);
                }
                PMDataOrdered2_currTrainingCycle_featureStr = featureStr;
                PMDataOrdered2_currTrainingCycle = new List<KeyValuePair<string, int>>();
                PMDataOrdered2_currTrainingCycle_Length = 0;
                newCycleOrGenStr = true;
            }

            //create new list item for PMDataOrdered2_currTrainingCycle if not already the last list item; otherwise add 1 to the last list item
            KeyValuePair<string, int> LastPMData;
            bool CycleIsEmpty = PMDataOrdered2_currTrainingCycle.Count <= 0;
            if (!CycleIsEmpty)
            {
                LastPMData = PMDataOrdered2_currTrainingCycle.Last();
                if (LastPMData.Key == genStr) //just increment weight if last weight
                {
                    PMDataOrdered2_currTrainingCycle[PMDataOrdered2_currTrainingCycle.Count - 1] = new KeyValuePair<string, int>(genStr, LastPMData.Value + 1);
                }
                else //add new list item with different genStr if not
                {
                    PMDataOrdered2_currTrainingCycle.Add(new KeyValuePair<string, int>(genStr, 1)); //add new element
                    newCycleOrGenStr = true;
                }
            }
            else
            {
                PMDataOrdered2_currTrainingCycle.Add(new KeyValuePair<string, int>(genStr, 1)); //alongside initializing, adding first element to the List
            }
            PMDataOrdered2_currTrainingCycle_Length++; //increment current training cycle length by 1
        }
        #endregion
        if(isDoingDistributionOfFixedRelativeMovementVecCycles)
        {
            if (resetMovementDirTimerThisUpdate) //One cycle for each period of the entirety of while the current interval for the respective time has not changed; new interval (with timer having been reset) - start new cycle with the current featureStr
            {
                if (PMDataOrdered3_currTrainingCycle_featureStr != "") //if not the initial str (i.e. before any cycle would have been started being created), then add the currCycle
                {
                    PMDataOrdered3_ofFixedLengthCycles[PMDataOrdered3_currTrainingCycle_featureStr].Add(PMDataOrdered3_currTrainingCycle);
                }
                PMDataOrdered3_currTrainingCycle_featureStr = featureStr;
                PMDataOrdered3_currTrainingCycle = new List<Pair<genBehaviourData, float>>();
                newCycleOrGenStr = true;
            }
            genBehaviourData genData = new genBehaviourData()
            {
                movement_thisUpdate_CCWAngleRelativeToSMPos = currData.movementDir_thisUpdate_CCWAngleRelativeToSMPos,
                movement_thisUpdate_CCWAngleRelativeToMNPos = currData.movementDir_thisUpdate_CCWAngleRelativeToMNPos,
                movement_thisUpdate_Magnitude = currData.movementDir_thisUpdate_Magnitude,
                isFiring_thisUpdate = FireShot.playerIsFiring
            };

            if (genData.movement_thisUpdate_CCWAngleRelativeToMNPos == 0)
            {
                Debug.Log("CWAngleRelativeToMNPos is 0 in Training() outside of the getData method right now");
            }
            //create new list item for PMDataOrdered3_currTrainingCycle
            //PMDataOrdered3_currTrainingCycle.Add(new Pair<genMovementData, float>(genData, Time.fixedDeltaTime)); //alongside initializing, adding first element to the List

            //create new list item for PMDataOrdered3_currTrainingCycle if not already (equal to) the last list item; otherwise add Time.fixedDeltaTime to the last list item
            Pair<genBehaviourData, float> LastPMData;
            bool CycleIsEmpty = PMDataOrdered3_currTrainingCycle.Count <= 0;
            if (!CycleIsEmpty)
            {
                LastPMData = PMDataOrdered3_currTrainingCycle.Last();
                if (LastPMData.First.Equals(genData)) //just increment weight if last weight
                {
                    PMDataOrdered3_currTrainingCycle[PMDataOrdered3_currTrainingCycle.Count - 1].Second = LastPMData.Second + Time.fixedDeltaTime; //Last() not allowing reassignment
                }
                else //add new list item with different genData if not (equal to) last list item
                {
                    PMDataOrdered3_currTrainingCycle.Add(new Pair<genBehaviourData, float>(genData, Time.fixedDeltaTime)); //add new element
                    newCycleOrGenStr = true;
                }
            }
            else //if not (equal to, with what I would mean by "equal to" here) last list item (being with the cycle being empty)
            {
                PMDataOrdered3_currTrainingCycle.Add(new Pair<genBehaviourData, float>(genData, Time.fixedDeltaTime)); //alongside initializing, adding first element to the List
            }
        }

        //Noting of weights and basically adding a weight where the fraction of the weight would be its probability of being triggered by the AI and\or such as a transition ...
        //noting of how eg. such would preserve the Markov data in a lot of frames ... noting of eg. how such would be probabilistic rather than eg. moreso periodic and/or such
        prevMarkovData1 = currData;
    }

    void Using()
    {
        // Randomly generate words that resemble the words in the dictionary.
        var rand = new System.Random();

        //get features and actions of the current feature combination, as stored in ...well, stored in what was CurrAIPrevMarkovUnit with features as set

        //get next action
        //regarding "get next action" above (as part of such): retrieve features from game state

        Vector2 shadowPos = transform.position;
        MarkovUnit currData = getData(shadowPos, false); //position of the Shadow for use //get current data, for the features


        //convert the data of prev. movement dirs relative to SM and M-N into strings, from oldest movement dirs to newest
        string prevMovementRelativeToCMAndMNStr = "";
        while (currData.QuantizedDirAngle_RelativeToCMAtData.Count > 0/* && (prevMovementRelativeToCMAndMNStr.Length <= 4) in case the string would have copied more than MAX_PREVDIRS * 2 chars somehow*/)
        {
            prevMovementRelativeToCMAndMNStr += currData.QuantizedDirAngle_RelativeToCMAtData.Dequeue(); //getting the oldest prev. data first
            prevMovementRelativeToCMAndMNStr += currData.QuantizedDirAngle_RelativeToMNAtData.Dequeue();
        }
        prevMovementRelativeToCMAndMNStr = prevMovementRelativeToCMAndMNStr.PadLeft(MAX_PREVDIRS * 2, '0'); //padding with the max length for dirs

        //PM feature combination and generation data, encoded as strings
        string featureStr = "";
        featureStr += ((char)(currData.eDistance + '0')) + "" + ((char)(currData.maxSpeed + '0')) + prevMovementRelativeToCMAndMNStr;

        //reset the currTrainingCycle if just loaded data
        if (resetCurrUsingCycle_withLoadedDataThisUpdate || SwappedPlacesThisUpdate) //with TODOs (as of 12/23/18) as with those seen in Training() (as I would be aware)
        {
            PMDataOrdered3_currUsingCycle = new List<Pair<genBehaviourData, float>>(); //dummy
            PMDataOrdered3_currUsingCycle.Add(new Pair<genBehaviourData, float>(new genBehaviourData(), 0f)); //dummy that would be to be replaced
            PMDataOrdered3_cyclePosn = Tuple.Create<int, float>(0, 0f); //in order to prevent index out of bounds errors
            Debug.Log("Using cycle reset");
        }
        if (resetCurrTrainingCycle_withLoadedDataThisUpdate)
        {
            resetCurrUsingCycle_withLoadedDataThisUpdate = false;
        }
        if (SwappedPlacesThisUpdate)
        {
            prevMovementDirTimer = 0f; //not setting prevMovementDirTimer to prevMovementDirInterval because of resetting eg. featureStr and/or such being already done in the prior if statement
            QuantizedDirAngle_RelativeToCM.Clear();
            QuantizedDirAngle_RelativeToMN.Clear();
            avgClosest2eDistances_player.Clear();
            avgClosest2eDistances_nonplayer.Clear();
            SwappedPlacesThisUpdate = false;
        }

        //if feature combination is not present, then do nothing (regarding the genStr, etc.)
        if (PMData.ContainsKey(featureStr))
        {
            int selWeightValue = 0; //with initial value to be changed if used - i.e. when isCycling would be false, as of 9/15/18, 2:55PM

            bool isCycling = false;
            bool isDoingDistributionOfCycles = false;
            bool isDoingDistributionOfFixedRelativeMovementVecCycles = true;
            if (isCycling)
            {
                #region isCycling code
                //increment selWeightValue for the given feature set ...or, use PMDataOrdered_cyclePosn for the value instead, which would be used to obtain the selectedGenStr
                //noting of the order of each value's weight obtained, with noting of such of the distribution and to be in the same order as obtained during training, rather than <<weight values><YKWIM>> just placed into 'buckets' and noting of modification of the PM accordingly to track such, with making values of each feature set into a List instead of a Dictionary
                //keeping track of the current List element index and weight of the element in PMData_maintainOrder as the cyclePosn

                //if ()
                int PMDataIndex = PMDataOrdered_cyclePosn[featureStr].Item1;
                int PMDataWeightPos = PMDataOrdered_cyclePosn[featureStr].Item2;
                if (PMDataWeightPos >= PMData_maintainOrder[featureStr][PMDataIndex].Value) //if position is at the last weight value (or later or somehow)
                {
                    //go to next element of List or cycle back if it is the last element
                    if (PMDataIndex >= PMData_maintainOrder[featureStr].Count - 1)
                    {
                        PMDataIndex = 0;
                    }
                    else
                    {
                        PMDataIndex++;
                    }
                }
                else
                {
                    PMDataWeightPos++;
                }

                PMDataOrdered_cyclePosn[featureStr] = Tuple.Create<int, int>(PMDataIndex, PMDataWeightPos)/*(PMData_cyclePosn[featureStr] + 1) % totalOfWeights*/; //increment by weight, where one weight val would be obtained each frame in training, and so doing the same in using
                                                                                                                                                                           //selWeightValue = PMData_cyclePosn[featureStr];
                #endregion
            }
            #region isDoingDistributionOfCycles
            else if(isDoingDistributionOfCycles)
            {
                //implementation to be done when with a new featureStr
                //with allowing completion of the cycle up to a time limit ...actually, already handled ...actually, current way of handling would not make the first condition using PMDataOrdered2_currUsingCycle to use the current featureStr, but the last featureStr ... though that being what would be intended in the design
                //PMDataOrdered2_currUsingCycle

                int PMDataIndex = PMDataOrdered2_cyclePosn.Item1;
                int PMDataWeightPos = PMDataOrdered2_cyclePosn.Item2;
                if (PMDataWeightPos >= PMDataOrdered2_currUsingCycle[PMDataIndex].Value) //if position is at the last weight value (or later or somehow)
                {
                    //go to next element of List or re-select a new cycle for the given featureStr if it is the last element
                    if (PMDataIndex >= PMDataOrdered2_currUsingCycle.Count - 1) //if end of cycle
                    {
                        PMDataIndex = 0;
                        int randCycleIndex = rand.Next(PMData_ofmaintainOrderCycles[featureStr].Count);
                        PMDataOrdered2_currUsingCycle = PMData_ofmaintainOrderCycles[featureStr][randCycleIndex]; //re-selection of cycle
                    }
                    else
                    {
                        PMDataWeightPos -= PMDataOrdered2_currUsingCycle[PMDataIndex].Value;
                        PMDataIndex++;
                    }
                }
                else
                {
                    PMDataWeightPos++;
                }

                PMDataOrdered2_cyclePosn = Tuple.Create<int, int>(PMDataIndex, PMDataWeightPos)/*(PMData_cyclePosn[featureStr] + 1) % totalOfWeights*/; //increment by weight, where one weight val would be obtained each frame in training, and so doing the same in using
            }
            #endregion
            if (isDoingDistributionOfFixedRelativeMovementVecCycles)
            {
                //implementation to be done when with a new featureStr
                //with allowing completion of the cycle up to a time limit ...actually, already handled ...actually, current way of handling would not make the first condition using PMDataOrdered3_currUsingCycle to use the current featureStr, but the last featureStr ... though that being what would be intended in the design

                int PMDataIndex = PMDataOrdered3_cyclePosn.Item1;
                float PMDataWeightPos = PMDataOrdered3_cyclePosn.Item2;
                cyclePosn_Index = PMDataOrdered3_cyclePosn.Item1;
                cyclePosn_weight = PMDataOrdered3_cyclePosn.Item2;
                if (PMDataWeightPos >= PMDataOrdered3_currUsingCycle[PMDataIndex].Second) //if position is at the last weight value (or later or somehow) //got an error 3/20/19
                {
                    //re-select a new cycle for the given featureStr if (it is the last element or timer resets), or otherwise, go to next element of List
                    if (resetMovementDirTimerThisUpdate || PMDataIndex >= PMDataOrdered3_currUsingCycle.Count - 1) //if reset timer with the interval, or end of cycle
                    {
                        PMDataIndex = 0;

                        //set new cycle if one exists
                        if (PMDataOrdered3_ofFixedLengthCycles.ContainsKey(featureStr) && PMDataOrdered3_ofFixedLengthCycles[featureStr].Count >= 1)
                        {
                            //check that there is at least one UsingCycle with a Count >= 1
                            bool hasAtLeast1CycleWithCountAtLeast1 = false;
                            foreach(List<Pair<genBehaviourData, float>> checkCycle in PMDataOrdered3_ofFixedLengthCycles[featureStr])
                            {
                                if(checkCycle.Count >= 1)
                                {
                                    hasAtLeast1CycleWithCountAtLeast1 = true;
                                    break;
                                }
                            }

                            if (hasAtLeast1CycleWithCountAtLeast1)
                            {
                                //randomly select a UsingCycle and discard it and reselect if its count is less than or equal to 0
                                int randCycleIndex = rand.Next(PMDataOrdered3_ofFixedLengthCycles[featureStr].Count);
                                PMDataOrdered3_currUsingCycle = PMDataOrdered3_ofFixedLengthCycles[featureStr][randCycleIndex]; //re-selection of cycle
                                while (PMDataOrdered3_currUsingCycle.Count <= 0)
                                {
                                    Debug.Log("Had to reselect using cycle");
                                    randCycleIndex = rand.Next(PMDataOrdered3_ofFixedLengthCycles[featureStr].Count);
                                    PMDataOrdered3_currUsingCycle = PMDataOrdered3_ofFixedLengthCycles[featureStr][randCycleIndex]; //re-selection of cycle
                                }
                            }
                            else
                            {
                                Debug.Log("Not at least 1 cycle with a count of at least 1");
                            }
                        }
                    }
                    else
                    {
                        PMDataWeightPos -= PMDataOrdered3_currUsingCycle[PMDataIndex].Second;
                        PMDataIndex++; //go to next element of List
                    }
                }
                else
                {
                    PMDataWeightPos += Time.fixedDeltaTime;
                }

                //go to next element of List or re-select a new cycle for the given featureStr if it is to reset at this point
                PMDataOrdered3_cyclePosn = new Tuple<int, float>(PMDataIndex, PMDataWeightPos); //increment by weight, where one weight val would be obtained each frame in training, and so doing the same in using
            }
            else
            {

                var totalOfWeights = PMData[/*feature string*/featureStr].Sum(w => w.Value);//
                selWeightValue = rand.Next(totalOfWeights) + 1; //value selected among the weights
            }


            string selectedGenStr = "9"; //generation that would have been randomly selected among the distribution of weights (being part of a probability distribution) //set to a dummy value that would be intended as "n/a" behaviour of the Shadow AI

            #region isCycling code
            if (isCycling)
            {
                //get selectedGenStr from this cycled version instead
                int PMDataIndex = PMDataOrdered_cyclePosn[featureStr].Item1;
                selectedGenStr = PMData_maintainOrder[featureStr][PMDataIndex].Key;
            }
            #endregion
            else if (isDoingDistributionOfCycles) //!isCycling
            {
                //get selectedGenStr from this cycled version instead
                int PMDataIndex = PMDataOrdered2_cyclePosn.Item1;
                selectedGenStr = PMDataOrdered2_currUsingCycle[PMDataIndex].Key;
            }
            else if (isDoingDistributionOfFixedRelativeMovementVecCycles)
            {
                selectedGenStr = ""; //selectedGenStr not used for this
            }
            else
            {
                var currentWeight = 0;
                foreach (var nextItem in PMData[featureStr]) //iterate through the weights and select the first item that would reach the random value threshold to add to the state
                {
                    currentWeight += nextItem.Value;
                    if (currentWeight >= selWeightValue) //selected item at the value
                    {
                        selectedGenStr = nextItem.Key;
                        break;
                    }
                }
            }
        }

        //get closest distances of enemy
        bool isDoingDistributionOfFixedRelativeMovementCycles = true;
        Action AIActionNum = 0;
        if (!isDoingDistributionOfFixedRelativeMovementCycles)
        {
            AIActionNum = (Action)(prevGenStr[0]/* - read as a char?*/ - '0'); //with AI action with index 0
        }
        Vector3 moveVec = Vector3.zero;
        float deltaMovementScale = 1/* / Time.fixedDeltaTime * Time.deltaTime*/; //given 0.1f of movement on each FixedUpdate as done by the player movement, what would be the movement with Update that would be at least roughly equivalent speed 
        #region !isDoingDistributionOfFixedRelativeMovementCycles code
        if (!isDoingDistributionOfFixedRelativeMovementCycles)
        {
            switch (AIActionNum)
            {
                case Action.Retreat:
                    //retreat generation behaviour
                    //move away from nearest enemy
                    moveVec = (shadowPos - currData.avgClosest2EnemyPosn).normalized * 0.1f * deltaMovementScale; //vector for delta in movement for the current Update - although would note how player would move via FixedUpdate
                                                                                                                  //quantize to closest movement angle and set animation
                    break;
                case Action.Attack:
                    //attack generation behaviour
                    //move towards nearest enemy
                    moveVec = (currData.avgClosest2EnemyPosn - shadowPos).normalized * 0.1f * deltaMovementScale;
                    //transform.position += new Vector3(moveVec.x, moveVec.y, 0);
                    break;
                case Action.Wander:
                    //wander generation behaviour
                    //move around in a 'random' manner but eg. humanly
                    //can just imitate already-existing wander behaviour as a current behaviour
                    //move with the Astar behaviour, initiating Astar seek towards an initialized Wander location each time that Wander behaviour would be just changed to, and ceasing such (of setting Wander location and Astar seek behaviour) when such Wander behaviour would be changed to something else
                    break;
            }
        }
        #endregion
        else //isDoingDistributionOfFixedRelativeMovementCycles
        {
            //determine moveVec based on the angle and such, with determining the movement vec based on such with weighting accordingly
            genBehaviourData moveData = PMDataOrdered3_currUsingCycle[PMDataOrdered3_cyclePosn.Item1].First; //index out of bounds error and\or such on this line 3/20/19 ...

            //Rotate CCW, as the training data with Vector2.SignedAngle as done in training would find the CCW angle; rotating CCW on 3/12/19, at 12:3xAM
            Vector2 moveVecRelativeToMNPosComponent = Quaternion.Euler(0f, 0f, moveData.movement_thisUpdate_CCWAngleRelativeToMNPos) * (currData.avgClosest2EnemyPosn - shadowPos).normalized * moveData.movement_thisUpdate_Magnitude;
            Vector2 moveVecRelativeToSMPosComponent = Quaternion.Euler(0f, 0f, moveData.movement_thisUpdate_CCWAngleRelativeToSMPos) * (currData.medianEnemyPosn - shadowPos).normalized * moveData.movement_thisUpdate_Magnitude;

            currMoveDirRelativeToSM_Using = moveData.movement_thisUpdate_CCWAngleRelativeToSMPos;
            currMoveDirRelativeToMN_Using = moveData.movement_thisUpdate_CCWAngleRelativeToMNPos;

            //moveData.movement_thisUpdate_CWAngleRelativeToMNPos
            moveVec = (moveVecRelativeToMNPosComponent + moveVecRelativeToSMPosComponent) / 2;
            if (isUsing && getCurrMovementAngleAndMag)
            {
                //Debug.Log("direction of vector with a calculation: " + Quaternion.Euler(0f, 0f, 45) * new Vector2(1, 0).normalized * moveData.movement_thisUpdate_Magnitude);
                Debug.Log("CWAngleRelativeToMNPos: " + moveData.movement_thisUpdate_CCWAngleRelativeToMNPos + "; \r\nCWAngleRelativeToSMPos: " + moveData.movement_thisUpdate_CCWAngleRelativeToSMPos + ";");
                Debug.Log("magnitude: " + moveData.movement_thisUpdate_Magnitude + "; \r\nmoveVec: " + moveVec);
                Debug.Log("moveData.isFiring_thisUpdate: " + moveData.isFiring_thisUpdate);
                Debug.Log("numPlayerAndAI matching updates of total of "+numPlayerAndAITotalTestingUpdates+": ");
                Debug.Log("Movement: "+numPlayerAndAIMovementMatchingUpdates+" ("+((int)((float)numPlayerAndAIMovementMatchingUpdates/numPlayerAndAITotalTestingUpdates*100))+"%); Shooting: "+numPlayerAndAIShootingMatchingUpdates+" ("+((int)((float)numPlayerAndAIShootingMatchingUpdates/numPlayerAndAITotalTestingUpdates*100))+"%); Both: "+numPlayerAndAIMovingAndShootingMatchingUpdates+" ("+((int)((float)numPlayerAndAIMovingAndShootingMatchingUpdates/numPlayerAndAITotalTestingUpdates * 100))+"%)");
                Debug.Log("Average difference in movement direction between player and AI: " + PlayerAndAIMovementAngleDiffTotalInAllUpdates / numPlayerAndAITotalTestingUpdates+" with "+ numPlayerAndAIMovementHavingNoMovementInPlayerOrAIButNotBothUpdates+" of the updates having no movement in the player or AI's action, but not both actions, and "+numPlayerAndAIMovementHavingMovementInBothPlayerAndAIUpdates+" of the updates having no movement in both actions - as I would understand such to mean");

                Debug.Log("For enemies on screen: ");
                Debug.Log("numPlayerAndAI matching updates of total of " + numPlayerAndAITotalTestingUpdates_enemiesOnScreen + ": ");
                Debug.Log("Movement: " + numPlayerAndAIMovementMatchingUpdates_enemiesOnScreen + " (" + ((int)((float)numPlayerAndAIMovementMatchingUpdates_enemiesOnScreen / numPlayerAndAITotalTestingUpdates_enemiesOnScreen * 100)) + "%); Shooting: " + numPlayerAndAIShootingMatchingUpdates_enemiesOnScreen + " (" + ((int)((float)numPlayerAndAIShootingMatchingUpdates_enemiesOnScreen / numPlayerAndAITotalTestingUpdates_enemiesOnScreen * 100)) + "%); Both: " + numPlayerAndAIMovingAndShootingMatchingUpdates_enemiesOnScreen + " (" + ((int)((float)numPlayerAndAIMovingAndShootingMatchingUpdates_enemiesOnScreen / numPlayerAndAITotalTestingUpdates_enemiesOnScreen * 100)) + "%)");
                Debug.Log("Number of times that the player has taken damage (since last resetting): " + player.GetComponentInChildren<PlayerHealth>().numTimesPlayerTakenDamage);
                Debug.Log("Average difference in movement direction between player and AI: " + PlayerAndAIMovementAngleDiffTotalInAllUpdates_enemiesOnScreen /numPlayerAndAITotalTestingUpdates_enemiesOnScreen + " with " + numPlayerAndAIMovementHavingNoMovementInPlayerOrAIButNotBothUpdates_enemiesOnScreen + " of the updates having no movement in the player or AI's action, but not both actions, and " + numPlayerAndAIMovementHavingMovementInBothPlayerAndAIUpdates_enemiesOnScreen + " of the updates having no movement in both actions - as I would understand such to mean");

                WaveManager thisWaveManager = GameObject.FindGameObjectWithTag("WaveManager").GetComponentInChildren<WaveManager>();
                for (int shadowNum = thisWaveManager.shadowNumMin; shadowNum <= thisWaveManager.shadowNumMax; ++shadowNum)
                {
                    int timesTakenDamage = 0;
                    float amtTimeSinceReset = 0;
                    if (PlayerPrefs.HasKey("numTimesShadow" + shadowNum + "TakenDamage"))
                    {
                        //PlayerPrefs.DeleteKey("numTimesShadow" + shadowNum + "TakenDamage");
                        timesTakenDamage = PlayerPrefs.GetInt("numTimesShadow" + shadowNum + "TakenDamage");
                    }
                    if (PlayerPrefs.HasKey("amtTimeSinceResetShadow" + shadowNum + "Results"))
                    {
                        amtTimeSinceReset = PlayerPrefs.GetFloat("amtTimeSinceResetShadow" + shadowNum + "Results");
                        //PlayerPrefs.DeleteKey("amtTimeSinceResetShadow" + shadowNum + "Results");
                    }
                    Debug.Log("Number of times that Shadow " + shadowNum + " has taken damage: " + timesTakenDamage + "; amount of time counted by Shadow "+shadowNum+"'s ShadowHealth component since the last reset of Shadow results: " + amtTimeSinceReset + ".");
                }

                int timesPlayerTakenDamage = 0;
                float amtTimeSinceResetPlayer = 0;
                if (PlayerPrefs.HasKey("numTimesPlayerTakenDamage"))
                {
                    //PlayerPrefs.DeleteKey("numTimesShadow" + shadowNum + "TakenDamage");
                    timesPlayerTakenDamage = PlayerPrefs.GetInt("numTimesPlayerTakenDamage");
                }
                if (PlayerPrefs.HasKey("amtTimeSinceResetPlayerResults"))
                {
                    amtTimeSinceResetPlayer = PlayerPrefs.GetFloat("amtTimeSinceResetPlayerResults");
                    //PlayerPrefs.DeleteKey("amtTimeSinceResetShadow" + shadowNum + "Results");
                }
                Debug.Log("Number of times that the player has taken damage: " + timesPlayerTakenDamage + "; amount of time counted by the player's PlayerHealth component since the last reset of the player's results: " + amtTimeSinceResetPlayer + ".");
                getCurrMovementAngleAndMag = false;
            }
            gizmoSpheresToDraw.Add(new gizmoSphereProperties() { color = Color.green, position = moveVec.normalized + transform.position, radius = 0.5f });

            isFiringThisUpdate = moveData.isFiring_thisUpdate;
        }

        #region MoveInnerInput-based code for getting closest movement point quantized to the closest movement angle, and setting the Shadow's animation for the corresponding direction
        //get closest movement point quantized to the closest movement angle
        //Get each of the 8 points, foreach through them? As one way ... Getting positions through sin and cos of each angle, times the radius
        Vector3 min_dist_degAnglePoint_Posn = new Vector3(0, 0);
        float min_dist_degAnglePoint_dist = 9 / transform.localScale.x/* - adding another GameObj_Radius length corresponding to the greater allowed input control acceptance range*/; //some impossibly large distance as default
        for (int degAngle = 0; degAngle < 360; degAngle += 45)
        {
            float degAngle_rad = degAngle * 2 * Mathf.PI / 360;

            //position of one of the 8 points
            Vector3 onePointPosition = (
                                  new Vector3(0f, 0.1f * Mathf.Sin(degAngle_rad), 0f) +
                                  new Vector3(0.1f * Mathf.Cos(degAngle_rad), 0f, 0f))
                                  ;
            float dist_degAnglePoint = (moveVec
                                  - onePointPosition).magnitude;
            if (dist_degAnglePoint < min_dist_degAnglePoint_dist)
            {
                min_dist_degAnglePoint_dist = dist_degAnglePoint;
                min_dist_degAnglePoint_Posn = onePointPosition;
                dir_angleRad = degAngle_rad;
                dir_angleDeg = degAngle;
                dir_lastFacedAngleDeg = degAngle;
            }
        }
        i_selected = dir_angleDeg / 45; //corresponding index to the degAngle for animation setting

        //Check center of the <<move control><YKWIM>> as well
        //Vector3 centerPointPosition = OuterMovePart.OuterMovePart_Pos;
        //float dist_center = (moveVec
        //                          - centerPointPosition).magnitude;
        //if (dist_center < min_dist_degAnglePoint_dist)
        //{
        //    min_dist_degAnglePoint_dist = dist_center;
        //    min_dist_degAnglePoint_Posn = centerPointPosition; //does it copy? Well, it's passed by value according to someone, where such a person would have said that it's a structure
        //    dir_angleRad = -100f;
        //    dir_angleDeg = -100;
        //}
        //else //if the center point would not be the closest point
        //{
        //set animator animation to the index corresponding to the <<angle at which the nearest point would have been found><YKWIM>>

        //if firing on this update, discard animation based on movement - will be done based on shooting direction instead, if shooting
        if (!isFiringThisUpdate)
        {
            Animator animator = GetComponentInChildren<Animator>();
            if (animator.runtimeAnimatorController != shadowAnimations[i_selected]) //making this condition with noting of how some initialize method for the animator would be being called in this getData method
            {
                animator.runtimeAnimatorController = shadowAnimations[i_selected]; //set animation for the Shadow AI
            }
        }
        //}
        #endregion
        if(GetComponent<ShadowHealth>().isDead) //still, cease movement if the Shadow is dead
        {
            moveVec = Vector3.zero;
        }

        Vector3 moveVec_quantized = min_dist_degAnglePoint_Posn.normalized * /*0.1f * deltaMovementScale*/moveVec.magnitude;
        //movement
        if (!usedForLoggingTraining)
        {
            transform.position += new Vector3(moveVec_quantized.x, moveVec_quantized.y, 0); //noting of movement being done on Update instead of FixedUpdate with such Shadows, with noting of such a frame rate difference in movement, and such
        }
        else //use this for prediction
        {
            //if movements match
            //check with MoveInnerInput
            numPlayerAndAITotalTestingUpdates++;
#if UNITY_EDITOR
            bool movementMatching = (player.GetComponent<TouchMove>().prevMoveDelta.normalized == moveVec_quantized.normalized); //check if the movement direction is matching or if there would be no direction
                                                                                                                                 
            //get difference between non-quantized moveVec and the player's movement direction
            Vector2 normalizedPlayerPrevMoveDelta = player.GetComponent<TouchMove>().prevMoveDelta.normalized;
            Vector2 moveVec_normalized = moveVec.normalized;
            float angleDiffBetweenPlayerAndAI = Mathf.Abs((Vector2.SignedAngle(player.GetComponent<TouchMove>().prevMoveDelta.normalized, moveVec.normalized)));
            if (angleDiffBetweenPlayerAndAI > 180)
            {
                angleDiffBetweenPlayerAndAI = 360 - angleDiffBetweenPlayerAndAI;
            }
            if (!movementMatching && (normalizedPlayerPrevMoveDelta == Vector2.zero || moveVec_normalized == Vector2.zero))
            {
                numPlayerAndAIMovementHavingNoMovementInPlayerOrAIButNotBothUpdates++;
            }
            if (moveVec_normalized == Vector2.zero && movementMatching)
            {
                angleDiffBetweenPlayerAndAI = 0;
                numPlayerAndAIMovementHavingMovementInBothPlayerAndAIUpdates++;
            }
            PlayerAndAIMovementAngleDiffTotalInAllUpdates += angleDiffBetweenPlayerAndAI;
#else
            bool movementMatching = (dir_angleDeg == MoveInnerInput.dir_angleDeg || (moveVec_quantized == Vector3.zero && MoveInnerInput.dir_angleDeg == -100))/* if using mobile device inputs*/;
#endif
            bool shootingMatching = isFiringThisUpdate == FireShot.playerIsFiring;
            if (movementMatching)
            {
                numPlayerAndAIMovementMatchingUpdates++;
                if(shootingMatching) //if both are matching
                {
                    numPlayerAndAIMovingAndShootingMatchingUpdates++;
                }
            }
            if(shootingMatching)
            {
                numPlayerAndAIShootingMatchingUpdates++;
            }

            //checking of such said updates only while enemies are on screen:             
            if (currData.eDistance != EnemyDistance.FAR_OFFSCREEN)
            {
                numPlayerAndAITotalTestingUpdates_enemiesOnScreen++;
#if UNITY_EDITOR
                //Vector2 normalizedPlayerPrevMoveDelta = player.GetComponent<TouchMove>().prevMoveDelta.normalized;
                //Vector2 moveVec_normalized = moveVec.normalized;
                bool movementMatching_enemiesOnScreen = (player.GetComponent<TouchMove>().prevMoveDelta.normalized == moveVec_quantized.normalized); //check if the movement direction is matching or if there would be no direction

                //get difference between non-quantized moveVec and the player's movement direction
                //Debug.Log("normalizedPlayerPrevMoveDelta: " + normalizedPlayerPrevMoveDelta + "; moveVec_normalized: " + moveVec_normalized);
                float angleDiffBetweenPlayerAndAI_enemiesOnScreen = Mathf.Abs((Vector2.SignedAngle(normalizedPlayerPrevMoveDelta, moveVec_normalized)));
                if(!movementMatching_enemiesOnScreen && (normalizedPlayerPrevMoveDelta == Vector2.zero || moveVec_normalized == Vector2.zero))
                {
                    numPlayerAndAIMovementHavingNoMovementInPlayerOrAIButNotBothUpdates_enemiesOnScreen++;
                }
                if(angleDiffBetweenPlayerAndAI_enemiesOnScreen > 180)
                {
                    angleDiffBetweenPlayerAndAI_enemiesOnScreen = 360 - angleDiffBetweenPlayerAndAI_enemiesOnScreen;
                }
                if (moveVec_normalized == Vector2.zero && movementMatching_enemiesOnScreen)
                {
                    angleDiffBetweenPlayerAndAI_enemiesOnScreen = 0;
                    numPlayerAndAIMovementHavingMovementInBothPlayerAndAIUpdates_enemiesOnScreen++;
                }
                PlayerAndAIMovementAngleDiffTotalInAllUpdates_enemiesOnScreen += angleDiffBetweenPlayerAndAI_enemiesOnScreen;
#else
                bool movementMatching_enemiesOnScreen = (dir_angleDeg == MoveInnerInput.dir_angleDeg || (moveVec_quantized == Vector3.zero && MoveInnerInput.dir_angleDeg == -100))/* if using mobile device inputs*/;
#endif
                bool shootingMatching_enemiesOnScreen = isFiringThisUpdate == FireShot.playerIsFiring;
                if (movementMatching_enemiesOnScreen)
                {
                    //Debug.Log("Player movement: " + player.GetComponent<TouchMove>().prevMoveDelta.normalized + "; Generated movement: "+moveVec_quantized.normalized);
                    numPlayerAndAIMovementMatchingUpdates_enemiesOnScreen++;
                    if (shootingMatching_enemiesOnScreen) //if both are matching
                    {
                        numPlayerAndAIMovingAndShootingMatchingUpdates_enemiesOnScreen++;
                    }
                }
                if (shootingMatching_enemiesOnScreen)
                {
                    numPlayerAndAIShootingMatchingUpdates_enemiesOnScreen++;
                }
            }
        }

        //collision handling of movement
        if (transform.position != prevMovePosition && !isColliding)
            prevMoveDelta = transform.position - prevMovePosition;

        collisionsThisUpdate.Clear();
        collisionsThisUpdate.TrimExcess();
    }

    void DebuggingPM()
    {
        //create a List with the current weights
        items_weights_alltogether = new List<string>();

        bool isCycling_debug = false;
        bool isDistributionOfCycles_debug = false;
        bool isDistributionOfFixedRelativeMovementCycles_debug = true;
        if (isCycling_debug)
        {
            #region isCycling_debug
            foreach (var item in PMData_maintainOrder)
            {
                if (/*item.Key.items.Length >= 1*//* - there is a state, or there are weights for that state?*/item.Value.Count >= 1)
                {
                    string itemFeatureStr = item.Key;
                    EnemyDistance curreDistance = (EnemyDistance)(itemFeatureStr[0] - '0');
                    char maxSpeed = (char)(itemFeatureStr[1] - '0');

                    string eDistanceText = "";
                    switch (curreDistance)
                    {
                        case EnemyDistance.FAR:
                            eDistanceText = "FAR";
                            break;
                        case EnemyDistance.NEAR:
                            eDistanceText = "NEAR";
                            break;
                    }

                    string nearEnemyMaxSpeedText = "";
                    nearEnemyMaxSpeedText += ((char)(maxSpeed + '0'));

                    //MarkovUnit currItemMarkov = item.Key.items.Last(); //get the last item in the state, if there is one
                    foreach (var genItem in PMData_maintainOrder[itemFeatureStr]) //list generations and corresponding weights for the corresponding feature
                    {
                        string AIActionText = "";
                        string itemGenStr = genItem.Key; //AI action in string form
                        Action currAIAction = (Action)(itemGenStr[0] - '0'); //parsed AI action string
                        switch (currAIAction)
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
                        //TODO since 8/13/18, 11:11PM, with whatever would still: make debug code for these
                        //and new version of this add as (with DONE? progress as of 8/15/18, with) TODO (as of 8/13/18, 11:13PM): items_weights_alltogether.Add("Unit "+ AIActionText + ", "/* +nearEnemyCountText+ ", "*/+nearEnemyMaxSpeedText+", "+ eDistanceText+": "); //MarkovUnit based on 

                        items_weights_alltogether.Add("With (" + eDistanceText + "," + nearEnemyMaxSpeedText + "), (" + AIActionText + ") w: "); //MarkovUnit based on 
                        int actionWeight = genItem.Value;
                        items_weights_alltogether.Add(actionWeight.ToString());
                    }
                }
                else
                {
                    items_weights_alltogether.Add("empty PM");
                }
            }
            #endregion
        }
        else if(isDistributionOfCycles_debug)
        {
            #region isDistributionOfCycles_debug
            foreach (var item in PMData_ofmaintainOrderCycles)
            {
                if (/*item.Key.items.Length >= 1*//* - there is a <<<state, or there are weights for that state><YKWIM>>?>*/item.Value.Count >= 1)
                {
                    string itemFeatureStr = item.Key;
                    EnemyDistance curreDistance = (EnemyDistance)(itemFeatureStr[0] - '0');
                    char maxSpeed = (char)(itemFeatureStr[1] - '0');

                    string eDistanceText = "";
                    switch (curreDistance)
                    {
                        case EnemyDistance.FAR:
                            eDistanceText = "FAR";
                            break;
                        case EnemyDistance.NEAR:
                            eDistanceText = "NEAR";
                            break;
                    }

                    string nearEnemyMaxSpeedText = "";
                    nearEnemyMaxSpeedText += ((char)(maxSpeed + '0'));

                    //MarkovUnit currItemMarkov = item.Key.items.Last(); //get the last item in the state, if there is one
                    foreach (var genCycleItem in PMData_ofmaintainOrderCycles[itemFeatureStr])
                    {
                        string genCycleText = "genCycle for ("+eDistanceText+", "+ nearEnemyMaxSpeedText +"): ";
                        items_weights_alltogether.Add(genCycleText);

                        foreach (var genItem in genCycleItem) //list generations and corresponding weights for the corresponding feature
                        {
                            string AIActionText = "";
                            string itemGenStr = genItem.Key; //AI action <<in string form><YKWIM>>
                            Action currAIAction = (Action)(itemGenStr[0] - '0'); //parsed AI action <<string><YKWIM>>
                            switch (currAIAction)
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
                            //TODO since 8/13/18, 11:11PM, with whatever would still: make debug code for these
                            //and new version of this add as (with DONE? progress as of 8/15/18, with) TODO (as of 8/13/18, 11:13PM): items_weights_alltogether.Add("Unit "+ AIActionText + ", "/* +nearEnemyCountText+ ", "*/+nearEnemyMaxSpeedText+", "+ eDistanceText+": "); //MarkovUnit based on 

                            int actionWeight = genItem.Value;
                            items_weights_alltogether.Add("With (" + AIActionText + ") w: "+actionWeight.ToString()); //MarkovUnit based on 
                        }
                    }
                }
                else
                {
                    items_weights_alltogether.Add("empty PM");
                }
            }
            #endregion
        }
        else if (isDistributionOfFixedRelativeMovementCycles_debug)
        {
            #region isDistributionOfFixedRelativeMovementCycles
            int item_count = 0;
            foreach (var genItem in PMDataOrdered3_currUsingCycle) //list generations and corresponding weights for the corresponding feature
            {
                string AIActionText = "";
                string itemGenStr1 = "MN: " + genItem.First.movement_thisUpdate_CCWAngleRelativeToMNPos + "; SM: " + genItem.First.movement_thisUpdate_CCWAngleRelativeToSMPos + "; mag: " + genItem.First.movement_thisUpdate_Magnitude + "; "; //AI action <<in string form><YKWIM>>
                string itemGenStr2 = "w: " + genItem.Second + "; shoot: " + genItem.First.isFiring_thisUpdate;

                //TODO since 8/13/18, 11:11PM, with whatever would still: make debug code for these
                //and new version of this add as (with DONE? progress as of 8/15/18, with) TODO (as of 8/13/18, 11:13PM): items_weights_alltogether.Add("Unit "+ AIActionText + ", "/* +nearEnemyCountText+ ", "*/+nearEnemyMaxSpeedText+", "+ eDistanceText+": "); //MarkovUnit based on 
                
                items_weights_alltogether.Add("With: (" + itemGenStr1); //MarkovUnit based on 
                items_weights_alltogether.Add(itemGenStr2+")");
            }

            #endregion
        }
        else
        {
            #region isDistribution
            foreach (var item in PMData)
            {
                if (/*item.Key.items.Length >= 1*//* - there is a state, or there are weights for that state?*/item.Value.Count >= 1)
                {
                    string itemFeatureStr = item.Key;
                    EnemyDistance curreDistance = (EnemyDistance)(itemFeatureStr[0] - '0');
                    char maxSpeed = (char)(itemFeatureStr[1] - '0');

                    string eDistanceText = "";
                    switch (curreDistance)
                    {
                        case EnemyDistance.FAR:
                            eDistanceText = "FAR";
                            break;
                        case EnemyDistance.NEAR:
                            eDistanceText = "NEAR";
                            break;
                    }

                    string nearEnemyMaxSpeedText = "";
                    nearEnemyMaxSpeedText += ((char)(maxSpeed + '0'));

                    //MarkovUnit currItemMarkov = item.Key.items.Last(); //get the last item in the state, if there is one
                    foreach (var genItem in PMData[itemFeatureStr]) //list generations and corresponding weights for the corresponding feature
                    {
                        string AIActionText = "";
                        string itemGenStr = genItem.Key; //AI action in string form
                        Action currAIAction = (Action)(itemGenStr[0] - '0'); //parsed AI action string
                        switch (currAIAction)
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
                        //TODO since 8/13/18, 11:11PM, with whatever would still: make debug code for these
                        //and new version of this add as (with DONE? progress as of 8/15/18, with) TODO (as of 8/13/18, 11:13PM): items_weights_alltogether.Add("Unit "+ AIActionText + ", "/* +nearEnemyCountText+ ", "*/+nearEnemyMaxSpeedText+", "+ eDistanceText+": "); //MarkovUnit based on 

                        items_weights_alltogether.Add("With (" + eDistanceText + "," + nearEnemyMaxSpeedText + "), (" + AIActionText + ") w: "); //MarkovUnit based on 
                        int actionWeight = genItem.Value;
                        items_weights_alltogether.Add(actionWeight.ToString());
                    }
                }
                else
                {
                    items_weights_alltogether.Add("empty PM");
                }
            }
            #endregion
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        prevMovePosition = transform.position; //used at least for collision handling


        if ((Input.GetKeyDown("u")/* || toggleTrainingAndUsing*/) && !usedForLoggingTraining) //toggle training and using - though this would not occur for the training logging Shadow
        {
            isUsing = !isUsing;
            isTraining = !isTraining;

            Debug.Log("Toggled so that " + (isUsing ? "isUsing" : "isTraining") + " is true.");
            toggleTrainingAndUsing = false;
            prevMovementDirTimer = prevMovementDirInterval; //reset movement dir //end of the period for the corresponding movement dir
        }
        if ((Input.GetKeyDown("y") || toggleTrainingAndUsing/* - where the on-screen button would be used for the logging Shadow*/) && usedForLoggingTraining) //toggle training and using
        {
            isUsing = !isUsing;
            isTraining = !isTraining;

            Debug.Log("Toggled so that " + (isUsing ? "isUsing" : "isTraining") + " is true for the logging Shadow.");
            toggleTrainingAndUsing = false;
            prevMovementDirTimer = prevMovementDirInterval; //reset movement dir //end of the period for the corresponding movement dir
        }

        //features as with ShadowAIMarkov (which was the old version of the AI that ShadowPM replaced)
        //with: 
        //-adding weights for eg. features and\or such>
        //  -with noting of such <<with isTraining><YKWIM>>
        //-using such for behaviours
        //  -with noting of such <<with isUsing><YKWIM>>
        //
        if (isTraining && !WaveManager.fadeScreenIsActive && !PlayerHealth.playerIsDead)
        {
            Training();
        }
        //else is being used and/or testing

        //if saving MarkovChain - save on the frame that "m" is first pressed
        if (Input.GetKeyDown("m"))
        {
            savingData = true;
        }

        if (Input.GetKeyDown(";"))
        {
            loadingData = true;
            resetCurrTrainingCycle_withLoadedDataThisUpdate = true;
            resetCurrUsingCycle_withLoadedDataThisUpdate = true;
        }
        if(Input.GetKeyDown("'") && usedForLoggingTraining/* - disabling such for what would be apart from the ShadowCharacter logging training at the moment*/)
        {
            getCurrMovementAngleAndMag = true;
        }

        if (savingData && usedForLoggingTraining/* saving only the Shadow used for training*/) //<<set this><YKWIM>> to true for one data save
        {
            SaveData();
            Debug.Log("Saving data of the PM into serializable data");
        }
        if(savingMovementData && usedForLoggingTraining) //done automatically when a player dies
        {
            PlayerPrefs.SetInt("numPlayerAndAIMovementMatchingUpdates", numPlayerAndAIMovementMatchingUpdates);
            PlayerPrefs.SetInt("numPlayerAndAIShootingMatchingUpdates", numPlayerAndAIShootingMatchingUpdates);
            PlayerPrefs.SetInt("numPlayerAndAIMovingAndShootingMatchingUpdates", numPlayerAndAIMovingAndShootingMatchingUpdates);
            PlayerPrefs.SetInt("numPlayerAndAITotalTestingUpdates", numPlayerAndAITotalTestingUpdates);
            PlayerPrefs.SetFloat("PlayerAndAIMovementAngleDiffTotalInAllUpdates", PlayerAndAIMovementAngleDiffTotalInAllUpdates);

            PlayerPrefs.SetInt("numPlayerAndAIMovementMatchingUpdates_enemiesOnScreen", numPlayerAndAIMovementMatchingUpdates_enemiesOnScreen);
            PlayerPrefs.SetInt("numPlayerAndAIShootingMatchingUpdates_enemiesOnScreen", numPlayerAndAIShootingMatchingUpdates_enemiesOnScreen);
            PlayerPrefs.SetInt("numPlayerAndAIMovingAndShootingMatchingUpdates_enemiesOnScreen", numPlayerAndAIMovingAndShootingMatchingUpdates_enemiesOnScreen);
            PlayerPrefs.SetInt("numPlayerAndAITotalTestingUpdates_enemiesOnScreen", numPlayerAndAITotalTestingUpdates_enemiesOnScreen);
            PlayerPrefs.SetFloat("PlayerAndAIMovementAngleDiffTotalInAllUpdates_enemiesOnScreen", PlayerAndAIMovementAngleDiffTotalInAllUpdates_enemiesOnScreen);
            
            savingMovementData = false;
        }
        
        //if (loadingData_nextUpdate)
        //{
        //}
        if (loadingData)
        {
            LoadData();
        }

        if (isUsing && !WaveManager.fadeScreenIsActive && !PlayerHealth.playerIsDead/* && !usedForLoggingTraining*//* preventing action if it would be the 'training' Shadow*/) //'action' mode as opposed to 'training'
        {
            Using();
        }

        if (WaveManager.DEBUG_ENABLED)
        {
            DebuggingPM();
        }
        if (!WaveManager.fadeScreenIsActive)
        {
            resetMovementDirTimerThisUpdate = false;
        }
    }

    void OnCollisionEnter2D(Collision2D c) //didn't realize the "2D" part of the name ... in doing this ... and such regarding collision ...
    {
        if (c.gameObject.CompareTag("Obstacle") && /*!collisionOccurredThisUpdate*/!collisionsThisUpdate.Contains(c))
        {
            //isColliding = true;
            collisionsThisUpdate.Add(c);

            //test if no more intersection when reversing just x
            thisBoxCollider2D.offset -= new Vector2(prevMoveDelta.x, 0); //set offset - with corresponding reversed position - for testing
            if (!thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, 0); //revert offset
                transform.position -= new Vector3(prevMoveDelta.x, 0, 0);/* //move Rigidbody instead*/
                return;
            }
            else
            {
                thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, 0); //revert offset
            }

            //test if no more collision\intersection when reversing just y
            thisBoxCollider2D.offset -= new Vector2(0, prevMoveDelta.y); //set offset - with corresponding reversed position - for testing
            if (!thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                thisBoxCollider2D.offset += new Vector2(0, prevMoveDelta.y); //revert offset
                transform.position -= new Vector3(0, prevMoveDelta.y, 0); //move Rigidbody instead
                return;
            }

            //x and y reversals alone lead to collision; reverse both
            thisBoxCollider2D.offset -= new Vector2(prevMoveDelta.x, 0);

            //if still colliding despite all reversal of thisBoxCollider2D, set this to true
            if (thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                isColliding = true;
            }
            thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, prevMoveDelta.y);
            transform.position -= new Vector3(prevMoveDelta.x, prevMoveDelta.y, 0);
        }
    }
    void OnCollisionStay2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Obstacle") && /*!collisionOccurredThisUpdate*/!collisionsThisUpdate.Contains(c))
        {
            collisionsThisUpdate.Add(c);

            //test if no more intersection when reversing just x
            thisBoxCollider2D.offset -= new Vector2(prevMoveDelta.x, 0); //set offset - with corresponding reversed position - for testing
            if (!thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, 0); //revert offset
                transform.position -= new Vector3(prevMoveDelta.x, 0, 0);/* //move Rigidbody instead*/
                return;
            }
            else
            {
                thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, 0); //revert offset
            }

            //test if no more collision\intersection when reversing just y
            thisBoxCollider2D.offset -= new Vector2(0, prevMoveDelta.y); //set offset - with corresponding reversed position - for testing
            if (!thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                thisBoxCollider2D.offset += new Vector2(0, prevMoveDelta.y); //revert offset
                transform.position -= new Vector3(0, prevMoveDelta.y, 0); //move Rigidbody instead
                return;
            }

            //x and y reversals alone lead to collision; reverse both
            thisBoxCollider2D.offset -= new Vector2(prevMoveDelta.x, 0);

            //if still colliding despite all reversal of thisBoxCollider2D, set this to true
            if (thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                isColliding = true;
            }
            thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, prevMoveDelta.y);
            transform.position -= new Vector3(prevMoveDelta.x, prevMoveDelta.y, 0);
        }
    }
    void OnCollisionExit2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Obstacle"))
        {
            isColliding = false; //allow movement once collision is exited
        }
    }


#if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        if(!enableOnDrawGizmos)
        {
            return;
        }
        foreach (gizmoSphereProperties currSphereProperties in gizmoSpheresToDraw)
        {
            Gizmos.color = currSphereProperties.color;
            Gizmos.DrawSphere(currSphereProperties.position, currSphereProperties.radius);
        }
        //}
    }
#endif
}
