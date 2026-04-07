using UnityEngine;
using UnityEngine.InputSystem;

public class Pickup : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] public Transform player;
    [SerializeField] public GameObject promptObject;
    private bool collected = false;

    private InputAction interactAction;

    void Awake()
    {
        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.Enable();
    }

    void OnDestroy()
    {
        interactAction.Disable();
        interactAction.Dispose();
    }

    void Start()
    {
        if (promptObject != null)
            promptObject.SetActive(false);
    }

    void Update()
    {
        if (collected) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionDistance)
        {
            if (promptObject != null)
                promptObject.SetActive(true);

            if (interactAction.WasPressedThisFrame())
                Collect();
        }
        else
        {
            if (promptObject != null)
                promptObject.SetActive(false);
        }
    }

    private void Collect()
    {
        collected = true;
        if (promptObject != null)
            promptObject.SetActive(false);

        OnCollected();
        gameObject.SetActive(false);
    }

    protected virtual void OnCollected() { }
}