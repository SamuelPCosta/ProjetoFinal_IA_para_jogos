using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private int frameRate = 60;
    [Space(10)]
    [Header("Game attributes")]
    private PlayerController playerController;
    [SerializeField] private int damage = 2;
    [SerializeField] SceneAsset GameWin_GameOver;
    [SerializeField] SceneAsset Phase1;
    private bool gameEnding = false;

    private bool win = false;

    static GameController instance;

    void Awake(){
        if (instance != null){
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;
    }

    private void Update(){
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (playerController != null && playerController.getLife() <= 0)
        {
            endGame(false);
        }
        if (SceneManager.GetActiveScene().name == GameWin_GameOver.name && 
           (Input.GetKeyDown(KeyCode.Return) ||
           Input.GetKeyDown(KeyCode.Space) ||
           Input.GetKeyDown(KeyCode.JoystickButton0))) {
           SceneManager.LoadScene(Phase1.name);
           win = false;
        }
    }

    public void endGame(bool status)
    {
        if (gameEnding) return;
        gameEnding = true;

        if (!status)
            print("Death");
        else
            print("Win");

        win = status;
        SceneManager.LoadScene(GameWin_GameOver.name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != GameWin_GameOver.name)
        {
            gameEnding = false;
            playerController = FindObjectOfType<PlayerController>();
            return;
        }

        GameObject panel = GameObject.Find("EndPanel");
        panel.transform.GetChild(0).gameObject.SetActive(win);
        panel.transform.GetChild(1).gameObject.SetActive(!win);
    }
}
