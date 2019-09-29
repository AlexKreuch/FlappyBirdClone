#define TESTING_MODE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PausePanelController : MonoBehaviour
{
    private const string ShowInstLabel = "show\ninstructions";
    private const string HideInstLabel = "hide\ninstructions";

    private enum Medal
    {
        WHITE = FlappyBirdUtil.Flags.Medals.White,
        ORANGE = FlappyBirdUtil.Flags.Medals.Orange,
        GOLD = FlappyBirdUtil.Flags.Medals.Gold
    }

    public enum MODE { PAUSED , GAMEOVER }

    public static PausePanelController instance = null;

    #region PausePanelFields
    private Text highScoreDisplay = null;
    private Text scoreDisplay = null;
    private Text gameOverDisplay = null;
    private Image medalDisplay = null;
    private Image instructionsDisplay = null;
    private Button PlayButton = null;
    private Button MainMenuButton = null;
    private Button InstructionsButton = null;
    private Text InstructionsButtonText = null;
    #endregion

    private SpriteResource.MedalSpriteSet? medalSprites = null;

    #region button-handlers
    private void PlayButtonHandler()
    {
        Debug.Log("play-btn : "+count++);
        switch (currentMode)
        {
            case MODE.PAUSED: resumeGameAction(); break;
            case MODE.GAMEOVER: restartGameAction(); break;
        }
    }
    private void MainMenuButtonHandler() { Debug.Log("menu-btn : " + count++); }
    private void InstructionsButtonHandler()
    {
        Debug.Log("instr-btn : " + count++);
        if (InstructionsButtonText.text == ShowInstLabel)
        {
            InstructionsButtonText.text = HideInstLabel;
            Color tmp = instructionsDisplay.color;
            tmp.a = 1f;
            instructionsDisplay.color = tmp;
        }
        else
        {
            InstructionsButtonText.text = ShowInstLabel;
            Color tmp = instructionsDisplay.color;
            tmp.a = 0f;
            instructionsDisplay.color = tmp;
        }
    }
    #endregion

    private void SetUp()
    {
        Debug.Log("pause-panel-setup : " + count++);
        instance = this;
        var texts = GetComponentsInChildren<Text>();
        foreach (var txt in texts)
            switch (txt.name)
            {
                case FlappyBirdUtil.Names.PausePanelFields.ScoreTxtDisplay:scoreDisplay = txt; break;
                case FlappyBirdUtil.Names.PausePanelFields.HighScoreTxtDisplay: highScoreDisplay = txt; break;
                case FlappyBirdUtil.Names.PausePanelFields.GameOverTxt: gameOverDisplay = txt; MainTainGameOverVisible(); break;
            }
        var images = GetComponentsInChildren<Image>();
        foreach (var img in images)
            switch (img.name)
            {
                case FlappyBirdUtil.Names.PausePanelFields.MedalImageDisplay:medalDisplay = img; break;
                case FlappyBirdUtil.Names.PausePanelFields.InstructionsImage:instructionsDisplay = img; break;
            }
        var buttons = GetComponentsInChildren<Button>();
        foreach(var btn in buttons)
            switch (btn.name)
            {
                case FlappyBirdUtil.Names.PausePanelFields.MainMenuBtn: MainMenuButton = btn; MainMenuButton.onClick.AddListener(MainMenuButtonHandler); break;
                case FlappyBirdUtil.Names.PausePanelFields.PlayBtn: PlayButton = btn; PlayButton.onClick.AddListener(PlayButtonHandler); break;
                case FlappyBirdUtil.Names.PausePanelFields.InstructionBtn:
                    InstructionsButton = btn;
                    InstructionsButton.onClick.AddListener(InstructionsButtonHandler);
                    InstructionsButtonText = InstructionsButton.gameObject.GetComponentInChildren<Text>();
                    InstructionsButtonText.text = ShowInstLabel;
                    break;
            }

        medalSprites = Resources.Load<SpriteResource>(FlappyBirdUtil.ResourcePaths.SpriteRec).GetMedalSprites();
    }


    int count = 0;
    void OnEnable()
    {
        Debug.Log("pause-panel-enable : " + count++);
        if (instance == null) { SetUp(); gameObject.SetActive(false); }
    }


    public bool PanelTurnedOn
    {
        get { return gameObject.activeSelf; }
        set { gameObject.SetActive(value); }
    }
    public void SetScore(int val) { scoreDisplay.text = val.ToString(); }
    public void SetHighScore(int val) { highScoreDisplay.text = val.ToString(); }
    public void SetMedal(int medalFlag)
    {
        switch((Medal)medalFlag)
        {
            case Medal.WHITE: medalDisplay.sprite = medalSprites.Value.White; break;
            case Medal.ORANGE: medalDisplay.sprite = medalSprites.Value.Orange; break;
            case Medal.GOLD: medalDisplay.sprite = medalSprites.Value.Orange; break;
        }
    }

    public void SetResumeGameAction(Action action)
    {
        if (action == null) action = () => { };
        resumeGameAction = action;
    }
    public void SetRestartGameAction(Action action)
    {
        if (action == null) action = () => { };
        restartGameAction = action;
    }
    public void AddMenuButtonListener(Action action) { MainMenuButton.onClick.AddListener(new UnityEngine.Events.UnityAction(action)); }


    private Action resumeGameAction = () => { };
    private Action restartGameAction = () => { };

    [SerializeField]
    private MODE currentMode = MODE.PAUSED;
    
    private void MainTainGameOverVisible()
    {
        if (gameOverDisplay == null) return;
        Color tmp = gameOverDisplay.color;
        tmp.a = (currentMode==MODE.GAMEOVER) ? 1f : 0f;
        gameOverDisplay.color = tmp;
    }
    public MODE Mode
    {
        get { return currentMode; }
        set { currentMode = value; MainTainGameOverVisible(); }
    }


 

#if TESTING_MODE
    private MODE savedMode = MODE.PAUSED;
    private void CheckMode()
    {
        if (savedMode != currentMode)
        {
            savedMode = currentMode;
            MainTainGameOverVisible();
        }
    }
    void Update() { CheckMode(); }
#endif
}
