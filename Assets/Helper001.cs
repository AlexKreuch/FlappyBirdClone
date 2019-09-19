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

    
    private class GetTextRect
    {
        private const string nm = "Title";
        private static RectTransform rect = null;
        private static Text FindText()
        {
            var lst = GameObject.FindObjectsOfType<Text>();
            foreach (var x in lst) if (x.name == nm) return x;
            return null;
        }
        public static RectTransform Get()
        {
            if (rect == null)
            {
                var txt = FindText();
                if (txt != null) rect = txt.rectTransform;
            }
            return rect;
        }
    }
    private void MaintainScale()
    {
        var rect = GetTextRect.Get();
        if (rect == null) return;
        var tmp = rect.localScale;
        if (tmp.x != tmp.y)
        {
            tmp.y = tmp.x;
            rect.localScale = tmp;
        }
    }
    // Update is called once per frame
    void Update()
    {
        MaintainScale();
    }
}
