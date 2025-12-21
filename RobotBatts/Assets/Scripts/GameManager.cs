using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    [Header("Канвасы")]
    [SerializeField] private GameObject startGameCanvas;
    [SerializeField] private GameObject settingCanvas;
    [SerializeField] private GameObject winGameCanvas;
    [SerializeField] private GameObject youDiedCanvas;

    [Header("XR Компоненты")]
    [SerializeField] private GameObject xrOrigin;
    [SerializeField] private GameObject enemy;

    [Header("Компоненты движения XR")]
    [SerializeField] private ActionBasedContinuousMoveProvider moveProvider;
    [SerializeField] private ActionBasedContinuousTurnProvider turnProvider;
    [SerializeField] private TeleportationProvider teleportationProvider;

    [Header("Настройки звука")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicText;

    private float defaultMusicVolume = 0.5f;

    void Start()
    {
        SetCanvasState(startGameCanvas, true);
        SetCanvasState(settingCanvas, false);
        SetCanvasState(winGameCanvas, false);
        SetCanvasState(youDiedCanvas, false);
        SetVRMovement(false);
        if (enemy != null)
            enemy.SetActive(false);
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
            musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
            ChangeMusicVolume(musicSlider.value);
        }
    }

    void SetCanvasState(GameObject canvas, bool state)
    {
        if (canvas != null)
            canvas.SetActive(state);
    }

    public void StartGame()
    {
        SetCanvasState(startGameCanvas, false);
        SetVRMovement(true);

        if (enemy != null)
            enemy.SetActive(true);
    }

    public void OpenSettings()
    {
        SetCanvasState(startGameCanvas, false);
        SetCanvasState(settingCanvas, true);
    }

    public void CloseSettings()
    {
        SetCanvasState(settingCanvas, false);
        SetCanvasState(startGameCanvas, true);
    }

    public void WinGame()
    {
        Invoke(nameof(ShowWinCanvas), 2f);
    }

    private void ShowWinCanvas()
    {
        SetCanvasState(winGameCanvas, true);
        SetVRMovement(false);

        if (enemy != null)
            enemy.SetActive(false);
    }

    public void PlayerDied()
    {
        SetCanvasState(youDiedCanvas, true);
        SetVRMovement(false);

        if (enemy != null)
            enemy.SetActive(false);
    }

    public void BackFromWin()
    {
        SetCanvasState(winGameCanvas, false);
        SetCanvasState(startGameCanvas, true);
        RestartScene();
    }

    public void BackFromDied()
    {
        SetCanvasState(youDiedCanvas, false);
        SetCanvasState(startGameCanvas, true);
        RestartScene();
    }

    void SetVRMovement(bool canMove)
    {
        if (moveProvider != null)
            moveProvider.enabled = canMove;

        if (turnProvider != null)
            turnProvider.enabled = canMove;

        if (teleportationProvider != null)
            teleportationProvider.enabled = canMove;
    }

    public void ChangeMusicVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = value;

        if (musicText != null)
            musicText.text = $"music: {Mathf.RoundToInt(value * 100)}%";

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}