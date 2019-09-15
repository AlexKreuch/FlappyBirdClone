#define BIRDIN_TESTIN_MODE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[ExecuteAlways]
public class Test_001 : MonoBehaviour
{
    private const string lowerPipeName = "Pipe_Green";
    private const string upperPipeName = "Pipe_Green (1)";
    private const string gateName = "Gate";

    private class Items
    {
        private static bool setup = false;
        private static GameObject upperPipe = null;
        private static GameObject lowerPipe = null;
        private static BoxCollider2D gate = null;
        private static Vector3 pipSize = new Vector3();
        private static void SetUp()
        {
            setup = true;
            GameObject gt = null;
            int flags = 0;
            var objs = GameObject.FindObjectsOfType<GameObject>();
            foreach (var x in objs)
            {
                if (flags == 7) break;
                switch (x.name)
                {
                    case lowerPipeName: lowerPipe = x; flags += 4; break;
                    case upperPipeName: upperPipe = x; flags += 2; break;
                    case gateName: gt = x; flags += 1; break;
                }
            }
            gate = gt.GetComponent<BoxCollider2D>();
            pipSize = lowerPipe.GetComponent<Collider2D>().bounds.size;
        }

        public static GameObject GetUpper()
        {
            if (!setup) SetUp();
            return upperPipe;
        }
        public static GameObject GetLower()
        {
            if (!setup) SetUp();
            return lowerPipe;
        }
        public static BoxCollider2D GetGate()
        {
            if (!setup) SetUp();
            return gate;
        }
        public static float GetPipeLength()
        {
            if (!setup) SetUp();
            return pipSize.y;
        }
        public static float GetPipeWidth()
        {
            if (!setup) SetUp();
            return pipSize.x;
        }
    }

    private void Scale(float gp)
    {
        var gate = Items.GetGate();
        
        gate.size = new Vector2(Items.GetPipeWidth(),gp);
        float space = (Items.GetPipeLength() + gp) / 2;
        Items.GetLower().transform.localPosition = new Vector3(0,-space,0);
        Items.GetUpper().transform.localPosition = new Vector3(0,space,0);

       
    }

    public float gap = 0f;
    private void MaintainGate()
    {
        Scale(gap);
    }

    private void btnMech(ref bool btn, System.Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }

    public GameObject theObject = null;
    public float space = 0f;
    private class CloneMech
    {
        private static Vector3 currentPos = new Vector3();
        private static Quaternion currentRot = new Quaternion();
        private static GameObject savedObj = null;
        public static void Clone(GameObject obj, float space)
        {
            if (obj == null) { savedObj = null;  return; }
            if (savedObj != obj)
            {
                savedObj = obj;
                currentPos = obj.transform.position;
                currentRot = obj.transform.rotation;
                currentPos.x += space;
                GameObject.Instantiate<GameObject>(obj, currentPos, currentRot);
            }
            else
            {
                currentPos.x += space;
                GameObject.Instantiate<GameObject>(obj, currentPos, currentRot);
            }
        }
        public static void Reset()
        {
            currentPos = new Vector3();
            currentRot = new Quaternion();
            savedObj = null;
        }
    }
    public bool clone = false;
    public bool reset = false;
    

    void Update()
    {
        //    btnMech(ref clone, ()=> { CloneMech.Clone(theObject, space); });
        //  btnMech(ref reset, ()=> { CloneMech.Reset(); });
        //btnMech(ref setPipesToTigger, SetPipHoldersToTrigger);
           MoveBird();
        // btnMech(ref setSortingOrders, SetSortingOrders);
        //  EditorPlayingChecker.Update();
        TestKeyDevice();
    }
    

    void SetPipHoldersToTrigger()
    {
        const string holderTag = "PipeHolder";
        var list = FindObjectsOfType<Collider2D>();
        foreach (var x in list)
        {
            if (x.tag == holderTag) x.isTrigger = true;
        }
    }

    public bool setPipesToTigger = false;

