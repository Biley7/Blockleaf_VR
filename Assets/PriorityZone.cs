using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PriorityZone : MonoBehaviour
{
    public TaskPriority zonePriority = TaskPriority.Urgent;
    public Vector3 dropOffset = new Vector3(0f, 0.08f, 0f);

    private void Reset()
    {
        // Ensure the zone acts as a trigger volume so blocks don't bounce off
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TaskBlock task = other.GetComponentInParent<TaskBlock>();
        if (task != null)
        {
            task.AssignToZone(this, transform.position + dropOffset);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        TaskBlock task = other.GetComponentInParent<TaskBlock>();
        if (task != null)
        {
            task.ClearZone(this);
        }
    }
}

