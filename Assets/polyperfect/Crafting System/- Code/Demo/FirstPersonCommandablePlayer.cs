using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    public class FirstPersonCommandablePlayer : CommandablePlayer
    {
        public override string __Usage =>
            $"A simple {nameof(CommandablePlayer)} that can Interact with and Place objects. Also handles locking and unlocking the mouse cursor.";

        public KeyCode MouseLockButton = KeyCode.E;
        public bool LockCursor = true;

        new void Start()
        {
            base.Start();
            OnStartInteract.AddListener(e => LockCursor = false);
            OnStopInteract.AddListener(() => LockCursor = true);
        }
        new void Update()
        {
            base.Update();
            Cursor.lockState = LockCursor ? CursorLockMode.Locked : CursorLockMode.None;

            if (Input.GetKeyDown(MouseLockButton))
                ToggleLock();
        }

        void ToggleLock()
        {
            LockCursor = !LockCursor;
        }

        public override void IssueCommand(ICommand command)
        {
            switch (command)
            {
                case InteractCommand interactCommand:
                    InteractWith(interactCommand.Interactable);
                    command.Complete();
                    break;
                case PlaceCommand placeCommand:
                    var placed = ExecutePlace(placeCommand.Info);
                    if (placed)
                        placeCommand.Complete();
                    else
                        placeCommand.Cancel();
                    break;
                case StopCommand stopCommand :
                    StopInteracting();
                    stopCommand.Complete();
                    break;
                default:
                    command.Cancel();
                    break;
            }
        }
    }
}