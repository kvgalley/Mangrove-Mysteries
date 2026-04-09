using UnityEngine;

// Attach to bird GameObjects (egret, pelican, heron, etc.)
// Starts the animation at a random offset so multiple birds don't sync up.
public class WildlifeBird : MonoBehaviour
{
    void Start()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.Play(0, 0, Random.value);
    }
}
