using static FlappyBirdUtil.MessageToController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameController : MonoBehaviour
{
    public static GameController instance;
    private void MakeInstance()
    {
        if (instance != null) { Destroy(this); return; }
        instance = this;
        if (!Initialized) InitialSetup();
        DontDestroyOnLoad(gameObject);
    }

    #region play-pref-keys
    private const string INITIALIZED = "initialized";
    private const string HIGHSCORE = "highScore";
    private const string UNLOCKEDBIRDS = "unlocked-birds";
    #endregion

    #region private-properties
    private int HighScore
    {
        get
        {
            return PlayerPrefs.GetInt(HIGHSCORE,0);
        }
        set
        {
            PlayerPrefs.SetInt(HIGHSCORE, value);
        }
    }
    private bool Initialized
    {
        get { return PlayerPrefs.GetInt(INITIALIZED, 0) == 1; }
        set { PlayerPrefs.SetInt(INITIALIZED,value ? 1 : 0); }
    }
    // NOTE : 1:=BLUE , 2:=RED , 4:=GREEN
    private bool BlueUnlocked
    {
        get { return _getBirdUnlocked(1); }
        set { _setBirdUnlocked(value, 1); }
    }
    private bool RedUnlocked
    {
        get { return _getBirdUnlocked(2); }
        set { _setBirdUnlocked(value, 2); }
    }
    private bool GreenUnlocked
    {
        get { return _getBirdUnlocked(4); }
        set { _setBirdUnlocked(value, 4); }
    }
    #endregion

    #region public-methods
    public void GetUnlockedBirds(ref bool blue, ref bool red, ref bool green)
    {
        blue = BlueUnlocked;
        red = RedUnlocked;
        green = GreenUnlocked;
    }

    public void SendInfo(IMessage message)
    {
        switch (message.GetMessageType())
        {
           case messageType.BIRD_SELECTION_SETUP_REQUEST: HandleBirdSelectorSetupRequest(message.GetUnit()); break;
        }
    }

    #endregion

    #region helper-methods
        private void InitialSetup()
        {
            if (Initialized) PlayerPrefs.DeleteAll(); // reset if needed

            HighScore = 0;

            BlueUnlocked  = true;
            RedUnlocked = false ;
            GreenUnlocked = true;
        
            Initialized = true;
        }
        private bool _getBirdUnlocked(int birdFlag)
        {
            int tmp = PlayerPrefs.GetInt(UNLOCKEDBIRDS, 0);
            tmp = tmp & birdFlag;
            return tmp != 0;
        }
        private void _setBirdUnlocked(bool val, int birdFlag)
    {
        int tmp = PlayerPrefs.GetInt(UNLOCKEDBIRDS, 0);
        if (val) tmp = tmp | birdFlag;
        else tmp = tmp & (~birdFlag);
        PlayerPrefs.SetInt(UNLOCKEDBIRDS, tmp);
    }
    #region message-handlers
    private void HandleBirdSelectorSetupRequest(Unit unit)
    {
        var bssr = (BirdSelectionSetupRequest)unit;
        char selected = 'B';
        bssr.BlueUnlocked = BlueUnlocked;
        bssr.RedUnlocked = RedUnlocked;
        bssr.GreenUnlocked = GreenUnlocked;
        switch (selected)
        {
            case 'B': bssr.SelectBlue(); break;
            case 'R': bssr.SelectRed(); break;
            case 'G': bssr.SelectGreen(); break;
        }
        bssr.Send();
    }
        #endregion
    #endregion



    void OnEnable()
    {
        MakeInstance();
    }

    
}

