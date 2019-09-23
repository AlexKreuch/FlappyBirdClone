using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{

    private void LoadUnlockedOptionsToBirdPicker()
    {
        bool b = false, r = false, g = false;
        GameController.instance.GetUnlockedBirds(ref b, ref r, ref g);
        BirdPicker.instance.SetUnlockedOption('B', b);
        BirdPicker.instance.SetUnlockedOption('R', r);
        BirdPicker.instance.SetUnlockedOption('G', g);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetUpBirdPicker();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetUpBirdPicker()
    {
        void SRGBsender(char selection, bool unlockRed, bool unlockGreen, bool unlockBlue)
        {
            BirdPicker.instance.SetUnlockedOption('R', unlockRed);
            BirdPicker.instance.SetUnlockedOption('G', unlockGreen);
            BirdPicker.instance.SetUnlockedOption('B', unlockBlue);
            BirdPicker.instance.SetChoice(selection);
        }
        GameController.instance.SendInfo(FlappyBirdUtil.MessageToController.BirdSelectionSetupRequest.Create(SRGBsender));
    }
}
