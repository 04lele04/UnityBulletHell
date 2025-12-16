using UnityEngine;
using TMPro;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject enemyPrefab;

    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text enemiesText;
    public TMP_Text nextWaveText;

    [Header("Parametri Base")]
    public float spawnRate = 1f;
    public int baseEnemyCount = 5;
    public float baseEnemySpeed = 2f;
    public int baseEnemyHP = 2;

    [Header("Incrementi per Wave")]
    public float spawnRateDecreasePerWave = 0.05f;
    public float speedIncreasePerWave = 0.1f;
    public int hpIncreasePerWave = 1;
    public int enemyCountIncreasePerWave = 2;

    [Header("Intervallo tra Wave")]
    public float timeBetweenWaves = 2f;

    private float spawnTimer;
    private int currentWave = 1;
    private int enemiesSpawned = 0;
    private int enemiesAlive = 0;
    private int enemiesInWave = 0;
    private float currentSpawnRate;
    private bool isWaveActive = false;
    private bool isPreparingNextWave = false;

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPrefab non assegnato!");
            enabled = false;
            return;
        }

        ClearNextWaveText();
        StartWave();
    }

    void Update()
    {
        if (!isWaveActive) return;

        // Spawn nemici durante la wave
        if (enemiesSpawned < enemiesInWave)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = currentSpawnRate;
                SpawnEnemy();
            }
        }
        // Verifica fine wave: tutti i nemici sono stati spawnati E non ci sono più nemici vivi
        else if (enemiesAlive <= 0 && !isPreparingNextWave)
        {
            StartCoroutine(PrepareNextWave());
        }

        UpdateUI();
    }

    void StartWave()
    {
        // Calcola parametri della wave
        enemiesInWave = baseEnemyCount + (currentWave - 1) * enemyCountIncreasePerWave;
        currentSpawnRate = Mathf.Max(0.1f, spawnRate - (currentWave - 1) * spawnRateDecreasePerWave);

        // Reset contatori
        enemiesSpawned = 0;
        enemiesAlive = 0;
        spawnTimer = 0f; // Spawn immediato del primo nemico

        // Reset flag
        isWaveActive = true;
        isPreparingNextWave = false;

        ClearNextWaveText();

        Debug.Log($"<color=cyan>Wave {currentWave} iniziata</color> - Nemici: {enemiesInWave}, SpawnRate: {currentSpawnRate:F2}s");
    }

    void SpawnEnemy()
    {
        // Spawn in posizione casuale fuori dallo schermo
        Vector2 spawnPos = Random.insideUnitCircle.normalized * 7f;
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Il prefab Enemy non ha il component Enemy!");
            Destroy(enemyObj);
            return;
        }

        // Configura statistiche del nemico
        enemy.hp = baseEnemyHP + (currentWave - 1) * hpIncreasePerWave;
        enemy.speed = baseEnemySpeed + (currentWave - 1) * speedIncreasePerWave;

        // Incrementa contatori
        enemiesSpawned++;
        enemiesAlive++;

        // Sottoscrivi evento morte
        enemy.OnDeath += OnEnemyDeath;

        Debug.Log($"Nemico #{enemiesSpawned}/{enemiesInWave} spawnato - HP: {enemy.hp}, Speed: {enemy.speed:F1}");
    }

    void OnEnemyDeath()
    {
        enemiesAlive--;

        // Sanity check
        if (enemiesAlive < 0)
        {
            Debug.LogWarning("enemiesAlive è negativo! Ripristino a 0.");
            enemiesAlive = 0;
        }

        Debug.Log($"Nemico ucciso - Rimasti: {enemiesAlive}/{enemiesInWave}");
    }

    IEnumerator PrepareNextWave()
    {
        isPreparingNextWave = true;
        isWaveActive = false;

        Debug.Log($"<color=yellow>Wave {currentWave} completata!</color>");

        // Mostra messaggio next wave
        if (nextWaveText != null)
            nextWaveText.text = $"Wave {currentWave + 1} in arrivo...";

        yield return new WaitForSeconds(timeBetweenWaves);

        // Incrementa wave e ricomincia
        currentWave++;
        StartWave();
    }

    void UpdateUI()
    {
        if (waveText != null)
            waveText.text = $"Wave: {currentWave}";

        if (enemiesText != null)
            enemiesText.text = $"Nemici: {enemiesAlive}";
    }

    void ClearNextWaveText()
    {
        if (nextWaveText != null)
            nextWaveText.text = "";
    }

    // Debug visuale in Scene View
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, 7f);
    }
}