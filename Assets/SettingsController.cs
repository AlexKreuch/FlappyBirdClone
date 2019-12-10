using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SettingsController : MonoBehaviour
{
    /*
Main-Menu (button)
High-Score (text)
UnlockedBird-list (text)
OptionSlider (Dropdown)
OVERRIDE (button)
Reset (button)
*/
    private enum FIELDTAG // Use these to index the fields in the scene
    {
        MAIN = 0b000,
        SCORE = 0b001,
        BIRDLIST = 0b010,
        DROPDOWN = 0b011,
        OVERRIDE = 0b100,
        RESET = 0b101,
    }

    private Dictionary<FIELDTAG, object> Fields = null;
    private void CollectFields() // initialize Fields and populate it
    {
        Fields = new Dictionary<FIELDTAG, object>();

        #region declaire variables
        Button[] buttons = null;
        Text[] texts = null;
        Dropdown[] dropdowns = null;
        #endregion

        #region get Buttons
        buttons = FindObjectsOfType<Button>();
        foreach (var btn in buttons) switch (btn.tag)
            {
                case FlappyBirdUtil.Tags.SettingsPageFields.MainMenu: Fields.Add(FIELDTAG.MAIN, btn); break;
                case FlappyBirdUtil.Tags.SettingsPageFields.UnlockControl: Fields.Add(FIELDTAG.OVERRIDE, btn); break;
                case FlappyBirdUtil.Tags.SettingsPageFields.ResetButton: Fields.Add(FIELDTAG.RESET, btn); break;
            }
        #endregion

        #region get Texts
        texts = FindObjectsOfType<Text>();
        foreach (var txt in texts) switch (txt.tag)
            {
                case FlappyBirdUtil.Tags.SettingsPageFields.HighScore: Fields.Add(FIELDTAG.SCORE, txt); break;
                case FlappyBirdUtil.Tags.SettingsPageFields.UnlockControl: Fields.Add(FIELDTAG.BIRDLIST, txt); break;
            }
        #endregion

        #region get Dropdowns
        dropdowns = FindObjectsOfType<Dropdown>();
        foreach (var dd in dropdowns) switch (dd.tag)
            {
                case FlappyBirdUtil.Tags.SettingsPageFields.UnlockControl: Fields.Add(FIELDTAG.DROPDOWN, dd); break;
            }
        #endregion
    }


    private class Util
    {
       private static IEnumerable<string> OptionLabels(int code)
        {
            string[] ord = new string[3];
            int n = 100;
            for (int i = 0; i < 3; i++)
            {
                int c = (code / n) % 10;
                switch (c)
                {
                    case 1: ord[i] = "Red"; break;
                    case 2: ord[i] = "Green"; break;
                    case 3: ord[i] = "Blue"; break;
                }
                n /= 10;
            }
            string res = ord[0];
            yield return res;
            res += ", " + ord[1];
            yield return res;
            res += ", " + ord[2];
            yield return res;

        }

        public static void SetUpDropDown(Dictionary<FIELDTAG, object> _fields, int[] data)
        {
            // TODO
        }
    }
    private void SetUp()
    {
        CollectFields();

        ((Button)Fields[FIELDTAG.MAIN]).onClick.AddListener(OnMenuClick);
        ((Button)Fields[FIELDTAG.OVERRIDE]).onClick.AddListener(OnOverrideClick);
        ((Button)Fields[FIELDTAG.RESET]).onClick.AddListener(OnResetClick);
    }


    #region onClickListeners
    private static int count = 0;
    private void OnMenuClick()
    {
        SceneFader.instance.StartFading(FlappyBirdUtil.FadeTime,FlappyBirdUtil.Names.MainMenuScene);
    }
    private void OnResetClick()
    {
        // TODO
        string nm = "RESET";
        Debug.Log(string.Format("{0} | {1}",count++,nm));
    }
    private void OnOverrideClick()
    {
        // TODO
        string nm = "OVERRIDE";
        Debug.Log(string.Format("{0} | {1}", count++, nm));
    }
    #endregion

    

    void Start() {
        SetUp();
        
    }

}
