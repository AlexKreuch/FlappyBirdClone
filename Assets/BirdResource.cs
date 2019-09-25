using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdResource : MonoBehaviour
{
    [SerializeField]
    private GameObject[] Birds = new GameObject[0]; // [Red, Green, Blue]

    private GameObject GetBird(int index)
    {
        Debug.Assert
            (
                Birds!=null
                && Birds.Length==3
                && Birds[0]!=null
                && Birds[1]!=null
                && Birds[2]!=null,
                "INVALID BIRD-RESOURCE"
            );
        return Birds[index];
    }

    public GameObject RedBird { get { return GetBird(0); } }
    public GameObject GreenBird { get { return GetBird(1); } }
    public GameObject BlueBird { get { return GetBird(2); } }
}
