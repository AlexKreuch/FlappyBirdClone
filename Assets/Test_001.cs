using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Test_001 : MonoBehaviour
{
    
    private void btnMech(ref bool btn, System.Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }

    public GameObject theBird = null;
    private Rigidbody2D theRigidbody2D = null;
    private float minVel = 0f;
    private float maxVel = 0f;

    public bool pub = false;
    private int count = 0;
    private void press_pub()
    {
        Debug.Log(string.Format("{0} | max={1},min={2}",count++,maxVel,minVel));
    }

    private void SetUp()
    {
        theRigidbody2D = theBird.GetComponent<Rigidbody2D>();
    }
    private void Step()
    {
        float curVel = theRigidbody2D.velocity.y;
        if (curVel < minVel) minVel = curVel;
        if (curVel > maxVel) maxVel = curVel;
    }
    void Start()
    {
        SetUp();
    }
    void Update()
    {
        btnMech(ref pub, press_pub);
        Step();
    }

}
