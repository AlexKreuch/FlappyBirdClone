using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BirdPicker : MonoBehaviour
{
    public static BirdPicker instance = null;
    private void MakeInstance()
    {
        if (instance != null) { Destroy(this); return; }
        instance = this;
    }

    private const string SpriteResourcePath = "SpriteBox";

    private Image theImage = null;
    private Button theButton = null;
    private Sprite[] sprites = null;

    private int currentChoice = 0; // 0:=Blue, 1:=Green, 2:=Red

    public char GetChoice() // 'B':=Blue, 'G':=Green, 'R':=Red
    {
        return "BGR"[currentChoice];
    }

    private void RotateChoice()
    {
        currentChoice = (currentChoice + 1) % 3;
    }

    private IEnumerator ButtonAnimation()
    {
        const float tempo = .1f;
        int cur = 1;
        int[] cycle = new int[] { 0 , 1 , 2 , 1 };
        theImage.sprite = sprites[(4 * currentChoice) + cycle[cur]];
        theImage.SetNativeSize();
        while (true)
        {
            yield return new WaitForSeconds(tempo);
            cur = (cur + 1) % cycle.Length;
            theImage.sprite = sprites[(4 * currentChoice) + cycle[cur]];
        }
    }

    private void SetUp()
    {
        theImage = GetComponent<Image>();
        theButton = GetComponent<Button>();
        theButton.onClick.AddListener(RotateChoice);
        sprites = Resources.Load<SpriteResource>(SpriteResourcePath).GetBirdSprites();
        StartCoroutine(ButtonAnimation());
    }

  
    
   
    void OnEnable()
    {
        MakeInstance();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetUp();
    }

    
}
