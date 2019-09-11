using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeGate : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("trigger : " + collider.gameObject.name);
    }
}
