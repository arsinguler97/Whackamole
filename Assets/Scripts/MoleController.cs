using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class MoleController : MonoBehaviour
{
    [Header("Hole References (UI)")]
    [SerializeField] private RectTransform leftHole;
    [SerializeField] private RectTransform belowLeftHole;
    [SerializeField] private RectTransform middleHole;
    [SerializeField] private RectTransform belowRightHole;
    [SerializeField] private RectTransform rightHole;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite[] upSprites;
    [SerializeField] private Sprite moveLeftSprite;
    [SerializeField] private Sprite moveRightSprite;
    [SerializeField] private Sprite[] deathSprites;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float upAnimSpeed = 0.2f;
    [SerializeField] private float moveAnimDuration = 0.2f;
    [SerializeField] private float dangerTime = 2f;   // how long you can stay up before exclamation
    [SerializeField] private float hitDelay = 0.5f;   // time after exclamation before hit

    [Header("UI References")]
    [SerializeField] private GameObject exclamationMark; // The warning sign image

    private Image _img;
    private RectTransform _rt;
    private RectTransform[] _holes;
    private int _currentIndex = 2;
    private bool _isRising = false;
    private float _defaultY;
    private Coroutine _upAnimCoroutine;
    private Coroutine _moveAnimCoroutine;
    private Coroutine _warningCoroutine;

    void Awake()
    {
        _img = GetComponent<Image>();
        _rt = GetComponent<RectTransform>();
        _holes = new RectTransform[] { leftHole, belowLeftHole, middleHole, belowRightHole, rightHole };
        if (exclamationMark != null) exclamationMark.SetActive(false);
    }

    void Start()
    {
        _defaultY = _rt.anchoredPosition.y;
        _rt.anchoredPosition = new Vector2(_holes[_currentIndex].anchoredPosition.x, _defaultY);
        _img.sprite = idleSprite;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDead) return;

        Vector2 targetPos = new Vector2(_holes[_currentIndex].anchoredPosition.x, _rt.anchoredPosition.y);
        _rt.anchoredPosition = Vector2.Lerp(_rt.anchoredPosition, targetPos, Time.deltaTime * moveSpeed);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDead) return;
        if (_isRising) return;
        if (!ctx.performed) return;

        float input = ctx.ReadValue<Vector2>().x;

        if (input < 0 && _currentIndex > 0)
        {
            _currentIndex--;
            PlayMoveAnimation(moveLeftSprite);
        }
        else if (input > 0 && _currentIndex < _holes.Length - 1)
        {
            _currentIndex++;
            PlayMoveAnimation(moveRightSprite);
        }
    }

    private void PlayMoveAnimation(Sprite moveSprite)
    {
        if (_moveAnimCoroutine != null) StopCoroutine(_moveAnimCoroutine);
        _moveAnimCoroutine = StartCoroutine(MoveAnimCoroutine(moveSprite));
    }

    private IEnumerator MoveAnimCoroutine(Sprite moveSprite)
    {
        _img.sprite = moveSprite;
        yield return new WaitForSeconds(moveAnimDuration);
        _img.sprite = idleSprite;
    }

    public void OnRise(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDead) return;

        if (ctx.started)
        {
            _isRising = true;
            float targetY = _holes[_currentIndex].anchoredPosition.y;
            _rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, targetY);

            if (_upAnimCoroutine != null) StopCoroutine(_upAnimCoroutine);
            _upAnimCoroutine = StartCoroutine(UpSpriteAnimation());

            if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
            _warningCoroutine = StartCoroutine(WarningTimer());
        }
        else if (ctx.canceled)
        {
            _isRising = false;
            _rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, _defaultY);

            if (_upAnimCoroutine != null) StopCoroutine(_upAnimCoroutine);
            if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
            if (exclamationMark != null) exclamationMark.SetActive(false);

            _img.sprite = idleSprite;
        }
    }

    private IEnumerator UpSpriteAnimation()
    {
        int frame = 0;
        while (_isRising)
        {
            _img.sprite = upSprites[frame];
            frame = (frame + 1) % upSprites.Length;
            yield return new WaitForSeconds(upAnimSpeed);
        }
    }

    private IEnumerator WarningTimer()
    {
        yield return new WaitForSecondsRealtime(dangerTime);

        // show exclamation mark
        if (exclamationMark != null && _isRising)
            exclamationMark.SetActive(true);

        // wait 0.5 more seconds before being hit
        yield return new WaitForSecondsRealtime(hitDelay);

        // if still up after that time, get hit
        if (_isRising)
        {
            CameraShake.Instance.Shake(0.25f, 0.3f);
            Die();
        }
    }

    public int GetCurrentHoleIndex() => _currentIndex;
    public bool IsUp() => _isRising;

    public void Die()
    {
        StopAllCoroutines();
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        _isRising = false;
        if (exclamationMark != null) exclamationMark.SetActive(false);

        for (int i = 0; i < deathSprites.Length; i++)
        {
            _img.sprite = deathSprites[i];
            yield return new WaitForSecondsRealtime(0.15f);
        }

        GameManager.Instance.PlayerDie();
    }
}
