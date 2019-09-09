using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Runtime.CompilerServices;
using System;
[ExecuteAlways]
public class Test_00 : MonoBehaviour
{
   
    private void btnMech(ref bool btn, System.Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }

    private class Acc<T>
    {
        private Func<T> getter = null;
        private Action<T> setter = null;
        public T Val
        {
            get { return getter(); }
            set { setter(value); }
        }
        private Acc() { }
        private Acc(Func<T> g, Action<T> s) { getter = g; setter = s; }
        public class Builder
        {
            private Func<T> _getter = null;
            private Action<T> _setter = null;
            public Builder SetGetter(Func<T> g) { _getter = g; return this; }
            public Builder SetSetter(Action<T> s) { _setter = s; return this; }
            public Acc<T> Build()
            {
                if (_getter == null || _setter == null) return null;
                return new Acc<T>(_getter, _setter);
            }
        }
        public static Builder MakeBuilder() { return new Builder(); }
    }

    private class PotentialAcc<C,T>
    {
        private Acc<T> acc = null;
        private Func<C, T> _get = null;
        private Action<C, T> _set = null;
        private PotentialAcc() { }
        private PotentialAcc(Func<C, T> g, Action<C, T> s) { _get = g; _set = s; }
        public class Builder
        {
            private Func<C,T> _getter = null;
            private Action<C,T> _setter = null;
            public Builder SetGetter(Func<C,T> g) { _getter = g; return this; }
            public Builder SetSetter(Action<C, T> s) { _setter = s; return this; }
            public PotentialAcc<C,T> Build()
            {
                if (_getter == null || _setter == null) return null;
                return new PotentialAcc<C,T>(_getter, _setter);
            }
        }
        public static Builder MakeBuilder() { return new Builder(); }
        public Acc<T> ToAcc(C context)
        {
            if (acc == null)
            {
                T g() { return _get(context); }
                void s(T v) { _set(context, v); }
                acc = Acc<T>.MakeBuilder().SetGetter(g).SetSetter(s).Build();
            }
            return acc;
        }
    }

    #region pacx and pacy
    private PotentialAcc<GameObject, float> PacX = PotentialAcc<GameObject, float>.MakeBuilder()
        .SetGetter((go) => go.transform.localScale.x)
        .SetSetter((go, v) =>
        {
            var tmp = go.transform.localScale;
            tmp.x = v;
            go.transform.localScale = tmp;
        })
        .Build();


    private PotentialAcc<GameObject, float> PacY = PotentialAcc<GameObject, float>.MakeBuilder()
        .SetGetter((go) => go.transform.localScale.y)
        .SetSetter((go, v) =>
        {
            var tmp = go.transform.localScale;
            tmp.y = v;
            go.transform.localScale = tmp;
        })
        .Build();
    #endregion




    private void Maintain0()
    {
        PacY.ToAcc(gameObject).Val = PacX.ToAcc(gameObject).Val;
    }

    private class FloatAdjuster
    {
        private float minVal = 0f;
        private float maxVal = 1f;
        private float curVal = 0f;
        private string codedVal = "";
        public FloatAdjuster() { }
        public FloatAdjuster(float min, float max) { minVal = min; maxVal = max; curVal = min;  }

        public string GetCode() { return codedVal; }
        public float GetVal() { return curVal; }

        private void SetCurVal()
        {
            float chg = maxVal / 2;
            curVal = minVal;
            foreach (char c in codedVal)
            {
                if (c == '1') curVal += chg;
                chg /= 2;
            }
        }

        public void _1() { codedVal += '1'; SetCurVal(); }
        public void _0() { codedVal += '0'; }
        public void _d()
        {
            int len_minus1 = codedVal.Length - 1;
            if (len_minus1 == -1) return;
            bool adjustNeeded = codedVal[len_minus1] == '1';
            codedVal = codedVal.Substring(0,len_minus1);
            if (adjustNeeded) SetCurVal();
        }
    }

    void Start() { Debug.Log("start-called"); }
    

    private class HelpfullThing
    {
        private class FloatBox
        {
            private static BoxCollider2D collider = null;
            public static void Set(float v)
            {
                if (collider == null) return;
                collider.size = new Vector2(v,v);
            }
            public static float Get()
            {
                if (collider == null) return 0f;
                return collider.size.x;
            }
            public static void SetUp(GameObject go)
            {
                go.TryGetComponent<BoxCollider2D>(out collider);
            }
            public static bool IsSetUp() { return collider != null; }
        }
        private static Acc<bool>[] keyboard = new Acc<bool>[3]; // [ _0 , _1 , _del ]
        private static Acc<string> display = null;
        private static int accFlags = 0;
        private static FloatAdjuster floatAdjuster = new FloatAdjuster();
        public static void SetContext(GameObject go)
        {
            if (go == null) return;
            FloatBox.SetUp(go);
        }
        public static void SetKey(Acc<bool> key, int index)
        {
            if (key == null || index < 0 || index > 2) return;
            keyboard[index] = key;
            int p = (int)Mathf.Pow(2, index);
            if ((accFlags / p) % 2 == 0) accFlags += p;
        }
        public static void SetDisplay(Acc<string> disp)
        {
            if (disp == null) return;
            display = disp;
            if (accFlags < 8) accFlags += 8;
        }
        public static bool IsSetUp()
        {
            var res = accFlags == 15 && FloatBox.IsSetUp();
            return accFlags == 15 && FloatBox.IsSetUp();
        }
        public static void Update()
        {
            if (!IsSetUp()) return;
            if (keyboard[0].Val) { keyboard[0].Val = false; floatAdjuster._0(); }
            if (keyboard[1].Val) { keyboard[1].Val = false; floatAdjuster._1(); }
            if (keyboard[2].Val) { keyboard[2].Val = false; floatAdjuster._d(); }
            display.Val = floatAdjuster.GetCode();
            FloatBox.Set(floatAdjuster.GetVal() );
        }
    }

    public bool _0 = false;
    public bool _1 = false;
    public bool _del = false;
    public string _dis = "";

    private void SetUpHelper()
    {
        #region Make Acc<T>'s
        var ac0 = Acc<bool>.MakeBuilder()
            .SetGetter(() => _0)
            .SetSetter(v => { _0 = v; })
            .Build();
        var ac1 = Acc<bool>.MakeBuilder()
            .SetGetter(() => _1)
            .SetSetter(v => { _1 = v; })
            .Build();
        var acdel = Acc<bool>.MakeBuilder()
            .SetGetter(() => _del)
            .SetSetter(v => { _del = v; })
            .Build();
        var acdis = Acc<string>.MakeBuilder()
            .SetGetter(() => _dis)
            .SetSetter(v => { _dis = v; })
            .Build();
        #endregion
        HelpfullThing.SetContext(gameObject);
        HelpfullThing.SetDisplay(acdis);
        HelpfullThing.SetKey(ac0, 0);
        HelpfullThing.SetKey(ac1, 1);
        HelpfullThing.SetKey(acdel, 2);
    }

    private void Maintain()
    {
        if (HelpfullThing.IsSetUp())
            HelpfullThing.Update();
        else
            SetUpHelper();
    }

    void Update() { Maintain(); }

    // gameObject.transform.localScale.x

    
}
