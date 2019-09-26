﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePanelController : MonoBehaviour
{
    private enum Medal
    {
        WHITE = FlappyBirdUtil.Flags.Medals.White,
        ORANGE = FlappyBirdUtil.Flags.Medals.Orange,
        GOLD = FlappyBirdUtil.Flags.Medals.Gold
    }

    public static PausePanelController instance = null;

    #region PausePanelFields
    private Text highScoreDisplay = null;
    private Text scoreDisplay = null;
    private Image medalDisplay = null;
    private Button PlayButton = null;
    private Button MainMenuButton = null;
    #endregion

    private SpriteResource.MedalSpriteSet? medalSprites = null;

    #region button-handlers
    private void PlayButtonHandler() { Debug.Log("play-btn : "+count++); }
    private void MainMenuButtonHandler() { Debug.Log("menu-btn : " + count++); }
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
            }
        var images = GetComponentsInChildren<Image>();
        foreach (var img in images)
            switch (img.name)
            {
                case FlappyBirdUtil.Names.PausePanelFields.MedalImageDisplay:medalDisplay = img; break;
            }
        var buttons = GetComponentsInChildren<Button>();
        foreach(var btn in buttons)
            switch (btn.name)
            {
                case FlappyBirdUtil.Names.PausePanelFields.MainMenuBtn: MainMenuButton = btn; MainMenuButton.onClick.AddListener(MainMenuButtonHandler); break;
                case FlappyBirdUtil.Names.PausePanelFields.PlayBtn: PlayButton = btn; PlayButton.onClick.AddListener(PlayButtonHandler); break;
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
}
