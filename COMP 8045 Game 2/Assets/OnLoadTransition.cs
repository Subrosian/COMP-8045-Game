using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Loading code based on https://www.windykeep.com/2018/02/15/make-loading-screen-unity/

// Basic loading screen - call whenever scene loading is called, and whenever e.g. AI would be loaded and/or saved
public class OnLoadTransition : MonoBehaviour {

    public static OnLoadTransition Instance;

    //// The reference to the current loading operation running in the background: //omitting this part for simplification
    //private AsyncOperation currentLoadingOperation;

    public static void Hide()
    {
        Instance.GetComponent<CanvasGroup>().alpha = 0;
        Debug.Log("OnLoadTransition.Hide();");
    }
    public static void Show()
    {
        Instance.GetComponent<CanvasGroup>().alpha = 1;
        Instance.GetComponentInChildren<Text>().text = "Loading...";
        Debug.Log("OnLoadTransition.Show();");
    }
    public static void Show(string showText)
    {
        Instance.GetComponent<CanvasGroup>().alpha = 1;
        Instance.GetComponentInChildren<Text>().text = showText;
        Debug.Log("OnLoadTransition.Show(string showText);");
    }

    public static void LoadScene(string sceneName)
    {
        Show();
        SceneManager.LoadScene(sceneName);
    }

    private void Awake()
    {
        // Singleton logic:
        if (Instance == null)
        {
            Instance = this;
            Hide(); //hide the available instance - which would be this instance
            // Don't destroy the loading screen while switching scenes:
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Hide(); //hide the available instance
            Destroy(gameObject);
            return;
        }
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
