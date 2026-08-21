using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class OnScreenUI : MonoBehaviour
{
    public static OnScreenUI Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI enemyCounterText;
    [SerializeField] private TextMeshProUGUI missionCompleteText;
    [SerializeField] private float restartDelay = 3f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private TextMeshProUGUI flashlightText;

    private float levelTimer;
    private bool timerRunning;

    private int enemiesRemaining;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        EnemyAi[] enemies = FindObjectsByType<EnemyAi>(FindObjectsSortMode.None);

        enemiesRemaining = enemies.Length;

        timerRunning = true;

        UpdateUI();

        flashlightText.text = "F: Toggle Flashlight";
    }

    private void Update()
    {
        if (!timerRunning) return;

        levelTimer += Time.deltaTime;

        UpdateTimerUI();
    }

    public void EnemyKilled()
    {
        enemiesRemaining--;

        if (enemiesRemaining < 0)
            enemiesRemaining = 0;

        UpdateUI();

        if (enemiesRemaining <= 0)
        {
            MissionComplete();
        }
    }

    private void UpdateUI()
    {
        if (enemyCounterText != null)
        {
            enemyCounterText.text = $"Enemies Remaining: {enemiesRemaining}";
        }
    }

    private void MissionComplete()
    {
        timerRunning = false;

        StartCoroutine(RestartLevel());
    }

    private IEnumerator RestartLevel()
    {
        if (missionCompleteText != null)
        {
            missionCompleteText.gameObject.SetActive(true);
            missionCompleteText.text = "MISSION COMPLETE";
        }

        if (finalTimeText != null)
        {
            finalTimeText.gameObject.SetActive(true);
            finalTimeText.text = $"Time: {timerText.text}";
        }

        yield return new WaitForSeconds(restartDelay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(levelTimer / 60f);
        int seconds = Mathf.FloorToInt(levelTimer % 60f);
        int milliseconds = Mathf.FloorToInt((levelTimer * 100f) % 100f);

        timerText.text = $"{minutes:00} : {seconds:00}: {milliseconds:00}";
       
    }
}