using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebSocketClient : MonoBehaviour
{
    public static WebSocketClient Instance { get; private set; }

    [SerializeField] private string _httpUrl     = "http://localhost:8001";
    [SerializeField] private float  _pollInterval = 1.5f;

    public event Action<FatigueReading> OnFatigueUpdate;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(PollFatigue());
    }

    private IEnumerator PollFatigue()
    {
        while (true)
        {
            yield return new WaitForSeconds(_pollInterval);

            if (GazeInputModule.Instance == null) continue;

            string json = "{" +
                $"\"pupil_diameter\":{GazeInputModule.Instance.PupilDiameter.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                $"\"is_blinking\":{(GazeInputModule.Instance.IsBlinking ? "true" : "false")}," +
                $"\"timestamp_ms\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}" +
                "}";

            byte[] body = Encoding.UTF8.GetBytes(json);
            using var www = new UnityWebRequest($"{_httpUrl}/fatigue", "POST");
            www.uploadHandler   = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var resp = JsonUtility.FromJson<FatigueResponse>(www.downloadHandler.text);
                OnFatigueUpdate?.Invoke(new FatigueReading
                {
                    playerId      = resp.player_id,
                    fatigueScore  = resp.fatigue_score,
                    pupilDiameter = GazeInputModule.Instance.PupilDiameter,
                    timestampMs   = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
            }
            else
            {
                Debug.LogWarning($"[WS] Poll failed: {www.error}");
            }
        }
    }
}