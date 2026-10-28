using UnityEngine;

namespace Echo.World
{
    /// <summary>
    /// Partículas ambientales que solo se ven de noche: controla el alpha del sistema con
    /// NightFactor y desactiva su renderer de día (sistema de luciérnagas de la región A).
    /// No crea ni mueve nada: solo gestiona la visibilidad del propio renderer.
    /// </summary>
    [DisallowMultipleComponent]
    public class AmbientParticles : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private TimeOfDayController timeOfDay;

        [Header("Visibilidad")]
        [Tooltip("NightFactor mínimo para encender las partículas.")]
        [SerializeField, Range(0f, 1f)] private float minNightFactor = 0.05f;
        [Tooltip("NightFactor al que las partículas alcanzan su brillo máximo.")]
        [SerializeField, Range(0f, 1f)] private float fullNightFactor = 0.5f;
        [Tooltip("Opacidad de las partículas en plena noche.")]
        [SerializeField, Range(0f, 1f)] private float baseAlpha = 0.85f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private ParticleSystemRenderer particleRenderer;
        private MaterialPropertyBlock block;

        private void Awake()
        {
            particleRenderer = GetComponent<ParticleSystemRenderer>();
            if (particleRenderer == null)
            {
                Debug.LogError("AmbientParticles: este objeto necesita un ParticleSystemRenderer.", this);
                enabled = false;
                return;
            }

            if (timeOfDay == null)
            {
                Debug.LogError("AmbientParticles: falta asignar 'Time Of Day' en el Inspector.", this);
                enabled = false;
                return;
            }

            block = new MaterialPropertyBlock();
        }

        private void LateUpdate()
        {
            if (particleRenderer == null || timeOfDay == null)
            {
                return;
            }

            float nightFactor = timeOfDay.NightFactor;
            bool visible = nightFactor >= minNightFactor;
            if (particleRenderer.enabled != visible)
            {
                particleRenderer.enabled = visible;
            }

            if (!visible)
            {
                return;
            }

            float alpha = fullNightFactor > minNightFactor
                ? Mathf.Clamp01((nightFactor - minNightFactor) / (fullNightFactor - minNightFactor))
                : 1f;

            block.SetColor(BaseColorId, new Color(1f, 1f, 1f, alpha * baseAlpha));
            particleRenderer.SetPropertyBlock(block);
        }
    }
}
