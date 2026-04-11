using UnityEngine;

public class SimplePoolable : MonoBehaviour
{
    SimpleObjectPool pool;
    public void SetPool(SimpleObjectPool pool) => this.pool = pool;
    public void ReturnToPool() => pool?.Release(this.gameObject);
}
