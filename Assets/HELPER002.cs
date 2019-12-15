using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

[ExecuteAlways]
public class HELPER002 : MonoBehaviour
{
    public Vector2Int screenSize = new Vector2Int();
    void MaintainScreenSize()
    {
        screenSize = new Vector2Int(Screen.width,Screen.height);
    }
    void Update()
    {
        MaintainScreenSize();
        updateBtn();
    }

    public bool btn = false;
    private void pressbtn()
    {
        Debug.Log("toast-code");
       Toast toast = Toast.Create("<title><tiles:getAsString name=\"title\"/></title>", "asdf");
        toast.Show();
    }
    private void updateBtn()
    {
        if (btn)
        {
            btn = false;
            pressbtn();
        }
    }
}
