using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}
