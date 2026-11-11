using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows a "Purchased Successfully!" panel for 2 seconds then hides it.
/// Call PurchaseSuccessNotification.Instance.Show() from anywhere after a successful purchase.
/// </summary>
public class PurchaseSuccessNotification : MonoBehaviour
{
    public static PurchaseSuccessNotification Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Text messageText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private string message = "Purchased Successfully!";

    private Coroutine _hideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Make sure the panel is hidden on start
        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (messageText != null)
            messageText.text = message;
    }

    /// <summary>
    /// Call this after a successful purchase to show the notification.
    /// </summary>
    public void Show()
    {
        if (notificationPanel == null) return;

        // Cancel any existing hide routine so it doesn't cut short a new show
        if (_hideCoroutine != null)
            StopCoroutine(_hideCoroutine);

        notificationPanel.SetActive(true);
        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        notificationPanel.SetActive(false);
        _hideCoroutine = null;
    }
}
