public interface IEnemyRepository
{
    BaseEnemyData GetEnemyData(int level);
    EnemyVariant GetEnemyVariant(string enemyName);
    EnemyVariant GetRandomEnemyVariant();
}