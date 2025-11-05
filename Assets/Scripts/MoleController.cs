using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MoleController : MonoBehaviour
{
    [Header("Hole References (UI)")]
    [SerializeField] private RectTransform leftHole;
    [SerializeField] private RectTransform belowLeftHole;
    [SerializeField] private RectTransform middleHole;
    [SerializeField] private RectTransform belowRightHole;
    [SerializeField] private RectTransform rightHole;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;         // Default sprite when idle
    [SerializeField] private Sprite[] upSprites;        // Two-frame animation when mole is up
    [SerializeField] private Sprite moveLeftSprite;     // One-frame animation when moving left
    [SerializeField] private Sprite moveRightSprite;    // One-frame animation when moving right

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 8f;      // Horizontal movement lerp speed
    [SerializeField] private float upAnimSpeed = 0.2f;  // Fake up animation swap speed
    [SerializeField] private float moveAnimDuration = 0.2f; // Duration to stay in move sprite before returning to idle

    private Image _img;
    private RectTransform _rt;
    private RectTransform[] _holes;
    private int _currentIndex = 2;
    private bool _isRising = false;
    private float _defaultY;
    private Coroutine _upAnimCoroutine;
    private Coroutine _moveAnimCoroutine;

    void Awake()
    {
        _img = GetComponent<Image>();
        _rt = GetComponent<RectTransform>();
        _holes = new RectTransform[] { leftHole, belowLeftHole, middleHole, belowRightHole, rightHole };
    }

    void Start()
    {
        _defaultY = _rt.anchoredPosition.y;
        _rt.anchoredPosition = new Vector2(_holes[_currentIndex].anchoredPosition.x, _defaultY);
        _img.sprite = idleSprite;
    }

    void Update()
    {
        // Smooth horizontal movement between holes
        Vector2 targetPos = new Vector2(_holes[_currentIndex].anchoredPosition.x, _rt.anchoredPosition.y);
        _rt.anchoredPosition = Vector2.Lerp(_rt.anchoredPosition, targetPos, Time.deltaTime * moveSpeed);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
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
        // Cancel any ongoing move animation
        if (_moveAnimCoroutine != null) StopCoroutine(_moveAnimCoroutine);
        _moveAnimCoroutine = StartCoroutine(MoveAnimCoroutine(moveSprite));
    }

    private System.Collections.IEnumerator MoveAnimCoroutine(Sprite moveSprite)
    {
        _img.sprite = moveSprite;
        yield return new WaitForSeconds(moveAnimDuration);
        _img.sprite = idleSprite;
    }

    public void OnRise(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            _isRising = true;
            float targetY = _holes[_currentIndex].anchoredPosition.y;
            _rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, targetY);

            if (_upAnimCoroutine != null) StopCoroutine(_upAnimCoroutine);
            _upAnimCoroutine = StartCoroutine(UpSpriteAnimation());
        }
        else if (ctx.canceled)
        {
            _isRising = false;
            _rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, _defaultY);

            if (_upAnimCoroutine != null) StopCoroutine(_upAnimCoroutine);
            _img.sprite = idleSprite;
        }
    }

    private System.Collections.IEnumerator UpSpriteAnimation()
    {
        // Loop between the two "up" sprites while mole is up
        int frame = 0;
        while (_isRising)
        {
            _img.sprite = upSprites[frame];
            frame = (frame + 1) % upSprites.Length;
            yield return new WaitForSeconds(upAnimSpeed);
        }
    }
}
