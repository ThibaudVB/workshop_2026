using UnityEngine;                                                            
  using UnityEngine.InputSystem;                                                
  using TMPro;                                                                  
  using System.Collections;                                                     
  using System.Collections.Generic;                                             
                  
  public class ReactorMinigame : MonoBehaviour
  {
      [Header("UI")]
      public GameObject minigamePanel;                                          
      public TMP_Text sequenceText;
      public TMP_Text timerText;                                                
      public TMP_Text resultText;                                               
   
      [Header("Sons")]                                                          
      public AudioClip failSound;
      public float failVolume = 1f;
      public float failPitch = 1f;

      public AudioClip successSound;                                            
      public float successVolume = 1f;
      public float successPitch = 1f;                                           
                  
      public AudioClip victorySound;
      public float victoryVolume = 1f;
      public float victoryPitch = 1f;
                                                                                
      public AudioSource audioSource;
                                                                                
      [Header("Références")]
      public InteractComputer interactComputer;
      public ReactorGameMenu reactorGameMenu;

      private string[] displayKeys = { "Z", "Q", "S", "D" };                    
      private int[] roundLengths = { 4, 6, 8 };
      private float[] roundTimeLimits = { 5f, 8f, 10f };                        
                  
      private List<int> sequence = new List<int>();                             
      private int currentIndex = 0;
      private bool gameActive = false;                                          
      private float timeLeft;
      private int roundsCompleted = 0;
      private MonoBehaviour playerMovement;

      void Start()                                                              
      {
          minigamePanel.SetActive(false);                                       
          sequenceText.text = "";
          timerText.text = "";
          resultText.text = "";

          GameObject player = GameObject.FindWithTag("Player");
          playerMovement = player.GetComponent<MonoBehaviour>();
      }                                                                         
   
      void PlaySound(AudioClip clip, float volume, float pitch)                 
      {           
          if (clip != null && audioSource != null)
          {
              audioSource.pitch = pitch;
              audioSource.PlayOneShot(clip, volume);
          }                                                                     
      }
                                                                                
      public void StartMinigame()
      {
          minigamePanel.SetActive(true);
          roundsCompleted = 0;

          playerMovement.enabled = false;
          Cursor.lockState = CursorLockMode.None;
          Cursor.visible = true;                                                
   
          StartCoroutine(NewRound());                                           
      }           

      IEnumerator NewRound()
      {
          resultText.text = "";
          sequenceText.text = "";
          timerText.text = "";
          currentIndex = 0;                                                     
          gameActive = false;
                                                                                
          int length = roundLengths[roundsCompleted];
          timeLeft = roundTimeLimits[roundsCompleted];

          sequence.Clear();
          for (int i = 0; i < length; i++)
              sequence.Add(Random.Range(0, displayKeys.Length));                
   
          string display = "";                                                  
          for (int i = 0; i < sequence.Count; i++)
          {
              if (i == 0)
                  display += "<color=#FFFF00>" + displayKeys[sequence[i]] +     
  "</color>  ";
              else                                                              
                  display += "<color=#005500>" + displayKeys[sequence[i]] +
  "</color>  ";
          }
          sequenceText.text = display;
          timerText.text = Mathf.CeilToInt(timeLeft).ToString();                
   
          yield return new WaitForSeconds(0.5f);                                
          gameActive = true;
      }

      void Update()
      {
          if (!gameActive) return;
                                                                                
          timeLeft -= Time.deltaTime;
          timerText.text = Mathf.CeilToInt(timeLeft).ToString();                
                  
          if (timeLeft < 5f)
              timerText.color = new Color(1f, 0f, 0f);
          else                                                                  
              timerText.color = new Color(0f, 1f, 0f);
                                                                                
          if (timeLeft <= 0f)
          {
              StartCoroutine(Fail());
              return;
          }

          if (Keyboard.current.zKey.wasPressedThisFrame ||                      
  Keyboard.current.wKey.wasPressedThisFrame)
              CheckKey(0);                                                      
          else if (Keyboard.current.qKey.wasPressedThisFrame ||
  Keyboard.current.aKey.wasPressedThisFrame)                                    
              CheckKey(1);
          else if (Keyboard.current.sKey.wasPressedThisFrame)                   
              CheckKey(2);
          else if (Keyboard.current.dKey.wasPressedThisFrame)
              CheckKey(3);                                                      
      }
                                                                                
      void CheckKey(int keyIndex)
      {
          if (!gameActive) return;

          if (keyIndex == sequence[currentIndex])
          {
              currentIndex++;
              PlaySound(successSound, successVolume, successPitch);
              UpdateSequenceDisplay();                                          
   
              if (currentIndex >= sequence.Count)                               
              {   
                  roundsCompleted++;
                  if (roundsCompleted >= roundLengths.Length)
                      StartCoroutine(Victory());
                  else                                                          
                      StartCoroutine(NewRound());
              }                                                                 
          }       
          else
          {
              StartCoroutine(Fail());
          }
      }

      void UpdateSequenceDisplay()
      {
          string display = "";
          for (int i = 0; i < sequence.Count; i++)                              
          {
              if (i < currentIndex)                                             
                  display += "<color=#00FF00>" + displayKeys[sequence[i]] +
  "</color>  ";
              else if (i == currentIndex)
                  display += "<color=#FFFF00>" + displayKeys[sequence[i]] +     
  "</color>  ";
              else                                                              
                  display += "<color=#005500>" + displayKeys[sequence[i]] +
  "</color>  ";
          }
          sequenceText.text = display;
      }                                                                         
   
      IEnumerator Fail()                                                        
      {           
          gameActive = false;
          PlaySound(failSound, failVolume, failPitch);
          resultText.text = "ERREUR - RECOMMENCE";
          resultText.color = new Color(1f, 0f, 0f);
                                                                                
          yield return new WaitForSeconds(1.5f);
          StartCoroutine(NewRound());                                           
      }           

      IEnumerator Victory()
      {
          gameActive = false;
          PlaySound(victorySound, victoryVolume, victoryPitch);                 
          resultText.text = "AUTODESTRUCTION LANCÉE !";
          resultText.color = new Color(0f, 1f, 0f);                             
                  
          yield return new WaitForSeconds(2f);                                  
   
          if (interactComputer != null)                                         
              interactComputer.isCompleted = true;

          if (reactorGameMenu != null)
              reactorGameMenu.SetCompleted();

          playerMovement.enabled = true;
          Cursor.lockState = CursorLockMode.Locked;
          Cursor.visible = false;                                               
   
          minigamePanel.SetActive(false);                                       
      }           
  }
