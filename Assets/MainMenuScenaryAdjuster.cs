using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[ExecuteAlways]
public class MainMenuScenaryAdjuster : MonoBehaviour
{
    private void BtnMech(ref bool btn, Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }

    private const string BackgroundName = "Background";
    private const string GroundName = "Ground";

    private class TwoPanelItem
    {
        private class PsRect
        {
            private RectTransform rect = null;
            public Vector2 Pos
            {
                get
                {
                    var tmp = rect.localPosition;
                    return new Vector2(tmp.x,tmp.y);
                }
                set
                {
                    float z = rect.localPosition.z;
                    rect.localPosition = new Vector3(value.x,value.y,z);
                }
            }
            public Vector2 Size
            {
                get
                {
                    var sca = rect.localScale;
                    var res = rect.sizeDelta;
                    res.x *= sca.x;
                    res.y *= sca.y;
                    return res;
                }
                set
                {
                    var sca = rect.localScale;
                    if (sca.x == 0) sca.x = .000001f;
                    if (sca.y == 0) sca.y = .000001f;
                    var res = value;
                    res.x /= sca.x;
                    res.y /= sca.y;
                    rect.sizeDelta = res;
                }
            }
            public PsRect(Image image) { rect = image.rectTransform; }
        }

        private PsRect[] rects = null;

        public TwoPanelItem(Image image0, Image image1)
        {
            rects = new PsRect[] { new PsRect(image0), new PsRect(image1) };
            Reset();
        }

        private Vector2 ComputePos()
        {
            return (rects[0].Pos + rects[1].Pos) / 2;
        }
        private Vector2 ComputeOffset()
        {
            return new Vector2(rects[0].Size.x/2,0f);
        }

        public void Reset()
        {
            if (rects[0].Size != rects[1].Size)
            {
                var tmp = .5f * (rects[0].Size + rects[1].Size);
                foreach (var rect in rects) { rect.Size = tmp; }
            }
            var pos = ComputePos();
            var delta = new Vector2(rects[0].Size.x/2,0f);
            rects[0].Pos = pos - delta;
            rects[1].Pos = pos + delta;
        }

        public Vector2 Pos
        {
            get { return ComputePos(); }
            set
            {
                var offset = ComputeOffset();
                rects[0].Pos = value - offset;
                rects[1].Pos = value + offset;
            }
        }
        public Vector2 Size
        {
            get
            {
                var tmp = rects[0].Size;
                tmp.x *= 2;
                return tmp;
            }
            set
            {
                var pos = ComputePos();
                var siz = value; siz.x /= 2;
                var del = value; del.y = 0;
                int sign = -1;
                foreach (var r in rects)
                {
                    r.Size = siz;
                    r.Pos = pos + sign * del;
                    sign *= -1;
                }
            }
        }

    }

    private TwoPanelItem BackGround = null;
    private TwoPanelItem Ground = null;

    public enum BtnFunction { LOAD_ITEMS, RESET_ITEMS }
    public BtnFunction btnFunction = BtnFunction.LOAD_ITEMS;
    public bool btn = false;
    private void PressBtn()
    {
        switch (btnFunction)
        {
            case BtnFunction.LOAD_ITEMS: LoadItems(); break;
            case BtnFunction.RESET_ITEMS: ResetItems(); break;
        }
    }
    private void LoadItems()
    {
        bool checkValid()
        {
            if (images == null || images.Length != 4) return false;
            foreach (var x in images) if (x == null) return false;
            return true;
        }
        if (!checkValid()) return;
        Ground = new TwoPanelItem(images[0],images[1]);
        BackGround = new TwoPanelItem(images[2],images[3]);

        BG = Util.Init(BackGround);
        GR = Util.Init(Ground);
    }
    private void ResetItems()
    {
        BackGround = null;
        Ground = null;
        BG = new Vector4();
        GR = new Vector4();
    }
    public Image[] images = new Image[0];

    public string ItemsLoaded = "FALSE";
    private void Maintain_ItemsLoaded()
    {
        bool val = BackGround != null && Ground != null;
        ItemsLoaded = val ? "TRUE" : "FALSE";
    }

