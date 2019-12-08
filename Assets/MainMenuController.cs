#define TESTING_GOOGLE_PLAY
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{

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
                case FlappyBirdUtil.Tags.MenuButtons.Settings: button.onClick.AddListener(SettingsButtonHandler); break;
            }
        }
    }

   

    #region Button-Handlers
        private void GamesButtonHandler() {  }
        private void PlayButtonHandler()
        {
            SaveBirdChoice();
            SceneFader.instance.StartFading(FlappyBirdUtil.FadeTime,FlappyBirdUtil.Names.GamePlayScene);
        }
        private void RankButtonHandler() { }
        private void ShareButtonHandler() {  }
        private void TwitterButtonHandler()
        {
            #if TESTING_GOOGLE_PLAY
                string nm = "TestScene02";
                SceneFader.instance.StartFading(FlappyBirdUtil.FadeTime,nm);
            #endif
        }
        private void SettingsButtonHandler() { }
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
