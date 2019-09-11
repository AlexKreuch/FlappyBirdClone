using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeController : MonoBehaviour
{
    private const string collectorTag = "PipeCollector";
    private const string holderTag = "PipeHolder";

    private void SetUp()
    {
        var trans = GetComponentsInChildren<Transform>();
        float _1st = 0f, _2nd = 0f, tmp = 0f;
        int flag = 0; // 0:=no-pipes found ; 1-pipe found 2(or-more)-pipes found
        int holderCount = 0;
        foreach (var tran in trans)
        {
            GameObject obj = tran.gameObject;
            switch (obj.tag)
            {
                case collectorTag:
                    obj.GetComponent<PipeCollector>().SetSendToController(HandlePipe);
                    break;
                case holderTag:
                    holderCount++;
                    switch (flag)
                    {
                        case 0: _1st = obj.transform.position.x; flag++; break;
                        case 1: 
                            tmp = obj.transform.position.x;
                            if (tmp > _1st) _2nd = tmp; else { _2nd = _1st; _1st = tmp; }
                            flag++;
                            break;
                        case 2:
                            tmp = obj.transform.position.x;
                            if (tmp < _2nd)
                            {
                                if (tmp < _1st){ _2nd = _1st; _1st = tmp; }
                                else _2nd = tmp;
                            }
                            break;
                    }
                    SetPipePos(obj);
                    break;
            }
        }
        pipeOffset = (_2nd - _1st) * holderCount;
    }
    
    [SerializeField]
    private float pipeOffset = 0f; // the horizontal distance a pipeHolder must be moved to be at the front

    /* Randomly set the vertical position of a given pipeHolder
     * 
     * **/
    private void SetPipePos(GameObject pipeHolder)
    {
        const float max = 2f;
        const float min = -1.4f;
        var tmp = pipeHolder.transform.position;
        tmp.y = Random.Range(min,max);
        pipeHolder.transform.position = tmp;
    }

    /* Move a pipe from the far left up to the front
     * 
     * 
     * **/
    private void HandlePipe(GameObject pipeHolder)
    {
        var tmp = pipeHolder.transform.position;
        tmp.x += pipeOffset;
        pipeHolder.transform.position = tmp;
        SetPipePos(pipeHolder);
    }

    void Start()
    {
        SetUp();
    }

}
