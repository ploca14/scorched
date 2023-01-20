using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalSystem
{
    public class SurvivalUIManager : MonoBehaviour
    {
        [SerializeField] private SurvivalManager _survivalManager;
        [SerializeField] private Image _hungerMeter, _thirstMeter, _oxygenMeter, _staminaMeter;

        private void FixedUpdate()
        {
            _hungerMeter.fillAmount = _survivalManager.HungerPercent;
            _thirstMeter.fillAmount = _survivalManager.ThirstPercent;
            _oxygenMeter.fillAmount = _survivalManager.OxygenPercent;
            _staminaMeter.fillAmount = _survivalManager.StaminaPercent;
        }
    }
}
    