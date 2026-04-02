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
          // Attends avant le premier scintillement                             
          yield return new WaitForSeconds(Random.Range(2f, 6f));

          while (true)
          {
              int flickerCount = Random.Range(2, 8);
                                                                                
              if (crackSounds.Length > 0 && audioSource != null)
              {                                                                 
                  AudioClip clip = crackSounds[Random.Range(0,
  crackSounds.Length)];
                  audioSource.PlayOneShot(clip);
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