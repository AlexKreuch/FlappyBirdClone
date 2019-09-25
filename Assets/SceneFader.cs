using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;


public class SceneFader : MonoBehaviour
{
    public static SceneFader instance = null;

    [SerializeField]
    private GameObject panel = null;
    [SerializeField]
    private Image image = null;

    private void SetUp()
    {
        image = GetComponentInChildren<Image>();
        panel = image.gameObject;
        panel.SetActive(false);
        instance = this;
        DontDestroyOnLoad(gameObject);
    }



    void OnEnable()
    {
        if (instance != null) { Destroy(gameObject); return; }
        SetUp();
    }

    private IEnumerator FadeMech(int frameCount, float time, Action action)
    {
        if (frameCount <= 0) frameCount = 1;
        if (time <= 0) time = .0001f;
        Color color = new Color(0f, 0f, 0f, 0f);
        Color deltaColor = new Color(0f, 0f, 0f, 1f/frameCount);
        float deltaTime = time / frameCount;
        image.color = color;
        panel.SetActive(true);
        for (int i = 0; i < frameCount; i++)
        {
            color += deltaColor;
            yield return new WaitForSeconds(deltaTime);
            image.color = color;
        }
        action();
        for (int i = 0; i < frameCount; i++)
        {
            color -= deltaColor;
            yield return new WaitForSeconds(deltaTime);
            image.color = color;
        }
        panel.SetActive(false);
    }

    private enum fadeType { ROUGH=0, SMOOTH=1, MAINTHREAD=2, GOTOGAME=3 , TEST_SCENE_LOAD_FROM_OUTSIDE_MAINTHREAD=4 }
    public void TestFade(int count = 10, float time = 1f, int ft = 0,int flag=0)
    {
        Action f = null;
        switch (ft)
        {
            case (int)fadeType.ROUGH:
                f = () => { Debug.Log("FADE-EVENT <rough> : " + flag); };
                StartCoroutine(FadeMech(count,time,f));
                break;
            case (int)fadeType.SMOOTH:
                f = () => { Debug.Log("Fade-event <smooth> : " + flag); };
                StartCoroutine(FadeMech_smooth(time,f));
                break;
            case (int)fadeType.MAINTHREAD:
                f = () => { Debug.Log("Fade-event <mainThread> : " + flag); };
                FadeMech_mainThread(time,f);
                break;
            case (int)fadeType.GOTOGAME:
                Debug.Log("goto-game called : " + flag);
                FadeToGamePlay();
                break;
            case (int)fadeType.TEST_SCENE_LOAD_FROM_OUTSIDE_MAINTHREAD:
                Debug.Log("testing to see if scenes can be loaded from outside the main thread : " + flag);
                TestSceneLoader(FlappyBirdUtil.Names.GamePlayScene);
                break;
        }

       
    }

    private IEnumerator FadeMech_smooth(float time, Action action)
    {
        if (time <= 0) time = .0001f;

        Func<float,float,Color> MakeFunc(Color color0, Color color1, float delta)
        {
            if (delta <= 0) { return (v,o)=>(v<=o ? color0 : color1); }
            Color _color = (color1 - color0) / delta;
            Color f(float cur, float offset)
            {
                cur = Mathf.Clamp(cur,offset,offset+delta);
                return _color * (cur - offset) + color0;
            }
            return f;
        }


        Color c0 = new Color(0f, 0f, 0f, 0f);
        Color c1 = new Color(0f, 0f, 0f, 1);
        var faderFunc0 = MakeFunc(c0,c1,time);
        var faderFunc1 = MakeFunc(c1,c0,time);

        float startTime = Time.time;
        float curTime = startTime;
        float endTime = startTime + time;

        image.color = new Color(0, 0, 0, 0);

        panel.SetActive(true);

        while (curTime<endTime)
        {
            yield return null;
            curTime = Time.time;
            image.color = faderFunc0(curTime, startTime);
        }

        action();

        startTime = curTime;
        endTime = startTime + time;

        while (curTime < endTime)
        {
            yield return null;
            curTime = Time.time;
            image.color = faderFunc1(curTime, startTime);
        }

        panel.SetActive(false);
        
    }

    private class FadeMech_mainThread_class
    {
        private const float defaultFadeTime = .0001f;
        private Action action = null;
        private Action<Color> colorSetter = null;
        private Action<bool> panelSetter = null;
        private float[] box = new float[] { 0f };
        private enum operation { NONE , PANEL_ON , RUNACTION , PANEL_OFF }
        private IEnumerator<operation> Mech(float delta)
        {
            if (delta <= 0) delta = defaultFadeTime;
            float clock = 0f;
            colorSetter(new Color(0f, 0f, 0f, 0f));
            yield return operation.PANEL_ON;
            while (clock < delta)
            {
                clock += box[0];
                colorSetter(new Color(0f, 0f, 0f, Mathf.Clamp(clock/delta,0,1)));
                yield return operation.NONE;
            }
            clock = 0;
            colorSetter(new Color(0f, 0f, 0f, 1f));
            yield return operation.RUNACTION;
            while (clock < delta)
            {
                clock += box[0];
                colorSetter(new Color(0f, 0f, 0f, 1f - Mathf.Clamp(clock / delta, 0, 1)));
                yield return operation.NONE;
            }
            colorSetter(new Color(0f, 0f, 0f, 0f));
            yield return operation.PANEL_OFF;
        }
        private IEnumerator<operation> mech = null;


        public bool START(float time, Action _action)
        {
            if (mech != null) return false;
            action = _action;
            mech = Mech(time);
            return true;
        }

        public void STEP()
        {
            if (mech == null) return;
            box[0] = Time.deltaTime;
            if (mech.MoveNext())
            {
                var op = mech.Current;
                switch (op)
                {
                    case operation.NONE: break;
                    case operation.PANEL_ON: panelSetter(true); break;
                    case operation.PANEL_OFF:panelSetter(false); break;
                    case operation.RUNACTION: action(); break;
                }
            }
            else
            {
                mech = null;
            }
        }


        public FadeMech_mainThread_class()
        {
            colorSetter = v => { return; };
            panelSetter = v => { return; };
        }
        public FadeMech_mainThread_class(Action<Color> cs, Action<bool> ps)
        {
            colorSetter = cs;
            panelSetter = ps;
        }
    }
    private FadeMech_mainThread_class fmMain = null;
    private FadeMech_mainThread_class GetFMmain()
    {
        if (fmMain == null)
        {
            void f(Color c) { image.color = c; }
            void g(bool b) { panel.SetActive(b); }
            fmMain = new FadeMech_mainThread_class(f, g);
        }
        return fmMain;
    }
    private void FadeMech_mainThread(float time, Action action)
    {
        GetFMmain().START(time,action);
    }
    void Update()
    {
        GetFMmain().STEP();
    }

    public void FadeToGamePlay(float time = 1.25f)
    {
        void f() { SceneManager.LoadScene(FlappyBirdUtil.Names.GamePlayScene); }
        GetFMmain().START(time, f);
    }

    private class TestSceneLoader_class
    {
        /*
         * Test whether a scene can be loaded from outside the main-thread
         * **/
        private static bool locked = false;
        private static IEnumerator enumerator(string nxtLevelName)
        {
            SceneManager.LoadScene(nxtLevelName);
            locked = false;
            yield return null;
        }
        public static void RUN(MonoBehaviour mono,string levelName)
        {
            if (locked) return;
            locked = true;
            mono.StartCoroutine(enumerator(levelName));
        }
    }
    public void TestSceneLoader(string ln)
    {
        TestSceneLoader_class.RUN(this,ln);
    }
}
