using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public Rigidbody Rbody{get; private set;}

    [SerializeField]
    private float thrustMultiplier = 1.0f;

    [SerializeField]
    private float torqueMultiplier = 1.0f;

    [SerializeField]
    public float MaxHealth{get; private set;} = 50.0f;

    private float _health;
    public float Health{
        get{
            return _health;
        }
        set{
            _health = Mathf.Clamp(value, 0.0f, MaxHealth);
        }
    }

    // This isn't a mistake; contacts will usually be altered with Add() and Remove(), and should be read-only from the outside.
    public HashSet<Collider> Contacts{get; private set;} = new();

    private LayerMask fireMask;
    public float SensorRadius{get; set;}

    void Start()
    {
        Health = MaxHealth;
        Rbody = GetComponentInChildren<Rigidbody>();
        SensorRadius = GetComponent<SphereCollider>().radius;

        // Add things as needed
        fireMask = LayerMask.GetMask("Ship Body");
    }

    void Update(){
        if(transform.localPosition.magnitude > SensorRadius){
            TakeDamage(MaxHealth);
        }
    }

    public void ControlAction(Vector3 thrustVector, Vector3 torqueVector){
        Rbody.AddRelativeForce(thrustMultiplier * thrustVector);
        Rbody.AddRelativeTorque(torqueMultiplier * torqueVector);
    }

    public void Fire(){
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, 100f, fireMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
            if (hit.collider)
                if(hit.collider.GetComponentInParent<ShipController>().TakeDamage(10f)){
                    BroadcastMessage("OnShotKill");
                }
                BroadcastMessage("OnShotHit");
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 100f, Color.yellow);
        }
    }

    // Yes these are stubby but maybe I'll need callbacks later idk
    public bool TakeDamage(float damage){
        BroadcastMessage("OnTakeDamage", damage);
        Health -= damage;
        if(Health == 0f){
            BroadcastMessage("OnDeath");

            // Use later with multiple ships
            // gameObject.SetActive(false);

            return true;
        }
        return false;
    }

    public void Repair(float repairValue){
        Health += repairValue;
    }

    private void OnTriggerEnter(Collider other){
        Contacts.Add(other);
    }

    private void OnTriggerExit(Collider other){
        Contacts.Remove(other);
    }
}
