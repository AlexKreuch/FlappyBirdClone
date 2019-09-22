using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{

    private void LoadUnlockedOptionsToBirdPicker()
    {
        bool b = false, r = false, g = false;
        GameController.instance.GetUnlockedBirds(ref b, ref r, ref g);
        var tmp = BirdPicker.Option.NONE;
        if (b) tmp = tmp | BirdPicker.Option.BLUE;
        if (r) tmp = tmp | BirdPicker.Option.RED;
        if (g) tmp = tmp | BirdPicker.Option.GREEN;
        BirdPicker.instance.UnlockedOptions = tmp;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
