using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


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

    public string joystick = "";
    private KeySystem joystickMech = null;
    private KeySystem GetJoystickMech()
    {
        if (joystickMech == null)
        {
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
                .Build();
        }
        return joystickMech;
    }
    private void RunJoyStick()
    {
        if (Application.isPlaying) GetJoystickMech().Update();
    }


    private Button flpBtn = null;
    private Button GetFlpBtn()
    {
        if (flpBtn == null)
        {
            var lst = GameObject.FindGameObjectsWithTag(FlappyBirdUtil.Tags.FlapButtonTag);
            if (lst.Length != 0) flpBtn = lst[0].GetComponent<Button>();
        }
        return flpBtn;
    }

    private void Flap()
    {
        var btn = GetFlpBtn();
        if (btn == null) return;
        btn.onClick.Invoke();
    }

    void Update()
    {
        RunJoyStick();
    }
}
