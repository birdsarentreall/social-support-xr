using UnityEngine;

public class GlanceTracker : MonoBehaviour
{
    public Transform head;
    public float cooldownSeconds = 0.75f;

    [Header("Debug")]
    public bool debugDrawRay = true;
    public float rayDistance = 20f;
    public LayerMask hitMask = ~0; // everything by default
    public bool debugLogs = false;

    public int lookCount { get; private set; }

    float lastLookTime = -999f;
    Collider lastHit;

    void Start()
    {
        if (head == null && Camera.main != null)
            head = Camera.main.transform;
    }

    public void ResetCount()
    {
        lookCount = 0;
        lastLookTime = -999f;
        lastHit = null;
    }

    void Update()
    {
        if (head == null) return;

        if (debugDrawRay)
            Debug.DrawRay(head.position, head.forward * rayDistance, Color.green);

        if (Physics.Raycast(head.position, head.forward, out RaycastHit hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (debugLogs && hit.collider != lastHit)
            {
                Debug.Log($"[Glance] Hit {hit.collider.name} tag={hit.collider.tag} dist={hit.distance:F2}");
                lastHit = hit.collider;
            }

            if (hit.collider.CompareTag("Spectator"))
            {
                if (Time.time - lastLookTime > cooldownSeconds)
                {
                    lookCount++;
                    lastLookTime = Time.time;

                    if (debugLogs)
                        Debug.Log($"[Glance] LOOK++ => {lookCount} (t={Time.time:F2})");
                }
            }
        }
        else
        {
            if (debugLogs && lastHit != null)
            {
                Debug.Log("[Glance] Hit nothing");
                lastHit = null;
            }
        }
    }
}