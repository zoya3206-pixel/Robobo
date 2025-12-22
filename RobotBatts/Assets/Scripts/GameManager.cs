using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject startGameCanvas;
    [SerializeField] private GameObject settingCanvas;
    [SerializeField] private GameObject winGameCanvas;
    [SerializeField] private GameObject youDiedCanvas;
    [SerializeField] private GameObject TutorialCanvas;
    [SerializeField] private Vector3 xrOrigin;
    [SerializeField] private GameObject enemy;

    public GameObject Colliders;

    void Start()
    {
        xrOrigin = new Vector3(-11.8999996f, 3.00159979f, -0.600000024f);

        SetCanvasState(startGameCanvas, true);
        SetCanvasState(settingCanvas, false);
        SetCanvasState(winGameCanvas, false);
        SetCanvasState(youDiedCanvas, false);
        SetCanvasState(TutorialCanvas, false);
        enemy.SetActive(false);
        Colliders.SetActive(true);
    }

    void SetCanvasState(GameObject canvas, bool state)
    {
        if (canvas != null)
            canvas.SetActive(state);
    }

    public void StartGame()
    {
        SetCanvasState(startGameCanvas, false);
        SetCanvasState(TutorialCanvas, true);
    }

    public void CloseTutorial()
    {
        SetCanvasState(TutorialCanvas, false);

        if (enemy != null)
            enemy.SetActive(true);
        Colliders.SetActive(false);
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
        xrOrigin = new Vector3(-11.8999996f, 3.00159979f, -0.600000024f);

        if (enemy != null)
            enemy.SetActive(false);
        Colliders.SetActive(true);
    }

    public void PlayerDied()
    {
        SetCanvasState(youDiedCanvas, true);
        xrOrigin = new Vector3(-11.8999996f, 3.00159979f, -0.600000024f);

        if (enemy != null)
            enemy.SetActive(false);
        Colliders.SetActive(true);
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

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}