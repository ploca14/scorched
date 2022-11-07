using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Example;

#if SURVIVAL_TEMPLATE_PRO
using SurvivalTemplatePro;
#endif

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
            DamagePlayerStandard(DamageAmount);

            //Creates damage text on the player's position, if enabled.
            CombatTextSystem.Instance.CreateCombatText(DamageAmount, transform.position, CriticalHit, false, true);

            //Sends damage to another function that will then send the damage to the RFPS player.
            //If you are using RFPS, you can uncomment this to allow Emerald Agents to damage your RFPS player.
            //DamageRFPS(DamageAmount, Target);

            //Sends damage to another function that will then send the damage to the RFPS player.
            //If you are using RFPS, you can uncomment this to allow Emerald Agents to damage your RFPS player.
            //DamageInvectorPlayer(DamageAmount, Target);

            //Damage UFPS Player
            //DamageUFPSPlayer(DamageAmount);

#if SURVIVAL_TEMPLATE_PRO
            //Damage STP Player
            DamageSTPPlayer(DamageAmount, Target);
#endif
        }

        void DamagePlayerStandard(int DamageAmount)
        {
            if (GetComponent<EmeraldAIPlayerHealth>() != null)
            {
                EmeraldAIPlayerHealth PlayerHealth = GetComponent<EmeraldAIPlayerHealth>();
                PlayerHealth.DamagePlayer(DamageAmount);

                if (PlayerHealth.CurrentHealth <= 0)
                {
                    IsDead = true;
                }
            }
        }

        #region Survival Template PRO
#if SURVIVAL_TEMPLATE_PRO
        [Header("Survival Template PRO")]

        [SerializeField, Range(0.01f, 10f)]
        private float m_DamageMod = 1f;

        private ICharacter m_Player;


        void DamageSTPPlayer(int DamageAmount, Transform Target)
        {
            if (m_Player == null)
                m_Player = GetComponentInParent<ICharacter>();

            if (m_Player != null)
            {
                var dmgInfo = new DamageInfo(DamageAmount * m_DamageMod, DamageType.Hit, Target.position, Target.position - transform.position, 1f);
                m_Player.HealthManager.ReceiveDamage(dmgInfo);

                IsDead = !m_Player.HealthManager.IsAlive;
            }
        }
#endif
        #endregion

        /*
        void DamageRFPS(int DamageAmount, Transform Target)
        {
            if (GetComponent<FPSPlayer>() != null)
            {
                GetComponent<FPSPlayer>().ApplyDamage((float)DamageAmount, Target, true);
            }
        }
        */

        /*
        void DamageInvectorPlayer (int DamageAmount, Transform Target)
        {
            if (GetComponent<Invector.vCharacterController.vCharacter>())
            {
                var PlayerInput = GetComponent<Invector.vCharacterController.vMeleeCombatInput>();

                if (!PlayerInput.blockInput.GetButton())
                {
                    var _Damage = new Invector.vDamage(DamageAmount);
                    _Damage.sender = Target;
                    _Damage.hitPosition = Target.position;
                    GetComponent<Invector.vCharacterController.vCharacter>().TakeDamage(_Damage);
                }
            }
        }
        */

        /*
        void DamageUFPSPlayer(int DamageAmount)
        {
            if (GetComponent<vp_FPPlayerDamageHandler>())
            {
                GetComponent<vp_FPPlayerDamageHandler>().Damage((float)DamageAmount);
            }
        }
        */
    }
}
