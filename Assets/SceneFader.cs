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

   
}
