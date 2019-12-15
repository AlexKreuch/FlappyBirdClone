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

    private enum Option { NONE=0, BLUE=1, GREEN=2, RED=4 };
    private Option unlockedOptions = Option.NONE;

    private class ChoiceUtil
    {
        private static Option[] options = new Option[] { Option.NONE, Option.BLUE, Option.GREEN, Option.RED };
        private static int[] ints = new int[] { -1, 0, 1, 2  };
        private static char[] chars = new char[] { 'N', 'B', 'G', 'R' };

        private static int ToIndex_opt(Option option)
        {
            switch (option)
            {
                case Option.NONE: return 0;
                case Option.BLUE: return 1;
                case Option.GREEN: return 2;
                case Option.RED: return 3;
                default:return 0;
            }
        }
        private static int ToIndex_int(int val)
        {
            switch (val)
            {
                case -1: return 0;
                case 0: return 1;
                case 1: return 2;
                case 2: return 3;
                default: return 0;
            }
        }
        private static int ToIndex_cha(char val)
        {

            switch (val)
            {
                case 'N': return 0;
                case 'B': return 1;
                case 'G': return 2;
                case 'R': return 3;
                default: return 0;
            }
        }

        private static Option FromIndex_opt(int index) { return options[index]; }
        private static int FromIndex_int(int index) { return ints[index]; }
        private static char FromIndex_cha(int index) { return chars[index]; }

        private static G Convert<T,G>(System.Func<T,int> _toind, System.Func<int,G> _fromind, T val)
        {
            return _fromind(_toind(val));
        }

        public static Option ItoO(int val) { return Convert<int, Option>(ToIndex_int, FromIndex_opt, val); }
        public static Option CtoO(char val) { return Convert<char, Option>(ToIndex_cha, FromIndex_opt, val); }
        public static int OtoI(Option val) { return Convert<Option, int>(ToIndex_opt, FromIndex_int, val); }
        public static int CtoI(char val) { return Convert<char, int>(ToIndex_cha, FromIndex_int, val); }
        public static char OtoC(Option val) { return Convert<Option, char>(ToIndex_opt, FromIndex_cha, val); }
        public static char ItoC(int val) { return Convert<int, char>(ToIndex_int, FromIndex_cha, val); }

        public static bool IsValid(Option val)
        {
            return val == Option.NONE || ToIndex_opt(val) != 0;
        }
        public static bool IsValid(int val)
        {
            return val == -1 || ToIndex_int(val) != 0;
        }
        public static bool IsValid(char val)
        {
            return val == 'N' || ToIndex_cha(val) != 0;
        }

    }

    private int currentChoice = -1; // 0:=Blue, 1:=Green, 2:=Red , -1:=No-choice

    public char GetChoice() // 'N':=No-choice, 'B':=Blue, 'G':=Green, 'R':=Red
    {
        return ChoiceUtil.ItoC(currentChoice);
    }
    public void SetChoice(char optionChar)// 'N':=No-choice, 'B':=Blue, 'G':=Green, 'R':=Red
    {
        /**
            Note : If the given option is invalid, or if the given option is not available,  
            then return without changing currentChoice. 
         */
        if (!ChoiceUtil.IsValid(optionChar)) return; // check-valid
        var option = ChoiceUtil.CtoO(optionChar);
        if ((option & unlockedOptions) != option) return; // check-unlocked 
        currentChoice = ChoiceUtil.OtoI(option); // set currentChoice
    }
    public void SetUnlockedOption(char birdChar, bool unlock)
    {
        Option opt = ChoiceUtil.CtoO(birdChar);
        if (unlock) unlockedOptions = unlockedOptions | opt;
        else unlockedOptions = unlockedOptions & (~opt);
    }
    public bool GetUnlockedOption(char birdChar)
    {
        Option option = ChoiceUtil.CtoO(birdChar);
        return ((option & unlockedOptions) != Option.NONE);
    }

    /* Returns true if and only if 'choice' is a correct and available value for 'currentChoice'
     * 
     * 
     * **/
    private bool IsValidChoice(int choice)
    {
        if (!ChoiceUtil.IsValid(choice)) return false;
        var optionChoice = ChoiceUtil.ItoO(choice);
        return ((optionChoice & unlockedOptions) == optionChoice);
    }
    private void RotateChoice()
    {
        if (unlockedOptions == Option.NONE) { currentChoice = -1; return; }
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
        ResetImage(cycle[cur]);
        theImage.SetNativeSize();
        yield return new WaitForSeconds(tempo);
        cur = (cur + 1) % cycle.Length;
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
