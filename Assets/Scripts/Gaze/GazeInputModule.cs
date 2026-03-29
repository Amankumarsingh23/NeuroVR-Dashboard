using UnityEngine;

public class GazeInputModule : MonoBehaviour
{
    public static GazeInputModule Instance { get; private set; }

    [SerializeField] private float _dwellTime    = 2.0f;
    [SerializeField] private float _gazeNoise    = 0.015f;

    public Vector3  GazeDirection { get; private set; }
    public Vector3  GazeOrigin    { get; private set; }
    public float    PupilDiameter { get; private set; }
    public bool     IsBlinking    { get; private set; }

    private float _blinkTimer;
    private float _blinkInterval = 4.5f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        _blinkTimer += Time.deltaTime;
        if (_blinkTimer >= _blinkInterval)
        {
            IsBlinking  = true;
            _blinkTimer = 0f;
            Invoke(nameof(EndBlink), 0.15f);
        }

        Vector3 noise = new Vector3(
            Random.Range(-_gazeNoise, _gazeNoise),
            Random.Range(-_gazeNoise, _gazeNoise), 0f);

        GazeDirection = (Camera.main.transform.forward + noise).normalized;
        GazeOrigin    = Camera.main.transform.position;
        PupilDiameter = 0.65f + Mathf.Sin(Time.time * 0.3f) * 0.1f;
    }

    private void EndBlink() => IsBlinking = false;
}