    public Vector4 BG = new Vector4();
    public Vector4 GR = new Vector4();
    private class Util
    {
        public static Vector4 Init(TwoPanelItem item)
        {
            var p = item.Pos;
            var s = item.Size;
            return new Vector4(p.x,p.y,s.x,s.y);
        }
        public static void Update(Vector4 vector, TwoPanelItem item)
        {
            var p = new Vector2(vector.x,vector.y);
            var s = new Vector2(vector.z,vector.w);
            item.Pos = p;
            item.Size = s;
        }
    }
    private void MaintainVecs()
    {
        if (BackGround == null || Ground == null) return;
        Util.Update(BG,BackGround);
        Util.Update(GR,Ground);
    }

    // Update is called once per frame
    void Update()
    {
        BtnMech(ref test, Test);
        BtnMech(ref btn, PressBtn);
        Maintain_ItemsLoaded();
        MaintainVecs();
    }

    public enum TT { MAKE_ITEM, TESTCENTER, TESTSIZE, TESTOFFSET, TESTPOS , TESTSCALE }
    public TT TestType = TT.MAKE_ITEM;
    public bool test = false;
    private void Test()
    {
        switch (TestType)
        {
            case TT.MAKE_ITEM: test_MakeItem(); break;
            case TT.TESTCENTER:test_center(); break;
            case TT.TESTSIZE:test_size(); break;
            case TT.TESTOFFSET:test_offset(); break;
            case TT.TESTPOS:test_pos(); break;
            case TT.TESTSCALE:test_scale(); break;
        }
    }
    private void test_MakeItem()
    {
        if (images == null || images.Length < 2 || images[0] == null || images[1] == null) return;
        var im0 = images[0];
        var im1 = images[1];
        var item = new TwoPanelItem(im0, im1);
    }
    private void test_center()
    {
        if (images == null || images.Length < 2 || images[0] == null || images[1] == null) return;
        var pos0 = images[0].rectTransform.localPosition;
        var pos1 = images[1].rectTransform.localPosition;

        var pos = (pos0 + pos1) / 2;

        var res = new Vector2(pos.x, pos.y);

        string msg = string.Format("center==({0},{1})", res.x, res.y);
        string start = (disp == null || disp.Length == 0 || disp[0] != 'A') ? "A | " : "B | ";
        disp = start + msg;
    }
    private void test_size()
    {
        if (images == null || images.Length == 0 || images[0] == null) return;
        Vector2 res = images[0].rectTransform.sizeDelta;

        string msg = string.Format("size==({0},{1})", res.x, res.y);
        string start = (disp == null || disp.Length == 0 || disp[0] != 'A') ? "A | " : "B | ";
        disp = start + msg;
    }
    private void test_offset()
    {
        if (images == null || images.Length == 0 || images[0] == null) return;
        Vector2 sizeDelta = images[0].rectTransform.sizeDelta;

        float res = sizeDelta.x / 2;

        string msg = string.Format("offset=={0}", res);
        string start = (disp == null || disp.Length == 0 || disp[0] != 'A') ? "A | " : "B | ";
        disp = start + msg;
    }
    private void test_pos()
    {
        if (images == null || images.Length == 0 || images[0] == null) return;
        Vector2 res = images[0].rectTransform.localPosition;

        string msg = string.Format("localPosition==({0},{1})", res.x, res.y);
        string start = (disp == null || disp.Length == 0 || disp[0] != 'A') ? "A | " : "B | ";
        disp = start + msg;
    }
    private void test_scale()
    {
        if (images == null || images.Length == 0 || images[0] == null) return;
        Vector2 res = images[0].rectTransform.localScale;

        string msg = string.Format("localScale==({0},{1})", res.x, res.y);
        string start = (disp == null || disp.Length == 0 || disp[0] != 'A') ? "A | " : "B | ";
        disp = start + msg;
    }
    public string disp = "";

   

    // return new Vector2(rects[0].Size.x/2,0f);
}
