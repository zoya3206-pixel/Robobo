using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject startGameCanvas;
    [SerializeField] private GameObject settingCanvas;
    [SerializeField] private GameObject winGameCanvas;
    [SerializeField] private GameObject youDiedCanvas;
    [SerializeField] private GameObject TutorialCanvas;
    public Transform XROOO;
    [SerializeField] private Vector3 xrOrigin;
    [SerializeField] private GameObject enemy;

    [SerializeField] private PlayerHealth playerHealth;

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

        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
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
        SetCanvasState(TutorialCanvas, true);
    }

    public void CloseTutorial()
    {
        SetCanvasState(TutorialCanvas, false);
        enemy.SetActive(true);
        Colliders.SetActive(false);

        if (playerHealth != null)
        {
            playerHealth.ResetHealthAndGlass();
        }

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.ResetEnemy();
        }
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
        XROOO.position = xrOrigin;
        Invoke("OffEnemyOnC", 1f);
    }

    public void PlayerDied()
    {
        SetCanvasState(youDiedCanvas, true);
        XROOO.position = xrOrigin;
        Invoke("OffEnemyOnC", 1f);
    }

    public void BackFromWin()
    {
        SetCanvasState(winGameCanvas, false);
        SetCanvasState(startGameCanvas, true);
        ResetPlayerHealthAndGlass();
        ResetEnemy();
    }

    public void BackFromDied()
    {
        SetCanvasState(youDiedCanvas, false);
        SetCanvasState(startGameCanvas, true);
        ResetPlayerHealthAndGlass();
        ResetEnemy();
    }

    private void ResetPlayerHealthAndGlass()
    {
        if (playerHealth != null)
        {
            playerHealth.ResetHealthAndGlass();
        }
    }

    private void ResetEnemy()
    {
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.ResetEnemy();
        }
    }

    public void OffEnemyOnC()
    {
        enemy.SetActive(false);
        Colliders.SetActive(true);
    }
}