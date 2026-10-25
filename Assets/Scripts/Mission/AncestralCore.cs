using Echo.Interactions;
using UnityEngine;

namespace Echo.Mission
{
    /// <summary>
    /// Núcleo ancestral. Va en la raíz del núcleo, con un Sphere Collider (trigger)
    /// en la capa "Interactable". Solo permite completar la misión con todos los fragmentos.
    /// </summary>
    public class AncestralCore : Interactable
    {
        [Header("Referencias")]
        [SerializeField] private MissionManager mission;
        [Tooltip("Renderer de la esfera del núcleo (su material debe tener Emission activada).")]
        [SerializeField] private Renderer coreRenderer;
        [SerializeField] private Light coreLight;

        [Header("Mensajes")]
        [SerializeField] private string lockedPrompt = "El núcleo necesita los fragmentos de energía";
        [SerializeField] private string readyPrompt = "Pulsa E para restaurar el núcleo";

        [Header("Estados visuales")]
        [ColorUsage(false, true)][SerializeField] private Color inactiveEmission = new Color(0.02f, 0.05f, 0.06f);
        [ColorUsage(false, true)][SerializeField] private Color readyEmission = new Color(0f, 1.5f, 2f);
        [ColorUsage(false, true)][SerializeField] private Color completedEmission = new Color(1.5f, 3f, 4f);
        [SerializeField] private float inactiveLight = 0.2f;
        [SerializeField] private float readyLight = 2f;
        [SerializeField] private float completedLight = 6f;
        [SerializeField] private float transitionSpeed = 3f;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private Material coreMaterial;
        private Color currentEmission;
        private Color targetEmission;
        private float currentLight;
        private float targetLight;

        public override string PromptText
        {
            get
            {
                if (mission == null || mission.IsCompleted) return string.Empty;
                if (!mission.AllFragmentsCollected)
                    return $"{lockedPrompt} ({mission.CollectedCount}/{mission.TotalFragments})";
                return readyPrompt;
            }
        }

        private void Awake()
        {
            if (mission == null)
            {
                Debug.LogError("AncestralCore: falta asignar 'Mission' (MissionManager).", this);
                enabled = false;
                return;
            }

            if (coreRenderer != null)
            {
                coreMaterial = coreRenderer.material; // instancia propia de este renderer
                coreMaterial.EnableKeyword("_EMISSION");
            }
            else
            {
                Debug.LogWarning("AncestralCore: no hay 'Core Renderer'; el núcleo no cambiará de brillo.", this);
            }
        }

        private void OnEnable()
        {
            if (mission == null) return;

            mission.FragmentCollected += HandleProgress;
            mission.MissionCompleted += HandleMissionCompleted;
        }

        private void OnDisable()
        {
            if (mission == null) return;

            mission.FragmentCollected -= HandleProgress;
            mission.MissionCompleted -= HandleMissionCompleted;
        }

        private void Start()
        {
            RefreshTargets();
            currentEmission = targetEmission;
            currentLight = targetLight;
            ApplyVisuals();
        }

        private void Update()
        {
            float t = 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime);
            currentEmission = Color.Lerp(currentEmission, targetEmission, t);
            currentLight = Mathf.Lerp(currentLight, targetLight, t);
            ApplyVisuals();
        }

        private void OnDestroy()
        {
            if (coreMaterial != null) Destroy(coreMaterial);
        }

        public override bool CanInteract(GameObject interactor)
        {
            return mission != null && !mission.IsCompleted;
        }

        public override void Interact(GameObject interactor)
        {
            if (!mission.AllFragmentsCollected) return;

            mission.CompleteMission();
        }

        private void HandleProgress(int collected, int total) => RefreshTargets();

        private void HandleMissionCompleted() => RefreshTargets();

        private void RefreshTargets()
        {
            if (mission.IsCompleted)
            {
                targetEmission = completedEmission;
                targetLight = completedLight;
            }
            else if (mission.AllFragmentsCollected)
            {
                targetEmission = readyEmission;
                targetLight = readyLight;
            }
            else
            {
                targetEmission = inactiveEmission;
                targetLight = inactiveLight;
            }
        }

        private void ApplyVisuals()
        {
            if (coreMaterial != null) coreMaterial.SetColor(EmissionColorId, currentEmission);
            if (coreLight != null) coreLight.intensity = currentLight;
        }
    }
}