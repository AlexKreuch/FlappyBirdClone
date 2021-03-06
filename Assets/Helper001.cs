﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using GooglePlayGames;


[ExecuteAlways]
public class Helper001 : MonoBehaviour
{

    private void BtnMech(ref bool btn, Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }
    
    private int count = 0;
    private void Report(string msg, params object[] fields)
    {
        string res = "" + count++ + " | " + string.Format(msg, fields);
        Debug.Log(res);
    }

    public enum birds { BLUE, RED, GREEN }
    public birds choice = birds.BLUE;
    public bool Blue = false, Red = false, Green = false;
    public bool RESET = false;
    private void Reset()
    {
        if (!Application.isPlaying) return;
        BirdPicker.instance.SetUnlockedOption('B', Blue);
        BirdPicker.instance.SetUnlockedOption('R', Red);
        BirdPicker.instance.SetUnlockedOption('G', Green);
        char c = 'B';
        switch (choice)
        {
            case birds.BLUE: c = 'B'; break;
            case birds.GREEN: c = 'G'; break;
            case birds.RED: c = 'R'; break;
        }
        BirdPicker.instance.SetChoice(c);
    }

   

    // Update is called once per frame
    void Update()
    {
        BtnMech(ref RESET, Reset);
        BtnMech(ref gotoGame, GotoGame);
    }

    public bool gotoGame = false;
    public float time = 0f;
    private void GotoGame()
    {
        if (!Application.isPlaying) return;
        bool flag = SceneFader.instance.StartFading(time,FlappyBirdUtil.Names.GamePlayScene);
        if (!flag) Debug.Log("fader was busy");
    }

    
    


}
