using UnityEngine;

public class Status: MonoBehaviour
{
    public float maxHp = 20;
    public float currentHp = 20;
    public int Level = 1;
    public float nextToLvel = 20;
    public float GiveExp = 10;
    public float maxStamina = 20; // スタミナの最大値
    public float currentStamina = 20; // スタミナの現在値
    void Start()
    {
        maxHp += 1.5f * Level;
        currentHp = maxHp;
        maxStamina += 1.5f * Level;
        currentStamina = maxStamina;
    }
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        if(currentHp < 0)
        {
            Die();
        }
    }
    public void Experience(float exp)
    {
        nextToLvel -= exp;
        if(nextToLvel < 0)
        {
            Level++;
            nextToLvel += 20f * Level;
            
        }
    }
    public void Die()
    {
        Destroy(gameObject);
    }
}
