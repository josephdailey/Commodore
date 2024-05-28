using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FighterAgent : Agent
{
    public ShipController Controller{get; private set;}
    protected BufferSensorComponent bufferSensor;
    // Start is called before the first frame update
    void Start()
    {
        Controller = GetComponent<ShipController>();
        bufferSensor = GetComponent<BufferSensorComponent>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // There are better/more compact representations; but the idea is to have this as close to the human-facing physical information as possible, to be "fair."

        // Rotation is frame-invariant...
        sensor.AddObservation(Controller.Rbody.angularVelocity);
        sensor.AddObservation(Controller.Health / Controller.MaxHealth);

        // ...position/velocity aren't, so we use the parent craft (or some other point) as a reference.
        // Transform[Point/Direction]() : self-centered -> global coordinates. InverseTransform is the... uh... inverse.

        // This is a stupid idiom but trust me bro
        Vector3 parentRelPos = transform.InverseTransformPoint(-transform.localPosition);
        Vector3 parentRelVel = transform.InverseTransformDirection(transform.parent.gameObject.GetComponentInChildren<Rigidbody>().velocity - Controller.Rbody.velocity);

        bufferSensor.AppendObservation(ToTaggedArray(parentRelPos, parentRelVel, 0));

        foreach(Collider contact in Controller.Contacts){
            Vector3 relPos = transform.InverseTransformPoint(contact.transform.position - transform.position);
            Vector3 relVel = transform.InverseTransformDirection(contact.GetComponent<Rigidbody>().velocity - Controller.Rbody.velocity);
            bufferSensor.AppendObservation(ToTaggedArray(relPos, relVel, 1));
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<float> contActions = actions.ContinuousActions;
        Vector3 commandThrust = new(contActions[0], contActions[1], contActions[2]);
        Vector3 commandTorque = new(contActions[3], contActions[4], contActions[5]);
        Controller.ControlAction(commandThrust, commandTorque);

        if(actions.DiscreteActions[0] == 1){
            Controller.Fire();
            AddReward(-0.01f); // Just so they don't shoot all the time, I'll implement ammo or something properly later
        }
    }

    private float[] ToTaggedArray(Vector3 relPos, Vector3 relVel, int tagIndex){
        Vector3 nRelPos = relPos / Controller.SensorRadius;

        // Need as many zeros as object types you want to differentiate; TODO do this in a way that doesn't suck, probably an enum
        float[] observation = {0.0f, 0.0f, nRelPos.x, nRelPos.y, nRelPos.z, relVel.x, relVel.y, relVel.z};

        // Puts a one-hot "entity type" encoding in position tagIndex; 
        observation[tagIndex] = 1.0f;
        return observation;
    }

    // Broadcast receivers

    void OnShotHit(){
        AddReward(0.1f);
    }

    void OnShotKill(){
        AddReward(1.0f);
        EndEpisode();
    }

    void OnTakeDamage(float damage){
        AddReward(-(damage / Controller.MaxHealth));
    }

    void OnShipDeath(){
        AddReward(-1.0f);
        EndEpisode();
    }
}
