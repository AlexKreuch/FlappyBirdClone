using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;


public class SceneFader : MonoBehaviour
{
    public static SceneFader instance = null;

    #region constaints
    public const int NORMAL = 0;
    public const int RED_UNL = 1;
    public const int GREEN_UNL = 2;
    public const int BLUE_UNL = 3;
    #endregion

    [SerializeField]
    private GameObject panel = null;
    [SerializeField]
    private Image image = null;
    private bool fadeInProgress = false;
    private SpriteResource.CurtainSpriteSet curtainsSprites = null;
    private enum Curtain { NORM=NORMAL , RED=RED_UNL , GREEN=GREEN_UNL , BLUE=BLUE_UNL };
    private Curtain curtain = Curtain.NORM;

    private void SetUp()
    {
        image = GetComponentInChildren<Image>();
        panel = image.gameObject;
        panel.SetActive(false);
        curtainsSprites = Resources.Load<SpriteResource>(FlappyBirdUtil.ResourcePaths.SpriteRec).GetCurtainSprites();
        image.sprite = curtainsSprites.Normal;
        instance = this;
        DontDestroyOnLoad(gameObject);
        
    }



    void OnEnable()
    {
        if (instance != null) { Destroy(gameObject); return; }
        SetUp();
    }

    private void CorrectCurtain(int curtainCode)
    {
        if (curtainCode == (int)curtain) return;
        switch (curtainCode)
        {
            case NORMAL: image.sprite = curtainsSprites.Normal; curtain = Curtain.NORM; break;
            case RED_UNL: image.sprite = curtainsSprites.RedUnlocked; curtain = Curtain.RED; break;
            case GREEN_UNL: image.sprite = curtainsSprites.GreenUnlocked; curtain = Curtain.GREEN; break;
            case BLUE_UNL: image.sprite = curtainsSprites.BlueUnlocked; curtain = Curtain.BLUE; break;
            default: image.sprite = curtainsSprites.Normal; curtain = Curtain.NORM; break;
        }
    }

    private IEnumerator FadeMech(float durration, string sceneName, int curtainCode)
    {
        CorrectCurtain(curtainCode);

        bool _somethingWentWrong = false;

        if (durration <= 0) durration = .0001f;
        Color c0 = new Color(1f, 1f, 1f, 0f);
        Color c1 = new Color(1f, 1f, 1f, 1f);
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

    public bool StartFading(float durration, string sceneName, int curtainCode = NORMAL)
    {
        if (fadeInProgress) return false;
        fadeInProgress = true;
        StartCoroutine(FadeMech(durration,sceneName,curtainCode));
        return true;
    }
    public bool CurrentlyFading() { return fadeInProgress; }
}
