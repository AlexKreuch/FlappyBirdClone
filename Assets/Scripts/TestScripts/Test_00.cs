using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Runtime.CompilerServices;
//using System;
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

    public GameObject[] birds = new GameObject[0];

    private Vector3[] posList = null;

    private void setup()
    {
        posList = new Vector3[birds.Length];
        int cur = 0;
        foreach (var x in birds)
        {
            posList[cur++] = x.transform.position;
        }
    }
    private void adjust()
    {
        int cur = 0;
        foreach (var x in birds)
        {
            x.transform.position = posList[cur++];
        }
    }

    void Start() { setup(); }
    void Update() { adjust(); }
}
