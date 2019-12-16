using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteResource : MonoBehaviour
{
    #region BirdSprites
    [SerializeField]
    private Sprite[] BirdSprites = new Sprite[0];
    public Sprite[] GetBirdSprites() { return BirdSprites; }
    #endregion

    #region MedalSprites
    [SerializeField]
    private Sprite[] MedalSprites = new Sprite[0]; // [White, Orange, Yellow];
    public struct MedalSpriteSet
    {
        public Sprite White { get; private set; }
        public Sprite Orange { get; private set; }
        public Sprite Yellow { get; private set; }
        public MedalSpriteSet(Sprite w, Sprite o, Sprite y)
        {
            White = w;
            Orange = o;
            Yellow = y;
        }
    }
    public MedalSpriteSet GetMedalSprites()
    {
        Debug.Assert(MedalSprites!=null && MedalSprites.Length==3 && MedalSprites[0] != null && MedalSprites[1] != null && MedalSprites[2] != null  ,"INVALED SPRITE-RESOURCE");
        return new MedalSpriteSet(MedalSprites[0], MedalSprites[1], MedalSprites[2]);
    }
    #endregion

    #region CurtainSprites
    [SerializeField]
    private Sprite[] CurtainSprites = new Sprite[0]; // [ None , Red , Green , Blue ]
    public class CurtainSpriteSet
    {
        private Sprite[] sprites = null;
        public CurtainSpriteSet(Sprite[] sprites) { this.sprites = sprites; }
        public Sprite Normal { get { return sprites[0]; } }
        public Sprite RedUnlocked { get { return sprites[1]; } }
        public Sprite GreenUnlocked { get { return sprites[2]; } }
        public Sprite BlueUnlocked { get { return sprites[3]; } }
    }
    public CurtainSpriteSet GetCurtainSprites() { return new CurtainSpriteSet(CurtainSprites); }
    #endregion
}
