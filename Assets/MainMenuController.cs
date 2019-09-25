using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

public class MainMenuController : MonoBehaviour
{
    int reportCount = 0;
    private void report([CallerLineNumber]int ln = 0, [CallerMemberName] string nm = "")
    {
        Debug.Log(string.Format("({0})<{1}|line:{2}>",reportCount++,nm,ln));
    }

    private void SetUpBirdPicker()
    {
        Debug.Log("MainMenuController-SetUpBirdPicker");
        char brd = 'B';
        bool r = false, g = false, b = false;
        GameController.MMPort.GetBirdPickerSetUp(ref brd, ref r, ref g, ref b);
        BirdPicker.instance.SetUnlockedOption('R',r);
        BirdPicker.instance.SetUnlockedOption('G', g);
        BirdPicker.instance.SetUnlockedOption('B', b);
        BirdPicker.instance.SetChoice(brd);
    }
    private void SaveBirdChoice()
    {
        GameController.MMPort.ReportNewBirdSelection(BirdPicker.instance.GetChoice());
    }
    private void SetUpButtons()
    {
        var buttons = FindObjectsOfType<Button>();
        foreach (var button in buttons)
        {
            switch (button.tag)
            {
                case FlappyBirdUtil.Tags.MenuButtons.Games: button.onClick.AddListener(GamesButtonHandler); break;
                case FlappyBirdUtil.Tags.MenuButtons.Play: button.onClick.AddListener(PlayButtonHandler); break;
                case FlappyBirdUtil.Tags.MenuButtons.Rank: button.onClick.AddListener(RankButtonHandler); break;
                case FlappyBirdUtil.Tags.MenuButtons.Share: button.onClick.AddListener(ShareButtonHandler); break;
                case FlappyBirdUtil.Tags.MenuButtons.Twitter: button.onClick.AddListener(TwitterButtonHandler); break;
            }
        }
    }

    #region Button-Handlers
    private void GamesButtonHandler() { report(); }
    private void PlayButtonHandler() { report(); }
    private void RankButtonHandler() { report(); }
    private void ShareButtonHandler() { report(); }
    private void TwitterButtonHandler() { report(); }
    #endregion



    // Start is called before the first frame update
    void Start()
    {
        SetUpBirdPicker();
        SetUpButtons();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

  
}
