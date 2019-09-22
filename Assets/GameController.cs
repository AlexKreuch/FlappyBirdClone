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
    private bool _getBirdUnlocked(int birdFlag)
    {
        int tmp = PlayerPrefs.GetInt(UNLOCKEDBIRDS, 0);
        tmp = tmp & birdFlag;
        return tmp != 0;
    }
    private void _setBirdUnlocked(bool val, int birdFlag)
    {
        int tmp = PlayerPrefs.GetInt(UNLOCKEDBIRDS, 0);
        tmp = tmp & (7 - birdFlag);
        if (val) tmp += birdFlag;
        PlayerPrefs.SetInt(UNLOCKEDBIRDS, tmp);
    }
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

    void OnEnable()
    {
        MakeInstance();
    }

    public void GetUnlockedBirds(ref bool blue, ref bool red, ref bool green)
    {
        blue = BlueUnlocked;
        red = RedUnlocked;
        green = GreenUnlocked;
    }

   
}

