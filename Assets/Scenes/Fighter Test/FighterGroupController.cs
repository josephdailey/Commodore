using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class FighterGroupController : MonoBehaviour
{
    public FighterAgent fighterPrefab;
    [SerializeField]
    int groupSize = 3;
    SimpleMultiAgentGroup FighterGroup;

    // Start is called before the first frame update
    void Awake(){
        FighterGroup = new();
        ResetGroup();
    }

    void ResetGroup(){
        // This should probably be a static iterable...
        foreach(FighterAgent fighter in FighterGroup.GetRegisteredAgents().Cast<FighterAgent>()){
            fighter.transform.position = 10.0f * Random.insideUnitSphere;
            fighter.transform.rotation = Random.rotation;
            fighter.Controller.Rbody.velocity = Vector3.zero;
        }
        while(FighterGroup.GetRegisteredAgents().Count < groupSize){
            FighterAgent newFighter = Instantiate(fighterPrefab, transform.position + 10.0f * Random.insideUnitSphere, Random.rotation, transform);
            FighterGroup.RegisterAgent(newFighter);
        }
    }

    void OnShipDeath(){
        // If this is the LAST fighter (won't be unregistered until this exits)...
        if(FighterGroup.GetRegisteredAgents().Count == 1){
            FighterGroup.SetGroupReward(-1.0f);
            FighterGroup.EndGroupEpisode();
            SendMessageUpwards("OnFighterGroupLoss", this);
            FighterGroup = new();
            ResetGroup();
        }
    }

    public void OnGroupWin(){
        FighterGroup.SetGroupReward(1.0f);
        FighterGroup.EndGroupEpisode();
        ResetGroup();
    }
}
