using UnityEngine;

public class DamageOnClick : MonoBehaviour
{
    public float damageAmount = 5f;

    void OnMouseDown()
    {
        var status = GetComponent<Status>();
        if (status != null)
        {
            status.TakeDamage(damageAmount);
            Debug.Log($"Damaged {gameObject.name} for {damageAmount}");
        }
    }
}