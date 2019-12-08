#define TESTING_MODE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if TESTING_MODE
#else

using System.Runtime.CompilerServices;
#endif

public class GameController : MonoBehaviour
{
    

    public static GameController instance;
    private void MakeInstance()
    {
        if (instance != null)
        {
            Destroy(this); return;
        }
        instance = this;
#if TESTING_MODE
        InitialSetup();
#else
        if (!Initialized)InitialSetup();
#endif
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

        BlueUnlocked = true;
        RedUnlocked = true;
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
        public static void ReportNewBirdSelection(char selectedBird) { instance.CurrentBird = selectedBird; }
    }
    public class GPPort // GP := GamePlay
    {
        public static char GetCurrentBird() { return instance.CurrentBird; }
        public static int GetHighScore() { return instance.HighScore; }
        public static void SetHighScore(int newHighScore) { instance.HighScore = newHighScore; }
        public static void UnlockNextBird()
        {
            if (!instance.BlueUnlocked) { instance.BlueUnlocked = true; return; }
            if (!instance.RedUnlocked) { instance.RedUnlocked = true; return; }
            if (!instance.GreenUnlocked) { instance.GreenUnlocked = true; return; }
        }
    }
    public class SPPort // SP := SettingsPage
    {
        public static void ResetAll()
        {
            PlayerPrefs.DeleteAll();
        }
        public static void OVERRIDE_UNLOCKED_BIRDS(bool red, bool green, bool blue)
        {
            GameController.instance.RedUnlocked = red;
            GameController.instance.GreenUnlocked = green;
            GameController.instance.BlueUnlocked = blue;
        }
        public static void OVERRIDE_BIRD_SELECTION(char brd)
        {
            GameController.instance.CurrentBird = brd;
        }
        public static bool CHECK_VALID()
        {
            var gci = GameController.instance;
            char brd = gci.CurrentBird;
            switch(brd)
            {
                case 'R': return gci.RedUnlocked;
                case 'B': return gci.BlueUnlocked;
                case 'G': return gci.GreenUnlocked;
            }
            return false;
        }
        public static void GetData(int[] vals)
        {
            /**
             * note : vals must be non-null and have at least 6 spaces.
             *        vals will be populated by the return-data
             *  key : 
             *     vals[0] := HighScore
             *     vals[1] := CurrentBird ( encoded as char cast to int )
             *     vals[2] := Initialized ( encoded as 1:=True and 0:=False )
             *     vals[3] := RedUnlocked ( encoded as 1:=True and 0:=False )
             *     vals[4] := GreenUnlocked ( encoded as 1:=True and 0:=False )
             *     vals[5] := BlueUnlocked ( encoded as 1:=True and 0:=False )
             *     
             */
            Debug.Assert(vals != null && vals.Length >= 6, "input must be non-null and have size>=6");
            #region implimentation-1
            /** 
             * less efficient. Should work as long as implementations of private properties remain unchanged
             */
            /*
           var gci = GameController.instance;
           vals[0] = gci.HighScore;
           vals[1] = (int)gci.CurrentBird;
           vals[2] = gci.Initialized ? 1 : 0;
           vals[3] = gci.RedUnlocked ? 1 : 0;
           vals[4] = gci.GreenUnlocked ? 1 : 0;
           vals[5] = gci.BlueUnlocked ? 1 : 0;
           */
            #endregion
            #region implimentation-2
            /** 
             * more efficient. May cease to work if implementations of private properties are changed
             */
            var gci = GameController.instance;
            vals[0] = gci.HighScore;
            vals[1] = (int)gci.CurrentBird;
            vals[2] = PlayerPrefs.GetInt(INITIALIZED, 0);
            int tmp = PlayerPrefs.GetInt(UNLOCKEDBIRDS, 0);
            vals[3] = (tmp / REDFLAG) % 2;
            vals[4] = (tmp / GREENFLAG) % 2;
            vals[5] = (tmp / BLUEFLAG) % 2;

            #endregion
        }
    }
}

