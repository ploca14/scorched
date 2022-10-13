using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using System.Linq;
using UnityEngine;

public class Demo_Upgradable_Interaction : Demo_Interactable
{
    public override string InteractionMessage
    {
        get
        {
            return string.Format("Press <b>F</b> To Place.\nProgression : {0}/{1}",
                GetComponent<AddonTheForestLike>().GetCurrentProgression(), GetComponent<AddonTheForestLike>().Elements.Length);
        }
    }

    public override void Show(Vector3 point)
    {
        if (GetComponent<AddonTheForestLike>() == null)
            return;

        if (GetComponent<AddonTheForestLike>().IsCompleted())
            return;

        Demo_UI_Tooltip.Instance.Show(point, InteractionMessage);
    }

    public override void Hide()
    {
        Demo_UI_Tooltip.Instance.Hide();
    }

    public override void Interaction()
    {
        for (int i = PickableController.Instance.Slots.Count-1; i >= 0; i--)
        {
            if (PickableController.Instance.Slots[i].IsAvailable)
            {
                if (!GetComponent<AddonTheForestLike>().IsCompleted())
                {
                    if (GetComponent<PieceBehaviour>().CurrentState != StateType.Queue)
                        return;

                    for (int m = 0; m < PickableController.Instance.TempElements.Count; m++)
                        if (GetComponent<AddonTheForestLike>().Elements.FirstOrDefault(x => !x.gameObject.activeSelf && x.tag == PickableController.Instance.TempElements[m]) == null)
                            return;

                    PickableController.Instance.Slots[i].IsAvailable = false;

                    PickableController.Instance.Slots[i].Hide();

                    GetComponent<AddonTheForestLike>().Upgrade(PickableController.Instance.TempElements[0]);

                    break;
                }
            }
        }
    }
}
