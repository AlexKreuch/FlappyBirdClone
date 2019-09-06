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

    public GameObject theObject;

    public int ind = 1;
    public float width = 0f;

    public bool btn = false;
    private void press_btn()
    {
        Vector3 tmp = theObject.transform.position;
        Quaternion qrt = theObject.transform.rotation;
        tmp.x += ind * width;
        Instantiate<GameObject>(theObject,tmp,qrt);
        ind++;
    }

    public bool getwidth = false;
    private void rungetwidth()
    {
        var bc = theObject.GetComponent<BoxCollider2D>();
        width = bc.size.x;
    }



    void Update()
    {
        btnMech(ref btn, press_btn);
        btnMech(ref getwidth, rungetwidth);
    }
   

}
