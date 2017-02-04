using UnityEngine;
using System.Collections;

public class Tracer : MonoBehaviour
{
    public float speed = 500.0f;
    public float lifetime = 1;
    
    public float maxInaccuracy = 4.0f;
    private float variableInaccuracy = 1.2f; // used in machineguns to increase inaccuracy if maintaining fire

    public GameObject bulletImpact;
    public GameObject bulletHole;

    Vector3 velocity = Vector3.zero;
    Vector3 newPos = Vector3.zero;
    Vector3 oldPos = Vector3.zero;
    Vector3 direction;

    int maxHits;

    public void SetUpBullet(float triggerTime)
    {        
        variableInaccuracy = triggerTime;
    }

    // Use this for initialization
    void Start()
    {
        direction = transform.TransformDirection(Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, 1);

        newPos = transform.position;
        oldPos = newPos;
        velocity = speed * transform.forward;
        
        Destroy(gameObject, lifetime);  // schedule for destruction if bullet never hits anything
    }

    // Update is called once per frame
    void Update()
    {
        // assume we move all the way
        newPos += (velocity + direction) * Time.deltaTime;

        // Check if we hit anything on the way

        Vector3 dir = newPos - oldPos;
        float dist = dir.magnitude;

        if (dist > 0)
        {
            // normalize
            dir /= dist;          
        }

        oldPos = transform.position;
        transform.position = newPos;
    }
}
