using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[ExecuteAlways]
public class Helper000 : MonoBehaviour
{

    private void BtnMech(ref bool btn, Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }
    
    private class ReportBuilder
    {
        private int gpSize = 10;
        private string title = "<REPORT> :";
        private List<String> fields = new List<string>();
        public ReportBuilder SetGap(int v)
        {
            gpSize = v >= 0 ? v : 0;
            return this;
        }
        public ReportBuilder SetTitle(string v)
        {
            if (v == null) v = "";
            title = v + ": ";
            return this;
        }
        public ReportBuilder AddField(string v)
        {
            if (v == null) v = "";
            fields.Add(v);
            return this;
        }
        public string Build()
        {
            IEnumerable<string> Lines()
            {
                yield return title;
                foreach (string x in fields) yield return x;
            }
            string gp = "\n".PadRight(gpSize + 1);
            return string.Join(gp, Lines());
        }
        public void Spit() { Debug.Log(this.Build()); }
        public static ReportBuilder CreateBuilder() { return new ReportBuilder(); }

    }

    private class ScalerClass
    {
        private static RectTransform[] rects = null;
        private static float tracker = 1f;
        private static RectTransform[] GetRects()
        {
            if (rects == null)
            {
                var ims = GameObject.FindObjectsOfType<Image>();
                rects = new RectTransform[ims.Length];
                int c = 0;
                foreach (var im in ims) rects[c++] = im.rectTransform;

            }
            return rects;
        }
        public static void Update(float size, Action<float> setter = null)
        {
            if (size == 0) size = .000001f;
            if (size == tracker) return;
            float factor = size / tracker;
            var list = GetRects();
            foreach (var r in list)
            {
                r.localScale *= factor;
                r.localPosition *= factor;
            }
            tracker = size;
            if (setter != null) setter(size);
        }
        public static void RESET()
        {
            var list = GetRects();
            Vector3 scale = new Vector3(1f, 1f, 1f);
            foreach (var r in list)
            {
                
                float xscale = r.localScale.x;
                if (xscale == 0f) xscale = .00001f;
                
                r.localScale = scale;
                r.localPosition /= xscale;
            }
            tracker = 1f;
        }
    }

    private class ShifterClass
    {
        private Func<Image, bool> filter = null;
        private RectTransform[] rects = null;
        public ShifterClass(Func<Image, bool> f) { filter = f; }
        private Vector2 tracker = new Vector2();
        private RectTransform[] GetRects()
        {
            if (rects == null)
            {
                Func<Image, bool> fil = filter != null ? filter : (v => true);
                List<RectTransform> list = new List<RectTransform>();
                var ims = GameObject.FindObjectsOfType<Image>();
                foreach (var im in ims)
                {
                    if (fil(im)) list.Add(im.rectTransform);
                }
                rects = list.ToArray();
            }
            return rects;
        }

        public void Update(Vector2 pos)
        {
            if (pos == tracker) return;
            var delta_v2 = pos - tracker;
            var delta = new Vector3(delta_v2.x,delta_v2.y,0f);
            var list = GetRects();
            foreach (var r in list) r.localPosition += delta;
            tracker = pos;
        }

    }

    private ShifterClass groundShifter = new ShifterClass(v=> (v.name.Length>0&&v.name[0]=='G'));
    private ShifterClass backgroundShifter = new ShifterClass(v => (v.name.Length > 0 && v.name[0] == 'B'));

    public float scaler = 1f;
    public Vector2 BG = new Vector2();
    public Vector2 GR = new Vector2();
    // Update is called once per frame
    void Update()
    {
        ScalerClass.Update(scaler, v=> { scaler = v; });
        groundShifter.Update(GR);
        backgroundShifter.Update(BG);
        BtnMech(ref btn, press_btn);
    }

    public enum FUN { RESET_SCALES }
    public FUN btnFunc = FUN.RESET_SCALES;
    public bool btn = false;
    private void press_btn()
    {
        switch (btnFunc)
        {
            case FUN.RESET_SCALES:ResetScales(); break;
        }
    }
    private void ResetScales()
    {
        scaler = 1f;
        ScalerClass.RESET();
    }
}
