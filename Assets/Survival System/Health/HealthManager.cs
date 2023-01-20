using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityTutorial.Manager;

public class HealthManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float _maxHealth = 1000f;
    public float CurrentHealth;
    public float HealthPercent => CurrentHealth / _maxHealth;
    public bool IsDead { get; private set; } = false;
    public UnityEvent onDeath;

    [SerializeField] private GameOverScreen _gameOverScreen;

    private void Start()
    {
        CurrentHealth = _maxHealth;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public void ReplenishHealth(float healthAmount)
    {
        CurrentHealth += healthAmount;

        if (CurrentHealth > _maxHealth) CurrentHealth = _maxHealth;
    }

    public void DamagePlayer(float damageAmount)
    {
        if (IsDead) return;
        CurrentHealth -= damageAmount;

        if (CurrentHealth <= 0)
        {
            IsDead = true;
            onDeath.Invoke();
        }
    }
}
