#define TESTING_MODE
#define BIRD_IN_TESTING_MODE
// USING_INITIAL_BIRD : use this if the GamePlayScene has a defualt bird pre-loaded
#define USING_INITIAL_BIRD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayManager : MonoBehaviour
{
    private Vector3 startingPosition = new Vector3(0f, 0f, 0f);
    private int highScore = 0;


    private void SetUp()
    {
        highScore = GameController.GPPort.GetHighScore();
        char brd = GameController.GPPort.GetCurrentBird();
        var brdBox = Resources.Load<BirdResource>(FlappyBirdUtil.ResourcePaths.BirdRec);
#if TESTING_MODE
        brd = 'G';
#endif
#if (USING_INITIAL_BIRD && BIRD_IN_TESTING_MODE)
        Debug.Log(string.Format("~~ destroying initial-bird : {0}",Bird.instance.gameObject.name));
        Bird.Testing_ERASE();
#endif
        switch (brd)
        {
            case 'R': Instantiate(brdBox.RedBird, startingPosition, new Quaternion()); break;
            case 'G': Instantiate(brdBox.GreenBird, startingPosition, new Quaternion()); break;
            case 'B': Instantiate(brdBox.BlueBird, startingPosition, new Quaternion()); break;
        }
    }

    void Start() { SetUp(); }
}
