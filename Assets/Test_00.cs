using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Runtime.CompilerServices;
//using System;
[ExecuteAlways]
public class Test_00 : MonoBehaviour
{
   
    public GameObject box = null;
    public AnimationClip anim = null;

  

    public bool test = false;
    private void press_test()
    {
        string gp = "\n     ";
        string result = "test : ";
        result += gp + "name:" + anim.name;
        result += gp + "empty:" + anim.empty;
        result += gp + "events_count:" + anim.events.Length;
        result += gp + "hasMotionCurves:" + anim.hasMotionCurves;
        result += gp + "length:" + anim.length;
        Debug.Log(result);
        
    }

    
    private void btnMech(ref bool btn, System.Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }

   

    void Update()
    {
        btnMech(ref test,press_test);

       
    }
}
