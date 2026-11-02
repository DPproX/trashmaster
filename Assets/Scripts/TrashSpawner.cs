using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    public GameObject[] trashPrefabs;
    public Transform[] spawnPoints;           
    public float delayBetweenBatches = 5f;    
    public bool hideMarkers = true;

    private Vector3[] positions;
    private Quaternion[] rotations;
    private readonly HashSet<TrashItem> pending = new HashSet<TrashItem>();

    void Start()
    {
        positions = new Vector3[spawnPoints.Length];
        rotations = new Quaternion[spawnPoints.Length];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            positions[i] = spawnPoints[i].position;
            rotations[i] = spawnPoints[i].rotation;

            if (hideMarkers)
                spawnPoints[i].gameObject.SetActive(false);
        }

        SpawnBatch();
    }

    void SpawnBatch()
    {
        if (pending.Count > 0) return;        

        for (int i = 0; i < positions.Length; i++)
        {
            var prefab = trashPrefabs[Random.Range(0, trashPrefabs.Length)];
            var obj = Instantiate(prefab, positions[i], rotations[i]);

            var item = obj.GetComponent<TrashItem>();
            pending.Add(item);
            item.OnDespawned += HandleDespawned;
        }
    }

    void HandleDespawned(TrashItem item)
    {
        item.OnDespawned -= HandleDespawned;
        if (!pending.Remove(item)) return;

        if (!this || !isActiveAndEnabled) return;   

        if (pending.Count == 0)                      
            StartCoroutine(NextBatch());
    }

    IEnumerator NextBatch()
    {
        yield return new WaitForSeconds(delayBetweenBatches);
        SpawnBatch();
    }
}