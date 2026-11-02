using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MonsterExample : MonoBehaviour, IMonster
{
    Animator animator;
    MonoBehaviour[] movementScripts;

    void Awake()
    {
        animator = GetComponent<Animator>();
        movementScripts = GetComponents<MonoBehaviour>();
    }

    public void OnCaptured()
    {
        if (animator != null)
            animator.SetTrigger("Captured");

        foreach (var m in movementScripts)
        {
            if (m != this)
                m.enabled = false;
        }
    }
}