using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


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


    private const string animName = "BlueBirdFlapping";
    private const string boxName = "SpriteBox";

    public bool test = false;
    private int count = 0;
    private void Report(string msg, params object[] fields)
    {
        string res = "" + count++ + " | " + string.Format(msg, fields);
        Debug.Log(res);
    }

    private class SpriteGetter
    {
        private static Sprite[] sprites = null;
        public static Sprite[] Get()
        {
            if (sprites == null)
            {
                sprites = Resources.Load<SpriteResource>(boxName).GetBirdSprites();
            }
            return sprites;
        }
    }

    public bool BLUE = false, GREEN = false, RED = false;
    private void maintain()
    {
        if (!Application.isPlaying) return;
        var tmp = BirdPicker.Option.NONE;
        if (BLUE) tmp = tmp | BirdPicker.Option.BLUE;
        if (GREEN) tmp = tmp | BirdPicker.Option.GREEN;
        if (RED) tmp = tmp | BirdPicker.Option.RED;
        BirdPicker.instance.UnlockedOptions = tmp;
    }

    // Update is called once per frame
    void Update()
    {
        maintain();
    }
}
