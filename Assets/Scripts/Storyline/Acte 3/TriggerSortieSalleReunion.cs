using UnityEngine;

public class TriggerSortieSalleReunion : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !triggered && TriggerSalleReunionCarl.carlDecouvert)
        {
            triggered = true;
            AlienMonster.cinematicMode = false;
        }
    }
}