using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public static Bird instance;


    private void MakeInstance()
    {
        if (instance == null)
        {
            instance = this;
            cameraOffset = Camera.main.transform.position.x - this.transform.position.x;
        }
        else
            Destroy(this);
    }

    #region fields
    private const string FlappingTriggerName = "Flapping";
    [SerializeField]
    private float horizontalSpeed = 10f, boostSpeed = 5f;
    private bool flappedWings = false;
    [SerializeField]
    private Rigidbody2D theRigidbody;
    [SerializeField]
    private Animator theAnimator;

    private float cameraOffset = 0f;

    #endregion

    #region Movement-helpers
  

    private void Movement()
    {
        this.transform.position += new Vector3(horizontalSpeed * Time.deltaTime, 0f, 0f);
        if (flappedWings)
        {
            flappedWings = false;
            theRigidbody.velocity = new Vector3(0f, boostSpeed, 0f);
            theAnimator.SetBool(FlappingTriggerName, true);
        }
    }
    public void Flap()
    {
        flappedWings = true;
        Debug.Log("pressed");
    }
    private void RegisterFlap() { theAnimator.SetBool(FlappingTriggerName, false); Debug.Log("register_flap"); }

    private void MaintainCameraOffset()
    {
        Vector3 tmp = Camera.main.transform.position;
        tmp.x = this.transform.position.x + cameraOffset;
        Camera.main.transform.position = tmp;
    }

    #endregion


    void Awake() { MakeInstance(); }

    void Update()
    {
        Movement();
        MaintainCameraOffset();
    }

  

    
}
