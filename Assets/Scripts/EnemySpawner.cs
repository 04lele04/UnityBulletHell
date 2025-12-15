using UnityEngine;
using System.Collections.Generic;
public class EnemySpawner : MonoBehaviour
{
public GameObject enemyPrefab;
public float spawnRate = 1f;
float timer;


void Update()
{
timer -= Time.deltaTime;
if (timer <= 0f)
{
timer = spawnRate;
Vector2 pos = Random.insideUnitCircle.normalized * 7f;
Instantiate(enemyPrefab, pos, Quaternion.identity);
}
}
}