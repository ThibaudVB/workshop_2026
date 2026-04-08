using UnityEngine;                                                            
  using UnityEngine.InputSystem;                                                
  using TMPro;                                                                  
                                                                                
  public class InteractComputer : MonoBehaviour                                 
  {                                                                             
      public float interactDistance = 3f;                                       
      public ReactorGameMenu reactorGameMenu;                                   
      public Transform player;            
      public Camera playerCamera;
      public TMP_Text interactText;                                             
                                          
      public bool isCompleted = false;                                          
                                                                                
      void Start()
      {                                                                         
          if (interactText != null)
              interactText.text = "";
      }                                       
                                          
      void Update()
      {                                                                         
          if (isCompleted)
          {                                                                     
              if (interactText != null)
                  interactText.text = "";
              return;
          }
                                              
          Ray ray = new Ray(playerCamera.transform.position,
  playerCamera.transform.forward);
          RaycastHit hit;                                                       
   
          if (Physics.Raycast(ray, out hit, interactDistance))                  
          {                               
              if (hit.transform == transform)
              {                                                                 
                  if (interactText != null)
                      interactText.text = "Appuie sur E pour interagir";        
                                              
                  if (Keyboard.current.eKey.wasPressedThisFrame)
                  {
                      if (interactText != null)                                 
                          interactText.text = "";
                      reactorGameMenu.StartTerminal();                          
                  }                                                             
              }                           
              else                                                              
              {                                                                 
                  if (interactText != null)
                      interactText.text = "";                                   
              }   
          }
          else
          {
              if (interactText != null)
                  interactText.text = "";     
          }                               
      }
  }  