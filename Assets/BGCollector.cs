using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGCollector : MonoBehaviour
{
    private const string bgtag = "Background"; // bgtag:=Background-tag
    private const string grtag = "Ground"; // grtag:=Ground-tag

    [SerializeField]
    private float bg_offset = 0f;
    [SerializeField]
    private float gr_offset = 0f;
    [SerializeField]
    private Vector3 cameraOffset = new Vector3();


    private void ComputeOffsets()
    {
        /*
           NOTE : 
              * bg:=Background ; gr:=Ground
               vals := [ <bg-started> , bg-width , bg-offset , <gr-started> , gr-width , gr-offset ]
                              0             1           2          3               4           5
         */
        var objs = FindObjectsOfType<GameObject>();
        float[] vals = new float[] { 0f, 0f, 0f, 0f, 0f, 0f };
        foreach (var x in objs)
        {
            int index = 0;
            switch (x.tag)
            {
                case bgtag: index = 0; break;
                case grtag: index = 3; break;
                default: index = -1; break;
            }
            if (index == -1) continue;
            if (vals[index] == 1f) // started
            {
                vals[index + 2] += vals[index + 1];
            }
            else // not-started
            {
                float tmp = x.GetComponent<BoxCollider2D>().size.x;
                vals[index++] = 1f;
                vals[index++] = tmp;
                vals[index] = tmp;
            }
        }
        bg_offset = vals[2];
        gr_offset = vals[5];

        cameraOffset = this.transform.position - Camera.main.transform.position;
        
    }

    private void MaintainPos()
    {
        this.transform.position = Camera.main.transform.position + cameraOffset;
    }

    void OnTriggerEnter(Collider collider)
    {
     
        string tg = collider.tag;
        Vector3 tmp = new Vector3();
        
        switch (tg)
        {
            case bgtag:
                tmp = collider.transform.position;
                tmp.x += bg_offset;
                collider.transform.position = tmp;
                break;
            case grtag:
                tmp = collider.transform.position;
                tmp.x += gr_offset;
                collider.transform.position = tmp;
                break;
            default:break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var collider = collision.collider;
        string tg = collider.tag;
        Vector3 tmp = new Vector3();

        switch (tg)
        {
            case bgtag:
                tmp = collider.transform.position;
                tmp.x += bg_offset;
                collider.transform.position = tmp;
                break;
            case grtag:
                tmp = collider.transform.position;
                tmp.x += gr_offset;
                collider.transform.position = tmp;
                break;
            default: break;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {

        string tg = collider.tag;
        Vector3 tmp = new Vector3();

        switch (tg)
        {
            case bgtag:
                tmp = collider.transform.position;
                tmp.x += bg_offset;
                collider.transform.position = tmp;
                break;
            case grtag:
                tmp = collider.transform.position;
                tmp.x += gr_offset;
                collider.transform.position = tmp;
                break;
            default: break;
        }
    }

    void OnEnable()
    {
        ComputeOffsets();
    }
    void Start() {  }
    void Update()
    {
        MaintainPos();
    }

}
