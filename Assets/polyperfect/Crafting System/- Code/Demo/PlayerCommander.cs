using Polyperfect.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Polyperfect.Crafting.Demo
{
    public class PlayerCommander:PolyMono
    {
        public override string __Usage => "Sends commands to the player. Typically used via UnityEvents, like those on buttons or Event Triggers. Given events are expected to be PointerEventData.";
        CommandablePlayer player;

        protected void Start()
        {
            player = FindObjectOfType<CommandablePlayer>();
            if (player==null)
            {
                Debug.LogError($"There is no player with an attached {nameof(CommandablePlayer)} in the scene, which is necessary for {nameof(PlayerCommander)} scripts.");
                enabled = false;
            }
        }

        public void MoveTo(BaseEventData arg) => MoveTo(((PointerEventData)arg).pointerCurrentRaycast.worldPosition);
        public void MoveTo(Vector3 position)=>player.IssueCommand(new MoveCommand(position));
        /*public void MoveToOrPlaceAt(BaseEventData data)
        {
            if (data is PointerEventData pointerEventData)
            {
                if (pointerEventData.button == PointerEventData.InputButton.Left)
                    MoveTo(data);
                else if (pointerEventData.button == PointerEventData.InputButton.Right)
                    PlaceAt(data);
                else
                    Debug.LogError("Undefined button");
            }
            else
                Debug.LogError("event was not pointer data");
        }*/

        public void MoveToObject(Transform trans) => MoveTo(trans.position);

        public void InteractWith(BaseInteractable interactable)
        {
            player.IssueCommand(new InteractCommand(interactable));
        }

        public void StopInteracting() => player.IssueCommand(new StopCommand());//player.StopInteracting();

        //public void PlaceAt(BaseEventData arg) => player.IssueCommand(new PlaceCommand(((PointerEventData)arg).pointerCurrentRaycast.worldPosition));
    }
}