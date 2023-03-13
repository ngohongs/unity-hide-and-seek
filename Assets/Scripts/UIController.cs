using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    private UIDocument document;
    public AudioMixer audioMixer;
    private AudioSource audioPlayer;

    public GameManager gameManager;

    public Font font;

    [Header("Desktop UI")]
    public VisualTreeAsset mainMenuUI;
    public VisualTreeAsset leaderboardUI;
    public VisualTreeAsset settingsUI;
    public VisualTreeAsset gameMenuUI;
    public VisualTreeAsset endGameUI; 

    [Header("Mobile UI")]
    public VisualTreeAsset mobileMainMenuUI;
    public VisualTreeAsset mobileLeaderboardUI;
    public VisualTreeAsset mobileSettingsUI;
    public VisualTreeAsset mobileGameMenuUI;
    public VisualTreeAsset mobileEndGameUI;

    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference rollAction;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;


    private UIType previous;
    private UIType current;
    public enum UIType { MainMenu, Settings, GameMenu, EndGame, Leaderboard};

    
    // -- Main Menu UI --
    private Button playButton;
    private Button leaderboardButton;
    private Button settingsButton;
    private Button exitButton;

    // -- Settings UI --
    private DropdownField resolutionDropdownField;

    private Slider volumeSlider;

    private Button keyForwardButton;
    private Button keyBackwardButton;
    private Button keyLeftButton;
    private Button keyRightButton;
    private Button keyJumpButton;
    private Button keyRollButton;

    private Button settingsBackButton;

    // -- Leaderboard UI --
    private ListView board;
    private Button backLeadButton;
    private Button resetButton;

    // -- Game Menu UI --
    private Button resumeButton;
    private Button restartButton;
    private Button settingsGameButton;
    private Button exitGameButton;

    // -- End Game Menu UI --
    private Label timeLabel;
    private Button restartEndButton;
    private TextField nameField;
    private Button exitEndButton;

    private string lastTime = "";

    private void OnEnable()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            mainMenuUI = mobileMainMenuUI;
            leaderboardUI = mobileLeaderboardUI;
            settingsUI = mobileSettingsUI;
            gameMenuUI = mobileGameMenuUI;
            endGameUI = mobileEndGameUI;
        }
    
        document = GetComponent<UIDocument>();
        if (gameManager == null)
            Show(UIType.MainMenu);
    }

    public void Awake()
    {
        audioPlayer = GameObject.FindGameObjectWithTag("Click").GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
     
    }

    public void HideUI()
    {
        document.visualTreeAsset = null;
    }

    public void Show(UIType type, string text = "")
    {
        previous = current;

        switch (type)
        {
            case UIType.MainMenu: SetupMainMenuUI(); break;
            case UIType.Settings: SetupSettingsUI(); break;
            case UIType.GameMenu: SetupGameMenuUI(); break;
            case UIType.EndGame: SetupEndGameUI(text); break;
            case UIType.Leaderboard: SetupLeaderboardUI(); break;
            default: break;
        }
        current = type;
    }

    void SetupLeaderboardUI()
    {
        document.visualTreeAsset = leaderboardUI;

        var json = PlayerPrefs.GetString("Data");
        var scores = JsonConvert.DeserializeObject<List<ResultEntry>>(json);
        if (scores != null)
            scores = scores.OrderBy(s => s.time).ToList();

        board = document.rootVisualElement.Q<ListView>("scores");

        board.itemsSource = scores;
        board.makeItem = () => {

            var cont = new VisualElement();
            cont.style.flexDirection = FlexDirection.Row;
            cont.style.justifyContent = Justify.SpaceBetween;
            var label1 = new Label();
            var label2 = new Label();

            label1.style.fontSize = Application.platform == RuntimePlatform.Android ? 8 : 30;
            label1.style.unityFont = font;
            label1.style.unityFontDefinition = new StyleFontDefinition();
            label2.style.fontSize = Application.platform == RuntimePlatform.Android ? 8 : 30;
            label2.style.unityFont = font;
            label2.style.unityFontDefinition = new StyleFontDefinition();
  
            cont.Add(label1);
            cont.Add(label2);

            return cont;
            };

        board.bindItem = (e, i) =>
        {
            var cont = (e as VisualElement);
            (cont.Children().First() as Label).text = (i + 1).ToString() + ". " + scores[i].name;
            (cont.Children().Last() as Label).text = scores[i].time.ToString("mm\\:ss\\:ff");
        };

        resetButton = document.rootVisualElement.Q<Button>("reset");
        backLeadButton = document.rootVisualElement.Q<Button>("back");

        backLeadButton.clicked += SettingsBackButtonClicked;
        backLeadButton.clicked += ClickSound;
        resetButton.clicked += () =>
        {
            ClickSound();
            PlayerPrefs.SetString("Data", "");
            SetupLeaderboardUI();
        };
    }

    void SetupGameMenuUI()
    
    {
        document.visualTreeAsset = gameMenuUI;

        resumeButton = document.rootVisualElement.Q<Button>("resume");
        restartButton = document.rootVisualElement.Q<Button>("restart");
        exitGameButton = document.rootVisualElement.Q<Button>("exit");

        resumeButton.clicked += () => { ClickSound(); HideUI(); gameManager.UserPauseOnOff(); };
        restartButton.clicked += RestartButtonClicked;
        restartButton.clicked += ClickSound;
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            settingsGameButton = document.rootVisualElement.Q<Button>("settings");
            settingsGameButton.clicked += SettingsButtonClicked;
            settingsGameButton.clicked += ClickSound;
        }
        exitGameButton.clicked += () => { ClickSound(); gameManager.UserPauseOnOff();  SceneManager.LoadScene("Menu"); };
    }

    void SetupEndGameUI(string text)
    {
        document.visualTreeAsset = endGameUI;

        timeLabel = document.rootVisualElement.Q<Label>("time");
        nameField = document.rootVisualElement.Q<TextField>("name");
        restartEndButton = document.rootVisualElement.Q<Button>("restart");
        exitEndButton = document.rootVisualElement.Q<Button>("exit");

        timeLabel.text = text == "" ? lastTime : text;
        if (text != "")
            lastTime = text;

        restartEndButton.clicked += RestartButtonClicked;
        restartEndButton.clicked += ClickSound;
        restartEndButton.clicked += () => SaveResult();
        exitEndButton.clicked += () => { ClickSound(); gameManager.UserPauseOnOff(); SceneManager.LoadScene("Menu"); };
        exitEndButton.clicked += () => SaveResult();
    }

    private void SaveResult()
    {
        ResultEntry entry = new ResultEntry();
        entry.name = nameField.text == "Enter your name here" ? "Anonymous" : nameField.text;
        entry.time = gameManager.result;

        string json = PlayerPrefs.GetString("Data");
        var list = JsonConvert.DeserializeObject<List<ResultEntry>>(json);
        if (list == null)
            list = new List<ResultEntry>();
        list.Add(entry);
        var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);
        PlayerPrefs.SetString("Data", convertedJson);
        PlayerPrefs.Save();
    }

    void SetupMainMenuUI()
    {
        Time.timeScale = 1.0f;
        document.visualTreeAsset = mainMenuUI;

        playButton = document.rootVisualElement.Q<Button>("play");
        leaderboardButton = document.rootVisualElement.Q<Button>("leaderboard");
        
        exitButton = document.rootVisualElement.Q<Button>("exit");

        playButton.clicked += PlayButtonClicked;
        playButton.clicked += ClickSound;
        leaderboardButton.clicked += LeaderboardButtonClicked;
        leaderboardButton.clicked += ClickSound;

        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            settingsButton = document.rootVisualElement.Q<Button>("settings");
            settingsButton.clicked += SettingsButtonClicked;
            settingsButton.clicked += ClickSound;
        }
        exitButton.clicked += ExitButtonClicked;
        exitButton.clicked += ClickSound;
    }

    void SetupSettingsUI()
    {
        document.visualTreeAsset = settingsUI;

        resolutionDropdownField = document.rootVisualElement.Q<DropdownField>("resolution");
        resolutionDropdownField.choices = new List<string>();
        foreach (var resolution in Screen.resolutions.Reverse())
        {
            resolutionDropdownField.choices.Add(resolution.ToString());
            if (resolution.ToString() == Screen.currentResolution.ToString())
            {
                resolutionDropdownField.value = resolution.ToString();
            }
        }
        resolutionDropdownField.RegisterValueChangedCallback(ResolutionChanged);

        volumeSlider = document.rootVisualElement.Q<Slider>("volume");
        volumeSlider.RegisterValueChangedCallback(VolumeChanged);
        audioMixer.GetFloat("MasterVolume", out float vol);
        volumeSlider.value = Mathf.Pow(10.0f, vol / 20.0f) * 100.0f; 

        keyForwardButton = document.rootVisualElement.Q<Button>("forward");
        keyBackwardButton = document.rootVisualElement.Q<Button>("backward");
        keyLeftButton = document.rootVisualElement.Q<Button>("left"); 
        keyRightButton = document.rootVisualElement.Q<Button>("right");
        keyJumpButton = document.rootVisualElement.Q<Button>("jump");
        keyRollButton = document.rootVisualElement.Q<Button>("roll");

        var index = moveAction.action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "up");
        keyForwardButton.text = InputControlPath.ToHumanReadableString(moveAction.action.bindings[index].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        index = moveAction.action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "down");
        keyBackwardButton.text = InputControlPath.ToHumanReadableString(moveAction.action.bindings[index].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        index = moveAction.action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "left");
        keyLeftButton.text = InputControlPath.ToHumanReadableString(moveAction.action.bindings[index].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        index = moveAction.action.bindings.IndexOf(x => x.isPartOfComposite && x.name == "right");
        keyRightButton.text = InputControlPath.ToHumanReadableString(moveAction.action.bindings[index].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        keyJumpButton.text = InputControlPath.ToHumanReadableString(jumpAction.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        keyRollButton.text = InputControlPath.ToHumanReadableString(rollAction.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

        keyForwardButton.clicked += KeyForwardClicked;
        keyForwardButton.clicked += ClickSound;
        keyBackwardButton.clicked += KeyBackwardClicked;
        keyBackwardButton.clicked += ClickSound;
        keyLeftButton.clicked += KeyLeftClicked;
        keyLeftButton.clicked += ClickSound;
        keyRightButton.clicked += KeyRightClicked;
        keyRightButton.clicked += ClickSound;
        keyJumpButton.clicked += KeyJumpButtonClicked;
        keyJumpButton.clicked += ClickSound;
        keyRollButton.clicked += KeyRollClicked;
        keyRollButton.clicked += ClickSound;

        settingsBackButton = document.rootVisualElement.Q<Button>("back");
        settingsBackButton.clicked += SettingsBackButtonClicked;
        settingsBackButton.clicked += ClickSound;
    }

    private void ResolutionChanged(ChangeEvent<string> evt)
    {
        foreach (var resolution in Screen.resolutions.Reverse())
        {
            if (resolution.ToString() == evt.newValue)
            {
                Screen.SetResolution(resolution.width, resolution.height, true, resolution.refreshRate);
            }
        }
    }

    private void VolumeChanged(ChangeEvent<float> evt)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(evt.newValue/ 100.0f) * 20);
    }

    private void KeyRollClicked()
    {
        Rebind(rollAction, keyRollButton);
    }

    private void KeyJumpButtonClicked()
    {
        Rebind(jumpAction, keyJumpButton);
    }

    private void KeyRightClicked()
    {
        Rebind(moveAction, keyRightButton, "right");

    }

    private void KeyLeftClicked()
    {
        Rebind(moveAction, keyLeftButton, "left");
    }

    private void KeyBackwardClicked()
    {
        Rebind(moveAction, keyBackwardButton, "down");
    }

    private void KeyForwardClicked()
    {
        Rebind(moveAction, keyForwardButton, "up");
    }

    private void Rebind(InputActionReference action, Button button, string compositeIndex = "")
    {
        action.action.Disable();
        if (compositeIndex == "")
            rebindingOperation = action.action.PerformInteractiveRebinding().WithControlsExcluding("Mouse").WithControlsExcluding("<Keyboard>/escape").OnComplete(o => RebindingComplete(action, button)).Start();
        else
        {
            var bindingIndex = action.action.bindings.IndexOf(x => x.isPartOfComposite && x.name == compositeIndex);
            rebindingOperation = action.action.PerformInteractiveRebinding().
                WithTargetBinding(bindingIndex).WithControlsExcluding("Mouse").WithControlsExcluding("<Keyboard>/escape").
                OnComplete(o => RebindingComplete(action,button, bindingIndex)).
                OnCancel(o => RebindingComplete(action,button)).
                Start();
        }
        button.text = "Press a key";
        DisableSettingsUI();
    }

    private void RebindingComplete(InputActionReference action, Button button, int compositeIndex = 0)
    {
        rebindingOperation.Dispose();
        button.text = InputControlPath.ToHumanReadableString(action.action.bindings[compositeIndex].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        action.action.Enable();
        EnableSettingsUI();
    }

    private void DisableSettingsUI()
    {
        resolutionDropdownField.SetEnabled(false);
        volumeSlider.SetEnabled(false);
        keyForwardButton.SetEnabled(false);
        keyBackwardButton.SetEnabled(false);
        keyLeftButton.SetEnabled(false);
        keyRightButton.SetEnabled(false);
        keyJumpButton.SetEnabled(false); ;
        keyRollButton.SetEnabled(false);
    }

    private void EnableSettingsUI()
    {
        resolutionDropdownField.SetEnabled(true);
        volumeSlider.SetEnabled(true);
        keyForwardButton.SetEnabled(true);
        keyBackwardButton.SetEnabled(true);
        keyLeftButton.SetEnabled(true);
        keyRightButton.SetEnabled(true);
        keyJumpButton.SetEnabled(true); ;
        keyRollButton.SetEnabled(true);
    }

    public void PlayButtonClicked()
    {
        SceneManager.LoadScene("HideAndSeek");
    }

    void LeaderboardButtonClicked()
    {
        Show(UIType.Leaderboard);
    }

    void SettingsButtonClicked()
    {
        Show(UIType.Settings);
    }

    void ExitButtonClicked()
    {
        Application.Quit();
    }

    void RestartButtonClicked()
    {
        HideUI();
        gameManager.UserPauseOnOff();
        gameManager.RestartGame();
    }

    void SettingsBackButtonClicked()
    {
        Show(previous);
    }

    void ClickSound()
    {
        audioPlayer.Play();
    }

    public void Back()
    {
        if (current == UIType.MainMenu)
            return;
        Show(previous);
    }
}
