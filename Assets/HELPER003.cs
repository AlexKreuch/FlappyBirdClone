#define BIRDIN_TESTING_MODE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[ExecuteAlways]
public class HELPER003 : MonoBehaviour
{
    private class KeySystem
    {
        private Func<string> access = null;
        private int recordedLen = 0;
        private Dictionary<char, Action> functions = null;
        private KeySystem() { }
        private KeySystem(Func<string> ac, Dictionary<char, Action> fns)
        {
            access = ac;
            recordedLen = access().Length;
            functions = fns;
        }
        public void Update()
        {
            string cur = access();
            int curLen = cur.Length;
            if (curLen == recordedLen + 1)
            {
                char k = cur[recordedLen];
                bool flg = functions.TryGetValue(k, out Action action);
                if (flg) action();
            }
            recordedLen = curLen;
        }

        public class Builder
        {
            private Func<string> ac = null;
            private Dictionary<char, Action> f = new Dictionary<char, Action>();
            public Builder SetAc(Func<string> a) { ac = a; return this; }
            public Builder AddKeyAction(char k, Action act) { f.Add(k, act); return this; }
            public KeySystem Build()
            {
                if (ac == null) ac = () => "";
                return new KeySystem(ac, f);
            }
        }
        public static Builder MakeBuilder() { return new Builder(); }
    }
    private void BtnMech(ref bool btn, Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }

    public string joystick = "";
    private KeySystem joystickMech = null;
    private KeySystem GetJoystickMech()
    {
        if (joystickMech == null)
        {
            #region conditionally define testing-mode 'invincible-toggle'
#if BIRDIN_TESTING_MODE
            void ToggleInvincible()
            {
                bool tmp = Bird.instance.Invincible;
                tmp = !tmp;
                Bird.instance.Invincible = tmp;
                flag = tmp ? "TRUE" : "FALSE";
            }
#else
            void ToggleInvincible(){}
#endif
            #endregion
            float scaler = 1f;
            Vector3 UP = new Vector3(0f, 1f, 0f);
            Vector3 DOWN = new Vector3(0f, -1f, 0f);
            Vector3 LEFT = new Vector3(-1f, 0f, 0f);
            Vector3 RIGHT = new Vector3(1f, 1f, 0f);
            
            Action mkAct(Vector3 vec)
            {
                vec *= scaler;
                void f()
                {
                    if (Bird.instance != null) Bird.instance.gameObject.transform.position += vec;
                }
                return f;
            }

            joystickMech = KeySystem.MakeBuilder()
                .SetAc(() => joystick)
                .AddKeyAction('i', mkAct(UP))
                .AddKeyAction('k', mkAct(DOWN))
                .AddKeyAction('j', mkAct(LEFT))
                .AddKeyAction('l', mkAct(RIGHT))
                .AddKeyAction(' ', Flap)
                .AddKeyAction('p', PressPause)
                .AddKeyAction('8',ToggleInvincible)
                .Build();
        }
        return joystickMech;
    }
    private void RunJoyStick()
    {
        if (Application.isPlaying) GetJoystickMech().Update();
    }


    private Button flpBtn = null, pauseBtn = null, playBtn;
    private Button GetFlpBtn()
    {
        if (flpBtn == null)
        {
            var lst = GameObject.FindGameObjectsWithTag(FlappyBirdUtil.Tags.FlapButtonTag);
            if (lst.Length != 0) flpBtn = lst[0].GetComponent<Button>();
        }
        return flpBtn;
    }
    private Button GetPauseBtn()
    {
        if (pauseBtn == null)
        {
            var lst = GameObject.FindGameObjectsWithTag(FlappyBirdUtil.Tags.PauseButtonTag);
            if (lst.Length != 0) pauseBtn = lst[0].GetComponent<Button>();
        }
        return pauseBtn;
    }
    private Button GetPlayBtn()
    {
        if (playBtn == null)
        {
            var btns = FindObjectsOfType<Button>();
            foreach (var btn in btns)
            {
                if (btn.name == FlappyBirdUtil.Names.PausePanelFields.PlayBtn)
                {
                    playBtn = btn;
                    break;
                }
            }
        }
        return playBtn;
    }
    private bool IsPauseed() { return PausePanelController.instance.PanelTurnedOn; }

    private void Flap()
    {
        var btn = GetFlpBtn();
        if (btn == null) return;
        btn.onClick.Invoke();
    }
    private void PressPause()
    {
        if (IsPauseed())
        {
            GetPlayBtn().onClick.Invoke();
        }
        else
        {
            GetPauseBtn().onClick.Invoke();
        }
    }


    void Update()
    {
        RunJoyStick();
        MaintainScale();
    }

    

    public GameObject ScaleThis = null;
    private class ScaleMech
    {
        private static void SetterDefault(Vector2 v) { }
        private static Vector2 GetterDefault() { return new Vector2(1f,1f); }

        private static GameObject savedObject = null;
        private static GameObject[] objs = null;
        private static Action<Vector2> setter = SetterDefault;
        private static Func<Vector2> getter = GetterDefault;

        public static Vector2 Size
        {
            get { return getter(); }
            set { setter(value); }
        }

        public static void SetUp(GameObject obj)
        {
            if (obj == savedObject) return;
            savedObject = obj;
            if (savedObject == null) { setter = SetterDefault; getter = GetterDefault; return; }
            objs = new GameObject[] { savedObject };
            void _setter(Vector2 v)
            {
                var z = savedObject.transform.localScale.z;
                var vec = new Vector3(v.x,v.y,z);
                foreach (var _obj in objs) { _obj.transform.localScale = vec; }
            }
            Vector2 _getter()
            {
                var v = savedObject.transform.localScale;
                return new Vector2(v.x,v.y);
            }
            setter = _setter;
            getter = _getter;
        }
    }
    private void MaintainScale()
    {
        ScaleMech.SetUp(ScaleThis);
        var tmp = ScaleMech.Size;
        if (tmp.x!=tmp.y)
        {
            tmp.y = tmp.x;
            ScaleMech.Size = tmp;
        }
    }


    public string flag = "???";
}
