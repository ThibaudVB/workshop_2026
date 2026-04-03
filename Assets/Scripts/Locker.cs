using UnityEngine;
using System.Collections;

public class Locker : MonoBehaviour
{
    [Header("Physics")]
    public Rigidbody lockerRb;
    public float force = 0.5f;

    [Header("Audio")]
    public AudioSource detectionSound1;
    public AudioSource crashSound;
    public AudioSource gasSound;

    [Header("Effects")]
    public ParticleSystem GasEffect;

    [Header("Delay Settings")]
    public bool useDelay = true;
    public float delayTime = 0.3f;

    private bool hasFallen = false;
    private bool hasHitGround = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasFallen) return;

        if (other.CompareTag("Player"))
        {
            hasFallen = true;

            detectionSound1.Play();

            lockerRb.AddForce(transform.forward * force, ForceMode.Impulse);
            lockerRb.AddTorque(transform.right * 2f, ForceMode.Impulse);

            if (useDelay)
            {
                StartCoroutine(ActivateGasWithDelay());
            }
            else
            {
                ActivateGas();
            }
        }
    }

    IEnumerator ActivateGasWithDelay()
    {
        yield return new WaitForSeconds(delayTime);
        ActivateGas();
    }

    void ActivateGas()
    {
        if (GasEffect != null)
        {
            GasEffect.gameObject.SetActive(true);
        }

        if (gasSound != null)
        {
            gasSound.Play();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"OnCollisionEnter triggered with object: {collision.gameObject.name}, tag: {collision.gameObject.tag}");

        if (hasHitGround)
        {
            Debug.Log("hasHitGround is already true, ignoring collision.");
            return;
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Collision with Ground detected.");

            hasHitGround = true;

            if (crashSound != null)
            {
                Debug.Log("Playing crash sound.");
                crashSound.Play();
            }
            else
            {
                Debug.LogWarning("crashSound is null!");
            }

            lockerRb.isKinematic = true;
            Debug.Log("lockerRb.isKinematic set to true.");
        }
        else
        {
            Debug.Log($"Collision with non-Ground object: {collision.gameObject.tag}");
        }
    }
}