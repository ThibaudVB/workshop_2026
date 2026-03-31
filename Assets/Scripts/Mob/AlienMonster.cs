 using UnityEngine;                                                            
  using UnityEngine.AI;                                                         
  using System.Collections;                                                     
                                                                                
  public class AlienMonster : MonoBehaviour                                     
  {                                                                             
      public Transform player;                                                  
      public float detectionRange = 15f;
      public float screamRange = 1.5f;                                          
      public float randomMoveRadius = 5f;                                       
      public float randomMoveInterval = 3f;                                     
      public float lungeDuration = 0.2f;                                        
                                                                                
      [Header("Screamer")]                                                      
      public GameObject screamerImage;                                          
      public AudioClip screamerSound;                                           
                                                                                
      private NavMeshAgent agent;                                               
      private AudioSource audioSource;                                          
      private float timer;                                                      
      private bool hasScreamed = false;                                         
      private Vector3 playerSpawnPoint;                                         
      private Vector3 monsterSpawnPoint;                                        
                                                                                
      void Start()                                                              
      {                                                                         
          agent = GetComponent<NavMeshAgent>();                                 
          audioSource = GetComponent<AudioSource>();                            
          player = GameObject.FindWithTag("Player").transform;                  
          playerSpawnPoint = player.position;                                   
          monsterSpawnPoint = transform.position;                               
      }                                                                         
                                                                                
      void Update()                                                             
      {           
          if (!agent.isOnNavMesh) return;                                       
                                                                                
          float distanceToPlayer = Vector3.Distance(transform.position,         
  player.position);                                                             
                                                                                
          if (distanceToPlayer <= screamRange && !hasScreamed)                  
          {
              hasScreamed = true;                                               
              StartCoroutine(ScreamerSequence());                               
              return;                                                           
          }                                                                     
                                                                                
          if (distanceToPlayer < detectionRange)
          {                                                                     
              ChasePlayer();                                                    
          }
          else                                                                  
          {       
              WanderRandomly();                                                 
          }                                                                     
      }                                                                         
                                                                                
      void ChasePlayer()                                                        
      {
          agent.speed = 3.5f;                                                   
          agent.SetDestination(player.position);                                
      }                                                                         
                                                                                
      void WanderRandomly()                                                     
      {           
          agent.speed = 1.5f;                                                   
          timer += Time.deltaTime;                                              
                                                                                
          if (timer >= randomMoveInterval)                                      
          {                                                                     
              Vector3 randomPos = transform.position + Random.insideUnitSphere *
   randomMoveRadius;                                                            
              NavMeshHit hit;                                                   
                                                                                
              if (NavMesh.SamplePosition(randomPos, out hit, randomMoveRadius,  
  NavMesh.AllAreas))                                                            
              {                                                                 
                  agent.SetDestination(hit.position);
              }                                                                 
   
              timer = 0f;                                                       
          }       
      }                                                                         
                  
      IEnumerator ScreamerSequence()                                            
      {           
          agent.enabled = false;                                                
                                                                                
          // Saut face caméra                                                   
          float elapsed = 0f;                                                   
          Vector3 startPos = transform.position;                                
          Vector3 targetPos = player.position + player.forward * 0.5f +         
  Vector3.up * 1f;                                                              
                                                                                
          while (elapsed < lungeDuration)                                       
          {       
              transform.position = Vector3.Lerp(startPos, targetPos, elapsed /
  lungeDuration);                                                               
              elapsed += Time.deltaTime;
              yield return null;                                                
          }       
                                                                                
          // Affiche screamer                                                   
          if (screamerImage != null)                                            
              screamerImage.SetActive(true);                                    
                                                                                
          if (screamerSound != null && audioSource != null)                     
              audioSource.PlayOneShot(screamerSound);                           

          yield return new WaitForSeconds(0f);                                

          // Cache screamer                                                     
          if (screamerImage != null)
              screamerImage.SetActive(false);                                   
                                                                                
          // Respawn joueur                                                     
          CharacterController cc = player.GetComponent<CharacterController>();  
          if (cc != null) cc.enabled = false;                                   
          player.position = playerSpawnPoint;                                   
          if (cc != null) cc.enabled = true;                                    
                                                                                
          // Respawn monstre                                                    
          agent.enabled = false;                                                
          transform.position = monsterSpawnPoint;                               
          agent.enabled = true;                                                 
          hasScreamed = false;                                                  
      }                                                                         
  }                      