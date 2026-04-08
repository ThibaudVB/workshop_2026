  using UnityEngine;
  using System.Collections;

  public class NeonFlicker : MonoBehaviour
  {
      [Header("Scintillement")]
      public float minIntensity = 0f;
      public float maxIntensity = 3f;
      public float flickerSpeed = 0.05f;

      [Header("Son")]
      public AudioClip[] crackSounds;
      public float[] crackVolumes;

      private Light[] neonLights;
      private AudioSource audioSource;
      private Material neonMaterial;
      private Color baseEmissionColor;
      private float baseIntensity;

      void Start()
      {
          neonLights = GetComponentsInChildren<Light>();
          audioSource = GetComponent<AudioSource>();
          neonMaterial = GetComponent<Renderer>().material;
          baseEmissionColor = neonMaterial.GetColor("_EmissionColor");
          baseIntensity = neonLights[0].intensity;

          StartCoroutine(FlickerRoutine());
      }

      IEnumerator FlickerRoutine()
      {
          yield return new WaitForSeconds(Random.Range(2f, 6f));

          while (true)
          {
              int flickerCount = Random.Range(2, 8);

              if (crackSounds != null && crackSounds.Length > 0 && audioSource
  != null)
              {
                  int index = Random.Range(0, crackSounds.Length);
                  float volume = (crackVolumes != null && index <
  crackVolumes.Length)
                      ? crackVolumes[index]
                      : 1f;
                  audioSource.PlayOneShot(crackSounds[index], volume);
              }

              for (int i = 0; i < flickerCount; i++)
              {
                  float t = Random.Range(minIntensity, maxIntensity);

                  foreach (Light l in neonLights)
                      l.intensity = t;

                  neonMaterial.SetColor("_EmissionColor", baseEmissionColor *
  t);

                  yield return new WaitForSeconds(flickerSpeed);
              }

              foreach (Light l in neonLights)
                  l.intensity = baseIntensity;

              neonMaterial.SetColor("_EmissionColor", baseEmissionColor);

              yield return new WaitForSeconds(Random.Range(2f, 6f));
          }
      }
  }

