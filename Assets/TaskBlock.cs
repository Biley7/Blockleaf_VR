using UnityEngine;
using TMPro;

public enum TaskPriority
{
    Backlog,
    Urgent,
    InProgress,
    Done
}

[RequireComponent(typeof(Rigidbody))]
public class TaskBlock : MonoBehaviour
{
    [Header("Task Details")]
    public string taskTitle = "Design VR Prototype";
    public TaskPriority currentPriority = TaskPriority.Backlog;

    [Header("Optional 3D Label")]
    public TextMeshPro labelText;

    private Rigidbody rb;
    private Renderer blockRenderer;
    private PriorityZone activeZone;
    private Vector3 snapTargetPosition;
    private bool shouldSnap = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        blockRenderer = GetComponent<Renderer>();
        UpdateVisuals();
    }

    void Update()
    {
        // Meta's Grabbable script sets isKinematic = true while grabbed.
        // Once released (isKinematic == false) inside a zone, smoothly settle into place.
        if (shouldSnap && !rb.isKinematic)
        {
            transform.position = Vector3.Lerp(transform.position, snapTargetPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, Time.deltaTime * 10f);
        }
    }

    public void AssignToZone(PriorityZone zone, Vector3 targetPos)
    {
        activeZone = zone;
        currentPriority = zone.zonePriority;
        snapTargetPosition = targetPos;
        shouldSnap = true;
        UpdateVisuals();
    }

    public void ClearZone(PriorityZone zone)
    {
        if (activeZone == zone)
        {
            activeZone = null;
            shouldSnap = false;
        }
    }

    public void UpdateVisuals()
    {
        if (labelText != null)
        {
            labelText.text = currentPriority == TaskPriority.Done
                ? $"<s>{taskTitle}</s>"
                : taskTitle;
        }

        if (blockRenderer == null) return;

        Color targetColor = currentPriority switch
        {
            TaskPriority.Urgent => new Color(0.92f, 0.28f, 0.28f), // Coral Red
            TaskPriority.InProgress => new Color(0.95f, 0.65f, 0.15f), // Amber Gold
            TaskPriority.Done => new Color(0.20f, 0.78f, 0.45f), // Emerald Green
            _ => new Color(0.25f, 0.52f, 0.96f)  // Default Blue
        };

        Material mat = blockRenderer.material;
        mat.color = targetColor;
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", targetColor);
        }
    }
}
