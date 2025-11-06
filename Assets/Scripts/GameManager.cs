using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject playAgainButton;
    [SerializeField] private PlayerInput playerInput; // Reference to PlayerInput component (from Mole)

    public bool IsPlayerDead { get; private set; } // Tracks player death

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Time.timeScale = 1f;
        playAgainButton.SetActive(false);
        IsPlayerDead = false;
    }

    public void PlayerDie()
    {
        IsPlayerDead = true;

        // Disable all input events so player can't move or rise anymore
        if (playerInput != null)
            playerInput.enabled = false;

        Time.timeScale = 0f;
        playAgainButton.SetActive(true);
    }

    public void RestartGame()
    {
        // Re-enable input before restarting
        if (playerInput != null)
            playerInput.enabled = true;

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}