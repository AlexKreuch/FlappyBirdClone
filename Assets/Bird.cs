﻿#define TESTING_MODE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using System;

public class Bird : MonoBehaviour
{
    public static Bird instance;

    private void MakeInstance()
    {
#if TESTING_MODE

        if (instance == null)
        {
            ReporterTest.report("making-bird : {0}", gameObject.name);
            instance = this;
        }
        else
        {
            ReporterTest.report("destroying-bird : {0}", gameObject.name);
            Destroy(this);
        }

#else
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(this);
#endif
    }

    #region AnimatorUtil-class (and set-up method)
        /* Use this to interact with Animator
         * 
         * **/
        private class AnimatorUtil
        {
            private static AnimatorUtil instance = new AnimatorUtil();
            public static AnimatorUtil GetInst() { return instance; }

            private Animator animator = null;
            public void SetUp(Animator anim) { animator = anim; }

            /* Set the int-parameter 'BirdState' of the animator to : 
             *    -> 0, for the Idol-animation
             *    -> 1, for the Flapping-animation
             *    -> 2, for the Dead-animation
             * **/
            private const string StateName = "BirdState";
            public void FlapWings()
            {
                if (animator == null) return;
                animator.SetInteger(StateName, 1);
            }
            public void Idol()
            {
                if (animator == null) return;
                animator.SetInteger(StateName, 0);
            }
            public void Dead()
            {
                if (animator == null) return;
                animator.SetInteger(StateName, 2);
            }

            #region testing-code
    #if TESTING_MODE
            public Animator GetAnimator() { return animator; }
    #endif
            #endregion
        }
        private void SetUpAnimatorUtil()
        {
            AnimatorUtil.GetInst().SetUp(theAnimator);
        }
    #endregion
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
                        auSo != null && clipArr != null && clipArr.Length == 3 &&
                        clipArr[0] != null && clipArr[1] != null && clipArr[2] != null
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
                audioSource.PlayOneShot(clips[index]);
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

        #region constant values
        private const string PipeTag = "Pipe";
        private const string GateTag = "PipeGate";
        private const string FlapButtonTag = "FlapButton";
        private const string GroundTag = "Ground";
        #endregion

        #region Serialized-Fields
        [SerializeField]
        private float horizontalSpeed = 10f, boostSpeed = 5f;
        [SerializeField]
        private Rigidbody2D theRigidbody;
        [SerializeField]
        private Animator theAnimator;
        [SerializeField]
        private AudioSource audioSource;
        [SerializeField]
        private AudioClip[] audioClips; // [flapping , ding , dead]
        #endregion

        #region state-fields
        private float cameraOffset = 0f;
        private bool shouldFlapWings = false;
        private bool alive = true;
        private int score = 0;
        private List<Action> OnDieListeners = new List<Action>();
        #endregion

    #endregion

    #region Helper-Methods

        #region control bird-movement

            /* Tilt the Bird according to its vertical movement.
             * 
             * 
             * 
             * **/
            private void MaintainRotation()
                {
                    const float maxSpeed = 20f, minSpeed = -12.8542f;
                    float yvel = theRigidbody.velocity.y;
                    float zRotation = yvel < 0 ? Mathf.Lerp(0, -45, yvel / minSpeed) : Mathf.Lerp(0, 45, yvel / maxSpeed);
                    Vector3 rotation = new Vector3(0f, 0f, zRotation);
                    gameObject.transform.rotation = Quaternion.Euler(rotation);
                }

            /* Move the Bird
             * 
             * 
             * **/
            private void Movement()
            {
                if (!alive) return;
                var tmp = theRigidbody.velocity;
                tmp.x = horizontalSpeed;
                if (shouldFlapWings)
                {
                    shouldFlapWings = false;
                    tmp.y = boostSpeed;
                    //theAnimator.SetBool(FlappingTriggerName, true);
                    AnimatorUtil.GetInst().FlapWings();
                    AudioController.GetInstance().PlayFlapping();
                }
                theRigidbody.velocity = tmp;
                MaintainRotation();
            }

        #endregion

        #region control-flapping
            /* Called by the Flap-Button to indicate that the bird should flap its wings on the next update
             * 
             * 
             * **/
            public void RequestFlap()
            {
                shouldFlapWings = true;
            }

