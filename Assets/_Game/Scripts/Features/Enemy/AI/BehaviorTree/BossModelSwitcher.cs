using UnityEngine;
using System.Collections;

public class BossModelSwitcher : MonoBehaviour
{
    [Header("Models")]
    [SerializeField] private GameObject soulModel;
    [SerializeField] private GameObject giantModel;

    [Header("Animators (auto-fetch nếu để trống)")]
    [SerializeField] private Animator soulAnimator;
    [SerializeField] private Animator giantAnimator;

    [Header("Phase 1 - Soul (flying)")]
    [SerializeField] private float soulFlyHeight = 0f;
    [SerializeField] private float soulMoveSpeed = 5f;
    [SerializeField] private float soulAttackRange = 3f;

    [Header("Phase 2 - Giant (grounded)")]
    [SerializeField] private float giantMoveSpeed = 2.5f;
    [SerializeField] private float giantAttackRange = 5f;

    [Header("Transition")]
    [SerializeField] private float transitionHPPercent = 0.5f;
    [SerializeField] private GameObject transitionVFX;
    [SerializeField] private float transitionAOEDamage = 30f;
    [SerializeField] private float transitionAOERadius = 4f;
    [SerializeField] private float slowMotionDuration = 0.5f;
    [SerializeField] private float transitionFreezeDuration = 1f;
    [SerializeField] private float vfxDuration = 5f;
    [SerializeField] private float fadeOutDuration = 1f;

    private static readonly WaitForSeconds WaitFreeze = new(1f);

    private BossBlackboard bb;
    private bool hasTransitioned = false;

    // Soul model fade — renderer instances được tạo khi Awake để tránh modify shared materials
    private Renderer[] _soulRenderers;
    private Color[][] _soulOriginalColors;

    public bool IsInPhase2 => hasTransitioned;
    public bool IsTransitioning { get; private set; }

    private void Awake()
    {
        bb = GetComponent<BossBlackboard>();

        if (soulAnimator == null && soulModel != null)
            soulAnimator = soulModel.GetComponent<Animator>()
                        ?? soulModel.GetComponentInChildren<Animator>(true);

        if (giantAnimator == null && giantModel != null)
            giantAnimator = giantModel.GetComponent<Animator>()
                         ?? giantModel.GetComponentInChildren<Animator>(true);

        CacheSoulRenderers();
    }

    private void Start()
    {
        if (soulModel != null) soulModel.SetActive(true);
        if (giantModel != null) giantModel.SetActive(false);

        if (bb != null && soulAnimator != null)
        {
            bb.animator = soulAnimator;
            bb.currentAnimState = BossAnimState.Idle;
        }

        if (bb != null && bb.agent != null)
        {
            bb.agent.baseOffset = soulFlyHeight;
            bb.agent.speed = soulMoveSpeed;
            bb.moveSpeed = soulMoveSpeed;
            bb.attackRange = soulAttackRange;
        }
    }

    private void OnEnable()  => GameEvent.Combat.OnHealthChanged += OnHealthChanged;
    private void OnDisable()
    {
        GameEvent.Combat.OnHealthChanged -= OnHealthChanged;
        Time.timeScale = 1f;
    }

    private void OnHealthChanged(HealthSystem hs, float current, float max)
    {
        if (hasTransitioned || bb == null) return;
        if (hs != bb.healthSystem) return;
        if (max <= 0f) return;

        if (current / max <= transitionHPPercent)
            StartCoroutine(TransitionToGiant());
    }

