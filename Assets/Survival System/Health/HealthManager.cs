using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTutorial.Manager;

public class HealthManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float _maxHealth = 1000f;
    public float CurrentHealth;
    public float HealthPercent => CurrentHealth / _maxHealth;

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
        CurrentHealth -= damageAmount;

        if (CurrentHealth <= 0)
        {
            HandleGameOver();
        }
    }

    private void HandleGameOver()
    {
        _gameOverScreen.Show();
    }
}
