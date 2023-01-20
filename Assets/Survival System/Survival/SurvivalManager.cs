using UnityEngine;
using UnityEngine.Events;
using UnityTutorial.Manager;
using Polyperfect.Crafting.Integration.UGUI;

namespace SurvivalSystem
{
    public class SurvivalManager : MonoBehaviour
    {
        [Header("Hunger")]
        [SerializeField] private float _maxHunger = 100f;
        [SerializeField] private float _hungerDepletionRate = 1f;
        [SerializeField] private float _hungerDamageRate = 1f;
        private float _currentHunger;
        public float HungerPercent => _currentHunger / _maxHunger;

        [Header("Thirst")]
        [SerializeField] private float _maxThirst = 100f;
        [SerializeField] private float _thirstDepletionRate = 1f;
        [SerializeField] private float _thirstDamageRate = 2f;
        private float _currentThirst;
        public float ThirstPercent => _currentThirst / _maxThirst;

        [Header("Oxygen")]
        [SerializeField] private float _maxOxygen = 100f;
        [SerializeField] private float _oxygenDepletionRate = 1f;
        [SerializeField] private float _oxygenDamageRate = 5f;
        private float _currentOxygen;
        public float OxygenPercent => _currentOxygen / _maxOxygen;

        [Header("Stamina")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _staminaDepletionRate = 1f;
        [SerializeField] private float _staminaRechargeRate = 2f;
        [SerializeField] private float _staminaRechargeDelay = 1f;
        private float _currentStamina;
        private float _currentStaminaDelayCounter;
        public float StaminaPercent => _currentStamina / _maxStamina;

        private InputManager _inputManager;
        private HealthManager _healthManager;

        public static UnityAction OnPlayerDied;

        public void Awake()
        {
            UGUIItemTransfer.OnFoodConsumed += ReplenishHunger;
            UGUIItemTransfer.OnWaterConsumed += ReplenishThirst;
            UGUIItemTransfer.OnOxygenConsumed += ReplenishOxygen;
        }

        private void Start()
        {
            _currentHunger = _maxHunger;
            _currentThirst = _maxThirst;
            _currentOxygen = _maxOxygen;
            _currentStamina = _maxStamina;

            _inputManager = GetComponent<InputManager>();
            _healthManager = GetComponent<HealthManager>();
        }

        private void Update()
        {
            _currentHunger -= _hungerDepletionRate * Time.deltaTime;
            _currentThirst -= _thirstDepletionRate * Time.deltaTime;
            _currentOxygen -= _oxygenDepletionRate * Time.deltaTime;

            HandleDamage(_currentHunger, _hungerDamageRate);
            HandleDamage(_currentThirst, _thirstDamageRate);
            HandleDamage(_currentOxygen, _oxygenDamageRate);
            HandleStaminaDepletion();

        }

        private void HandleDamage(float currentValue, float damageRate)
        {
            if (currentValue <= 0)
            {
                currentValue = 0;
                _healthManager.DamagePlayer(damageRate);
            }
        }

        private void HandleStaminaDepletion()
        {
            if (_inputManager.Run)
            {
                _currentStamina -= _staminaDepletionRate * Time.deltaTime;
                if (_currentStamina <= 0) _currentStamina = 0;
                _currentStaminaDelayCounter = 0;
            }

            if (!_inputManager.Run && _currentStamina < _maxStamina)
            {
                if (_currentStaminaDelayCounter < _staminaRechargeDelay)
                {
                    _currentStaminaDelayCounter += Time.deltaTime;
                }


                if (_currentStaminaDelayCounter >= _staminaRechargeDelay)
                {
                    _currentStamina += _staminaRechargeRate * Time.deltaTime;
                    if (_currentStamina > _maxStamina) _currentStamina = _maxStamina;
                }

            }
        }

        public bool HasStamina()
        {
            return _currentStamina != 0;
        }

        public void ReplenishHunger(int hungerAmount)
        {
            _currentHunger += hungerAmount;

            if (_currentHunger > _maxHunger) _currentHunger = _maxHunger;
        }

        public void ReplenishThirst(int thirstAmount)
        {
            _currentThirst += thirstAmount;

            if (_currentThirst > _maxThirst) _currentThirst = _maxThirst;
        }

        public void ReplenishOxygen(int oxygenAmount)
        {
            _currentOxygen += oxygenAmount;

            if (_currentOxygen > _maxOxygen) _currentOxygen = _maxOxygen;
        }
    }
}
