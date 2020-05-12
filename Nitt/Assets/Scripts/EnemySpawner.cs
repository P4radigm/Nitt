using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject batPrefab;
    [SerializeField] private float spawnCooldown;
    private List<BatBehaviour> bats = new List<BatBehaviour>();

    private Coroutine spawnBatRoutine = null;

    // Update is called once per frame
    void Update()
    {
        if(FindObjectsOfType<BatBehaviour>().Length < 1)
        {
            if (spawnBatRoutine == null) { spawnBatRoutine = StartCoroutine(SpawnBat()); } 
        }
    }

    private IEnumerator SpawnBat()
    {
        yield return new WaitForSeconds(spawnCooldown);
        Instantiate(batPrefab, transform.position, Quaternion.identity);
        spawnBatRoutine = null;
    }
}
