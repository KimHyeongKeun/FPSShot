using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMemoryPool : MonoBehaviour
{
    [SerializeField]
    private GameObject enemySpawnPointPrehab;
    [SerializeField]
    private GameObject enemyPrehab;
    [SerializeField]
    private float enemySpawnTime =1; 
    [SerializeField]
    private float enemySpawnLatency =1;

    private MemoryPool spawnPointMemoryPool;
    private MemoryPool enemyMemoryPool;

    private int numberOfEnemiesSpawnAtOnce =1;
    private Vector2Int mapSize = new Vector2Int(100, 100);

    private void Awake()
    {
        spawnPointMemoryPool = new MemoryPool(enemySpawnPointPrehab);
        enemyMemoryPool = new MemoryPool(enemyPrehab);

        StartCoroutine("SpawnTile");
    }

    private IEnumerator SpawnTile()
    {
        int currentNumber=0;
        int maximumNumber = 50;

        while(true)
        {
            for (int i=0; i< numberOfEnemiesSpawnAtOnce; ++i)
            {
                GameObject item = spawnPointMemoryPool.ActivatePoolItem();

                item.transform.position = new Vector3(Random.Range(-mapSize.x*0.49f, mapSize.x*0.49f), 1, 
                                                      Random.Range(-mapSize.y*0.49f, mapSize.y*0.49f));

                StartCoroutine("SpawnEnemy", item);
            }

            currentNumber ++;

            if(currentNumber >= maximumNumber)
            {
                currentNumber =0;
                numberOfEnemiesSpawnAtOnce ++;
            }

            yield return new WaitForSeconds(enemySpawnTime);
        }
    }

    private IEnumerator SpawnEnemy(GameObject point)
    {
        yield return new WaitForSeconds(enemySpawnLatency);

        GameObject item = enemyMemoryPool.ActivatePoolItem();
        item.transform.position = point.transform.position;

        spawnPointMemoryPool.DeactivatePoolItems(point);
    }


}
