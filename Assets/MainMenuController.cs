using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{

    private void SetUpBirdPicker()
    {
        Debug.Log("MainMenuController-SetUpBirdPicker");
        char brd = 'B';
        bool r = false, g = false, b = false;
        GameController.MMPort.GetBirdPickerSetUp(ref brd, ref r, ref g, ref b);
        BirdPicker.instance.SetUnlockedOption('R',r);
        BirdPicker.instance.SetUnlockedOption('G', g);
        BirdPicker.instance.SetUnlockedOption('B', b);
        BirdPicker.instance.SetChoice(brd);
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

  
}
