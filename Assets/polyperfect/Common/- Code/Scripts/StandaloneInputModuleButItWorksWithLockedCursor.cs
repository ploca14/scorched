using UnityEngine;
using UnityEngine.EventSystems;

public class StandaloneInputModuleButItWorksWithLockedCursor : StandaloneInputModule
{
    protected override MouseState GetMousePointerEventData(int id)
    {
        var created = GetPointerData(kMouseLeftId, out var leftData, true);

        leftData.Reset();

        if (created)
            leftData.position = input.mousePosition;

        var pos = input.mousePosition;
        leftData.delta = pos - leftData.position;
        leftData.position = pos;
        leftData.scrollDelta = input.mouseScrollDelta;
        leftData.button = PointerEventData.InputButton.Left;
        eventSystem.RaycastAll(leftData, m_RaycastResultCache);
        var raycast = FindFirstRaycast(m_RaycastResultCache);
        leftData.pointerCurrentRaycast = raycast;
        m_RaycastResultCache.Clear();

        GetPointerData(kMouseRightId, out var rightData, true);
        CopyFromTo(leftData, rightData);
        rightData.button = PointerEventData.InputButton.Right;

        GetPointerData(kMouseMiddleId, out var middleData, true);
        CopyFromTo(leftData, middleData);
        middleData.button = PointerEventData.InputButton.Middle;
        var mouseState = new MouseState();
        mouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), leftData);
        mouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
        mouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);

        return mouseState;
    }

    protected override void ProcessMove(PointerEventData pointerEvent)
    {
        HandlePointerExitAndEnter(pointerEvent, pointerEvent.pointerCurrentRaycast.gameObject);
    }

    protected override void ProcessDrag(PointerEventData pointerEvent)
    {
        if (pointerEvent.pointerDrag == null)
            return;

        if (!pointerEvent.dragging && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
        {
            ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
            pointerEvent.dragging = true;
        }

        if (pointerEvent.dragging)
        {
            if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
            {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;
            }

            ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
        }
    }

    static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
    {
        if (!useDragThreshold)
            return true;

        return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
    }
}