    private IEnumerator TransitionToGiant()
    {
        hasTransitioned = true;
        IsTransitioning = true;

        // ── Dừng AI ──
        if (bb.agent != null && bb.agent.isOnNavMesh)
            bb.agent.isStopped = true;
        bb.PlayAnimation(BossAnimState.SwitchElement);

        // ── Slow motion ──
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Time.timeScale = 1f;

        // ── Spawn VFX + bắt đầu fade soul song song ──
        GameObject vfxInstance = null;
        Vector3 position = new Vector3(transform.position.x, transform.position.y - soulFlyHeight, transform.position.z);
        if (transitionVFX != null)
            vfxInstance = Instantiate(transitionVFX, position, Quaternion.identity);
        StartCoroutine(FadeOutSoulModel());

        yield return new WaitForSeconds(vfxDuration);

        // ── Restore material trước khi ẩn (để pool reuse không bị kẹt alpha=0) ──
        RestoreSoulMaterials();
        if (soulModel != null) soulModel.SetActive(false);
        if (giantModel != null) giantModel.SetActive(true);

        if (vfxInstance != null)
            Destroy(vfxInstance);

        // ── Switch animator ──
        if (bb != null && giantAnimator != null)
        {
            bb.animator = giantAnimator;
            bb.currentAnimState = BossAnimState.Idle;
        }

        // ── AOE damage ──
        Collider[] hits = Physics.OverlapSphere(transform.position, transitionAOERadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            var ph = hit.GetComponent<HealthSystem>();
            if (ph != null)
                ph.TakeDamage(transitionAOEDamage, ElementType.Fire);
        }

        // ── Cập nhật stats phase 2 ──
        if (bb.agent != null)
        {
            bb.agent.baseOffset = 0f;
            bb.agent.speed = giantMoveSpeed;
        }
        bb.moveSpeed = giantMoveSpeed;
        bb.attackRange = giantAttackRange;

        // ── Freeze thêm 1s để player nhận biết ──
        yield return WaitFreeze;

        if (bb.agent != null && bb.agent.isOnNavMesh)
            bb.agent.isStopped = false;

        IsTransitioning = false;
    }

    // ── Fade helpers ──────────────────────────────────────────────────────────

    private void CacheSoulRenderers()
    {
        if (soulModel == null) return;

        _soulRenderers = soulModel.GetComponentsInChildren<Renderer>(true);
        _soulOriginalColors = new Color[_soulRenderers.Length][];

        for (int i = 0; i < _soulRenderers.Length; i++)
        {
            // .materials tạo instance riêng — không modify shared material
            var mats = _soulRenderers[i].materials;
            _soulOriginalColors[i] = new Color[mats.Length];
            for (int j = 0; j < mats.Length; j++)
                _soulOriginalColors[i][j] = GetMaterialColor(mats[j]);
            _soulRenderers[i].materials = mats;
        }
    }

    private IEnumerator FadeOutSoulModel()
    {
        if (_soulRenderers == null) yield break;

        // Bật chế độ transparent cho tất cả material instances
        foreach (var r in _soulRenderers)
            foreach (var mat in r.materials)
                SetMaterialTransparent(mat);

        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            SetSoulAlpha(1f - Mathf.Clamp01(t / fadeOutDuration));
            yield return null;
        }
        SetSoulAlpha(0f);
    }

    private void RestoreSoulMaterials()
    {
        if (_soulRenderers == null) return;

        for (int i = 0; i < _soulRenderers.Length; i++)
        {
            var mats = _soulRenderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                SetMaterialOpaque(mats[j]);
                SetMaterialColor(mats[j], _soulOriginalColors[i][j]);
            }
        }
    }

    private void SetSoulAlpha(float alpha)
    {
        for (int i = 0; i < _soulRenderers.Length; i++)
        {
            var mats = _soulRenderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                Color c = _soulOriginalColors[i][j];
                SetMaterialColor(mats[j], new Color(c.r, c.g, c.b, alpha));
            }
        }
    }

    private static Color GetMaterialColor(Material mat)
    {
        if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
        if (mat.HasProperty("_Color"))     return mat.color;
        return Color.white;
    }

    private static void SetMaterialColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        else if (mat.HasProperty("_Color")) mat.color = color;
    }

    private static void SetMaterialTransparent(Material mat)
    {
        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 3f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
        else if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1f);
            mat.renderQueue = 3000;
        }
    }

    private static void SetMaterialOpaque(Material mat)
    {
        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 0f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
        }
        else if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 0f);
            mat.renderQueue = -1;
        }
    }

    // ── Pool reset ────────────────────────────────────────────────────────────

    public void ResetSwitcher()
    {
        hasTransitioned = false;
        IsTransitioning = false;

        RestoreSoulMaterials();
        if (soulModel != null) soulModel.SetActive(true);
        if (giantModel != null) giantModel.SetActive(false);

        if (bb != null && soulAnimator != null)
            bb.animator = soulAnimator;

        if (bb != null && bb.agent != null)
        {
            bb.agent.baseOffset = soulFlyHeight;
            bb.agent.speed = soulMoveSpeed;
            bb.moveSpeed = soulMoveSpeed;
            bb.attackRange = soulAttackRange;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, transitionAOERadius);
    }
}

