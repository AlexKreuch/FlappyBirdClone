using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteResource : MonoBehaviour
{
    [SerializeField]
    private Sprite[] BirdSprites = new Sprite[0];

    public Sprite[] GetBirdSprites() { return BirdSprites; }
}
