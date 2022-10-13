using UnityEngine;

public class Demo_Log_Interaction : Demo_Interactable
{
    public override string InteractionMessage
    {
        get
        {
            return "Press <b>F</b> To Take.";
        }
    }

    public override void Show(Vector3 point)
    {
        Demo_UI_Tooltip.Instance.Show(point, InteractionMessage);
    }

    public override void Hide()
    {
        Demo_UI_Tooltip.Instance.Hide();
    }

    public override void Interaction()
    {
        PickableController.Instance.Pick(gameObject);
    }
}
