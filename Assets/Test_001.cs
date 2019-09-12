using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        btnMech(ref clone, ()=> { CloneMech.Clone(theObject, space); });
        btnMech(ref reset, ()=> { CloneMech.Reset(); });
    }

    

   
}

