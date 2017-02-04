using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rocket : MonoBehaviour
{
    public float lifetime = 5.0f;
    public GameObject explosion;        // instanced explosion
    private float damage;               // damage bullet applies to a target
    //private float maxHits;              // number of collisions before bullet gets destroyed
    private float impactForce;          // force applied to a rigid body object
    private float maxInaccuracy;        // maximum amount of inaccuracy
    private float variableInaccuracy;   // used in machineguns to decrease accuracy if maintaining fire
    private float speed;                // bullet speed
    //private string ownersName = "";     // name of player that launched missle

    private float framesTillRedirect = 0.0f;  // time till rocket picks a new random flight path

    private Vector3 velocity = Vector3.zero; // bullet velocity
    private Vector3 newPos = Vector3.zero;   // bullet's new position
    private Vector3 direction;               // direction bullet is travelling

    private GameObject owner;

    public GameObject Owner  // sets and returns the bullet owners gameObject
    {
        get { return owner; }
        set { owner = value; }
    }

    public void SetUp(float[] info)
    {
        damage = info[0];
        impactForce = info[1];
        //maxHits = info[2];
        maxInaccuracy = info[3];
        variableInaccuracy = info[4];
        speed = info[5];

        direction = transform.TransformDirection(Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, 1);

        newPos = transform.position;
        velocity = speed * transform.forward;

        // schedule for destruction if bullet never hits anything
        Invoke("Kill", lifetime);
    }

    void Update()
    {
        if (Time.time >= framesTillRedirect)
        {
            direction = transform.TransformDirection(Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, 1);
            framesTillRedirect = Time.time + Random.Range(10, 50);
        }
        
        // assume we move all the way
        newPos += (velocity + direction) * Time.deltaTime;

        transform.position = newPos;
    }


    void OnCollisionEnter(Collision collision)
    {
        // Instantiate explosion at the impact point and rotate the explosion
        // so that the y-axis faces along the surface normal
        ContactPoint contact = collision.contacts[0];
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
        GameObject exp = Instantiate(explosion, contact.point, rotation) as GameObject;

        ExplosionInfo expInfo = new ExplosionInfo();

        expInfo.owner = owner;
        expInfo.damage = damage;
        expInfo.impactForce = impactForce;
        
        exp.GetComponent<Explosion>().expInfo = expInfo;

        // And kill our selves
        Kill();
    }

    void Kill()
    {
        // Stop emitting particles in any children
        ParticleEmitter emitter = (ParticleEmitter)GetComponentInChildren<ParticleEmitter>();
        if (emitter)
            emitter.emit = false;

        // Detach children - We do this to detach the trail rendererer which should be set up to auto destruct
        transform.DetachChildren();

        // Destroy the projectile
        Destroy(gameObject);
    }

    /*public void setPlayer(string pName)
    {
        ownersName = pName;
        Debug.Log(ownersName + " owns this Rocket-  Send via setLauncher function");
    }*/

}
