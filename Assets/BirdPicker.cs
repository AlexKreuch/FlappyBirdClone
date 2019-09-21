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

    public enum Option { NONE=0, BLUE=1, GREEN=2, RED=4 };
    private Option _unlockedOptions = Option.NONE;
    public Option UnlockedOptions { get { return _unlockedOptions; } set { _unlockedOptions = value; } }

    private int currentChoice = -1; // 0:=Blue, 1:=Green, 2:=Red , -1:=No-choice

    public Option GetChoice() 
    {
        switch (currentChoice)
        {
            case -1: return Option.NONE;
            case 0: return Option.BLUE;
            case 1: return Option.GREEN;
            case 2: return Option.RED;
            default: return Option.NONE;
        }
    }

    /* Returns true if and only if 'choice' is a correct and available value for 'currentChoice'
     * 
     * 
     * **/
    private bool IsValidChoice(int choice)
    {
        switch (choice)
        {
            case -1: return true;
            case 0: return ((_unlockedOptions & Option.BLUE) != Option.NONE);
            case 1: return ((_unlockedOptions & Option.GREEN) != Option.NONE);
            case 2: return ((_unlockedOptions & Option.RED) != Option.NONE);
            default: return false;
        }
    }
    private void RotateChoice()
    {
        if (_unlockedOptions == Option.NONE) { currentChoice = -1; return; }
        int tmp = currentChoice;
        do
        {
            tmp = (tmp + 1) % 3;
        }
        while (!IsValidChoice(tmp));
        currentChoice = tmp;
    }

    private void ResetImage(int offset)
    {
        // NOTE : offset MUST be in [0,2]
        Debug.Assert(offset>=0 && offset<=2, "INVALID ANIMATION-OFFSET IN BIRDPICKER");
        Debug.Assert(sprites!=null, "SPRITES NOT LOADED");
        if (currentChoice == -1) theImage.sprite = null;
        else
        {
            bool firstBird = theImage.sprite == null;
            theImage.sprite = sprites[(4 * currentChoice) + offset];
            if (firstBird) theImage.SetNativeSize();
        }
    }

    private IEnumerator ButtonAnimation()
    {
        const float tempo = .1f;
        int cur = 1;
        int[] cycle = new int[] { 0 , 1 , 2 , 1 };
        while (true)
        {
            ResetImage(cycle[cur]);
            yield return new WaitForSeconds(tempo);
            cur = (cur + 1) % cycle.Length;
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
