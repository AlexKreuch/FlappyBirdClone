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
        foreach (char c in "BRG") BirdPicker.instance.SetUnlockedOption(c,true);
        BirdPicker.instance.SetChoice('B');
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
