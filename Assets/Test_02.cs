using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class Test_02 : MonoBehaviour
{

    private const string controllerName = "BlueBirdFlapping_controller";

    private void btnMech(ref bool btn, System.Action action)
    {
        if (btn)
        {
            btn = false;
            action();
        }
    }

   

    private void SetUp()
    {
        Animator anim = gameObject.AddComponent<Animator>();
        anim.runtimeAnimatorController = Resources.Load<AnimatorController>(controllerName);

        var sr = gameObject.AddComponent<SpriteRenderer>();
    }
    void Start() { SetUp(); }

   
}
