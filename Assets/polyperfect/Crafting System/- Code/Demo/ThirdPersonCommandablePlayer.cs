using System;
using System.Collections.Generic;
using System.Threading;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using Polyperfect.Crafting.Placement;
using UnityEngine;
using UnityEngine.AI;

namespace Polyperfect.Crafting.Demo
{
    [DefaultExecutionOrder(10)]
    [RequireComponent(typeof(NavMeshAgent))]
    public class ThirdPersonCommandablePlayer : CommandablePlayer
    {
        #region Fields

        // public enum ActionType
        // {
        //     None,Move,Place,Interact
        // }

        //ActionType currentAction;
        public override string __Usage => "Allows various commands such as Move, Gather, and others to be issued.";
        public BaseItemStackInventory TargetInventory;
        public float InteractDistance = 1f;
        public float WalkSpeed = 1.5f;
        public float RunSpeed = 5f;
        public float VerticalReach = 3f;

        ICommand currentCommand;
        NavMeshAgent _agent;
        #endregion
        protected override void Start()
        {
            base.Start();
            
            _agent = GetComponent<NavMeshAgent>();
            if (!TargetInventory)
            {
                Debug.LogError($"No target inventory for {nameof(CommandablePlayer)} on {gameObject.name}.");
                enabled = false;
                return;
            }
        }

        new void Update()
        {
            base.Update();
            switch (currentCommand)
            {
                case MoveCommand moveCommand:
                    if (IsInRangeOf(moveCommand.Position)) 
                        moveCommand.Complete();
                    break;
            }
            
        }


        // bool IsAbleToStartPlacing()
        // {
        //     return Vector3.Distance(placeInfo.Position, transform.position) < InteractDistance;
        // }
        //
        // bool IsAbleToStartInteracting() => interactTarget && Vector3.Distance(transform.position,interactTarget.transform.position)<InteractDistance;

        public void MoveTo(Vector3 item)
        {
            StopInteracting();
            SetDestination(item);
            //currentAction = ActionType.Move;
        }  

        Vector3 lastSetDest;
        void SetDestination(Vector3 item)
        {
            var shouldRun = Vector3.Distance(item, lastSetDest) < .5f;
            _agent.speed = shouldRun ? RunSpeed : WalkSpeed;
            lastSetDest = item;
            
            _agent.SetDestination(item);
            _agent.stoppingDistance = .5f;
        }
        
        /*public void PlaceAtPosition(Vector3 position)
        {
            StopInteracting();
            SetDestination(position);
            //currentAction = ActionType.Place;
            var placeOrientation = Quaternion.LookRotation(Vector3.ProjectOnPlane(position - transform.position, Vector3.up), Vector3.up);
            placeInfo = new PlacementInfo(position, placeOrientation);
        }
        public void PlaceWithInfo(PlacementInfo info,Action<GameObject> onComplete)
        {
            StopInteracting();
            SetDestination(info.Position);
            //currentAction = ActionType.Place;
            placeInfo = info;
            onPlaceComplete = onComplete;
        }*/

        public void StopMoving()
        {
            SetDestination(_agent.nextPosition);
            //currentAction = ActionType.None;
        }


        // void MoveToAndInteractWith(BaseInteractable target)
        // {
        //     SetDestination(target.transform.position);
        //     if (interactTarget == target)
        //         return;
        //     StopInteracting();   
        //     _agent.stoppingDistance = InteractDistance*.5f;
        //     interactTarget = target;
        //     //currentAction = ActionType.Interact;
        // }

        


        public override void IssueCommand(ICommand command)
        {
            command.OnDone += () =>
            {
                if (currentCommand == command) 
                    currentCommand = null;
            };
            switch (command)
            {
                case MoveCommand moveCommand:
                    currentCommand?.Cancel();
                    MoveTo(moveCommand.Position);
                    currentCommand = command;
                    break; 
                case InteractCommand interactCommand:
                    var targetPosition = interactCommand.Interactable.transform.position;
                    if (!interactCommand.Interactable.transform.IsChildOf(transform)&&!IsInRangeOf(targetPosition))
                    {
                        var interimMoveCommand = new MoveCommand(targetPosition);
                        interimMoveCommand.OnComplete += ()=>IssueCommand(interactCommand);
                        interimMoveCommand.OnCancel += ()=>interactCommand.Cancel();
                        IssueCommand(interimMoveCommand);
                    }
                    else
                    {
                        InteractWith(interactCommand.Interactable);
                        interactCommand.Complete();
                    }

                    break;
                case PlaceCommand placeCommand:
                    var placePosition = placeCommand.Info.Position;
                    if (!IsInRangeOf(placePosition))
                    {
                        var interimMoveCommand = new MoveCommand(placePosition);
                        interimMoveCommand.OnComplete += ()=>IssueCommand(placeCommand);
                        interimMoveCommand.OnCancel += ()=>placeCommand.Cancel();
                        IssueCommand(interimMoveCommand);
                    }
                    else
                    {
                        var placed = ExecutePlace(placeCommand.Info);
                        if (placed)
                            placeCommand.Complete();
                        else
                            placeCommand.Cancel();
                    }

                    break;
                case StopCommand _:
                    currentCommand?.Cancel();
                    StopInteracting();
                    StopMoving();
                    command.Complete();
                    break;
                default:
                    command.Cancel();
                    break;
            }
        }

        bool IsInRangeOf(Vector3 targetPosition)
        {
            var displacement = targetPosition - transform.position;
            var up = Vector3.up;
            var verticalDisplacement = Vector3.Project(displacement,up);
            var horizontalDisplacement = Vector3.ProjectOnPlane(displacement, up);
            if (Vector3.Dot(verticalDisplacement,up) > 0)
                verticalDisplacement = Vector3.MoveTowards(verticalDisplacement, Vector3.zero, VerticalReach);
            
            return (verticalDisplacement+horizontalDisplacement).magnitude < InteractDistance;
        }

        
    }
}