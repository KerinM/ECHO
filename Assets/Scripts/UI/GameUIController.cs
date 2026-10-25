using Echo.Mission;
using TMPro;
using UnityEngine;

namespace Echo.UI
{
    /// <summary>
    /// Interfaz de progreso: contador de fragmentos, objetivo de la misión y
    /// confirmaciones breves. Va en HUD_Canvas. Solo escucha eventos de MissionManager.
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private MissionManager mission;
        [SerializeField] private TMP_Text counterText;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text toastText;

        [Header("Confirmación")]
        [SerializeField] private float toastSeconds = 3f;
        [SerializeField] private float toastFadeSeconds = 0.6f;

        private float toastTimer;

        private void Awake()
        {
            if (mission == null || counterText == null || objectiveText == null || toastText == null)
            {
                Debug.LogError("GameUIController: faltan referencias en el Inspector " +
                               "(Mission, Counter Text, Objective Text, Toast Text).", this);
                enabled = false;
                return;
            }

            toastText.text = string.Empty;
            toastText.alpha = 0f;
        }

        private void OnEnable()
        {
            mission.FragmentCollected += HandleFragmentCollected;
            mission.StageChanged += HandleStageChanged;
            mission.MissionCompleted += HandleMissionCompleted;

            RefreshCounter();
            RefreshObjective();
        }

        private void OnDisable()
        {
            if (mission == null) return;

            mission.FragmentCollected -= HandleFragmentCollected;
            mission.StageChanged -= HandleStageChanged;
            mission.MissionCompleted -= HandleMissionCompleted;
        }

        private void Update()
        {
            if (toastTimer <= 0f) return;

            toastTimer -= Time.unscaledDeltaTime;
            toastText.alpha = toastTimer >= toastFadeSeconds
                ? 1f
                : Mathf.Clamp01(toastTimer / toastFadeSeconds);

            if (toastTimer <= 0f) toastText.text = string.Empty;
        }

        private void HandleFragmentCollected(int collected, int total)
        {
            RefreshCounter();
            ShowToast($"Fragmento de energía recogido ({collected}/{total})");
        }

        private void HandleStageChanged(MissionStage stage)
        {
            RefreshObjective();

            // MissionManager avisa del fragmento y después de la etapa en el mismo fotograma,
            // así que esta línea se añade a la confirmación ya visible.
            if (stage == MissionStage.PortalAndFragments)
                AppendToast("El portal ancestral se ha despertado");
            else if (stage == MissionStage.ActivateCore)
                AppendToast("El núcleo ancestral está listo para despertar");
        }

        private void HandleMissionCompleted()
        {
            RefreshObjective();
            toastTimer = 0f;
            toastText.text = string.Empty;
            toastText.alpha = 0f;
        }

        private void RefreshCounter()
        {
            counterText.text = $"Fragmentos: {mission.CollectedCount}/{mission.TotalFragments}";
        }

        private void RefreshObjective()
        {
            objectiveText.text = $"Objetivo: {mission.StageDescription}";
        }

        private void ShowToast(string message)
        {
            toastText.text = message;
            toastText.alpha = 1f;
            toastTimer = toastSeconds + toastFadeSeconds;
        }

        private void AppendToast(string line)
        {
            if (toastTimer <= 0f)
            {
                ShowToast(line);
                return;
            }

            toastText.text += "\n" + line;
        }
    }
}