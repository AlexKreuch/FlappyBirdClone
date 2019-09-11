using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PipeCollector : MonoBehaviour
{

    private const string holderTag = "PipeHolder"; 


    /*
     * use this to send PipeHolder's to the PipeController-Object
     */
    private Action<GameObject> sendToController = null;
    public void SetSendToController(Action<GameObject> stc)
    {
        sendToController = stc;
    }

    #region follow camera
    private float cameraOffset = 0f;
    private void ComputeCameraOffset()
    {
        cameraOffset = gameObject.transform.position.x - Camera.main.transform.position.x;
    }
    private void FollowCamera()
    {
        
        Vector3 tmp = gameObject.transform.position;
        tmp.x = Camera.main.transform.position.x + cameraOffset;
        gameObject.transform.position = tmp;
              
    }
    #endregion

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == holderTag && sendToController != null) sendToController(collider.gameObject);
    }

    void Start()
    {
        ComputeCameraOffset();
    }

    void Update()
    {
        FollowCamera();
    }

}
