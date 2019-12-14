using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class SettingsController : MonoBehaviour
{
    private static SettingsController instance;
    private void MakeInstance()
    {
        if (instance != null) { Destroy(this); return; }
        instance = this;
    }
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
        PROMPT = 0b110
    }

    private Dictionary<FIELDTAG, object> Fields = null;


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
            List<Dropdown.OptionData> lst = new List<Dropdown.OptionData>();
            foreach (string labl in OptionLabels(data[6])) lst.Add(new Dropdown.OptionData(labl));
            var dropDown = (Dropdown)_fields[FIELDTAG.DROPDOWN];
            dropDown.ClearOptions();
            dropDown.AddOptions(lst);
        }


        public static void UpdateScoreDisplay(Dictionary<FIELDTAG, object> _fields, int[] data)
        {
            // HIGH-SCORE : 0000
            const string template = "HIGH-SCORE : {0}";
            int score = data[0];
            string res = score.ToString();
            while (res.Length < 4) res = '0' + res;
            res = string.Format(template, res);
            Text txt = (Text)_fields[FIELDTAG.SCORE];
            txt.text = res;
        }

        public static void UpdateUnlockedBirdsDisplay(Dictionary<FIELDTAG, object> _fields, int[] data)
        {
            #region template
            /*   
            |UnlockedBirds : 
            |                        -> -----
            |                        -> -----
            |                        -> -----
            */
            #endregion

            int[] ComputeOrd(int ordCode)
            {
                int[] ord = new int[3];
                ord[0] = ((ordCode / 100) % 10) - 1;
                ord[1] = ((ordCode / 10) % 10) - 1;
                ord[2] = ((ordCode / 1) % 10) - 1;
                return ord;
            }
            string GetNm(int ind, string[] nms, int[] ord) { return nms[ord[ind]]; }
            bool GetUnl(int ind, int[] _data, int[] ord)
            {
                return _data[ord[ind] + 3] == 1;
            }
            IEnumerable<string> enu(string[] nms, int[] ord, int[] _data)
            {
                const string Header = "UnlockedBirds : ";
                const string Line = "                        -> {0}";
                yield return Header;
                for (int i = 0; i < 3; i++)
                {
                    if (GetUnl(i, _data, ord)) yield return string.Format(Line, GetNm(i, nms, ord));
                    else yield return "";
                }
            }
            void Compute(Dictionary<FIELDTAG, object> _fie, int[] dat)
            {
                string[] nms = new string[] { "Red", "Green", "Blue" };
                int[] ord = ComputeOrd(dat[6]);
                IEnumerable<string> _textBuilder = enu(nms, ord, dat);
                string res = string.Join("\n", _textBuilder);
                Text text = (Text)_fie[FIELDTAG.BIRDLIST];
                text.text = res;
            }
            Compute(_fields, data);
        }

        public class ConfirmDialog
        {
            #region field-names
            const string imageName = "prompt_Background";
            const string textName = "prompt_Text";
            const string confirmBtnName = "Confirm";
            const string cancelBtnName = "Cancel";
            #endregion

            #region fields
            private GameObject panel = null;
            private Image image = null;
            private Text text = null;
            private Button confirmButton = null;
            private Button cancelButton = null;
            #endregion

            #region constructors
            private ConfirmDialog() { }
            private ConfirmDialog(GameObject panel, Image image, Text text, Button confirmButton, Button cancelButton)
            {
                this.panel = panel;
                this.image = image;
                this.text = text;
                this.confirmButton = confirmButton;
                this.cancelButton = cancelButton;
            }
            public static ConfirmDialog Create(GameObject panel)
            {
                if (panel == null) return null;

                IEnumerable<Component> enu(GameObject _panel)
                {
                    Type[] types = new Type[] { typeof(Image), typeof(Text), typeof(Button) };
                    foreach(Type type in types)
                    {
                        var lst = _panel.GetComponentsInChildren(type);
                        foreach (var comp in lst)
                        {
                            yield return comp;
                        }
                    }
                }

                const int img = 0, txt = 1, conBtn = 2, canBtn = 3, siz = 4;

                int count = 0;
                Component[] arr = new Component[siz];

                foreach (Component comp in enu(panel))
                {
                    switch (comp.name)
                    {
                        case imageName: arr[img] = comp; count++; break;
                        case textName: arr[txt] = comp; count++; break;
                        case confirmBtnName: arr[conBtn] = comp; count++; break;
                        case cancelBtnName: arr[canBtn] = comp; count++; break;
                    }
                    if (count == siz) break;
                }
                if (count != siz) return null;
                

                return new ConfirmDialog
                        (
                            panel,
                            arr[img].GetComponent<Image>(),
                            arr[txt].GetComponent<Text>(),
                            arr[conBtn].GetComponent<Button>(),
                            arr[canBtn].GetComponent<Button>()
                        );
                
            }
            #endregion
        }

        /**
             * note : vals must be non-null and have at least 7 spaces.
             *        vals will be populated by the return-data
             *  key : 
             *     vals[0] := HighScore
             *     vals[1] := CurrentBird ( encoded as char cast to int )
             *     vals[2] := Initialized ( encoded as 1:=True and 0:=False )
             *     vals[3] := RedUnlocked ( encoded as 1:=True and 0:=False )
             *     vals[4] := GreenUnlocked ( encoded as 1:=True and 0:=False )
             *     vals[5] := BlueUnlocked ( encoded as 1:=True and 0:=False )
             *     vals[6] := UnlockOrder  ( encoded as described in 'EncodeBirdOrder' above ) 
             *     
             */
    }

    private Util.ConfirmDialog confirmDialog = null;

    #region helper-methods
    private int[] GetDataArr()
    {
        int[] res = new int[GameController.SPPort.RequiredDataSize()];
        GameController.SPPort.GetData(res);
        return res;
    }

    private void CollectFields() // initialize Fields and populate it
    {
        Fields = new Dictionary<FIELDTAG, object>();

        #region declaire variables
        Button[] buttons = null;
        Text[] texts = null;
        Dropdown[] dropdowns = null;
        GameObject[] panels = null;
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

        #region get Panels
        panels = FindObjectsOfType<GameObject>();
        foreach (var pr in panels) switch (pr.tag)
            {
                case FlappyBirdUtil.Tags.SettingsPageFields.PromptPanel: Fields.Add(FIELDTAG.PROMPT, pr); break;
            }
        #endregion
    }

    
    

    private void SetUp()
    {
        CollectFields();

        #region connect buttons
        ((Button)Fields[FIELDTAG.MAIN]).onClick.AddListener(OnMenuClick);
        ((Button)Fields[FIELDTAG.OVERRIDE]).onClick.AddListener(OnOverrideClick);
        ((Button)Fields[FIELDTAG.RESET]).onClick.AddListener(OnResetClick);
        #endregion

        #region setup confirmDialog
        confirmDialog = Util.ConfirmDialog.Create((GameObject)Fields[FIELDTAG.PROMPT]);
        Debug.Assert( confirmDialog!=null , "SOMETHING-WENT-WRONG!!!" );
        #endregion

        int[] data = GetDataArr();

        UpdateTextDisplays(data);

        Util.SetUpDropDown(Fields,data);
    }

    private void UpdateTextDisplays(int[] data = null)
    {
        int siz = GameController.SPPort.RequiredDataSize();
        if (data == null)
        {
            data = new int[siz];
            GameController.SPPort.GetData(data);
        }
        else
        {
            if (data.Length < siz) throw new ArgumentException("input-array not big-enough");
        }
        Util.UpdateScoreDisplay(Fields, data);
        Util.UpdateUnlockedBirdsDisplay(Fields, data);
    }
    #endregion

    #region onClickListeners
    private static int count = 0;
    private void OnMenuClick()
    {
        SceneFader.instance.StartFading(FlappyBirdUtil.FadeTime, FlappyBirdUtil.Names.MainMenuScene);
    }
    private void OnResetClick()
    {
        GameController.SPPort.ResetAll();
        UpdateTextDisplays();
        ((Dropdown)Fields[FIELDTAG.DROPDOWN]).value = 0;
    }
    private void OnOverrideClick()
    {
        int val = ((Dropdown)Fields[FIELDTAG.DROPDOWN]).value;
        GameController.SPPort.OVERRIDE_UNLOCKED_BIRDS(val+1);
        int[] data = GetDataArr();
        Util.UpdateUnlockedBirdsDisplay(Fields,data);
    }
    #endregion


    private int instCount = 0;
    void OnEnable() { MakeInstance(); }
    void Start() {
        SetUp();
        Debug.Log(string.Format("starting-settings | staticCount=={0} | instCount=={1}", count++, instCount++));
    }

    
}
