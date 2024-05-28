using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterTestArenaController : MonoBehaviour
{
    HashSet<FighterGroupController> fighterGroupControllers;
    // Start is called before the first frame update
    void Awake()
    {
        fighterGroupControllers = new HashSet<FighterGroupController>(GetComponentsInChildren<FighterGroupController>());
    }

    void OnFighterGroupLoss(FighterGroupController lostGroup){
        foreach(FighterGroupController group in fighterGroupControllers){
            if(group != lostGroup){
                group.OnGroupWin();
            }
        }
    }
}
// Gross.