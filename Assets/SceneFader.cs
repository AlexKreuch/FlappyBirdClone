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

    public void TestFade(int count=10, float time=1f,int flag=0)
    {
        void f() { Debug.Log("FADE-EVENT : " + flag); }
        StartCoroutine(FadeMech(count,time,f));
    }
}
