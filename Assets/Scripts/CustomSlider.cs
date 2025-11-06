using UnityEngine;
using UnityEngine.UI;

public class CustomSlider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fill;
    [SerializeField] private Image handle;

    [Header("Handle Sprites")]
    [SerializeField] private Sprite pokerSprite;
    [SerializeField] private Sprite angrySprite;
    [SerializeField] private Sprite madSprite;

    [Header("Values")]
    [Range(0f, 1f)] public float value;
    [SerializeField] private float handleMinY = -150f;
    [SerializeField] private float handleMaxY = 150f;
    [SerializeField] private float fillBaseY = -160f;
    [SerializeField] private float maxFillHeight = 160f;

    private RectTransform _fillRect;
    private RectTransform _handleRect;

    void Awake()
    {
        _fillRect = fill.GetComponent<RectTransform>();
        _handleRect = handle.GetComponent<RectTransform>();

        _fillRect.pivot = new Vector2(0.5f, 0f);
        _fillRect.anchoredPosition = new Vector2(_fillRect.anchoredPosition.x, fillBaseY);
        _fillRect.sizeDelta = new Vector2(_fillRect.sizeDelta.x, 0);
    }

    void Update()
    {
        UpdateBar(value);
    }

    public void SetValue(float v)
    {
        value = Mathf.Clamp01(v);
        UpdateBar(value);
    }

    private void UpdateBar(float v)
    {
        float newHeight = Mathf.Lerp(0f, maxFillHeight, v);
        _fillRect.sizeDelta = new Vector2(_fillRect.sizeDelta.x, newHeight);
        _fillRect.anchoredPosition = new Vector2(_fillRect.anchoredPosition.x, fillBaseY);

        float newY = Mathf.Lerp(handleMinY, handleMaxY, v);
        _handleRect.anchoredPosition = new Vector2(_handleRect.anchoredPosition.x, newY);

        if (v < 0.33f)
            handle.sprite = pokerSprite;
        else if (v < 0.66f)
            handle.sprite = angrySprite;
        else
            handle.sprite = madSprite;
    }
}