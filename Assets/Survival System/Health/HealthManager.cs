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
    private bool _isDead = false;
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
        if (_isDead) return;
        CurrentHealth -= damageAmount;

        if (CurrentHealth <= 0)
        {
            _isDead = true;
            onDeath.Invoke();
        }
    }
}
