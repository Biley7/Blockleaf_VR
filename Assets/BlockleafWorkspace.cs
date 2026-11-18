using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockleafWorkspace : MonoBehaviour
{
    public TaskBlock starterTaskBlock;

    private Transform workspaceRoot;
    private readonly List<TaskBlock> allTasks = new List<TaskBlock>();
    private Renderer focusButtonRenderer;
    private bool isFocusMode = false;

    IEnumerator Start()
    {
        // Wait 0.5s so OpenXR headset height initializes above the floor
        yield return new WaitForSeconds(0.5f);

        workspaceRoot = new GameObject("WorkspaceRoot").transform;
        PositionRootInFrontOfUser();

        BuildSeatedDesk();
        BuildPriorityTrays();
        BuildFocusButton();
        SetupTaskBlocks();
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleFocusMode();
        }

        if (OVRInput.GetDown(OVRInput.Button.Two) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            RecenterWorkspace();
        }
    }

    private void PositionRootInFrontOfUser()
    {
        Transform eye = Camera.main != null ? Camera.main.transform : null;
        if (eye == null)
        {
            OVRCameraRig rig = FindFirstObjectByType<OVRCameraRig>();
            if (rig != null) eye = rig.centerEyeAnchor;
        }

        if (eye != null && eye.position.sqrMagnitude > 0.01f)
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(eye.forward, Vector3.up).normalized;
            if (flatForward.sqrMagnitude < 0.001f) flatForward = Vector3.forward;

            workspaceRoot.position = eye.position + flatForward * 0.55f + Vector3.down * 0.25f;
            workspaceRoot.rotation = Quaternion.LookRotation(flatForward, Vector3.up);
        }
        else
        {
            workspaceRoot.position = new Vector3(0f, 1.0f, 0.55f);
            workspaceRoot.rotation = Quaternion.identity;
        }
    }

    public void RecenterWorkspace()
    {
        Vector3 oldPos = workspaceRoot.position;
        Quaternion oldRot = workspaceRoot.rotation;

        PositionRootInFrontOfUser();

        foreach (TaskBlock task in allTasks)
        {
            if (task == null) continue;
            Vector3 localOffset = Quaternion.Inverse(oldRot) * (task.transform.position - oldPos);
            task.transform.position = workspaceRoot.position + workspaceRoot.rotation * localOffset;
        }
    }

    private void BuildSeatedDesk()
    {
        GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "SeatedDesk";
        desk.transform.SetParent(workspaceRoot, false);
        desk.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        desk.transform.localScale = new Vector3(1.15f, 0.04f, 0.55f);
        ApplyColor(desk, new Color(0.14f, 0.15f, 0.18f));
    }

    private void BuildPriorityTrays()
    {
        CreateTray("Tray_Urgent", "URGENT", TaskPriority.Urgent,
            new Vector3(-0.18f, -0.02f, 0f), new Color(0.45f, 0.16f, 0.16f));
        CreateTray("Tray_InProgress", "IN PROGRESS", TaskPriority.InProgress,
            new Vector3(0.10f, -0.02f, 0f), new Color(0.45f, 0.32f, 0.12f));
        CreateTray("Tray_Done", "DONE", TaskPriority.Done,
            new Vector3(0.38f, -0.02f, 0f), new Color(0.14f, 0.38f, 0.24f));
    }

    private void CreateTray(string objName, string labelText, TaskPriority priority, Vector3 localPos, Color tint)
    {
        GameObject tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tray.name = objName;
        tray.transform.SetParent(workspaceRoot, false);
        tray.transform.localPosition = localPos;
        tray.transform.localScale = new Vector3(0.24f, 0.02f, 0.24f);
        ApplyColor(tray, tint);

        BoxCollider col = tray.GetComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1f, 8f, 1f);

        PriorityZone zone = tray.AddComponent<PriorityZone>();
        zone.zonePriority = priority;
        zone.dropOffset = new Vector3(0f, 0.07f, 0f);

        CreateWorldLabel(objName + "_Label", labelText, localPos + new Vector3(0f, 0.025f, -0.145f));
    }

    private void BuildFocusButton()
    {
        GameObject btn = GameObject.CreatePrimitive(PrimitiveType.Cube);
        btn.name = "FocusModeButton";
        btn.transform.SetParent(workspaceRoot, false);
        btn.transform.localPosition = new Vector3(-0.42f, -0.01f, -0.18f);
        btn.transform.localScale = new Vector3(0.18f, 0.035f, 0.08f);
        focusButtonRenderer = btn.GetComponent<Renderer>();
        ApplyColor(btn, new Color(0.22f, 0.25f, 0.30f));

        BoxCollider col = btn.GetComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1.2f, 3f, 1.2f);

        FocusTrigger trigger = btn.AddComponent<FocusTrigger>();
        trigger.workspace = this;

        CreateWorldLabel("FocusButton_Label", "FOCUS MODE", new Vector3(-0.42f, 0.025f, -0.18f));
    }

    private void SetupTaskBlocks()
    {
        if (starterTaskBlock == null) starterTaskBlock = FindFirstObjectByType<TaskBlock>();
        if (starterTaskBlock == null) return;

        starterTaskBlock.gameObject.name = "Task_1";
        starterTaskBlock.transform.position = workspaceRoot.TransformPoint(new Vector3(-0.42f, 0.05f, -0.05f));
        starterTaskBlock.transform.rotation = workspaceRoot.rotation;
        starterTaskBlock.taskTitle = "Build Quest APK";
        starterTaskBlock.currentPriority = TaskPriority.Backlog;
        starterTaskBlock.UpdateVisuals();
        allTasks.Add(starterTaskBlock);

        allTasks.Add(SpawnTaskClone("Task_2", "Record Demo Video", new Vector3(-0.42f, 0.05f, 0.12f)));
        allTasks.Add(SpawnTaskClone("Task_3", "Test VR Workspace", new Vector3(-0.42f, 0.17f, 0.03f)));
    }

    private TaskBlock SpawnTaskClone(string objName, string title, Vector3 localPos)
    {
        GameObject clone = Instantiate(
            starterTaskBlock.gameObject,
            workspaceRoot.TransformPoint(localPos),
            workspaceRoot.rotation
        );
        clone.name = objName;
        TaskBlock tb = clone.GetComponent<TaskBlock>();
        tb.taskTitle = title;
        tb.currentPriority = TaskPriority.Backlog;
        tb.UpdateVisuals();
        return tb;
    }

    public void ToggleFocusMode()
    {
        isFocusMode = !isFocusMode;
        if (focusButtonRenderer != null)
        {
            ApplyColor(focusButtonRenderer.gameObject,
                isFocusMode ? new Color(0.92f, 0.28f, 0.28f) : new Color(0.22f, 0.25f, 0.30f));
        }

        foreach (TaskBlock task in allTasks)
        {
            if (task == null) continue;
            task.gameObject.SetActive(!isFocusMode || task.currentPriority == TaskPriority.Urgent || task.currentPriority == TaskPriority.InProgress);
        }
    }

    private void CreateWorldLabel(string objName, string text, Vector3 localPos)
    {
        GameObject labelObj = new GameObject(objName);
        labelObj.transform.SetParent(workspaceRoot, false);
        labelObj.transform.localPosition = localPos;
        labelObj.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
        labelObj.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 8f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    private void ApplyColor(GameObject obj, Color color)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null) return;
        rend.material.color = color;
        if (rend.material.HasProperty("_BaseColor")) rend.material.SetColor("_BaseColor", color);
    }
}

public class FocusTrigger : MonoBehaviour
{
    public BlockleafWorkspace workspace;
    private float lastToggleTime = -1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<TaskBlock>() != null) return;
        if (Time.time - lastToggleTime < 0.8f) return;

        lastToggleTime = Time.time;
        if (workspace != null) workspace.ToggleFocusMode();
    }
}
