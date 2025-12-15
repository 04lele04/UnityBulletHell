using UnityEngine;
using TMPro; // TextMeshPro
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    // UI
    public TMP_Text waveText;
    public TMP_Text enemiesText;
    public TMP_Text nextWaveText;

    // Parametri base
    public float spawnRate = 1f;
    public int baseEnemyCount = 5;
    public float baseEnemySpeed = 2f;
    public int baseEnemyHP = 2;

    // Incrementi per wave
    public float spawnRateDecreasePerWave = 0.05f;
    public float speedIncreasePerWave = 0.1f;
    public int hpIncreasePerWave = 1;
    public int enemyCountIncreasePerWave = 2;

    private float timer;
    private int currentWave = 1;
    private int enemiesSpawned = 0;
    private int enemiesAlive = 0;
    private int enemiesInWave = 0;
    private float currentSpawnRate;

    // Flag per evitare coroutine multiple
    private bool preparingNextWave = false;

    void Start()
    {
        if (nextWaveText != null)
            nextWaveText.text = "";
        StartWave();
    }

    void Update()
    {
        if (enemiesSpawned < enemiesInWave)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = currentSpawnRate;
                SpawnEnemy();
            }
        }

        // Controlla la fine della wave
        if (enemiesSpawned >= enemiesInWave && enemiesAlive <= 0 && !preparingNextWave)
        {
            StartCoroutine(PrepareNextWave());
        }

        UpdateUI();
    }

    void StartWave()
    {
        enemiesInWave = baseEnemyCount + (currentWave - 1) * enemyCountIncreasePerWave;
        currentSpawnRate = Mathf.Max(0.1f, spawnRate - (currentWave - 1) * spawnRateDecreasePerWave);
        enemiesSpawned = 0;
        enemiesAlive = 0; // <- inizializza a 0, aumenterà ad ogni spawn
        preparingNextWave = false;

        if (nextWaveText != null)
            nextWaveText.text = "";

        Debug.Log($"Wave {currentWave} - Nemici: {enemiesInWave}, SpawnRate: {currentSpawnRate}");
    }

    void SpawnEnemy()
    {
        Vector2 pos = Random.insideUnitCircle.normalized * 7f;
        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

        Enemy e = enemy.GetComponent<Enemy>();
        e.hp = baseEnemyHP + (currentWave - 1) * hpIncreasePerWave;
        e.speed = baseEnemySpeed + (currentWave - 1) * speedIncreasePerWave;

        enemiesAlive++; // <- incremento reale solo quando il nemico esiste
        enemiesSpawned++;

        e.OnDeath += () =>
        {
            enemiesAlive--;
            if (enemiesAlive < 0)
                enemiesAlive = 0; // sicurezza
        };
    }

    IEnumerator PrepareNextWave()
    {
        preparingNextWave = true;
        if (nextWaveText != null)
            nextWaveText.text = "Next Wave Incoming!";

        yield return new WaitForSeconds(2f);

        currentWave++;
        StartWave();
    }

    void UpdateUI()
    {
        if (waveText != null)
            waveText.text = $"Wave: {currentWave}";
        if (enemiesText != null)
            enemiesText.text = $"Enemies Left: {enemiesAlive}";
    }
}
