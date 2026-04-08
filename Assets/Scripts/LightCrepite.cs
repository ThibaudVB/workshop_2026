using UnityEngine;
  using System.Collections;

  public class FlickerLight : MonoBehaviour
  {
      [Header("Scintillement")]
      public float minIntensity = 0f;
      public float maxIntensity = 2f;
      public float flickerSpeed = 0.05f;

      [Header("Son")]
      public AudioClip[] crackSounds;
      public float[] crackVolumes;

      private Light pointLight;
      private AudioSource audioSource;
      private float baseIntensity;

      void Start()
      {
          pointLight = GetComponent<Light>();
          audioSource = GetComponent<AudioSource>();
          baseIntensity = pointLight.intensity;

          StartCoroutine(FlickerRoutine());
      }

      IEnumerator FlickerRoutine()
      {
          yield return new WaitForSeconds(Random.Range(2f, 6f));

          while (true)
          {
              int flickerCount = Random.Range(2, 8);

              if (crackSounds.Length > 0 && audioSource != null)
              {
                  int index = Random.Range(0, crackSounds.Length);
                  AudioClip clip = crackSounds[index];

                  float volume = (crackVolumes != null && index <
  crackVolumes.Length)
                      ? crackVolumes[index]
                      : 1f;

                  audioSource.PlayOneShot(clip, volume);
              }

              for (int i = 0; i < flickerCount; i++)
              {
                  pointLight.intensity = Random.Range(minIntensity,
  maxIntensity);
                  yield return new WaitForSeconds(flickerSpeed);
              }

              pointLight.intensity = baseIntensity;
              yield return new WaitForSeconds(Random.Range(2f, 6f));
          }
      }
  }
