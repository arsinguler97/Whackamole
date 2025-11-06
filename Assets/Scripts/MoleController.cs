using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class MoleController : MonoBehaviour
{
    [SerializeField] private RectTransform leftHole;
    [SerializeField] private RectTransform belowLeftHole;
    [SerializeField] private RectTransform middleHole;
    [SerializeField] private RectTransform belowRightHole;
    [SerializeField] private RectTransform rightHole;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite[] upSprites;
    [SerializeField] private Sprite moveLeftSprite;
    [SerializeField] private Sprite moveRightSprite;
    [SerializeField] private Sprite[] deathSprites;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float upAnimSpeed = 0.2f;
    [SerializeField] private float moveAnimDuration = 0.2f;
    [SerializeField] private float dangerTime = 2f;
    [SerializeField] private float hitDelay = 0.5f;
    [SerializeField] private GameObject exclamationMark;

    private Image _img;
    private RectTransform _rt;
    private RectTransform[] _holes;
    private int _currentIndex = 2;
    private bool _isRising;
    private float _defaultY;
    private Coroutine _upAnimCoroutine;
    private Coroutine _moveAnimCoroutine;
    private Coroutine _warningCoroutine;
    private float _gruntTimer;

    void Awake()
    {
        _img = GetComponent<Image>();
        _rt = GetComponent<RectTransform>();
        _holes = new[] { leftHole, belowLeftHole, middleHole, belowRightHole, rightHole };
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
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        Vector2 targetPos = new Vector2(_holes[_currentIndex].anchoredPosition.x, _rt.anchoredPosition.y);
        _rt.anchoredPosition = Vector2.Lerp(_rt.anchoredPosition, targetPos, Time.deltaTime * moveSpeed);

        if (_isRising)
        {
            _gruntTimer += Time.deltaTime;
            if (_gruntTimer >= 5f)
            {
                _gruntTimer = 0f;
                AudioManager.Instance.PlayRandom("grunt1", "grunt2", "grunt3", "grunt4", "grunt5", "grunt6", "grunt7");
            }
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
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
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        if (ctx.started)
        {
            _isRising = true;
            AudioManager.Instance.PlayLoop("raspberryLoop");
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
            AudioManager.Instance.StopLoop("raspberryLoop");
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
        while (_isRising && !GameManager.Instance.IsGameOver)
        {
            _img.sprite = upSprites[frame];
            frame = (frame + 1) % upSprites.Length;
            yield return new WaitForSeconds(upAnimSpeed);
        }
    }

    private IEnumerator WarningTimer()
    {
        yield return new WaitForSecondsRealtime(dangerTime);
        if (GameManager.Instance.IsGameOver) yield break;
        if (exclamationMark != null && _isRising)
            exclamationMark.SetActive(true);
        yield return new WaitForSecondsRealtime(hitDelay);
        if (_isRising && !GameManager.Instance.IsGameOver)
        {
            CameraShake.Instance.Shake(0.25f, 0.3f);
            Die();
        }
    }

    public int GetCurrentHoleIndex() => _currentIndex;
    public bool IsUp() => _isRising;

    public void Die()
    {
        if (GameManager.Instance.IsGameOver) return;
        StopAllCoroutines();
        AudioManager.Instance.Play("splat");
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        _isRising = false;
        AudioManager.Instance.StopLoop("raspberryLoop");
        if (exclamationMark != null) exclamationMark.SetActive(false);
        foreach (var t in deathSprites)
        {
            _img.sprite = t;
            yield return new WaitForSecondsRealtime(0.15f);
        }
        GameManager.Instance.PlayerDie();
    }

    public void HideForSqueeze()
    {
        _img.enabled = false;
    }

    public void ShowExclamation()
    {
        if (exclamationMark != null) exclamationMark.SetActive(true);
    }

    public void HideExclamation()
    {
        if (exclamationMark != null) exclamationMark.SetActive(false);
    }
}
