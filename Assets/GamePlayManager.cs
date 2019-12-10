#define TESTING_MODE
#define BIRD_IN_TESTING_MODE
// USING_INITIAL_BIRD : use this if the GamePlayScene has a defualt bird pre-loaded
#define USING_INITIAL_BIRD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour
{
    private Vector3 startingPosition = new Vector3(0f, 0f, 0f);
    private int highScore = 0;


    private void SetUp()
    {
        GameObject.FindGameObjectWithTag(FlappyBirdUtil.Tags.PauseButtonTag).GetComponent<Button>().onClick.AddListener(PauseButtonHandler);
        PausePanelController.instance.SetResumeGameAction(ResumeButtonHandler);
        PausePanelController.instance.SetRestartGameAction(RestartButtonHandler);
        PausePanelController.instance.AddMenuButtonListener(MenuButtonHandler);
        highScore = GameController.GPPort.GetHighScore();
        char brd = GameController.GPPort.GetCurrentBird();
        var brdBox = Resources.Load<BirdResource>(FlappyBirdUtil.ResourcePaths.BirdRec);
        #if TESTING_MODE
          //  brd = 'G';
        #endif
        #if (USING_INITIAL_BIRD && BIRD_IN_TESTING_MODE)
            Debug.Log(string.Format("~~ destroying initial-bird : {0}",Bird.instance.gameObject.name));
            Bird.Testing_ERASE();
        #endif
        switch (brd)
        {
            case 'R': Instantiate(brdBox.RedBird, startingPosition, new Quaternion()); break;
            case 'G': Instantiate(brdBox.GreenBird, startingPosition, new Quaternion()); break;
            case 'B': Instantiate(brdBox.BlueBird, startingPosition, new Quaternion()); break;
        }
        Bird.instance.AddOnDieListener(RunGameOver);
        StartCoroutine(WaitToFinishFadin());
    }

    private IEnumerator WaitToFinishFadin()
    {
        Bird.instance.SuspendMovement = true;
        while (SceneFader.instance.CurrentlyFading()) yield return null;
        Bird.instance.SuspendMovement = false;
    }

    private void PauseButtonHandler()
    {
        UpdateHighScore();
        var pausePanel = PausePanelController.instance;
        if (pausePanel.PanelTurnedOn) return;
        Time.timeScale = 0f;
        pausePanel.PanelTurnedOn = true;
        pausePanel.SetHighScore(highScore);
        pausePanel.SetScore(Bird.instance.GetCurrentScore());
    }
    private void ResumeButtonHandler()
    {
        var pausePanel = PausePanelController.instance;
        pausePanel.PanelTurnedOn = false;
        Time.timeScale = 1f;
    }
    private void RestartButtonHandler()
    {
        ReportNewHighScore();
        var pausePanel = PausePanelController.instance;
        Bird.instance.SuspendMovement = true;
        Time.timeScale = 1f;
        SceneFader.instance.StartFading(FlappyBirdUtil.FadeTime,FlappyBirdUtil.Names.GamePlayScene);
    }
    private void MenuButtonHandler()
    {
        ReportNewHighScore();
        Bird.instance.SuspendMovement = true;
        Time.timeScale = 1f;
        SceneFader.instance.StartFading(FlappyBirdUtil.FadeTime,FlappyBirdUtil.Names.MainMenuScene);
    }

    private void UpdateHighScore()
    {
        int currentScore = Bird.instance.GetCurrentScore();
        if (currentScore > highScore) highScore = currentScore;
    }
    private void ReportNewHighScore()
    {
        GameController.GPPort.SetHighScore(highScore);
    }
    private void EvaluateFinalScore()
    {
        /*
             RULES : 
                ->  if     score<=10 : medal==white
                    elsif  score<=20 : medal==orange
                    else             : medal==gold   && unlock-next-bird

         */
        float score = Bird.instance.GetCurrentScore();
        if (score <= 10) PausePanelController.instance.SetMedal(FlappyBirdUtil.Flags.Medals.White);
        else if (score <= 20) PausePanelController.instance.SetMedal(FlappyBirdUtil.Flags.Medals.Orange);
        else
        {
            PausePanelController.instance.SetMedal(FlappyBirdUtil.Flags.Medals.Gold);
            GameController.GPPort.UnlockNextBird();
        }
    }
    private void RunGameOver()
    {
        UpdateHighScore();
        ReportNewHighScore();
        var pausePanel = PausePanelController.instance;
        pausePanel.Mode = PausePanelController.MODE.GAMEOVER;
        pausePanel.SetScore(Bird.instance.GetCurrentScore());
        pausePanel.SetHighScore(highScore);
        EvaluateFinalScore();
        pausePanel.PanelTurnedOn = true;
    }

    void Start()
    {
        Debug.Log("SETTING-UP");
        SetUp();

    }

 

}