            /* Called by the animator to indicate that the bird has just started flapping its wings
             * 
             * 
             * **/
            private void RegisterFlap()
            {
                // theAnimator.SetBool(FlappingTriggerName, false);
                if (alive) AnimatorUtil.GetInst().Idol();
                else AnimatorUtil.GetInst().Dead();
            }

        #endregion

        #region in-game events
            private void Die()
            {
                #region testing code
                #if TESTING_MODE
                                    if (Invincible) return;
                        ReporterTest.report("Die-method : onDieListener-count = {0}", OnDieListeners.Count);
                #endif
                #endregion
                            alive = false;
                            AnimatorUtil.GetInst().Dead();
                            AudioController.GetInstance().PlayDead();
                            theRigidbody.constraints = RigidbodyConstraints2D.None;
                #if TESTING_MODE
                        int count = 0;
                        foreach (Action action in OnDieListeners) { action(); count++; }
                        ReporterTest.report("{0} action(s) were run",count);
                #else
                        foreach (Action action in OnDieListeners) action();
                #endif
            }
            private void ScorePoint()
            {
                score++;
                AudioController.GetInstance().PlayDing();
            }
        #endregion

        #region control camera
            private void ComputeCameraOffset()
            {
                cameraOffset = Camera.main.transform.position.x - this.transform.position.x;
            }
            private void MaintainCameraOffset()
            {
                Vector3 tmp = Camera.main.transform.position;
                tmp.x = this.transform.position.x + cameraOffset;
                Camera.main.transform.position = tmp;
            }
        #endregion

    #endregion

    #region public methods/Properties
        /* Note : 'Register-flap' has been placed in the 'Helper-Methods' region even though it is public
         * 
         * 
         * **/

        public int GetCurrentScore() { return score; }

        #region Suspend-movement
            #region state-info
                private bool _SuspednMovement_value = false;
                private RigidbodyConstraints2D _SuspendMovement_savedRBC = RigidbodyConstraints2D.None;
            #endregion
            public bool SuspendMovement
                {
                    get
                    {
                        return _SuspednMovement_value;
                    }
                    set
                    {
                        int code = 0;
                        if (value) code += 1;
                        if (_SuspednMovement_value) code += 2;
                        switch (code)
                        {
                            case 1:
                                _SuspendMovement_savedRBC = theRigidbody.constraints;
                                theRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
                                _SuspednMovement_value = true;
                                break;
                            case 2:
                                theRigidbody.constraints = _SuspendMovement_savedRBC;
                                _SuspednMovement_value = false;
                                break;
                        }
                    }
                }
        #endregion

        public void AddOnDieListener(Action action)
        {
            #if TESTING_MODE
                ReporterTest.report("adding-ondielistener");
            #endif
            if (action == null) return;
            OnDieListeners.Add(action);
        }

    #endregion

    #region lifecycle-methods
        void Awake()
        {
            MakeInstance();
            #region setup flap-button
                GameObject.FindGameObjectWithTag(FlapButtonTag)
                    .GetComponent<Button>()
                    .onClick.AddListener(RequestFlap);
            #endregion
            SetUpAudioController();
            SetUpAnimatorUtil();
            ComputeCameraOffset();
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = FlappyBirdUtil.SortingOrders.PlayerOrder;
            #if TESTING_MODE
                SetInvincibleToDefault();
            #endif
        }

        void Update()
        {
            Movement();
            MaintainCameraOffset();
        }
    
        void OnTriggerEnter2D(Collider2D collider)
        {
            if (alive && collider.tag == GateTag) ScorePoint();
        }
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (alive && (collision.collider.tag == PipeTag || collision.collider.tag == GroundTag)) Die();
        }

    #endregion

    #region Testing Code
    #if TESTING_MODE
        private class ReporterTest
        {
            private static int count = 0;
            public static void report(string msg, params object[] data)
            {
                msg = string.Format(msg,data);
                Debug.Log(string.Format("<{0}> {1}",count++,msg));
            }
        }
        public void Testing_REVIVE()
        {
            alive = true;
            AnimatorUtil.GetInst().Idol();
            theRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        public Animator Testing_GetSavedAnimator()
        {
            return AnimatorUtil.GetInst().GetAnimator();
        }
        public Rigidbody2D Testing_GetRigidbody() { return theRigidbody; }
        public bool Invincible = false;
        public static void Testing_ERASE()
        {
            Destroy(instance.gameObject);
            instance = null;
        }

        private bool invincible_default = false;
        private void SetInvincibleToDefault() { Invincible = invincible_default; }
    #endif
    #endregion

    

}
