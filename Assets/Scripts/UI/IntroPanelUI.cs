using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Echo.UI
{
    /// <summary>
    /// Panel de título e instrucciones. Se muestra al comenzar sin bloquear al jugador,
    /// se desvanece solo y se puede mostrar u ocultar con H. Va en HUD_Canvas.
    /// </summary>
    public class IntroPanelUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup panel;
        [Tooltip("Texto pequeño de créditos (opcional). Lo crea Tools/ECHO/Setup Credits.")]
        [SerializeField] private TMP_Text creditsText;
        [SerializeField] private float visibleSeconds = 8f;
        [SerializeField] private float fadeSeconds = 1f;

        private bool shown = true;
        private float hideAt;

        private void Awake()
        {
            if (panel == null)
            {
                Debug.LogError("IntroPanelUI: falta asignar 'Panel' (el CanvasGroup de IntroPanel).", this);
                enabled = false;
                return;
            }

            if (creditsText == null)
            {
                Debug.LogWarning("IntroPanelUI: sin 'Credits Text' asignado; no se mostrarán los créditos. " +
                                 "Ejecuta Tools/ECHO/Setup Credits.", this);
            }

            panel.alpha = 1f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }

        private void Start()
        {
            hideAt = Time.unscaledTime + visibleSeconds;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
            {
                shown = !shown;
                // Si se abre con H, se queda visible hasta que se vuelva a pulsar H.
                hideAt = shown ? float.PositiveInfinity : 0f;
            }

            if (shown && Time.unscaledTime >= hideAt) shown = false;

            float targetAlpha = shown ? 1f : 0f;
            panel.alpha = Mathf.MoveTowards(
                panel.alpha, targetAlpha, Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeSeconds));
        }
    }
}