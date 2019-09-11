using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private const string FlapButtonName = "FlapButton";
    private const string FlappingTriggerName = "Flapping";
    [SerializeField]
    private float horizontalSpeed = 10f, boostSpeed = 5f;
    private bool flappedWings = false;
    [SerializeField]
    private Rigidbody2D theRigidbody;
    [SerializeField]
    private Animator theAnimator;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] audioClips; // [flapping , ding , dead]

    private float cameraOffset = 0f;
    #endregion

    #region Movement-helpers

    private void MaintainRotation()
    {
        const float maxSpeed = 5f, minSpeed = -12.8542f;

        float yvel = theRigidbody.velocity.y;
        float zRotation = yvel < 0 ? Mathf.Lerp(0, -45, yvel / minSpeed) : Mathf.Lerp(0, 45, yvel / maxSpeed);
        Vector3 vec = new Vector3(0f,0f,zRotation);
        gameObject.transform.rotation = Quaternion.Euler(vec);
    }
    private void Movement()
    {
        this.transform.position += new Vector3(horizontalSpeed * Time.deltaTime, 0f, 0f);
        if (flappedWings)
        {
            flappedWings = false;
            theRigidbody.velocity = new Vector3(0f, boostSpeed, 0f);
            theAnimator.SetBool(FlappingTriggerName, true);
        }
        MaintainRotation();
    }
    public void Flap()
    {
        flappedWings = true;
    }
    private void RegisterFlap() { theAnimator.SetBool(FlappingTriggerName, false);  }

    private void MaintainCameraOffset()
    {
        Vector3 tmp = Camera.main.transform.position;
        tmp.x = this.transform.position.x + cameraOffset;
        Camera.main.transform.position = tmp;
    }

    #endregion


    void Awake()
    {
        MakeInstance();
        #region setup flap-button
        GameObject.FindGameObjectWithTag(FlapButtonName)
            .GetComponent<Button>()
            .onClick.AddListener(Flap);
        
        #endregion
    }

    void Update()
    {
        Movement();
        MaintainCameraOffset();
    }

  

    
}
