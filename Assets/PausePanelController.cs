#define TESTING_MODE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PausePanelController : MonoBehaviour
{
    private const string ShowInstLabel = "show\ninstructions";
    private const string HideInstLabel = "hide\ninstructions";


    /* Use this helper-class to constain and manage fields which be visible ONLY in game-over mode
     * Items should be of type  : Image or Text
     * **/
    private class GameOverItemHolder
    {
        private enum ItemType { IMAGE , TEXT , NA }
        private ItemType GetItemType(object x)
        {
            if (x == null) return ItemType.NA;
            Type t = x.GetType();
            if (t == typeof(Image))
                return ItemType.IMAGE;
            else if (t == typeof(Text))
                return ItemType.TEXT;
            else
                return ItemType.NA;
        }
        private struct ColorItem
        {
            private object data;
            private Action<object, Color> setter;
            private Func<object, Color> getter;
            public Color TheColor
            {
                get { return getter(data); }
                set { setter(data, value); }
            }
            public ColorItem(object d, Action<object, Color> s, Func<object, Color> g)
            {
                data = d;
                setter = s;
                getter = g;
            }
        }
        private class Item
        {
            private ColorItem? data = null;
            public bool GetVisible()
            {
                if (!data.HasValue) return false;
                return data.Value.TheColor.a != 0f;
            }
            public void SetVisible(bool value)
            {
                if (!data.HasValue) return;
                var colorItem = data.Value;
                var tmp = colorItem.TheColor;
                tmp.a = value ? 1f : 0f;
                colorItem.TheColor = tmp;
            }
            public Item(ColorItem d) { data = d; }
        }

        private Color ImgColorGetter(object data)
        {
            var tmp = (Image)data;
            return tmp.color;
        }
        private void ImgColorSetter(object data, Color color)
        {
            var tmp = (Image)data;
            tmp.color = color;
        }
        private Color TxtColorGetter(object data)
        {
            var tmp = (Text)data;
            return tmp.color;
        }
        private void TxtColorSetter(object data, Color color)
        {
            var tmp = (Text)data;
            tmp.color = color;
        }

        private Item ToItem(object obj)
        {
            ColorItem? ci = null;
            ItemType itemType = GetItemType(obj);
            switch (itemType)
            {
                case ItemType.IMAGE: ci = new ColorItem(obj,ImgColorSetter,ImgColorGetter); break;
                case ItemType.TEXT: ci = new ColorItem(obj, TxtColorSetter, TxtColorGetter); break;
            }
            if (ci.HasValue) return new Item(ci.Value);
            return null;
        }

        private bool visible = false;

        private List<Item> items = new List<Item>();

        public void AddItem(object obj)
        {
            var item = ToItem(obj);
            if (item == null) return;
            item.SetVisible(visible);
            items.Add(item);
        }

        public bool Visible
        {
            get { return visible; }
            set
            {
                if (value != visible)
                {
                    visible = value;
                    foreach (Item item in items) item.SetVisible(visible);
                }
            }
        }

    }

    private enum Medal
    {
        WHITE = FlappyBirdUtil.Flags.Medals.White,
        ORANGE = FlappyBirdUtil.Flags.Medals.Orange,
        GOLD = FlappyBirdUtil.Flags.Medals.Gold
    }

    public enum MODE { PAUSED , GAMEOVER }

    public static PausePanelController instance = null;

    #region PausePanelFields
    private Text highScoreDisplay = null;
    private Text scoreDisplay = null;
    private Text gameOverDisplay = null;
    private Image medalDisplay = null;
    private Image instructionsDisplay = null;
    private Button PlayButton = null;
    private Button MainMenuButton = null;
    private Button InstructionsButton = null;
    private Text InstructionsButtonText = null;
    #endregion

    private SpriteResource.MedalSpriteSet? medalSprites = null;
    private GameOverItemHolder gameOverItems = new GameOverItemHolder();
    


    #region button-handlers
    private void PlayButtonHandler()
    {
        Debug.Log("play-btn : "+count++);
        switch (currentMode)
        {
            case MODE.PAUSED: resumeGameAction(); break;
            case MODE.GAMEOVER: restartGameAction(); break;
        }
    }
    private void MainMenuButtonHandler() { Debug.Log("menu-btn : " + count++); }
    private void InstructionsButtonHandler()
    {
        Debug.Log("instr-btn : " + count++);
        if (InstructionsButtonText.text == ShowInstLabel)
        {
            InstructionsButtonText.text = HideInstLabel;
            Color tmp = instructionsDisplay.color;
            tmp.a = 1f;
            instructionsDisplay.color = tmp;
        }
        else
        {
            InstructionsButtonText.text = ShowInstLabel;
            Color tmp = instructionsDisplay.color;
            tmp.a = 0f;
            instructionsDisplay.color = tmp;
        }
    }
    #endregion

    private void SetUp()
    {
        Debug.Log("pause-panel-setup : " + count++);
        instance = this;
        gameOverItems.Visible = currentMode == MODE.GAMEOVER;
        var texts = GetComponentsInChildren<Text>();
        foreach (var txt in texts)
            switch (txt.name)s
            {
                case FlappyBirdUtil.Names.PausePanelFields.ScoreTxtDisplay:scoreDisplay = txt; break;
                case FlappyBirdUtil.Names.PausePanelFields.HighScoreTxtDisplay: highScoreDisplay = txt; break;
                //case FlappyBirdUtil.Names.PausePanelFields.GameOverTxt: gameOverDisplay = txt; MainTainGameOverVisible(); break;
                case FlappyBirdUtil.Names.PausePanelFields.GameOverTxt:
                    gameOverDisplay = txt;
                    gameOverItems.AddItem(gameOverDisplay);
                    break;
                case FlappyBirdUtil.Names.PausePanelFields.MedalTxtLabel: gameOverItems.AddItem(txt); break;
            }
        var images = GetComponentsInChildren<Image>();
        foreach (var img in images)
            switch (img.name)
            {
                case FlappyBirdUtil.Names.PausePanelFields.MedalImageDisplay:medalDisplay = img; gameOverItems.AddItem(medalDisplay); break;
                case FlappyBirdUtil.Names.PausePanelFields.InstructionsImage:instructionsDisplay = img; break;
            }
        var buttons = GetComponentsInChildren<Button>();
        foreach(var btn in buttons)
            switch (btn.name)
            {
                case FlappyBirdUtil.Names.PausePanelFields.MainMenuBtn: MainMenuButton = btn; MainMenuButton.onClick.AddListener(MainMenuButtonHandler); break;
                case FlappyBirdUtil.Names.PausePanelFields.PlayBtn: PlayButton = btn; PlayButton.onClick.AddListener(PlayButtonHandler); break;
                case FlappyBirdUtil.Names.PausePanelFields.InstructionBtn:
                    InstructionsButton = btn;
                    InstructionsButton.onClick.AddListener(InstructionsButtonHandler);
                    InstructionsButtonText = InstructionsButton.gameObject.GetComponentInChildren<Text>();
                    InstructionsButtonText.text = ShowInstLabel;
                    break;
            }

        medalSprites = Resources.Load<SpriteResource>(FlappyBirdUtil.ResourcePaths.SpriteRec).GetMedalSprites();
    }


    int count = 0;
    void OnEnable()
    {
        Debug.Log("pause-panel-enable : " + count++);
        if (instance == null) { SetUp(); gameObject.SetActive(false); }
    }


    public bool PanelTurnedOn
    {
        get { return gameObject.activeSelf; }
        set { gameObject.SetActive(value); }
    }
    public void SetScore(int val) { scoreDisplay.text = val.ToString(); }
    public void SetHighScore(int val) { highScoreDisplay.text = val.ToString(); }
    public void SetMedal(int medalFlag)
    {
        switch((Medal)medalFlag)
        {
            case Medal.WHITE: medalDisplay.sprite = medalSprites.Value.White; break;
            case Medal.ORANGE: medalDisplay.sprite = medalSprites.Value.Orange; break;
            case Medal.GOLD: medalDisplay.sprite = medalSprites.Value.Orange; break;
        }
    }

    public void SetResumeGameAction(Action action)
    {
        if (action == null) action = () => { };
        resumeGameAction = action;
    }
    public void SetRestartGameAction(Action action)
    {
        if (action == null) action = () => { };
        restartGameAction = action;
    }
    public void AddMenuButtonListener(Action action) { MainMenuButton.onClick.AddListener(new UnityEngine.Events.UnityAction(action)); }


    private Action resumeGameAction = () => { };
    private Action restartGameAction = () => { };

    [SerializeField]
    private MODE currentMode = MODE.PAUSED;
    
    private void MainTainGameOverVisible()
    {/*
        if (gameOverDisplay == null) return;
        Color tmp = gameOverDisplay.color;
        tmp.a = (currentMode==MODE.GAMEOVER) ? 1f : 0f;
        gameOverDisplay.color = tmp;
        */
        gameOverItems.Visible = currentMode == MODE.GAMEOVER;
    }
    public MODE Mode
    {
        get { return currentMode; }
        set
        {
            currentMode = value;
            MainTainGameOverVisible();
            
        }
    }


 

#if TESTING_MODE
    private MODE savedMode = MODE.PAUSED;
    private void CheckMode()
    {
        if (savedMode != currentMode)
        {
            savedMode = currentMode;
            MainTainGameOverVisible();
        }
    }
    void Update() { CheckMode(); }
#endif
}
