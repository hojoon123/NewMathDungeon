using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int stageIndex;
    public PlayerMove player;
    public GameObject[] Stages;

    
    //다음스테이지 일단 변경할건데 기본틀 ㅇㅇ
    public void NextStage()
    {
        if(stageIndex < Stages.Length - 1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerReposition();
        }
        
    }


    //플레이어 원위치
    public void PlayerReposition()
    {
        player.transform.position = new Vector3(0f, 0f, 0);
        player.VelocityZero();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
