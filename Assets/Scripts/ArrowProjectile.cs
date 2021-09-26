using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] GameObject hitVFX;

    public float force;
    public Rigidbody rigidbody;
    Cinemachine.CinemachineImpulseSource source;
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.centerOfMass = transform.position;
    }

    public void Fire()
    {
        rigidbody.AddForce(transform.forward * (100 * Random.Range(1.3f, 1.7f)), ForceMode.Impulse);
        source = GetComponent<Cinemachine.CinemachineImpulseSource>();

        source.GenerateImpulse(Camera.main.transform.forward);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name != "Player")
        {
            // PoolManager.Release(hitVFX, collision.GetContact(0).point, Quaternion.LookRotation(collision.GetContact(0).normal * -1f));
            GameObject hitVFXObject = Instantiate(hitVFX, collision.GetContact(0).point, Quaternion.LookRotation(collision.GetContact(0).normal * -1f));
            rigidbody.isKinematic = true;
            StartCoroutine(Countdown());
        }

        if (collision.gameObject.tag == "Spider_leg" ||
            collision.gameObject.tag == "Spider_head" ||
            collision.gameObject.tag == "Spider_back" ||
            collision.gameObject.tag == "Spider_body"
        )
        {
            GameObject AIController = GameObject.Find("SpiderAIController");
            if (AIController != null) {
                SpiderAIController cs = AIController.GetComponent<SpiderAIController>();
                cs.lifeChange(-5);
            }
            
        }
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }


}
