using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FatigueDashboard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshPro _playerIdText;
    [SerializeField] private TextMeshPro _fatigueScoreText;
    [SerializeField] private TextMeshPro _statusText;
    [SerializeField] private TextMeshPro _pupilText;
    [SerializeField] private Renderer    _fatigueBarRenderer;
    [SerializeField] private Renderer    _panelRenderer;

    [Header("Colors")]
    [SerializeField] private Color _freshColor   = new Color(0.0f, 0.8f, 0.4f);
    [SerializeField] private Color _mediumColor  = new Color(1.0f, 0.7f, 0.0f);
    [SerializeField] private Color _fatiguedColor= new Color(0.9f, 0.2f, 0.1f);

    private readonly List<float> _history = new List<float>(60);
    private float _currentScore;
    private float _displayScore;

    private void OnEnable()
    {
        if (WebSocketClient.Instance != null)
            WebSocketClient.Instance.OnFatigueUpdate += UpdateDashboard;
    }

    private void OnDisable()
    {
        if (WebSocketClient.Instance != null)
            WebSocketClient.Instance.OnFatigueUpdate -= UpdateDashboard;
    }

    private void Update()
    {
        // Smooth score display
        _displayScore = Mathf.Lerp(_displayScore, _currentScore, Time.deltaTime * 3f);
        UpdateVisuals();
    }

    private void UpdateDashboard(FatigueReading reading)
    {
        _currentScore = reading.fatigueScore;
        _history.Add(reading.fatigueScore);
        if (_history.Count > 60) _history.RemoveAt(0);

        if (_playerIdText    != null) _playerIdText.text    = $"Player\n{reading.playerId}";
        if (_pupilText       != null) _pupilText.text       = $"Pupil\n{reading.pupilDiameter:F3}";
        if (_statusText      != null) _statusText.text      = GetStatusText(_currentScore);
    }

    private void UpdateVisuals()
    {
        if (_fatigueScoreText != null)
            _fatigueScoreText.text = $"{_displayScore:F2}";

        Color c = GetFatigueColor(_displayScore);

        if (_fatigueBarRenderer != null)
        {
            _fatigueBarRenderer.material.color = c;
            _fatigueBarRenderer.transform.localScale = new Vector3(
                _displayScore, 1f, 1f);
        }

        if (_panelRenderer != null)
        {
            Color panelCol = Color.Lerp(
                new Color(0.05f, 0.05f, 0.15f, 0.95f), c, _displayScore * 0.2f);
            _panelRenderer.material.color = panelCol;
        }
    }

    private Color GetFatigueColor(float score)
    {
        if (score < 0.4f) return Color.Lerp(_freshColor,  _mediumColor,  score / 0.4f);
        return                  Color.Lerp(_mediumColor, _fatiguedColor, (score - 0.4f) / 0.6f);
    }

    private string GetStatusText(float score)
    {
        if (score < 0.3f) return "STATUS: FOCUSED";
        if (score < 0.6f) return "STATUS: MODERATE";
        return                   "STATUS: FATIGUED";
    }
}