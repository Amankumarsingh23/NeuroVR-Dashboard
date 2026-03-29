using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class DwellButton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float       _dwellDuration = 2.0f;
    [SerializeField] private Color       _normalColor   = new Color(0.1f, 0.1f, 0.3f, 0.9f);
    [SerializeField] private Color       _hoverColor    = new Color(0.2f, 0.5f, 0.9f, 0.9f);
    [SerializeField] private Color       _fillColor     = new Color(0.0f, 0.8f, 0.6f, 1.0f);

    [Header("Events")]
    public UnityEvent OnDwellComplete;

    private Renderer     _renderer;
    private GameObject   _fillBar;
    private float        _dwellProgress;
    private bool         _isGazing;
    private bool         _activated;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.material.color = _normalColor;

        // Create fill bar child
        _fillBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _fillBar.transform.SetParent(transform);
        _fillBar.transform.localPosition = new Vector3(0, -0.55f, -0.01f);
        _fillBar.transform.localScale    = new Vector3(0f, 0.05f, 0.02f);
        _fillBar.GetComponent<Renderer>().material.color = _fillColor;
        Destroy(_fillBar.GetComponent<Collider>());
    }

    private void Update()
    {
        if (_activated) return;

        Ray ray = new Ray(GazeInputModule.Instance.GazeOrigin,
                          GazeInputModule.Instance.GazeDirection);

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
        {
            _isGazing = true;
            _renderer.material.color = _hoverColor;
            _dwellProgress += Time.deltaTime / _dwellDuration;
            _dwellProgress  = Mathf.Clamp01(_dwellProgress);

            _fillBar.transform.localScale = new Vector3(
                _dwellProgress, 0.05f, 0.02f);

            if (_dwellProgress >= 1.0f)
            {
                _activated = true;
                OnDwellComplete?.Invoke();
                _renderer.material.color = _fillColor;
            }
        }
        else
        {
            _isGazing      = false;
            _dwellProgress = Mathf.Max(0f, _dwellProgress - Time.deltaTime);
            _renderer.material.color = _normalColor;
            _fillBar.transform.localScale = new Vector3(_dwellProgress, 0.05f, 0.02f);
        }
    }

    public void Reset()
    {
        _activated     = false;
        _dwellProgress = 0f;
        _renderer.material.color = _normalColor;
        _fillBar.transform.localScale = new Vector3(0f, 0.05f, 0.02f);
    }
}