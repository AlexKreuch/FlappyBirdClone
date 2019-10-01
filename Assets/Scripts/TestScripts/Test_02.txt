using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

[ExecuteAlways]
public class Test_02 : MonoBehaviour
{
    private CircleCollider2D collider = null;
    private CircleCollider2D GetCollider()
    {
        if (collider == null)
        {
            collider = GetComponent<CircleCollider2D>();
        }
        return collider;
    }
    

    private void btnMech(ref bool btn, System.Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }
    
    
    private class accessor<T>
    {
        private System.Action<T> setter;
        private System.Func<T> getter;
        public T VAL
        {
            get
            {
                return getter();
            }
            set
            {
                setter(value);
            }
        }
        private accessor() { }
        private accessor(System.Action<T> s, System.Func<T> g) { setter = s; getter = g; }
        public class Builder
        {
            private bool defaultSet = false;
            private object defaultVal = null;
            private System.Action<T> setter;
            private System.Func<T> getter;
            public Builder SetDefault(T val) { defaultSet = true; defaultVal = val; return this; }
            public Builder SetSetter(System.Action<T> s) { setter = s; return this; }
            public Builder SetGetter(System.Func<T> g) { getter = g; return this; }
            private static void DefaultSetter(T v) { }
            private static System.Func<T> MakeDefaultGetter(T v)
            {
                T f() { return v; }
                return f;
            }
            public accessor<T> Build()
            {
                if (getter == null && !defaultSet) return null;
                var g = getter == null ? MakeDefaultGetter((T)defaultVal) : getter;
                var s = setter == null ? DefaultSetter : setter;
                return new accessor<T>(s, g);
            }
        }
        public static Builder MakeBuilder() { return new Builder(); }
    }
    

    private class FloatAdjuster
    {
        private accessor<float> fval = null;
        private accessor<bool> _0 = null;
        private accessor<bool> _1 = null;
        private accessor<bool> del = null;
        private accessor<string> disp = null;
        private void adjustString_helper0(char c)
        {
            if (c == 'd')
            {
                if (codedVal.Length == 0) return;
                codedVal = codedVal.Substring(0, codedVal.Length - 1);
            }
            else
                codedVal += c;
        }
        private void adjustString_helper1(accessor<bool> btn, char c)
        {
            if (btn.VAL)
            {
                btn.VAL = false;
                adjustString_helper0(c);
            }
        }
        private void adjustString()
        {
            adjustString_helper1(_0, '0');
            adjustString_helper1(_1, '1');
            adjustString_helper1(del, 'd');
        }
        private void adjustVal()
        {
            float cur = .5f, res = 0f;
            foreach (var x in codedVal)
            {
                if (x == '1') res += cur;
                cur /= 2;
            }
            fval.VAL = res;
        }
        private void adjustDisp()
        {
            if (disp.VAL != codedVal) disp.VAL = codedVal;
        }
        private string codedVal = "";
        public void Update()
        {
            adjustString();
            adjustVal();
            adjustDisp();
        }
        public FloatAdjuster
            (
            accessor<float> val,
            accessor<bool> _0btn,
            accessor<bool> _1btn,
            accessor<bool> _delbtn,
            accessor<string> display
            ) { fval = val; _0 = _0btn; _1 = _1btn; del = _delbtn; disp = display; }
    }


    #region offsetAdjuster
    public bool negate = false;
    private float altVal(float v) { int s = negate ? -1 : 1; return -.5f + (v * s); }
    public bool offset_0 = false;
    public bool offset_1 = false;
    public bool offset_d = false;
    public string offset_disp = "";
    private FloatAdjuster offsetAdjuster = null;
    private FloatAdjuster GetOffsetAdjuster()
    {
        if (offsetAdjuster == null)
        {
            var _v = accessor<float>.MakeBuilder()
                .SetGetter(() => altVal(GetCollider().offset.x))
                .SetSetter(v => { var c = GetCollider(); var y = c.offset.y; c.offset = new Vector2(altVal(v), y); })
                .Build();
            var _b0 = accessor<bool>.MakeBuilder()
                .SetGetter(() => offset_0)
                .SetSetter(v => { offset_0 = v; })
                .Build();
            var _b1 = accessor<bool>.MakeBuilder()
                .SetGetter(() => offset_1)
                .SetSetter(v => { offset_1 = v; })
                .Build();
            var _d = accessor<bool>.MakeBuilder()
                .SetGetter(() => offset_d)
                .SetSetter(v => { offset_d = v; })
                .Build();
            var _s = accessor<string>.MakeBuilder()
                .SetGetter(() => offset_disp)
                .SetSetter(v => { offset_disp = v; })
                .Build();
            offsetAdjuster = new FloatAdjuster(_v,_b0,_b1,_d,_s);
        }
        return offsetAdjuster;
    }
    #endregion

    #region radiousAdjuster
    public bool _1 = false;
    public bool _0 = false;
    public bool del = false;
    public string display = "";
    private FloatAdjuster radiousAdjuster = null;
    private FloatAdjuster GetRadiousAdjuster()
    {
        if (radiousAdjuster == null)
        {
            var _v = accessor<float>.MakeBuilder()
                .SetGetter(() => GetCollider().radius)
                .SetSetter(v => { GetCollider().radius = v; })
                .Build();
            var b1 = accessor<bool>.MakeBuilder()
                .SetGetter(() => _1)
                .SetSetter(v => { _1 = v; })
                .Build();
            var b0 = accessor<bool>.MakeBuilder()
                .SetGetter(() => _0)
                .SetSetter(v => { _0 = v; })
                .Build();
            var bd = accessor<bool>.MakeBuilder()
                .SetGetter(() => del)
                .SetSetter(v => { del = v; })
                .Build();
            var di = accessor<string>.MakeBuilder()
                .SetGetter(() => display)
                .SetSetter(v => { display = v; })
                .Build();
            radiousAdjuster = new FloatAdjuster(_v,b0,b1,bd,di);
        }
        return radiousAdjuster;
    }
    #endregion
    void Update()
    {
      
        GetOffsetAdjuster().Update();
        GetRadiousAdjuster().Update();
    }

   
}
