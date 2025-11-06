using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.Collections;

public class DudeController : MonoBehaviour
{
    [SerializeField] private Light2D[] holeLights;
    [SerializeField] private MoleController mole;
    [SerializeField] private Sprite handCenterSprite;
    [SerializeField] private Sprite handLeftSprite;
    [SerializeField] private Sprite handRightSprite;
    [SerializeField] private Sprite[] squeezeSprites;
    [SerializeField] private RectTransform handLeftPos;
    [SerializeField] private RectTransform handMidPos;
    [SerializeField] private RectTransform handRightPos;
    [SerializeField] private GameObject warnLeft;
    [SerializeField] private GameObject warnMid;
    [SerializeField] private GameObject warnRight;
    [SerializeField] private float attackInterval = 5f;
    [SerializeField] private float dimDuration = 1f;
    [SerializeField] private float scoopInterval = 10f;
    [SerializeField] private float scoopStepDuration = 0.5f;
    [SerializeField] private float warningDuration = 0.5f;

    private int _currentTarget = -1;
    private float _timeSinceMoleLastUp;
    private bool _isScooping;
    private Image _img;
    private RectTransform _rt;
    private Coroutine _warningCoroutine;
    private Coroutine _gruntCoroutine;

    void Awake()
    {
        _img = GetComponent<Image>();
        _rt = GetComponent<RectTransform>();
        _img.enabled = false;
        HideAllWarnings();
    }

    public void BeginLogic()
    {
        if (gameObject.activeInHierarchy && enabled)
        {
            StartCoroutine(MainLoop());
            _gruntCoroutine = StartCoroutine(PlayRandomGrunts());
        }
    }

    private IEnumerator MainLoop()
    {
        while (!GameManager.Instance.IsGameOver)
        {
            yield return new WaitForSeconds(attackInterval);
            if (GameManager.Instance.IsGameOver) yield break;

            ChooseRandomHole();
            yield return DimAndHit();

            if (GameManager.Instance.IsGameOver) yield break;

            if (!mole.IsUp())
            {
                _timeSinceMoleLastUp += attackInterval;
                if (_timeSinceMoleLastUp >= scoopInterval && !_isScooping)
                {
                    yield return ScoopAttack();
                    _timeSinceMoleLastUp = 0f;
                }
            }
        }
    }

    private void ChooseRandomHole()
    {
        int newTarget;
        do { newTarget = Random.Range(1, 4); }
        while (newTarget == _currentTarget);
        _currentTarget = newTarget;
    }

    private IEnumerator DimAndHit()
    {
        if (GameManager.Instance.IsGameOver) yield break;

        Light2D target = holeLights[_currentTarget];
        float startIntensity = target.intensity;
        float t = 0f;

        while (t < 1f)
        {
            if (GameManager.Instance.IsGameOver) yield break;
            t += Time.deltaTime / dimDuration;
            target.intensity = Mathf.Lerp(startIntensity, 0f, t);
            yield return null;
        }

        CameraShake.Instance.Shake(0.2f, 0.2f);
        AudioManager.Instance.PlayRandom("smash1", "smash2");

        if (!GameManager.Instance.IsGameOver && mole.IsUp() && mole.GetCurrentHoleIndex() == _currentTarget)
            mole.Die();

        yield return new WaitForSeconds(0.5f);
        target.intensity = startIntensity;
    }

    private IEnumerator ScoopAttack()
    {
        if (GameManager.Instance.IsGameOver) yield break;

        _isScooping = true;

        if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
        _warningCoroutine = StartCoroutine(ShowWarningBeforeAttack(_currentTarget));

        yield return new WaitForSecondsRealtime(warningDuration);
        if (GameManager.Instance.IsGameOver) yield break;

        _img.enabled = true;

        if (_currentTarget == 1) _rt.anchoredPosition = handLeftPos.anchoredPosition;
        else if (_currentTarget == 2) _rt.anchoredPosition = handMidPos.anchoredPosition;
        else _rt.anchoredPosition = handRightPos.anchoredPosition;

        int startIndex = _currentTarget;
        int[] sequence;
        if (startIndex == 1) sequence = new[] { 1, 0, 1, 2, 1 };
        else if (startIndex == 2) sequence = new[] { 2, 1, 2, 3, 2 };
        else sequence = new[] { 3, 2, 3, 4, 3 };

        for (int i = 0; i < sequence.Length; i++)
        {
            if (GameManager.Instance.IsGameOver) yield break;

            int targetIndex = sequence[i];

            if (i > 0)
            {
                int diff = sequence[i] - sequence[i - 1];
                if (diff < 0) _img.sprite = handLeftSprite;
                else if (diff > 0) _img.sprite = handRightSprite;
                else _img.sprite = handCenterSprite;
            }
            else _img.sprite = handCenterSprite;

            AudioManager.Instance.PlayRandom("swoosh1", "swoosh2");
            CameraShake.Instance.Shake(0.1f, 0.1f);

            if (!GameManager.Instance.IsGameOver && !mole.IsUp() && mole.GetCurrentHoleIndex() == targetIndex)
            {
                mole.HideForSqueeze();
                yield return SqueezeAnimation();
                yield break;
            }

            yield return new WaitForSecondsRealtime(scoopStepDuration);
        }

        _img.enabled = false;
        _isScooping = false;
        HideAllWarnings();
    }

    private IEnumerator ShowWarningBeforeAttack(int index)
    {
        HideAllWarnings();

        if (index == 1 && warnLeft) warnLeft.SetActive(true);
        else if (index == 2 && warnMid) warnMid.SetActive(true);
        else if (index == 3 && warnRight) warnRight.SetActive(true);

        yield return new WaitForSecondsRealtime(warningDuration);
        HideAllWarnings();
    }

    private void HideAllWarnings()
    {
        if (warnLeft) warnLeft.SetActive(false);
        if (warnMid) warnMid.SetActive(false);
        if (warnRight) warnRight.SetActive(false);
    }

    private IEnumerator SqueezeAnimation()
    {
        if (GameManager.Instance.IsGameOver) yield break;

        foreach (var s in squeezeSprites)
        {
            _img.sprite = s;
            yield return new WaitForSecondsRealtime(0.15f);
        }

        if (!GameManager.Instance.IsGameOver)
        {
            CameraShake.Instance.Shake(0.25f, 0.3f);
            AudioManager.Instance.Play("splat");
            GameManager.Instance.PlayerDie();
        }

        _isScooping = false;
        HideAllWarnings();
    }

    private IEnumerator PlayRandomGrunts()
    {
        while (!GameManager.Instance.IsGameOver)
        {
            yield return new WaitForSecondsRealtime(5f);
            if (GameManager.Instance.IsGameOver) yield break;

            AudioManager.Instance.PlayRandom(
                "grunt1", "grunt2", "grunt3",
                "grunt4", "grunt5", "grunt6", "grunt7"
            );
        }
    }

    void OnDisable()
    {
        if (_gruntCoroutine != null)
            StopCoroutine(_gruntCoroutine);
    }
}
