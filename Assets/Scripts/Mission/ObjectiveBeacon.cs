using System;
using UnityEngine;

namespace Echo.Mission
{
    /// <summary>
    /// Haz de luz vertical que indica el siguiente objetivo de la misión. Sin colliders y
    /// nunca estático (se mueve). Elige el objetivo según MissionManager (por eventos, no en
    /// Update): etapa 1 sobre Fragment_1; etapa 2 sobre Portal_A si el jugador está en la
    /// región A (X menor que 100) o sobre el fragmento pendiente más cercano si está en B;
    /// etapa 3 sobre Portal_B si está en B o sobre el núcleo si está en A. Se oculta al ganar.
    /// </summary>
    [DisallowMultipleComponent]
    public class ObjectiveBeacon : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private MissionManager missionManager;
        [SerializeField] private Transform player;

        [Header("Objetivos")]
        [SerializeField] private EnergyFragment fragment1;
        [SerializeField] private EnergyFragment fragment2;
        [SerializeField] private EnergyFragment fragment3;
        [SerializeField] private Transform portalA;
        [SerializeField] private Transform portalB;
        [SerializeField] private Transform ancestralCore;

        [Header("Haz")]
        [Tooltip("Renderers del haz cuyo emisión pulsa suavemente (opcional).")]
        [SerializeField] private Renderer[] pulseRenderers;
        [Tooltip("Velocidad de desplazamiento del haz hacia el objetivo (m/s).")]
        [SerializeField, Min(0.5f)] private float moveSpeed = 3f;
        [Tooltip("Distancia a partir de la cual el haz se teletransporta al objetivo nuevo.")]
        [SerializeField, Min(1f)] private float snapDistance = 15f;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private const float RegionSeparationX = 100f;
        private const float PulseFrequency = 2.2f;

        private Transform target;
        private Transform lastTarget;
        private EnergyFragment[] fragments;
        private MaterialPropertyBlock block;
        private Color[] baseEmissions;

        private void Awake()
        {
            if (missionManager == null || player == null || fragment1 == null ||
                portalA == null || portalB == null || ancestralCore == null)
            {
                Debug.LogError("ObjectiveBeacon: faltan referencias (Mission, Player, Fragment_1, " +
                               "Portal_A, Portal_B, Ancestral Core).", this);
                enabled = false;
                return;
            }

            fragments = new[] { fragment1, fragment2, fragment3 };
            block = new MaterialPropertyBlock();

            baseEmissions = new Color[pulseRenderers != null ? pulseRenderers.Length : 0];
            if (pulseRenderers == null || pulseRenderers.Length == 0)
            {
                Debug.LogWarning("ObjectiveBeacon: sin 'Pulse Renderers' asignado; el haz no pulsará.", this);
            }
            else
            {
                for (int i = 0; i < pulseRenderers.Length; i++)
                {
                    Material material = pulseRenderers[i] != null ? pulseRenderers[i].sharedMaterial : null;
                    baseEmissions[i] = material != null && material.HasProperty(EmissionColorId)
                        ? material.GetColor(EmissionColorId)
                        : Color.black;
                }
            }
        }

        private void OnEnable()
        {
            missionManager.FragmentCollected += HandleFragmentsChanged;
            missionManager.StageChanged += HandleStageChanged;
            missionManager.MissionCompleted += HandleMissionCompleted;
            RefreshTarget();
        }

        private void OnDisable()
        {
            if (missionManager == null) return;

            missionManager.FragmentCollected -= HandleFragmentsChanged;
            missionManager.StageChanged -= HandleStageChanged;
            missionManager.MissionCompleted -= HandleMissionCompleted;
        }

        private void LateUpdate()
        {
            if (target == null || missionManager == null || missionManager.IsCompleted)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position, target.position, moveSpeed * Time.deltaTime);
            UpdatePulse();
        }

        private void HandleStageChanged(MissionStage stage)
        {
            RefreshTarget();
        }

        private void HandleFragmentsChanged(int collected, int total)
        {
            RefreshTarget();
        }

        private void HandleMissionCompleted()
        {
            Show(false);
        }

        private void RefreshTarget()
        {
            if (missionManager.IsCompleted)
            {
                Show(false);
                return;
            }

            Transform next;
            switch (missionManager.Stage)
            {
                case MissionStage.ExploreSanctuary:
                    next = fragment1.transform;
                    break;
                case MissionStage.PortalAndFragments:
                    next = IsPlayerInRegionA() ? portalA : NearestPendingFragment();
                    break;
                case MissionStage.ActivateCore:
                    next = IsPlayerInRegionA() ? ancestralCore : portalB;
                    break;
                default:
                    next = null;
                    break;
            }

            target = next;
            Show(target != null);

            if (target != null && (lastTarget == null || lastTarget != target) &&
                (target.position - transform.position).sqrMagnitude > snapDistance * snapDistance)
            {
                // Cambio de zona: el haz aparece sobre el nuevo objetivo sin cruzar el mapa.
                transform.position = target.position;
            }

            lastTarget = target;
        }

        private Transform NearestPendingFragment()
        {
            EnergyFragment best = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < fragments.Length; i++)
            {
                EnergyFragment candidate = fragments[i];
                if (candidate == null || candidate.IsCollected) continue;

                float sqr = (candidate.transform.position - player.position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = candidate;
                }
            }

            return best != null ? best.transform : null;
        }

        private bool IsPlayerInRegionA()
        {
            return player.position.x < RegionSeparationX;
        }

        private void Show(bool value)
        {
            if (gameObject.activeSelf != value)
            {
                gameObject.SetActive(value);
            }
        }

        private void UpdatePulse()
        {
            if (pulseRenderers == null || baseEmissions == null) return;

            float pulse = 1f + Mathf.Sin(Time.time * PulseFrequency) * 0.25f;
            for (int i = 0; i < pulseRenderers.Length; i++)
            {
                Renderer renderer = pulseRenderers[i];
                if (renderer == null) continue;

                renderer.GetPropertyBlock(block);
                block.SetColor(EmissionColorId, baseEmissions[i] * pulse);
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
