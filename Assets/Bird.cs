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

    #region audio-controller-class (and set-up method)
    /*
     * Use this to play Audio-clips
     * 
     * **/
    private class AudioController
    {
        private static AudioController instance = new AudioController();
        public static AudioController GetInstance() { return instance; }

        private const int FlappingSoundIndex = 0;
        private const int DingSoundIndex = 1;
        private const int DeadSoundIndex = 2;

        private bool isSetUp = false;
        private AudioSource audioSource = null;
        private AudioClip[] clips = null;

        /* Both fields must be non-null, and clipArr must have the form : 
         *   [ flappingClip , DingClip , DeadClip ]
         * 
         * **/
        public void SetUp(AudioSource auSo, AudioClip[] clipArr)
        {
            #region check inputs
            Debug.Assert
                (
                    auSo!=null && clipArr!=null && clipArr.Length==3 && 
                    clipArr[0]!=null && clipArr[1]!=null && clipArr[2]!=null
                    ,
                    "INVALID AudioController-setup"
                );
            #endregion

            isSetUp = true;
            audioSource = auSo;
            clips = clipArr;
        }

        private void PlaySound(int index)
        {
            if (!isSetUp) return;
            audioSource.clip = clips[index];
            audioSource.Play();
        }
        public void PlayFlapping() { PlaySound(FlappingSoundIndex); }
        public void PlayDing() { PlaySound(DingSoundIndex); }
        public void PlayDead() { PlaySound(DeadSoundIndex); }

      
        
    }
    private void SetUpAudioController()
    {
        AudioController.GetInstance().SetUp(audioSource, audioClips);
    }
    #endregion

    #region fields
    private const string FlapButtonName = "FlapButton";
    private const string FlappingTriggerName = "Flapping";
    private const int FlappingSoundIndex = 0;
    private const int DingSoundIndex = 1;
    private const int DeadSoundIndex = 2;
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
            AudioController.GetInstance().PlayFlapping();
           
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
        SetUpAudioController();
    }

    void Update()
    {
        Movement();
        MaintainCameraOffset();
    }

  

    
}
