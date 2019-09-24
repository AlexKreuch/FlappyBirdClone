using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class SceneFader : MonoBehaviour
{
    public static SceneFader instance = null;

    private GameObject panel = null;
    private Image image = null;

    private void SetUp()
    {
        image = GetComponentInChildren<Image>();
        panel = image.gameObject;
        panel.SetActive(false);
        instance = this;
        DontDestroyOnLoad(gameObject);
    }



    void OnEnable()
    {
        if (instance != null) return;
        SetUp();
    }

    private IEnumerator FadeMech(int frameCount, float time, Action action)
    {
        if (frameCount <= 0) frameCount = 1;
        if (time <= 0) time = .0001f;
        Color color = new Color(0f, 0f, 0f, 0f);
        Color deltaColor = new Color(0f, 0f, 0f, 1f/frameCount);
        float deltaTime = time / frameCount;
        image.color = color;
        panel.SetActive(true);
        for (int i = 0; i < frameCount; i++)
        {
            color += deltaColor;
            yield return new WaitForSeconds(deltaTime);
            image.color = color;
        }
        action();
        for (int i = 0; i < frameCount; i++)
        {
            color -= deltaColor;
            yield return new WaitForSeconds(deltaTime);
            image.color = color;
        }
        panel.SetActive(false);
    }

    public void TestFade(int count = 10, float time = 1f, bool smooth = false,int flag=0)
    {
        if (smooth)
        {
            void f() { Debug.Log("Fade-event (smooth-version) : " + flag); }
            StartCoroutine(FadeMech_smooth(time,f));
        }
        else
        {
            void f() { Debug.Log("FADE-EVENT : " + flag); }
            StartCoroutine(FadeMech(count, time, f));
        }
    }

    private IEnumerator FadeMech_smooth(float time, Action action)
    {
        if (time <= 0) time = .0001f;

        Func<float,float,Color> MakeFunc(Color color0, Color color1, float delta)
        {
            if (delta <= 0) { return (v,o)=>(v<=o ? color0 : color1); }
            Color _color = (color1 - color0) / delta;
            Color f(float cur, float offset)
            {
                cur = Mathf.Clamp(cur,offset,offset+delta);
                return _color * (cur - offset) + color0;
            }
            return f;
        }


        Color c0 = new Color(0f, 0f, 0f, 0f);
        Color c1 = new Color(0f, 0f, 0f, 1);
        var faderFunc0 = MakeFunc(c0,c1,time);
        var faderFunc1 = MakeFunc(c1,c0,time);

        float startTime = Time.time;
        float curTime = startTime;
        float endTime = startTime + time;

        image.color = new Color(0, 0, 0, 0);

        panel.SetActive(true);

        while (curTime<endTime)
        {
            yield return null;
            curTime = Time.time;
            image.color = faderFunc0(curTime, startTime);
        }

        action();

        startTime = curTime;
        endTime = startTime + time;

        while (curTime < endTime)
        {
            yield return null;
            curTime = Time.time;
            image.color = faderFunc1(curTime, startTime);
        }

        panel.SetActive(false);
        
    }
}
