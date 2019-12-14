using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class HELPER008 : MonoBehaviour
{

    private class Counter
    {
        private const string ADDRESS = "counter-address";
        public static int Val
        {
            get { return PlayerPrefs.GetInt(ADDRESS,0); }
            set { PlayerPrefs.SetInt(ADDRESS,value); }
        }
    }

    const string imgNm = "prompt_Background";
    const string txtNm = "prompt_Text";
    const string confirmBtnName = "Confirm";
    const string cancelBtnName = "Cancel";


    private interface IValbox<T>
    {
        T Get();
        void Set(T val);
    }
    private class ValboxGrp<T> : IValbox<T>
    {
        private IValbox<T>[] valboxes = null;
        private Func<T,T, bool> Eq = null;
        public ValboxGrp(Func<T,T,bool> eq, IValbox<T>[] boxes) { valboxes = boxes; Eq = eq; }
        public T Get() { return valboxes[0].Get(); }
        public void Set(T val)
        {
            if (Eq(valboxes[0].Get(),val)) return;
            foreach (var bx in valboxes) bx.Set(val);
        }
    }
    private class ValBoxMaker<T, G>
    {
        private Func<G, IValbox<T>> mkr = null;
        private Func<T, T, bool> Eq = null;
        public ValBoxMaker(Func<G, IValbox<T>> _mkr, Func<T, T, bool> _Eq)
        {
            mkr = _mkr;
            Eq = _Eq;
        }
        public ValboxGrp<T> MakeBox(G g, params G[] gs)
        {
            IValbox<T>[] valboxes = new IValbox<T>[gs.Length+1];
            valboxes[0] = mkr(g);
            for (int i = 1; i < valboxes.Length; i++)
                valboxes[i] = mkr(gs[i-1]);

            return new ValboxGrp<T>(Eq,valboxes);
        }
    }
    private T FindThing<T>(string name) where T:UnityEngine.Object
    {
        var lst = FindObjectsOfType<T>();
        foreach (T t in lst) if (t.name == name) return t;
        return null;
    }
     

    private class SizeDeltaBox : IValbox<Vector2>
    {
        private RectTransform rt = null;
        private SizeDeltaBox(RectTransform rt) { this.rt = rt; }
        public Vector2 Get() { return rt.sizeDelta; }
        public void Set(Vector2 val) { rt.sizeDelta = val; }
        public static bool Eq(Vector2 v0, Vector2 v1) { return v0 == v1; }
        public static IValbox<Vector2> Mkr(RectTransform rectTransform) { return new SizeDeltaBox(rectTransform); }
    }
    private class YBox : IValbox<float>
    {
        private Transform transform = null;
        private YBox(Transform transform) { this.transform = transform; }
        public float Get() { return transform.position.y; }
        public void Set(float val)
        {
            var tmp = transform.position;
            tmp.y = val;
            transform.position = tmp;
        }

        public static bool Eq(float f0, float f1) { return f0 == f1; }
        public static IValbox<float> Mkr(Transform transform) { return new YBox(transform); }
    }

    private IValbox<float> _buttonY = null;
    private IValbox<float> _dispY = null;
    private IValbox<Vector2> _dispR = null;
    private Text text = null;

    private void _setup_0()
    {
        var img = FindThing<Image>(imgNm);
        var bt0 = FindThing<Button>(confirmBtnName);
        var bt1 = FindThing<Button>(cancelBtnName);
        var txt = FindThing<Text>(txtNm);

        var fboxMkr = new ValBoxMaker<float, Transform>(YBox.Mkr, YBox.Eq);
        var vboxMkr = new ValBoxMaker<Vector2, RectTransform>(SizeDeltaBox.Mkr,SizeDeltaBox.Eq);

        _buttonY = fboxMkr.MakeBox(bt0.transform,bt1.transform);
        _dispY = fboxMkr.MakeBox(img.transform,txt.transform);
        _dispR = vboxMkr.MakeBox(img.rectTransform,txt.rectTransform);

        text = txt;
    }

    public float btn_h = 0.0f;
    public float dis_h = 0.0f;
    public Vector2 dis_r = new Vector2();

    [TextArea]
    public string msg = "";

    private void _setup()
    {
        _setup_0();
        btn_h = _buttonY.Get();
        dis_h = _dispY.Get();
        dis_r = _dispR.Get();
        msg = text.text;
    }

    private void _update()
    {
        _buttonY.Set(btn_h);
        _dispY.Set(dis_h);
        _dispR.Set(dis_r);
        text.text = msg;
    }

    void Start()
    {
        Debug.Log("start-called : " + Counter.Val++);
        _setup();
    }
    void Update() { _update(); }


    #region reset-dialog-data
    /*
    btn_h := 470
    dis_h := 690
    dir_r := (X=500 , Y=325)
    msg = "Are you sure?\n(this will reset-\ngame progress)"
    */
    #endregion

    #region override-unlocked_birds-data
    /*
    btn_h := 530
    dis_h := 690
    dir_r := (X=615 , Y=170)
    msg = "Override Unlocked-\nBirds?"
    */
    #endregion
}