    void MoveBird()
    {
        if (!EditorApplication.isPlaying) return;
        #region conditionally define BirdRevive
#if BIRDIN_TESTIN_MODE
        void BirdRevive(){ Bird.instance.Testing_REVIVE(); }
#else
        void BirdRevive() { Debug.Log("try-birdRevive"); }
#endif
        #endregion
        const float space = 1f;
        const float limit = -3.4209394f;

        Vector3 H = new Vector3(space,0,0);
        Vector3 V = new Vector3(0,space, 0);
        Vector3 delta = new Vector3();

        if (Input.GetKeyDown(KeyCode.U)) BirdRevive();
        if (Input.GetKeyDown(KeyCode.Alpha8)) delta += 2 * V;
        if (Input.GetKeyDown(KeyCode.I)) delta += V;
        if (Input.GetKeyDown(KeyCode.K)) delta -= V;
        if (Input.GetKeyDown(KeyCode.J)) delta -= H;
        if (Input.GetKeyDown(KeyCode.L)) delta += H;
        if (Input.GetKeyDown(KeyCode.H)) delta -= 2 * H;
        if (Input.GetKeyDown(KeyCode.Semicolon)) delta += 2 * H;

        if (delta.y < 0 && Bird.instance.transform.position.y <= limit) delta.y = 0;

        Bird.instance.transform.position += delta;
    }
    
    /*
     * Set the Sorting-layer-order of each SprinteRenderer
     * 
     * **/
    private void SetSortingOrders()
    {
  
        Dictionary<string, int> sortingOrders = new Dictionary<string, int>();
        sortingOrders.Add("Background",0);
        sortingOrders.Add("Player", 1);
        sortingOrders.Add("Pipe", 2);
        sortingOrders.Add("Ground", 3);

        var objs = FindObjectsOfType<GameObject>();
        foreach (var obj in objs)
        {
            bool found = sortingOrders.TryGetValue(obj.tag,out int order);
            if (found) obj.GetComponent<SpriteRenderer>().sortingOrder = order;
        }
       
    }
    public bool setSortingOrders = false;



    class EditorPlayingChecker
    {
        private static int count = 0;
        private static int flag = -1; // -1:=Not-started ; 0:=started&notPlaying ; 1:=started&playing
        private static string[] results = new string[]
        {
            "Playing",
            "not-Playing",
            "started-Playing",
            "stopped-Playing"
        };
        public static void Update()
        {
            bool curState = EditorApplication.isPlaying;
            if (flag != -1 && ((flag == 1) == curState)) return;
            int index = flag == -1 ? 0 : 2;
            if (!curState) index++;
            flag = (index + 1) % 2;
            Debug.Log(count++ + " | " + results[index]);
        }
       
    }

    private class KeyDevice
    {
        private Func<string> getter = null;
        private int oldLen = 0;
        private Dictionary<char, Action> Functions = null;
        private KeyDevice() { }
        private KeyDevice(Func<string> g, Dictionary<char, Action> fs)
        {
            getter = g;
            Functions = fs;
            oldLen = getter().Length;
        }
        public class Builder
        {
            private Func<string> getter = null;
            private Dictionary<char, Action> Functions = new Dictionary<char, Action>();
            public Builder SetGetter(Func<string> g) { getter = g; return this; }
            public Builder AddKeyFunction(char k, Action f) { Functions.Add(k,f); return this; }
            public KeyDevice Build() { return new KeyDevice(getter,Functions); }
        }
        public static Builder CreateBuilder() { return new Builder(); }

        public void Update()
        {
            string cur = getter();
            int curlen = cur.Length;
            if (curlen == oldLen + 1)
            {
                char k = cur[oldLen];
                bool found = Functions.TryGetValue(k,out Action action);
                if (found) action();
            }
            oldLen = curlen;
        }
    }
    public string keyLogger = "";
    private KeyDevice keyDevice = null;
    private KeyDevice GetKeyDevice()
    {
        if (keyDevice == null)
        {
            Action MakeAction(string msg)
            {
                void f() { Debug.Log(msg); }
                return f;
            }
            keyDevice = KeyDevice.CreateBuilder()
                .SetGetter(() => keyLogger)
                .AddKeyFunction('j',MakeAction("pressed-j"))
                .AddKeyFunction('k',MakeAction("pressed-k"))
                .Build();
        }
        return keyDevice;
    }
    private void TestKeyDevice()
    {
        GetKeyDevice().Update();
    }

    
}

