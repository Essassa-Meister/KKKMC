using System;
using UnityEngine;

public class Status: MonoBehaviour
{
    public float maxHp = 20;
    public float currentHp = 20;
    public int Level = 1;
    public float nextToLvel = 20;
    public float GiveExp = 10;
    public float maxStamina = 20; // スタミナの最大値
    [SerializeField] private float _currentStamina = 20; // スタミナの現在値
    public float currentStamina
    {
        get => _currentStamina;
        set
        {
            _currentStamina = Mathf.Clamp(value, 0, maxStamina);
            NotifyStamina();
        }
    }

    // (currentHp, maxHp)
    public event Action<float, float> OnHpChanged;
    // (currentStamina, maxStamina)
    public event Action<float, float> OnStaminaChanged;

    void Start()
    {
        maxHp += 1.5f * Level;
        currentHp = maxHp;
        maxStamina += 1.5f * Level;
        currentStamina = maxStamina;
        NotifyHp();
        NotifyStamina();
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0f);
        NotifyHp();
        if (currentHp <= 0f)
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

    // スタミナを消費する
    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(currentStamina, 0f);
        NotifyStamina();
    }

    // スタミナを回復する
    public void RestoreStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        NotifyStamina();
    }

    void NotifyHp()
    {
        OnHpChanged?.Invoke(currentHp, maxHp);
    }

    void NotifyStamina()
    {
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void Die()
    {
        NotifyHp();
        NotifyStamina();
        Destroy(gameObject);
    }
}
