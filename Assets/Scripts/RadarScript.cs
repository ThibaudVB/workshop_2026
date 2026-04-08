using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AudioSource))]
public class RadarScript : MonoBehaviour
{
    [Header("Paramètres du bip")]
    public float minInterval = 0.2f;
    public float maxInterval = 1f;

    private float maxDistance;
    private float nextBeepTime = 0f;

    private bool targetInZone = false;
    private Transform target; 

    private AudioSource audioSource;

    void Start()
    {
        SphereCollider sc = GetComponent<SphereCollider>();
        maxDistance = sc != null ? sc.radius : 5f;

        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sphere"))
        {
            target = other.transform;
            targetInZone = true;

            Debug.Log("Sphere détectée !");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Sphere"))
        {
            targetInZone = false;
            target = null;
        }
    }

    void Update()
    {
        if (targetInZone && target != null)
        {
            float dist = Vector3.Distance(target.position, transform.position);

            float t = Mathf.Clamp01(dist / maxDistance);
            float interval = Mathf.Lerp(minInterval, maxInterval, t);

            if (Time.time >= nextBeepTime)
            {
                audioSource.PlayOneShot(audioSource.clip);
                nextBeepTime = Time.time + interval;
            }
        }
    }
}