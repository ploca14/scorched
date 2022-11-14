using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI
{
    //This script will automatically be added to player targets. You can customize the DamagePlayerStandard function
    //or create your own. Ensure that it will be called within the SendPlayerDamage function. This allows users to customize
    //how player damage is received and applied without having to modify any main system scripts. The EmeraldComponent can
    //be used for added functionality such as only allowing blocking if the received AI is using the Melee Weapon Type.
    public class EmeraldAIPlayerDamage : MonoBehaviour
    {
        public List<string> ActiveEffects = new List<string>();
        public bool IsDead;


        public void SendPlayerDamage(int DamageAmount, Transform Target, EmeraldAISystem EmeraldComponent, bool CriticalHit = false)
        {
            //The standard damage function that sends damage to the Emerald AI demo player
            DamagePlayerCustom(DamageAmount);

            //Creates damage text on the player's position, if enabled.
            CombatTextSystem.Instance.CreateCombatText(DamageAmount, transform.position, CriticalHit, false, true);
        }

        void DamagePlayerCustom(int DamageAmount)
        {
            if (GetComponent<HealthManager>() != null)
            {
                HealthManager PlayerHealth = GetComponent<HealthManager>();
                PlayerHealth.DamagePlayer(DamageAmount);

                if (PlayerHealth.CurrentHealth <= 0)
                {
                    IsDead = true;
                }
            }
        }
    }
}
