using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;


public class SceneFader : MonoBehaviour
{
    public static SceneFader instance = null;

    [SerializeField]
    private GameObject panel = null;
    [SerializeField]
    private Image image = null;
    private bool fadeInProgress = false;

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
        if (instance != null) { Destroy(gameObject); return; }
        SetUp();
    }

    private IEnumerator FadeMech(float durration, string sceneName)
    {
        bool _somethingWentWrong = false;

        if (durration <= 0) durration = .0001f;
        Color c0 = new Color(0f, 0f, 0f, 0f);
        Color c1 = new Color(0f, 0f, 0f, 1f);
        Color slope = (c1 - c0) / durration;
         
        image.color = c0;

        float startTime = Time.time;
        float curTime = startTime;
        float endTime = startTime + durration;

        panel.SetActive(true);

        while (curTime < endTime)
        {
            curTime = Time.time;
            image.color = slope * (Mathf.Clamp(curTime, startTime, endTime) - startTime) + c0;
            yield return null;
        }

        image.color = c1;

        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch(Exception ex)
        {
            Debug.Log(string.Format("could not load <{0}> | message : {1}",sceneName,ex.Message));
            _somethingWentWrong = true;
        }

        if (!_somethingWentWrong)
        {
            startTime = Time.time;
            curTime = startTime;
            endTime = startTime + durration;
            slope *= -1;

            while (curTime < endTime)
            {
                curTime = Time.time;
                image.color = slope * (Mathf.Clamp(curTime, startTime, endTime) - startTime) + c1;
                yield return null;
            }
        }
        image.color = c0;
        panel.SetActive(false);
        fadeInProgress = false;
    }

    public bool StartFading(float durration, string sceneName)
    {
        if (fadeInProgress) return false;
        fadeInProgress = true;
        StartCoroutine(FadeMech(durration,sceneName));
        return true;
    }
    public bool CurrentlyFading() { return fadeInProgress; }
}
