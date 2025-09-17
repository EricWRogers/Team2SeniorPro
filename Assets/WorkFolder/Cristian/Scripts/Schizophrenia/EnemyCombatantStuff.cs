using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyCombatantStuff : MonoBehaviour
{
    
    
    public float enemyMoveSpeed;


    public enum CloneState
    {
        Stalking,
        Chasing, 
        Robbing, 
        Retreiving, 
        Defending
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void EnemyStateHandler()
    {
         //.test(gameObject.name or enemy is in a specific range of the player))
        //{
        //    state = CloneState.Stalking;

        //}
    }
}
