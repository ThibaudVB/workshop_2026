using UnityEngine;

public class CarlController : MonoBehaviour
{
    public Animator anim;
    public float heightOffset = 0.5f;

    private bool isDead = false;

    public void OnSoinRecupere()
    {
        if (isDead) return;
        isDead = true;
        anim.SetBool("isDead", true);
        transform.position += new Vector3(0, heightOffset, 0);
    }
}