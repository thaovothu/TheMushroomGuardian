using UnityEngine;

public class SceneSpawnPointRegistry : MonoBehaviour
{
    [SerializeField] private string spawnGroupId; // "2"
    [SerializeField] private Transform[] spawnPoints;

    private void OnEnable()
    {
        UILoading.OnLoadingComplete += Register;
    }

    private void OnDisable()
    {
        UILoading.OnLoadingComplete -= Register;
    }

    private void Register()
    {
        QuestSpawnManager.Instance?.RegisterSceneSpawnPoints(spawnGroupId, spawnPoints);
    }
}