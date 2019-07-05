using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class TitleMenuButton : MonoBehaviour, IPointerClickHandler
{
    public AudioSource ButtonAudioSource;
    public AudioClip ButtonClickSound;

    // Use this for initialization
    private void Awake()
    {
        switch (name)
        {
            case "StartGameButton":
                if (PlayerPrefs.HasKey("currentLevel")) //if leading to creating a new game
                {
                    GetComponentInChildren<Text>().text = "New Game";
                }
                break;
            case "ContinueButton":
                if (!PlayerPrefs.HasKey("currentLevel"))
                {
                    transform.localScale = Vector3.zero; //hide if there would not be a game currently in progress
                }
                break;
            default:
                break;
        }
    }
    void Start () {
        ButtonAudioSource = GameObject.Find("SingletonSoundAudioSource").GetComponent<AudioSource>(); //update audio source of button
    }
	
	// Update is called once per frame
	void Update () {

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (name)
        {
            case "ContinueButton":
                if (PlayerPrefs.HasKey("currentLevel"))
                {
                    OnLoadTransition.LoadScene("GameplayScene");
                    WaveManager.isNewGame = false;
                }
                break;
            case "StartGameButton": //is also New Game button
                if (!(PlayerPrefs.GetInt("ShadowModeUnlocked") == 1 || WaveManager.DEBUG_SHADOWAITESTING))
                {
                    if (PlayerPrefs.HasKey("currentLevel")) //where "New Game" would be the button
                    {
                        SceneManager.LoadScene("ConfirmNewGame");
                    }
                    else
                    {
                        WaveManager.isNewGame = true;
                        OnLoadTransition.LoadScene("GameplayScene");
                    }
                }
                else
                {
                    SceneManager.LoadScene("SelectModeScene");
                }
                break;
            case "OptionsButton":
                SceneManager.LoadScene("OptionsMenuScene");
                break;
            case "CongratulationsEndButton":
                OnLoadTransition.LoadScene("GameplayScene");
                break;
            case "ToMainMenuButton":
                SceneManager.LoadScene("TitleScreenScene");
                break;
            case "NormalModeButton":
                WaveManager.isShadowMode = false;
                WaveManager.isNewGame = true;
                OnLoadTransition.LoadScene("GameplayScene");
                break;
            case "ShadowModeButton":
                WaveManager.isShadowMode = true;
                WaveManager.isNewGame = true;
                OnLoadTransition.LoadScene("GameplayScene");
                break;
            case "BackButton":
                SceneManager.LoadScene("TitleScreenScene");
                break;
            case "GameOverEndGameButton":
                OnLoadTransition.LoadScene("TitleScreenScene");
                break;
            case "GameOverContinueButton":
                //reset health, and Shadow health if such would exist, and add to death counter
                WaveManager.continueFromGameOver = true;
                PlayerHealth.continueCount++;
                ContinuesUsedText.updateText = true;
                Debug.Log("updateText set to true if so");

                OnLoadTransition.LoadScene("GameplayScene");
                WaveManager.isNewGame = false;
                break;
            case "ResetAIButton":
                SceneManager.LoadScene("ConfirmResetAI");
                break;
            case "ConfirmResetAIButton":
                //Reset AI - with initial values of the PMs and such here
                Dictionary<string, Dictionary<string, int>> PMData = new Dictionary<string, Dictionary<string, int>>();
                Dictionary<string, List<KeyValuePair<string, int>>> PMData_maintainOrder = new Dictionary<string, List<KeyValuePair<string, int>>>();
                Dictionary<string, List</*Tuple<*/List<KeyValuePair<string, int>>/*, int>*/>> PMData_ofmaintainOrderCycles = new Dictionary<string, List</*Tuple<*/List<KeyValuePair<string, int>>/*, int>*/>>();
                Dictionary<string, List<List<ShadowPM.Pair<ShadowPM.genBehaviourData, float>>>> PMDataOrdered3_ofFixedLengthCycles = new Dictionary<string, List<List<ShadowPM.Pair<ShadowPM.genBehaviourData, float>>>>();

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
                SceneManager.LoadScene("OptionsMenuScene");
                break;
            case "BackToOptionsButton":
                SceneManager.LoadScene("OptionsMenuScene");
                break;
            default:
                break;
        }
        ButtonAudioSource.clip = ButtonClickSound;
        ButtonAudioSource.PlayOneShot(ButtonClickSound);
    }
}
