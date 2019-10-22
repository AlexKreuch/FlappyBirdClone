using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using GooglePlayGames;

public class HELPER04 : MonoBehaviour
{
    #region NAMES
    private const string button0Nm = "BUTTON0";
    #endregion

    #region canvasFields
    private Button button0 = null;



    #endregion

    private class SetUpUtils
    {
        public class BtnSetterUpper
        {
            private Dictionary<string, Action> data = new Dictionary<string, Action>();
            public static BtnSetterUpper Create() { return new BtnSetterUpper(); }
            public BtnSetterUpper AddItem(string nm, Action action) { data.Add(nm,action); return this; }
            public void SetUp()
            {
                var btnList = GameObject.FindObjectsOfType<Button>();
                foreach (var btn in btnList)
                {
                    bool b = data.TryGetValue(btn.name,out Action act);
                    if (b) btn.onClick.AddListener( new UnityEngine.Events.UnityAction(act) );
                }
            }
        }

        public class ItemFinder<T> where T:UnityEngine.Object
        {
            private Dictionary<string, Action<T>> data = new Dictionary<string, Action<T>>();
            public static ItemFinder<T> Create() { return new ItemFinder<T>(); }
            public ItemFinder<T> AddItem(string nm, Action<T> setter) { data.Add(nm,setter); return this; }
            public void Find()
            {
                T[] ts = GameObject.FindObjectsOfType<T>();
                foreach (T obj in ts)
                {
                    var b = data.TryGetValue(obj.name,out Action<T> set);
                    if (b) set(obj);
                }
            }

        }
    }

    private void pressBtn() { SpaceMaker.Report(); }

    private void SetUp()
    {
        SetUpUtils.BtnSetterUpper.Create().AddItem(button0Nm, pressBtn)
            .AddItem(AutTester.authBtnNm,AutTester.press_Auth)
            .AddItem(AutTester.unAuthBtnNm,AutTester.press_unAuth)
            .SetUp();
        SetUpUtils.ItemFinder<Text>.Create().AddItem(AutTester.displayTxtNm, AutTester.SetUpDisplay).Find();
        AutTesterSetUp();
    }

    #region authTester
    private class AutTester
    {
        public const string displayTxtNm = "autText_display";
        public const string authBtnNm = "AUTH";
        public const string unAuthBtnNm = "UN_AUTH";
        
        private static Text displayTxt = null;
        private static Func<bool> IsAuth = () => false;
        private static Action doAuth = () => { };
        private static Action unAuth = () => { };
        public static void SetUpDisplay(Text text) { displayTxt = text; }
        public static void SetUp_isAuth(Func<bool> isA) { IsAuth = isA; }
        public static void SetUp_doAuth(Action doA) { doAuth = doA; }
        public static void SetUp_unAuth(Action unA) { unAuth = unA; }

        public static void press_Auth()
        {
            if (IsAuth()) ShowResult("Already Authorized!");
            else
            {
                doAuth();
            }
        }
        public static void press_unAuth()
        {
            if (!IsAuth()) ShowResult("Already un-Authorized!");
            else
            {
                unAuth();
                ShowResult("logged-out");
            }
        }

        public static void CallBack(bool success, string msg)
        {
            string s0 = success ? "success" : "fail";
            string s = string.Format("LOGGIN-{0} | msg={1}",s0,msg);
            ShowResult(s);
        }

        private static void ShowResult(string res)
        {
            if (displayTxt == null) return;
            string oldStr = displayTxt.text;
            string start = (oldStr == null || oldStr.Length < 2 || oldStr[1] != 'A') ? "<A> " : "<B> ";
            string newStr = start + res;
            displayTxt.text = newStr;
        }



    }

    private void AutTesterSetUp()
    {
        AutTester.SetUp_doAuth(() => { Social.localUser.Authenticate(AutTester.CallBack); });
        AutTester.SetUp_isAuth(() => Social.localUser.authenticated);
        AutTester.SetUp_unAuth(() => { PlayGamesPlatform.Instance.SignOut(); });
    }
    #endregion

    void Start()
    {
        PlayGamesPlatform.Activate();
        SetUp();
    }

    private class SpaceMaker
    {
        private static int count = 0;
        private const int SideBuffer = 10;
        private const int HEIGHT = 10;

        private static void SplitM(int in_ind, int out_ind0, int out_ind1, int[] arr)
        {
            int x = arr[in_ind];
            int y = x / 2;
            int z = y + (x % 2);
            arr[out_ind0] = y;
            arr[out_ind1] = z;
        }
        private static void ComputeDim(int back_ind, int front_ind, int top_ind, int bot_ind, int working_ind, int[] arr)
        {
            arr[working_ind] = SideBuffer;
            SplitM(working_ind, back_ind, front_ind, arr);
            arr[working_ind] = HEIGHT - 1;
            SplitM(working_ind, top_ind, bot_ind, arr);
        }

        private static string mkBuf(int size, char buf)
        {
            return "".PadLeft(size, buf);
        }

        private static IEnumerable<string> LineGenerator(string cap, string msgLine, string empLine, int top, int bot)
        {
              
            yield return cap;
            for (int i = 0; i < top; i++) yield return empLine;
            yield return msgLine;
            for (int i = 0; i < bot; i++) yield return empLine;
            yield return cap;
            
            
        }
        
        private static string GenerateString(string msg)
        {
            
            int bckInd = 0, fntInd = 1, topInd = 2, botInd = 3, workInd = 4;
            int[] arr = new int[5];
            ComputeDim(bckInd, fntInd, topInd, botInd, workInd, arr);

            int lineLen = msg.Length + SideBuffer;

            string cap = mkBuf(lineLen + 2, '*');
            string empLine = "*" + mkBuf(lineLen, ' ') + "*";
            string msgLine = "*" + mkBuf(arr[bckInd], ' ') + msg + mkBuf(arr[fntInd], ' ') + "*";

            var gen = LineGenerator(cap, msgLine, empLine, arr[topInd], arr[botInd]);
            
            return string.Join("\n", gen);
            
        }

        public static void Report()
        {
            string msg = string.Format("SPACER({0})", count++);
            Debug.Log(GenerateString(msg));
        }
    }

    
}
