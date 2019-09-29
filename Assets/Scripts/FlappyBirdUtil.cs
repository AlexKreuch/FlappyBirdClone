using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlappyBirdUtil : MonoBehaviour
{
    public class Flags
    {
        public class Medals
        {
            public const int White = 1;
            public const int Orange = 2;
            public const int Gold = 4;
        }
    }

    public class MessageToController
    {
        public enum messageType
        {
            BIRD_SELECTION_SETUP_REQUEST
        };
        public class Unit { };
        public interface IMessage
        {
            messageType GetMessageType();
            Unit GetUnit();
        }

        public class BirdSelectionSetupRequest : Unit
        {
            private System.Action<char, bool, bool, bool> Send_Choice_rgbUnlocked = null;

            public enum BirdChoice { RED=0 , GREEN=1 , BLUE=2 }
            private BirdChoice choice = BirdChoice.BLUE;
            private bool[] unlocked = new bool[] { false, false, true };
            public BirdChoice Choice { get { return choice; } }
            public bool IsValid()
            {
                return unlocked[(int)choice];
            }
            private void _setunlocked(BirdChoice bc, bool val) { unlocked[(int)bc] = val; }
            private bool _getunlocked(BirdChoice bc) { return unlocked[(int)bc]; }
            public bool BlueUnlocked { get { return _getunlocked(BirdChoice.BLUE); } set { _setunlocked(BirdChoice.BLUE, value); } }
            public bool GreenUnlocked { get { return _getunlocked(BirdChoice.GREEN); } set { _setunlocked(BirdChoice.GREEN, value); } }
            public bool RedUnlocked { get { return _getunlocked(BirdChoice.RED); } set { _setunlocked(BirdChoice.RED, value); } }

            public void SelectBlue() { choice = BirdChoice.BLUE; }
            public void SelectRed() { choice = BirdChoice.RED; }
            public void SelectGreen() { choice = BirdChoice.GREEN; }

            public void Send()
            {
                char c = "RGB"[(int)choice];
                Send_Choice_rgbUnlocked(c,RedUnlocked,GreenUnlocked,BlueUnlocked);
            }

            private BirdSelectionSetupRequest() { }
            private BirdSelectionSetupRequest(System.Action<char, bool, bool, bool> sender) { Send_Choice_rgbUnlocked = sender; }
            public class Envelope : IMessage
            {
                private BirdSelectionSetupRequest data = null;
                public messageType GetMessageType() { return messageType.BIRD_SELECTION_SETUP_REQUEST; }
                public Unit GetUnit() { return data; }
                private Envelope() { }
                public Envelope(System.Action<char, bool, bool, bool> sender) { data = new BirdSelectionSetupRequest(sender); }
            }
            public static IMessage Create(System.Action<char, bool, bool, bool> SRGBsender)
            {
                return new Envelope(SRGBsender);
            }
        }
    }

    public class Names
    {
        public const string GamePlayScene = "GamePlay";
        public const string MainMenuScene = "MainMenu";
        public class PausePanelFields
        {
            public const string MedalTxtLabel = "Label_Medal";
            public const string ScoreTxtLabel = "Label_Score";
            public const string GameOverTxt = "GameOverText";
            public const string HighScoreTxtLabel = "Label_HighScore";
            public const string MedalImageDisplay = "Medal";
            public const string ScoreTxtDisplay = "Display_Score";
            public const string HighScoreTxtDisplay = "Display_HighScore";
            public const string MainMenuBtn = "Button_Menu";
            public const string PlayBtn = "Button_Play";
            public const string InstructionBtn = "Button_Instructions";
            public const string InstructionsImage = "Image_Instructions";
        }
    }

    public class ResourcePaths
    {
        public const string SpriteRec = "SpriteBox";
        public const string BirdRec = "BirdBox";
    }

    public class Tags
    {
        public class MenuButtons
        {
            public const string Play = "PlayButton";
            public const string Rank = "RankButton";
            public const string Games = "GamesButton";
            public const string Selector = "BirdSelectorButton";
            public const string Twitter = "TwitterButton";
            public const string Share = "ShareButton";
        }
        public static string FlapButtonTag = "FlapButton";
        public static string PauseButtonTag = "PauseButton";
    }

    public const float FadeTime = 1f;

}
