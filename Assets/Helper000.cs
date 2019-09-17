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
    
    

    public class BGbox
    {
        private RectTransform rect0 = null;
        private RectTransform rect1 = null;

        public BGbox(Image image0, Image image1)
        {
            var rs = new RectTransform[] { image0.rectTransform , image1.rectTransform };
            int off = rs[0].localPosition.x < rs[1].localPosition.x ? 0 : 1;
            rect0 = rs[(0 + off) % 2];
            rect1 = rs[(1 + off) % 2];
        }

        public Vector2 Pos
        {
            get
            {
                var v0 = rect0.localPosition;
                var v1 = rect1.localPosition;
                var v = (v0 + v1) / 2;
                return new Vector2(v.x,v.y);
            }
            set
            {
                var cur = (rect0.localPosition + rect1.localPosition) / 2;
                var nxt = new Vector3(value.x,value.y,0);
                var dif = nxt - cur;
                dif.z = 0;
                rect0.localPosition += dif;
                rect1.localPosition += dif;
            }
        }

        public Vector2 Size
        {
            get
            {
                var x = 2 * rect0.sizeDelta.x;
                var y = rect0.sizeDelta.y;
                return new Vector2(x,y);
            }
            set
            {
                var curDist = rect1.localPosition.x - rect0.localPosition.x;
                var newDist = value.x / 2;
                var delDist = (newDist - curDist) / 2;
                var deltaPos = new Vector3(delDist,0f,0f);
                var newSize = new Vector2(value.x/2,value.y);
                rect1.localPosition += deltaPos;
                rect0.localPosition -= deltaPos;
                rect0.sizeDelta = newSize;
                rect1.sizeDelta = newSize;
            }
        }
    }


    
    

    // Update is called once per frame
    void Update()
    {
        
      
    }
}
