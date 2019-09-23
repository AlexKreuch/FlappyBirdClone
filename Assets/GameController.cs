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
    private const string CURRENTBIRD = "currently-selected-bird";
    private const int BLUEFLAG = 1;
    private const int REDFLAG = 2;
    private const int GREENFLAG = 4;
    private const int DEFAULTBIRDFLAG = BLUEFLAG;
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
    private bool BlueUnlocked
    {
        get { return _getBirdUnlocked(BLUEFLAG); }
        set { _setBirdUnlocked(value, BLUEFLAG); }
    }
    private bool RedUnlocked
    {
        get { return _getBirdUnlocked(REDFLAG); }
        set { _setBirdUnlocked(value, REDFLAG); }
    }
    private bool GreenUnlocked
    {
        get { return _getBirdUnlocked(GREENFLAG); }
        set { _setBirdUnlocked(value, GREENFLAG); }
    }
    private char CurrentBird
    {
        get
        {
            int flag = PlayerPrefs.GetInt(CURRENTBIRD,DEFAULTBIRDFLAG);
            switch (flag)
            {
                case BLUEFLAG: return 'B';
                case REDFLAG: return 'R';
                case GREENFLAG: return 'G';
            }
            return 'B';
        }
        set
        {
            int flag = DEFAULTBIRDFLAG;
            switch (value)
            {
                case 'R': flag = REDFLAG; break;
                case 'B': flag = BLUEFLAG; break;
                case 'G': flag = GREENFLAG; break;
            }
            PlayerPrefs.SetInt(CURRENTBIRD,flag);
        }
    }
    #endregion

    #region public-methods
 

    #endregion

    #region helper-methods
        private void InitialSetup()
        {
            if (Initialized) PlayerPrefs.DeleteAll(); // reset if needed

            HighScore = 0;

            BlueUnlocked  = true;
            RedUnlocked = true ;
            GreenUnlocked = false;

            CurrentBird = 'R';
        
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
   
    #endregion



    void OnEnable()
    {
        MakeInstance();
    }

    public class MMPort // MM := MainMenu
    {
        public static void GetBirdPickerSetUp(ref char selectedBird, ref bool redUnlocked, ref bool greenUnlocked, ref bool blueUnlocked)
        {
            selectedBird = instance.CurrentBird;
            blueUnlocked = instance.BlueUnlocked;
            redUnlocked = instance.RedUnlocked;
            greenUnlocked = instance.GreenUnlocked;
        }
    }
    
    
}

