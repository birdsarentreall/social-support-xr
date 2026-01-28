using UnityEngine;

public class TimingAttackMeter : MonoBehaviour
{
    public RectTransform marker;

    public float range = 250f;          // px
    public float cyclesPerSecond = 1.2f; // how fast it oscillates
    public float perfectRadius = 15f;
    public float goodRadius = 45f;

    public bool running;
    float t0;

    public System.Action<float, string> OnResult;

    public void StartMeter()
    {
        running = true;
        t0 = Time.time;
    }

    void Update()
    {
        if (!running) return;

        float t = (Time.time - t0) * cyclesPerSecond * Mathf.PI * 2f;
        float x = Mathf.Sin(t) * range;

        var p = marker.anchoredPosition;
        p.x = x;
        marker.anchoredPosition = p;
    }

    public void Confirm()
    {
        if (!running) return;
        running = false;

        float dist = Mathf.Abs(marker.anchoredPosition.x);

        string grade;
        float mult;

        if (dist <= perfectRadius) { grade = "PERFECT"; mult = 1.5f; }
        else if (dist <= goodRadius) { grade = "GOOD"; mult = 1.0f; }
        else { grade = "MISS"; mult = 0.5f; }

        OnResult?.Invoke(mult, grade);
    }
}
