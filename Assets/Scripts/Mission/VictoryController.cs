using System.Collections;
using Echo.CameraSystem;
using Echo.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Echo.Mission
{
    /// <summary>
    /// Reacciona a la victoria: bloquea al jugador, libera el cursor, muestra el panel
    /// "Santuario restaurado" y permite reiniciar el nivel (recarga la misma escena).
    /// </summary>
    public class VictoryController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private MissionManager mission;
        [SerializeField] private PlayerController player;
        [SerializeField] private ThirdPersonCamera cameraRig;

        [Header("Interfaz de victoria")]
        [Tooltip("Panel completo (debe empezar desactivado).")]
        [SerializeField] private GameObject victoryPanel;
        [Tooltip("CanvasGroup del panel, para el fundido de entrada.")]
        [SerializeField] private CanvasGroup victoryGroup;
        [SerializeField] private Button restartButton;
        [Tooltip("Espera antes de mostrar el panel, para ver el núcleo activarse.")]
        [SerializeField] private float panelDelaySeconds = 1.5f;
        [SerializeField] private float fadeInSeconds = 0.8f;

        private bool restarting;

        private void Awake()
        {
            if (mission == null || player == null || cameraRig == null ||
                victoryPanel == null || victoryGroup == null || restartButton == null)
            {
                Debug.LogError("VictoryController: faltan referencias en el Inspector " +
                               "(Mission, Player, Camera Rig, Victory Panel, Victory Group, Restart Button).", this);
                enabled = false;
                return;
            }

            victoryPanel.SetActive(false);
        }

        private void OnEnable()
        {
            mission.MissionCompleted += HandleMissionCompleted;
            restartButton.onClick.AddListener(RestartLevel);
        }

        private void OnDisable()
        {
            if (mission != null) mission.MissionCompleted -= HandleMissionCompleted;
            if (restartButton != null) restartButton.onClick.RemoveListener(RestartLevel);
        }

        private void HandleMissionCompleted()
        {
            player.SetControlEnabled(false);
            StartCoroutine(ShowVictoryRoutine());
        }

        private IEnumerator ShowVictoryRoutine()
        {
            yield return new WaitForSeconds(panelDelaySeconds);

            cameraRig.SetInputEnabled(false);

            victoryGroup.alpha = 0f;
            victoryGroup.interactable = true;
            victoryGroup.blocksRaycasts = true;
            victoryPanel.SetActive(true);

            float elapsed = 0f;
            while (elapsed < fadeInSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                victoryGroup.alpha = Mathf.Clamp01(elapsed / fadeInSeconds);
                yield return null;
            }

            victoryGroup.alpha = 1f;
        }

        /// <summary>Recarga la escena activa desde cero. No se carga ninguna otra escena.</summary>
        public void RestartLevel()
        {
            if (restarting) return;

            int index = SceneManager.GetActiveScene().buildIndex;
            if (index < 0)
            {
                Debug.LogError("VictoryController: la escena actual no está en Build Profiles > Scene List. " +
                               "Agrégala para poder reiniciar.", this);
                return;
            }

            restarting = true;
            restartButton.interactable = false;
            SceneManager.LoadScene(index);
        }
    }
}