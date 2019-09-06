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
    void Awake() { MakeInstance(); }

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
    

    void FixedUpdate()
    {
      // Movement();
    }

    void Update()
    {
        Movement();
        MaintainCameraOffset();
        // HoldPos();
        saveBird();
    }

    public float limit = 0f;
    struct Acc<T>
    {
        private System.Func<T> getter;
        private System.Action<T> setter;
        public T VAL
        {
            get{ return getter(); }
            set { setter(value); }
        }
        public Acc(System.Func<T> g, System.Action<T> s) { getter = g; setter = s; }
        public Acc(T df)
        {
            T g() { return df; }
            void s(T v) { }
            getter = g;
            setter = s;
        }
    }
    private Acc<float> MakeYAcc(float center)
    {
        float g(){ return center - this.transform.position.y; }
        void s(float v)
        {
            Vector3 tmp = this.transform.position;
            tmp.y = center - v;
            this.transform.position = tmp;
        }
        return new Acc<float>(g,s);
    }
    private class Holder
    {
        private static bool isset = false;
        private static float limit = 0f;
        private static Acc<float> acc = new Acc<float>(0f);
        public static void SetAcc(Acc<float> a, float lim) { acc = a; limit = Mathf.Abs(lim); isset = true; }
        public static bool Isset() { return isset; }
        public static void Update()
        {
            if (limit == 0f) { acc.VAL = 0f; return; }
            acc.VAL = Mathf.Clamp(acc.VAL,-1*limit,limit);
        }
    }
    private void HoldPos()
    {
        if (Holder.Isset()) Holder.Update();
        else
        {
            Holder.SetAcc(MakeYAcc(this.transform.position.y), limit);
        }
    }

    private void saveBird()
    {
        const float limit = -4f;
        Vector3 tmp = this.transform.position;
        if (tmp.y < limit)
        {
            tmp.y = limit;
            this.transform.position = tmp;
        }
    }
}
