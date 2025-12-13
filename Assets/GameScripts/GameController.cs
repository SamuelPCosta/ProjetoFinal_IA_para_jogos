using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GameController : MonoBehaviour
{
    [SerializeField] private int frameRate = 60;
    [Space(10)]
    [Header("Game attributes")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private int damage = 2;

    private void Start()
    {
#if UNITY_EDITOR
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;
#endif
    }

    private void Update(){
        if(playerController.getLife() <= 0)
        {

        }
        //if ()
        //{

        //}
    }
}
