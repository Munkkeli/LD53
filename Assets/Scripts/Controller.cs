using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Controller : MonoBehaviour {
    public static Controller Current;
    
    public Transform Canvas;
    public Transform EventSystem;
    public GameObject StartScreen;
    public GameObject TutorialScreen;
    public GameObject LevelCompleteScreen;
    public GameObject WinScreen;
    public GameObject EndScreen;
    public GameObject Car;
    public GameObject DeliveryCar;
    public GameState state = GameState.Start;
    public int stageIndex;

    private Button _startButton;
    private Button _goButton;
    private Button _nextButton;
    private Button _restartButton;
    private Button _resetButton;

    public enum GameState {
        Start,
        Tutorial,
        Running,
        Complete,
        Win,
        End,
    }

    public Text goalText;

    public string[] Scenes;

    public Color[] houseColors;
    private int _houseIndex;

    public Dictionary<CarType, GameObject> CarTypeToPrefab;

    public enum CarType {
        AI,
        Delivery
    }

    public Queue<CarType> _spawnQueue = new();
    public float _spawnTimer = 1f;
    public int _deliveryDelay;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(Camera.main.gameObject);
        DontDestroyOnLoad(Canvas.gameObject);
        DontDestroyOnLoad(EventSystem.gameObject);
        Current = this;

        CarTypeToPrefab = new Dictionary<CarType, GameObject> {
            {CarType.AI, Car},
            {CarType.Delivery, DeliveryCar}
        };
    }

    private void Start() {
        _startButton = StartScreen.GetComponentInChildren<Button>();
        _goButton = TutorialScreen.GetComponentInChildren<Button>();
        _nextButton = LevelCompleteScreen.GetComponentInChildren<Button>();
        _restartButton = EndScreen.GetComponentInChildren<Button>();
        _resetButton = WinScreen.GetComponentInChildren<Button>();
        
        _startButton.onClick.AddListener(() => { state = GameState.Tutorial; });
        _goButton.onClick.AddListener(() => { LoadScene(); });
        _nextButton.onClick.AddListener(() => { LoadScene(); });
        _restartButton.onClick.AddListener(() => { LoadScene(SceneManager.GetActiveScene().name); });
        _resetButton.onClick.AddListener(() => { Restart(); });
    }

    private void LoadScene(string scene = null) {
        Debug.Log(Scenes[stageIndex]);
        Config.Current = null;
        _houseIndex = 0;
        var loadScene = scene ?? Scenes[stageIndex];
        SceneManager.LoadScene(loadScene, LoadSceneMode.Single);
        state = GameState.Running;
        if (scene == null) stageIndex++;
    }
    
    private void Restart() {
        Config.Current = null;
        Config.Current.currentDeliveries = 0;
        _houseIndex = 0;
        stageIndex = 0;
        state = GameState.Start;
    }

    private void Update() {
        var startState = state == GameState.Start;
        var tutorialState = state == GameState.Tutorial;
        var completeState = state == GameState.Complete;
        var winState = state == GameState.Win;
        var endState = state == GameState.End;
        
        if (StartScreen.activeSelf != startState) StartScreen.SetActive(startState);
        if (TutorialScreen.activeSelf != tutorialState) TutorialScreen.SetActive(tutorialState);
        if (LevelCompleteScreen.activeSelf != completeState) LevelCompleteScreen.SetActive(completeState);
        if (WinScreen.activeSelf != winState) WinScreen.SetActive(winState);
        if (EndScreen.activeSelf != endState) EndScreen.SetActive(endState);
        
        if (completeState) return;
        if (winState) return;
        if (endState) return;

        var config = Config.Current;
        if (!config) return;

        if (_spawnTimer <= 0 && _spawnQueue.Count < 5) {
            _spawnQueue.Enqueue(_deliveryDelay <= 0 ? CarType.Delivery : CarType.AI);
            var maxDelay = 60f / config.maximumSpawnRateInMinute;
            _spawnTimer = maxDelay - (config.spawnRateOverTime.Evaluate(Time.time / (60 * config.estimatedLevelDurationInMinutes)) * maxDelay);
            if (_deliveryDelay <= 0) {
                _deliveryDelay =
                    config.maximumDeliveryDelay - (int)(config.deliverySpawnRateOverTime.Evaluate(Time.time / (60 * config.estimatedLevelDurationInMinutes)) * config.maximumDeliveryDelay);
            }
            _deliveryDelay--;
        }

        _spawnTimer -= Time.deltaTime;

        goalText.text = $"{config.currentDeliveries} / {config.targetDeliveries}";

        if (config.currentDeliveries >= config.targetDeliveries) {
            var isWin = stageIndex + 1 >= Scenes.Length;
            state = isWin ? GameState.Win : GameState.Complete;
        }
    }

    public (string, Color) GetNextHouseBranding() {
        string alphabet = "ABCDEFG";
        string letter = "" + alphabet[_houseIndex];
        Color color = houseColors[_houseIndex];
        _houseIndex++;
        return (letter, color);
    }
}
