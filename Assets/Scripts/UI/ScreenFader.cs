using System.Collections;
using UnityEngine;

namespace Echo.UI
{
    /// <summary>
    /// Fundido a negro. Va en un panel negro a pantalla completa con CanvasGroup.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        private CanvasGroup group;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        /// <summary>Lleva la opacidad a 'targetAlpha' (0 = transparente, 1 = negro) en 'duration' segundos.</summary>
        public IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float start = group.alpha;

            if (duration <= 0f)
            {
                group.alpha = targetAlpha;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            group.alpha = targetAlpha;
        }
    }
}