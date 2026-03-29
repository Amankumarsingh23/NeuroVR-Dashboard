using UnityEngine;
using TMPro;

public class SceneBuilder : MonoBehaviour
{
    private void Start()
    {
        BuildDashboardPanel();
        BuildDwellButtons();
        Debug.Log("[SceneBuilder] NeuroVR Dashboard scene built successfully.");
    }

    private void BuildDashboardPanel()
    {
        // Main panel
        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "DashboardPanel";
        panel.transform.position = new Vector3(0, 1.6f, 3f);
        panel.transform.localScale = new Vector3(2.0f, 1.2f, 0.02f);
        panel.GetComponent<Renderer>().material.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);

        // Player ID text
        CreateTMPText("PlayerIDText", panel.transform,
            new Vector3(-0.35f, 0.25f, -1f), 6f, "Player\n---");

        // Fatigue score — big number
        CreateTMPText("FatigueScore", panel.transform,
            new Vector3(0.1f, 0.0f, -1f), 14f, "0.00");

        // Status text
        CreateTMPText("StatusText", panel.transform,
            new Vector3(0f, -0.25f, -1f), 5f, "STATUS: CONNECTING...");

        // Pupil text
        CreateTMPText("PupilText", panel.transform,
            new Vector3(-0.35f, -0.1f, -1f), 5f, "Pupil\n0.000");

        // Fatigue bar background
        GameObject barBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barBg.name = "FatigueBarBg";
        barBg.transform.SetParent(panel.transform);
        barBg.transform.localPosition = new Vector3(0f, 0.35f, -1f);
        barBg.transform.localScale    = new Vector3(0.9f, 0.08f, 0.5f);
        barBg.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);

        // Fatigue bar fill
        GameObject barFill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barFill.name = "FatigueBarFill";
        barFill.transform.SetParent(panel.transform);
        barFill.transform.localPosition = new Vector3(-0.45f, 0.35f, -1.1f);
        barFill.transform.localScale    = new Vector3(0.01f, 0.07f, 0.6f);
        barFill.GetComponent<Renderer>().material.color = new Color(0f, 0.8f, 0.4f);

        // Wire up FatigueDashboard component
        FatigueDashboard dash = panel.AddComponent<FatigueDashboard>();
    }

    private void BuildDwellButtons()
    {
        string[] labels   = { "RESET", "PROFILE", "HISTORY" };
        Vector3[] positions = {
            new Vector3(-0.8f, 1.0f, 3f),
            new Vector3( 0.0f, 1.0f, 3f),
            new Vector3( 0.8f, 1.0f, 3f)
        };

        for (int i = 0; i < labels.Length; i++)
        {
            GameObject btn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            btn.name = $"DwellButton_{labels[i]}";
            btn.transform.position   = positions[i];
            btn.transform.localScale = new Vector3(0.4f, 0.15f, 0.02f);
            btn.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.3f, 0.9f);

            DwellButton dwell = btn.AddComponent<DwellButton>();
            string label = labels[i];
            dwell.OnDwellComplete.AddListener(() =>
                Debug.Log($"[DwellButton] '{label}' activated by gaze!"));

            CreateTMPText($"Label_{labels[i]}", btn.transform,
                new Vector3(0f, 0f, -1.5f), 5f, labels[i]);
        }
    }

    private GameObject CreateTMPText(string name, Transform parent,
        Vector3 localPos, float size, string text)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale    = Vector3.one * 0.1f;

        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(4f, 2f);
        return go;
    }
}