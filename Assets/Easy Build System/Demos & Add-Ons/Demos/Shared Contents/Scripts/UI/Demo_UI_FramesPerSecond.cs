using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Demo_UI_FramesPerSecond : MonoBehaviour
{
    #region Private Fields

    private const float MeasurePeriod = 0.5f;
    private int Accumulator = 0;
    private float NextPeriod = 0;
    private int CurrentFps;
    private const string DisplayFormat = "{0} FPS";
    private Text Text;

    #endregion

    #region Private Methods

    private void Awake()
    {
        Text = GetComponent<Text>();
    }

    private void Start()
    {
        NextPeriod = Time.realtimeSinceStartup + MeasurePeriod;
    }

    private void Update()
    {
        Accumulator++;

        if (Time.realtimeSinceStartup > NextPeriod)
        {
            CurrentFps = (int)(Accumulator / MeasurePeriod);

            Accumulator = 0;

            NextPeriod += MeasurePeriod;

            if (CurrentFps > 60)
                Text.color = Color.green;
            else if (CurrentFps < 60)
                Text.color = Color.yellow;
            else if (CurrentFps < 30)
                Text.color = Color.red;

            Text.text = "FPS : " + string.Format(DisplayFormat, CurrentFps);
        }
    }

    #endregion
}