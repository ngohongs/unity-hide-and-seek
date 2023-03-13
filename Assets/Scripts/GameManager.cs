using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Transform> enemySpawnPoints = new List<Transform>();
    public List<Transform> playerSpawnPoints = new List<Transform>();
    public PlayerController player;
    public Transform enemy;

    public VisualTreeAsset mobileGameUI;

    public UIController uiController;
    public AudioSource audioSource;
    public AudioClip startSound;
    public AudioClip endSound;
    private VisualElement inGameUi = null;

    private Stopwatch stopwatch = new Stopwatch();

    private TimeSpan elapsedTime;

    public TimeSpan result { get; private set; }

    public bool isRunning { get; private set; } = false;
    public bool isPaused { get; private set; } = false;
    public bool isEnd { get; private set; } = false;

    public int preTimer = 5;
    void OnEnable()
    {
        UIDocument doc = GetComponent<UIDocument>();
        
        if (Application.platform == RuntimePlatform.Android)
        {
            doc.visualTreeAsset = mobileGameUI;
        }

        inGameUi = GetComponent<UIDocument>().rootVisualElement;
    }

    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        Label label = inGameUi.Q<Label>("Timer");
        string time = stopwatch.Elapsed.ToString("mm\\:ss\\:ff");

        if (!isRunning)
        {
            TimeSpan remainingTime = new TimeSpan(0, 0, preTimer);
            remainingTime = remainingTime.Subtract(stopwatch.Elapsed);
            if (remainingTime.Milliseconds < 0)
            {
                isRunning = true;
                label.style.color = Color.white;
                audioSource.PlayOneShot(startSound);
                UnpauseGame();
                stopwatch.Restart();
            }
            else
            {
                label.style.color = Color.red;
                time = remainingTime.ToString("mm\\:ss\\:ff");
            }
        }
        
        
        label.text = time;
    }


    void StartGame()
    {
        isPaused = false;
        isEnd = false;
        isRunning = false;
        stopwatch.Restart();
        int playerSpawnPoint = Random.Range(0, playerSpawnPoints.Count);
        int enemySpawnPoint = Random.Range(0,enemySpawnPoints.Count);
        player.transform.position = playerSpawnPoints[playerSpawnPoint].position;
        enemy.transform.position = enemySpawnPoints[enemySpawnPoint].position;
        stopwatch.Start();
        PauseGame();
    }

    public void RestartGame()
    {
        player.Reset();
        StartGame();
    }

    public void EndGame()
    {
        audioSource.PlayOneShot(endSound);
        isEnd = true;
        stopwatch.Stop();
        UserPauseOnOff();
        result = stopwatch.Elapsed;
        string text = stopwatch.Elapsed.ToString("mm\\:ss\\:ff");
        uiController.Show(UIController.UIType.EndGame, text);
    }

    public void UserPauseOnOff()
    {
        if (!isPaused)
        {
            isPaused = true;
            stopwatch.Stop();
            PauseGame();
        }
        else
        {
            isPaused = false;
            stopwatch.Start();
            if (isRunning)
                UnpauseGame();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;
    }

    void UnpauseGame()
    {
        Time.timeScale = 1;
    }
}
