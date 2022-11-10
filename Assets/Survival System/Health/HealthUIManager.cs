using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    [SerializeField] private HealthManager _healthManager;
    [SerializeField] private Image _healthMeter;

    private void FixedUpdate()
    {
        _healthMeter.fillAmount = _healthManager.HealthPercent;
    }
}
