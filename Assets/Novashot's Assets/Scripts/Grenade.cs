using UnityEngine;

public class Grenade : MonoBehaviour
{

    public float lifetime = 10.0f;
    public GameObject explosion;   // instanced explosion
    
    private float damage;             // damage bullet applies to a target
    private float impactForce;        // force applied to a rigid body object
    public float maxInaccuracy = 2.0f;      // maximum amount of inaccuracy
    public float variableInaccuracy = 0.2f; // used in machineguns to decrease accuracy if maintaining fire
    private float speed = 75.0f;              // bullet speed

    private Vector3 velocity = Vector3.zero; // bullet velocity    
    private Vector3 direction;               // direction bullet is travelling
    private GameObject owner;               // owner of bullet
    private string ownersName;              // owner's name

    private string[] grenadeInfo = new string[2];

    public void SetUp(float[] info)
    {
        damage = info[0];
        impactForce = info[1];
        //maxHits = info[2];
        maxInaccuracy = info[3];
        variableInaccuracy = info[4];
        speed = info[5];

        //direction = transform.TransformDirection(0, 0, 1);
        direction = transform.TransformDirection(Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, Random.Range(-maxInaccuracy, maxInaccuracy) * variableInaccuracy, 1);

        velocity = speed * transform.forward;

        GetComponent<Rigidbody>().velocity = velocity + direction;

        // schedule for destruction if bullet never hits anything
        Destroy(gameObject, lifetime);
 
    }

    void Start()
    {
        
    }

    void OnCollisionEnter(Collision enterObject)
    {
        // things to add:
        // maybe a distance or time check to see if grenade is far enough away to arm before exploding
        // ... maybe a non armed grenade will bounce then explode
        // similar to direct noob tube shots in CoD

        switch (enterObject.transform.tag)
        {
            case "bullet":
                //return;                
                break;            
            default:
                Destroy(gameObject, 0);//GetComponent<Rigidbody>().useGravity = false;
                ContactPoint contact = enterObject.contacts[0];
                Quaternion rotation = gameObject.GetComponent<Rigidbody>().rotation;  //Quaternion.FromToRotation(Vector3.up, contact.normal); 

                Instantiate(explosion, contact.point, rotation);

                grenadeInfo[0] = ownersName;
                grenadeInfo[1] = damage.ToString();

                enterObject.collider.SendMessageUpwards("ImHit", grenadeInfo, SendMessageOptions.DontRequireReceiver);


                if (enterObject.rigidbody)
                {
                    enterObject.rigidbody.AddForce(transform.forward * impactForce, ForceMode.Impulse);
                }
                break;
        }
        
    }

    public void setPlayer(string pName)
    {
        ownersName = pName;
        Debug.Log(ownersName + " owns this Rocket-  Send via setLauncher function");
    }
}