using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTutorial.Manager;

public class HealthManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float _maxHealth = 1000f;
    private float _currentHealth;
    public float HealthPercent => _currentHealth / _maxHealth;

    [SerializeField] private GameOverScreen _gameOverScreen;

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public void ReplenishHealth(float healthAmount)
    {
        _currentHealth += healthAmount;

        if (_currentHealth > _maxHealth) _currentHealth = _maxHealth;
    }

    public void DamagePlayer(float damageAmount)
    {
        _currentHealth -= damageAmount;

        if (_currentHealth <= 0)
        {
            HandleGameOver();
        }
    }

    private void HandleGameOver()
    {
        _gameOverScreen.Show();
    }
}
