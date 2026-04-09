using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TriggerMonstreCinematique : MonoBehaviour
{
    [SerializeField] private Animator monsterAnimator;
    [SerializeField] private NavMeshAgent monsterAgent;
    [SerializeField] private GameObject monster;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform waypointStart;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float cinematicDuration = 5f;
    [SerializeField] private FearSystem fearSystem;
    [SerializeField] private StorylineManager.VoiceLine[] postCinematicVoiceLines;
    [SerializeField] private AudioSource monsterVoiceAudioSource;
    [SerializeField] private AudioClip monsterPatrolSound;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered && PickupSeringue.seringueCollected)
        {
            triggered = true;
            StartCoroutine(PlayCinematic());
        }
    }

    private IEnumerator PlayCinematic()
    {
        PlayerController.cinematicMode = true;
        AlienMonster.cinematicMode = true;
        fearSystem.ForceMaxFear();

        monster.SetActive(true);
        monsterAgent.enabled = false;
        monster.transform.position = pointA.position;
        monster.transform.LookAt(pointB);

        if (monsterVoiceAudioSource != null && monsterPatrolSound != null)
        {
            monsterVoiceAudioSource.clip = monsterPatrolSound;
            monsterVoiceAudioSource.loop = true;
            monsterVoiceAudioSource.Play();
        }

        monsterAnimator.SetTrigger("Near");

        float elapsed = 0f;
        while (elapsed < cinematicDuration)
        {
            monster.transform.position = Vector3.MoveTowards(
                monster.transform.position,
                pointB.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(monster.transform.position, pointB.position) < 0.1f)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        monsterAgent.enabled = true;
        monsterAgent.Warp(waypointStart.position);
        monster.transform.position = waypointStart.position;

        if (monsterVoiceAudioSource != null)
            monsterVoiceAudioSource.Stop();

        fearSystem.ResetFear();
        PlayerController.cinematicMode = false;
        AlienMonster.cinematicMode = false;

        StorylineManager.Instance.PlayVoiceLines(postCinematicVoiceLines);
    }
}