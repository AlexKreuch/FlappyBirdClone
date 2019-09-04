using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_01 : MonoBehaviour
{
    [SerializeField]
    private Sprite[] sprites = new Sprite[0];

    #region AnimationHelper
    private class AnimationHelper
    {
        private static SpriteRenderer spriteRenderer;
        private static Sprite[] sprites = null;
        private static int[] indices = null;
        private static int index = 0;
        private static bool isStepUp = false;

        private static void setupInds(int len)
        {
            if (len == 0) { indices = new int[0]; return; }
            if (len == 1) { indices = new int[] { 0 }; return; }
            int looplen = len + len - 1;
            indices = new int[looplen];
            int cur = 0, ind = 0;
            while (cur < len) { indices[cur] = ind; cur++; ind++; }
            ind -= 2;
            while (cur < looplen) { indices[cur] = ind; cur++; ind--; }
        }

        public static void SetUp(SpriteRenderer sr, Sprite[] ss)
        {
            spriteRenderer = sr;
            sprites = ss;
            setupInds(ss.Length);
            isStepUp = true;
        }

        public static void Increment()
        {
            if (!isStepUp) return;
            index = (index + 1) % indices.Length;
            spriteRenderer.sprite = sprites[indices[index]];
        }
    }
    private void SetUpHelper()
    {
        AnimationHelper.SetUp( GetComponent<SpriteRenderer>() , sprites );
    }
    private void IncrementHelper() { AnimationHelper.Increment(); }
    #endregion

    void Start()
    {
      //  SetUpHelper();
    }

    private AnimationClip GetClip()
    {
        return GetComponentInChildren<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip;
    }

    private void btnMech(ref bool btn, System.Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }

    public bool btn = false;
    private void press_btn()
    {
        // todo

    }


    #region animator
    private class Anim
    {
        private static SpriteRenderer spriteRenderer;
        private static Sprite[] sprites;
        private const float loopTime = .25f;
        private const int idol_frame = 1;
        private static bool IdolMode = true;
        private static bool issetup = false;

        public static bool IsSetUp() { return issetup; }

        private static IEnumerator program()
        {
            int ind = 0, len = sprites.Length;
            float delta = loopTime / len;
            while (true)
            {
                if (IdolMode && ind == idol_frame)
                    yield return null;
                else
                {
                    ind = (ind + 1) % len;
                    spriteRenderer.sprite = sprites[ind];
                    yield return new WaitForSeconds(delta);
                }
            }
        }

        public static void SetUp(SpriteRenderer sr, Sprite[] ss, System.Func<IEnumerator,Coroutine> starter)
        {
            spriteRenderer = sr;
            sprites = ss;
            starter(program());
            issetup = true;
        }

        public static void toggleMoveMent()
        {
            IdolMode = !IdolMode;
        }
    }
    private void setup_anim()
    {
        Anim.SetUp(GetComponent<SpriteRenderer>(), sprites,StartCoroutine);
    }
    private void testThis()
    {
        if (Anim.IsSetUp())
            Anim.toggleMoveMent();
        else
            setup_anim();
    }
    
    #endregion



    void Update()
    {
        btnMech(ref btn, testThis);
    }
   
  

    

}
