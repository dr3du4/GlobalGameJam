using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UI component for displaying the synchronized game timer.
/// Both players see the same timer value.
/// </summary>
public class NetworkTimerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerFillImage;
    [SerializeField] private Image timerBackground;

    [Header("Timer Formatting")]
    [SerializeField] private string timerFormat = "mm\\:ss";
    [SerializeField] private bool showMilliseconds = false;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private float warningThreshold = 60f; // 1 minute
    [SerializeField] private float criticalThreshold = 30f; // 30 seconds

    [Header("Animation")]
    [SerializeField] private bool pulseOnCritical = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMinScale = 0.9f;
    [SerializeField] private float pulseMaxScale = 1.1f;

    private float totalDuration;
    private bool isPulsing = false;
    private Vector3 originalScale;

    private void Start()
    {
        if (timerText != null)
        {
            originalScale = timerText.transform.localScale;
        }

        // Subscribe to timer updates from NetworkGameManager
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.OnTimerUpdated.AddListener(UpdateTimer);
            NetworkGameManager.Instance.OnGameStarted.AddListener(OnGameStarted);
            NetworkGameManager.Instance.OnGameEnded.AddListener(OnGameEnded);
        }
        else
        {
            // If no NetworkGameManager, try to find it later
            InvokeRepeating(nameof(TryFindGameManager), 0.5f, 0.5f);
        }
    }

    private void TryFindGameManager()
    {
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.OnTimerUpdated.AddListener(UpdateTimer);
            NetworkGameManager.Instance.OnGameStarted.AddListener(OnGameStarted);
            NetworkGameManager.Instance.OnGameEnded.AddListener(OnGameEnded);
            CancelInvoke(nameof(TryFindGameManager));
        }
    }

    private void OnDestroy()
    {
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.OnTimerUpdated.RemoveListener(UpdateTimer);
            NetworkGameManager.Instance.OnGameStarted.RemoveListener(OnGameStarted);
            NetworkGameManager.Instance.OnGameEnded.RemoveListener(OnGameEnded);
        }
    }

    private void OnGameStarted()
    {
        // Store total duration for fill calculations
        totalDuration = NetworkGameManager.Instance.GetRemainingTime();
    }

    private void OnGameEnded()
    {
        // Stop pulsing
        isPulsing = false;
        
        if (timerText != null)
        {
            timerText.transform.localScale = originalScale;
            timerText.text = "00:00";
            timerText.color = criticalColor;
        }
    }

    public void UpdateTimer(float remainingTime)
    {
        UpdateTimerText(remainingTime);
        UpdateTimerFill(remainingTime);
        UpdateTimerColor(remainingTime);
        UpdatePulse(remainingTime);
    }

    private void UpdateTimerText(float remainingTime)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        if (showMilliseconds)
        {
            int milliseconds = Mathf.FloorToInt((remainingTime % 1f) * 100f);
            timerText.text = $"{minutes:00}:{seconds:00}.{milliseconds:00}";
        }
        else
        {
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void UpdateTimerFill(float remainingTime)
    {
        if (timerFillImage == null || totalDuration <= 0) return;

        float fillAmount = remainingTime / totalDuration;
        timerFillImage.fillAmount = fillAmount;
    }

    private void UpdateTimerColor(float remainingTime)
    {
        Color targetColor;

        if (remainingTime <= criticalThreshold)
        {
            targetColor = criticalColor;
        }
        else if (remainingTime <= warningThreshold)
        {
            targetColor = warningColor;
        }
        else
        {
            targetColor = normalColor;
        }

        if (timerText != null)
        {
            timerText.color = targetColor;
        }

        if (timerFillImage != null)
        {
            timerFillImage.color = targetColor;
        }
    }

    private void UpdatePulse(float remainingTime)
    {
        if (!pulseOnCritical || timerText == null) return;

        if (remainingTime <= criticalThreshold && remainingTime > 0)
        {
            isPulsing = true;
        }
        else
        {
            isPulsing = false;
            timerText.transform.localScale = originalScale;
        }
    }

    private void Update()
    {
        if (isPulsing && timerText != null)
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, pulse);
            timerText.transform.localScale = originalScale * scale;
        }
    }

    #region Public Methods

    /// <summary>
    /// Manually set timer display (useful for testing or offline mode)
    /// </summary>
    public void SetTimer(float seconds)
    {
        UpdateTimer(seconds);
    }

    /// <summary>
    /// Set total duration for fill bar calculations
    /// </summary>
    public void SetTotalDuration(float duration)
    {
        totalDuration = duration;
    }

    #endregion
}

