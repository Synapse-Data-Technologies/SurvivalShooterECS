using Unity.Entities;

public struct EnemySpawnerData : IComponentData
{
    public float SpawnTime;
    public float CurrentTime;
    public Entity PrefabEntity;
}
