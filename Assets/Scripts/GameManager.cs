using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject playAgainButton;
    [SerializeField] private GameObject youWinImage;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private DudeController dude;
    [SerializeField] private MoleController mole;
    [SerializeField] private CustomSlider rageBar;
    [SerializeField] private float rageIncreaseSpeed = 0.2f;

    private float _rageValue;
    private bool _hasWon;
    private bool IsPlayerDead { get; set; }
    public bool IsGameOver => IsPlayerDead || _hasWon;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Time.timeScale = 1f;
        playAgainButton.SetActive(false);
        if (youWinImage != null) youWinImage.SetActive(false);

        IsPlayerDead = false;
        _hasWon = false;
        _rageValue = 0f;
    }

    void Start()
    {
        AudioManager.Instance.PlayLoop("amusementLoop");
        AudioManager.Instance.PlayLoop("creepyLoop");
        StartCoroutine(StartDudeSafely());
    }

    void Update()
    {
        if (IsGameOver) return;

        if (mole.IsUp())
            _rageValue += rageIncreaseSpeed * Time.deltaTime;

        _rageValue = Mathf.Clamp01(_rageValue);
        rageBar.SetValue(_rageValue);

        if (_rageValue >= 1f)
            PlayerWin();
    }

    private System.Collections.IEnumerator StartDudeSafely()
    {
        yield return null;
        yield return new WaitUntil(() => dude != null && dude.gameObject.activeInHierarchy && dude.enabled);
        yield return null;
        dude.BeginLogic();
    }

    public void PlayerDie()
    {
        if (IsGameOver) return;
        IsPlayerDead = true;
        AudioManager.Instance.Play("splat");
        if (playerInput != null)
            playerInput.enabled = false;
        playAgainButton.SetActive(true);
        Time.timeScale = 0f;
    }

    private void PlayerWin()
    {
        if (_hasWon) return;
        _hasWon = true;
        if (playerInput != null)
            playerInput.enabled = false;
        if (youWinImage != null)
            youWinImage.SetActive(true);
        playAgainButton.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        if (playerInput != null)
            playerInput.enabled = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
