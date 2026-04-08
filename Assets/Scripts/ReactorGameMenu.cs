 using UnityEngine;                                                            
  using UnityEngine.InputSystem;
  using TMPro;                                                                  
  using System.Collections;                                                     
   
  public class ReactorGameMenu : MonoBehaviour                                  
  {               
      public TMP_Text terminalText;
      public AudioClip typingSound;
      public AudioSource audioSource;                                           
      public GameObject terminalPanel;
      public ReactorMinigame reactorMinigame;                                   
                                                                                
      [Header("Son clavier")]
      public float typingVolume = 0.5f;                                         
      public float typingPitch = 1f;                                            
      public float typingInterval = 0.05f;
                                                                                
      private string fullText = "";
      private bool waitingForInput = false;                                     
      private bool isCompleted = false;
                                                                                
      string[] lines = {
          "> INITIALISATION DU SYSTÈME...",                                     
          "> CHARGEMENT DES MODULES...",                                        
          "> CONNEXION AU SERVEUR...",
          "> AVERTISSEMENT : ACTIVITÉ SUSPECTE DÉTECTÉE",                       
          "> LANCEMENT DU PROTOCOLE D'URGENCE...",                              
          "> AUTODESTRUCTION DISPONIBLE",                                       
          "> ",                                                                 
          "> APPUIE SUR ENTRÉE POUR LANCER_"                                    
      };                                                                        
   
      void Start()                                                              
      {           
          terminalPanel.SetActive(false);                                       
      }           

      public void StartTerminal()
      {
          if (isCompleted) return;
                                                                                
          terminalPanel.SetActive(true);
          fullText = "";                                                        
          waitingForInput = false;
          terminalText.text = "";                                               
          StartCoroutine(TypeLines());
      }                                                                         
                  
      public void SetCompleted()                                                
      {
          isCompleted = true;                                                   
          terminalPanel.SetActive(false);
      }

      IEnumerator TypeLines()                                                   
      {
          foreach (string line in lines)                                        
          {       
              yield return StartCoroutine(TypeLine(line));
              yield return new WaitForSeconds(0.3f);
          }                                                                     
   
          StartCoroutine(BlinkCursor());                                        
          waitingForInput = true;
      }

      IEnumerator TypeLine(string line)                                         
      {
          foreach (char c in line)                                              
          {       
              fullText += c;
              terminalText.text = fullText;

              // Joue le son seulement si c'est pas un espace                   
              if (c != ' ' && typingSound != null && audioSource != null)
              {                                                                 
                  audioSource.pitch = typingPitch + Random.Range(-0.1f, 0.1f);
                  audioSource.PlayOneShot(typingSound, typingVolume);           
              }                                                                 
   
              yield return new WaitForSeconds(typingInterval);                  
          }       

          // Stop le son quand la ligne est finie                               
          audioSource.Stop();
                                                                                
          fullText += "\n";
          terminalText.text = fullText;
      }                                                                         
   
      IEnumerator BlinkCursor()                                                 
      {           
          bool show = true;
          while (waitingForInput)
          {                                                                     
              string last = fullText.TrimEnd('_', ' ');
              terminalText.text = show ? fullText : last;                       
              show = !show;                                                     
              yield return new WaitForSeconds(0.5f);
          }                                                                     
      }           

      void Update()                                                             
      {
          if (waitingForInput && Keyboard.current.enterKey.wasPressedThisFrame) 
          {       
              waitingForInput = false;
              terminalPanel.SetActive(false);
              reactorMinigame.StartMinigame();                                  
          }
      }                                                                         
  }               
