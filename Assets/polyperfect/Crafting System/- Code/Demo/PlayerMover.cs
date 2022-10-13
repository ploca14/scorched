using System;
using Polyperfect.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(CommandablePlayer))]
    public class PlayerMover : PolyMono
    {
        public override string __Usage => "Allows commanding the player to move to locations clicked.";

        CommandablePlayer player;

        void Start()
        {
            player = GetComponent<CommandablePlayer>();
            
        }

        void OnEnable()
        {
            InteractiveCursor.CursorUpdate += HandleUpdate;
        }

        void OnDisable()
        {
            InteractiveCursor.CursorUpdate -= HandleUpdate;
        }

        void HandleUpdate(RaycastResult hit)
        {
            if (!(hit.module is PhysicsRaycaster))
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (hit.gameObject && hit.gameObject.GetComponentInParent<BaseInteractable>()) 
                    return;
                player.IssueCommand(new MoveCommand(hit.worldPosition));
            }
        }
    }
}