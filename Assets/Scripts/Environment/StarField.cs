using UnityEngine;

namespace Echo.World
{
    /// <summary>
    /// Cascarón esférico de estrellas que sigue a la cámara principal. Su opacidad depende de
    /// NightFactor (invisible de día). El radio es corto a propósito: con la densidad de niebla
    /// nocturna (0.010) un cascarón lejano quedaría oculto, mientras que a ~70 m se ve tenue.
    /// El script no mueve estrellas: solo centra el emisor en la cámara y regula el alpha.
    /// </summary>
    [DisallowMultipleComponent]
    public class StarField : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private TimeOfDayController timeOfDay;

        [Header("Visibilidad")]
        [Tooltip("NightFactor mínimo para mostrar alguna estrella.")]
        [SerializeField, Range(0f, 1f)] private float minNightFactor = 0.05f;
        [Tooltip("NightFactor al que las estrellas alcanzan su brillo máximo.")]
        [SerializeField, Range(0f, 1f)] private float fullNightFactor = 0.6f;
        [Tooltip("Opacidad de las estrellas en plena noche.")]
        [SerializeField, Range(0f, 1f)] private float starAlpha = 0.95f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private Camera targetCamera;
        private ParticleSystemRenderer particleRenderer;
        private MaterialPropertyBlock block;

        private void Awake()
        {
            particleRenderer = GetComponent<ParticleSystemRenderer>();
            if (particleRenderer == null)
            {
                Debug.LogError("StarField: este objeto necesita un ParticleSystemRenderer.", this);
                enabled = false;
                return;
            }

            if (timeOfDay == null)
            {
                Debug.LogError("StarField: falta asignar 'Time Of Day' en el Inspector.", this);
                enabled = false;
                return;
            }

            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("StarField: no hay cámara con la etiqueta 'MainCamera' en la escena.", this);
                enabled = false;
                return;
            }

            block = new MaterialPropertyBlock();
        }

        private void LateUpdate()
        {
            if (targetCamera == null || particleRenderer == null || timeOfDay == null)
            {
                return;
            }

            transform.position = targetCamera.transform.position;

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

            block.SetColor(BaseColorId, new Color(1f, 1f, 1f, alpha * starAlpha));
            particleRenderer.SetPropertyBlock(block);
        }
    }
}
