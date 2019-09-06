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

    private GameObject[] gameObjects = new GameObject[0];

    public bool flag = false;
    public bool btn = false;
    private void press_btn()
    {
        const string tag0 = "Ground", tag1 = "Background";
        var arr = FindObjectsOfType<GameObject>();
        var lst = new List<GameObject>();
        foreach (var x in arr)
        {
            if (x.tag == tag0 || x.tag == tag1) lst.Add(x);
        }
        gameObjects = lst.ToArray();
        flag = !flag;
    }

    public float xoffset = 0f;
    private float curoffset = 0f;
    private void maintainOffset()
    {
        var d = xoffset - curoffset;
        if (d == 0f) return;
        var vec = new Vector3(d,0f,0f);
        foreach (var x in gameObjects)
        {
            x.transform.position += vec;
        }
        curoffset = xoffset;
    }

    void Update()
    {
        btnMech(ref btn, press_btn);
        maintainOffset();
    }
   

}
