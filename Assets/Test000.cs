using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;


public class Test000 : MonoBehaviour
{
    private class Utils
    {
        private static string MessageBox(string input, int buffer = 20)
        {
            string[] lines = input.Split('\n');
            if (lines.Length == 0) lines = new string[] { "" };
            int maxLen = 0;
            foreach (var x in lines) if (x.Length > maxLen) maxLen = x.Length;
            string space = "".PadLeft(buffer);
            string cap = space + "".PadLeft(maxLen + 2, '*');
            IEnumerable<string> enumerable()
            {
                yield return cap;
                foreach (string x in lines)
                {
                    yield return space + '*' + x.PadRight(maxLen)+ '*';
                }
                yield return cap;
            }
            return string.Join("\n",enumerable());
        }
        public class LogBuilder
        {
            private struct LogUnit
            {
                public bool isBlock;
                public string title;
                public string msg;
                public LogUnit(bool isbl, string t, string m)
                {
                    isBlock = isbl;
                    title = t;
                    msg = m;
                }
            }
            private string title = "";
            private Queue<LogUnit> logUnits = new Queue<LogUnit>();

            private int offset = 10, blockOffset = 20, maxTitleLen = 0;
            public LogBuilder SetTitle(string t) { title = t; return this; }
            public LogBuilder SetOffset(int v) { offset = v; return this; }
            public LogBuilder SetBlockOffset(int v) { blockOffset = v; return this; }
            public LogBuilder AddItem(string name, string msg)
            {
                if (name.Length > maxTitleLen) maxTitleLen = name.Length;
                logUnits.Enqueue(new LogUnit(false,name,msg));
                return this;
            }
            public LogBuilder AddBlock(string name, string msg)
            {
                logUnits.Enqueue(new LogUnit(true, name, msg));
                return this;
            }
            public string Build()
            {
                string space = "\n".PadRight(offset+1);
                string res = title + " : ";
                string tmp = "";
                foreach (var unit in logUnits)
                {
                    res += space;
                    if (unit.isBlock)
                    {
                        res += unit.title + " : \n" + MessageBox(unit.msg);
                    }
                    else
                    {
                        tmp = unit.title.PadRight(maxTitleLen) + " : " + unit.msg;
                        res += tmp;
                    }
                }
                return res;
            }
            public static LogBuilder Create() { return new LogBuilder(); }
        }

        public class StatBox
        {
            public static int TypeToInt(Tp t)
            {
                switch (t)
                {
                    case Tp.NONE:return 0;
                    case Tp.DYN: return 1;
                    case Tp.KIN: return 2;
                    case Tp.STATIC: return 3;
                    default: return -1;
                }
            }
            public static Tp IntToType(int i)
            {
                const int len = 4;
                bool negative = i < 0;
                if (negative) i *= -1;
                i = i % len;
                if (negative && i != 0) i = len - i;
                switch (i)
                {
                    case 0: return Tp.NONE;
                    case 1: return Tp.DYN;
                    case 2: return Tp.KIN;
                    case 3: return Tp.STATIC;
                    default: return Tp.NONE;
                }
            }
            public enum Tp { NONE, DYN, KIN, STATIC }
            private bool isSetUp = false;
            private Collider2D col = null;
            private Rigidbody2D rig = null;
            private void SetUp(GameObject go)
            {
                if (go == null) return;
                isSetUp = true;
                bool flg = go.TryGetComponent<Collider2D>(out Collider2D c);
                if (flg) col = c;
                flg = go.TryGetComponent<Rigidbody2D>(out Rigidbody2D r);
                if (flg) rig = r;
            }
            public bool HasCollider() { return col != null; }
            public bool HasRigidBody() { return rig != null; }
            public bool IsTrigger()
            {
                return col==null ? false : col.isTrigger;
            }
            public Tp RigidBodyType()
            {
                if (rig == null) return Tp.NONE;
                switch (rig.bodyType)
                {
                    case RigidbodyType2D.Dynamic: return Tp.DYN;
                    case RigidbodyType2D.Kinematic:return Tp.KIN;
                    case RigidbodyType2D.Static: return Tp.STATIC;
                }
                return Tp.NONE;
            }
            public int RigidBodyTypeInt()
            {
                var t = RigidBodyType();
                return TypeToInt(t);
            }
            public void Load(GameObject go)
            {
                if (isSetUp) return;
                SetUp(go);
            }
        }
    }

    private Utils.StatBox statbox = new Utils.StatBox();

    private void LOG(string msg, [CallerLineNumber] int ln=-1, [CallerMemberName] string nm="")
    {
        statbox.Load(gameObject);

        string rbCode = "" + "ndks"[statbox.RigidBodyTypeInt()];

        var lb = Utils.LogBuilder.Create()
            .SetTitle("log-called")
            .AddItem("message", msg)
            .AddItem("called-from", nm)
            .AddItem("called-at", ln.ToString())
            .AddItem("in-object", gameObject.name)
            .AddItem("hasRigid", statbox.HasRigidBody().ToString())
            .AddItem("rigidBodyType", rbCode)
            .AddItem("isTrigger", statbox.IsTrigger().ToString())
            .Build();
        Debug.Log(lb);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        LOG("other-name==" + other.name);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        LOG("other-name==" + collision.gameObject.name);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        LOG("other-name==" + other.name);
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        LOG("other-name==" + collision.gameObject.name);
    }

    public bool moveUp = false;
    public float upSpeed = 0.05f;
    private void MoveUp()
    {
        if (moveUp)
        {
            float delta = upSpeed * Time.deltaTime;
            gameObject.transform.position += new Vector3(0f,delta,0f);
        }
    }

    void Update()
    {
        MoveUp();
    }


    // OnTriggerStay2D

    //SquareThing | CircleThing 

    private void SetUp()
    {
        const string SquareName = "SquareThing";
        const string CircleName = "CircleThing";

        Rigidbody2D rb = null;
        Collider2D cl = null;

        switch (gameObject.name)
        {
            case SquareName:
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.AddForce(new Vector2(0f,10f));
                break;
            case CircleName:
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1;
                cl = gameObject.GetComponent<Collider2D>();
            //    cl.isTrigger = true;
                break;
        }
    }

    void Start()
    {
        SetUp();
    }


}

