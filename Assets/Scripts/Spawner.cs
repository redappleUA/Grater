using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] List<GameObject> _vegetablesPrefabs;
    [SerializeField] Transform _spawnPosition; 

    public Vector3 Rotate { get; set; } = new Vector3(0, 0, 80);

    public void Spawn(GameObject previousObject)
    {
        Destroy(previousObject);

        var go = Instantiate(_vegetablesPrefabs[Random.Range(0, _vegetablesPrefabs.Count)], _spawnPosition.position, Quaternion.Euler(Rotate));
        go.AddComponent<Movement>();
    }
}
