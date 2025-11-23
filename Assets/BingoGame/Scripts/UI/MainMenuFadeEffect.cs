using UnityEngine;
using System.Collections;

namespace BingoGame.Network
{
    // Handles fade-in effect for main menu canvas groups
    public class MainMenuFadeEffect : MonoBehaviour
    {
        [Header("Canvas Groups to Fade")]
        [SerializeField] private CanvasGroup canvasGroup1;
        [SerializeField] private CanvasGroup canvasGroup2;

        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private bool fadeOnStart = true;

        private bool hasPlayedInitialFade = false;

        private void Start()
        {
            if (fadeOnStart && !hasPlayedInitialFade)
            {
                FadeIn();
                hasPlayedInitialFade = true;
            }
            else
            {
                // Ensure canvas groups are visible
                if (canvasGroup1 != null)
                    canvasGroup1.alpha = 1f;
                if (canvasGroup2 != null)
                    canvasGroup2.alpha = 1f;
            }
        }

        public void FadeIn()
        {
            StopAllCoroutines();
            StartCoroutine(FadeInCoroutine());
        }

        private IEnumerator FadeInCoroutine()
        {
            // Set initial alpha to 0
            if (canvasGroup1 != null)
                canvasGroup1.alpha = 0f;
            if (canvasGroup2 != null)
                canvasGroup2.alpha = 0f;

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);

                if (canvasGroup1 != null)
                    canvasGroup1.alpha = alpha;
                if (canvasGroup2 != null)
                    canvasGroup2.alpha = alpha;

                yield return null;
            }

            // Ensure final alpha is exactly 1
            if (canvasGroup1 != null)
                canvasGroup1.alpha = 1f;
            if (canvasGroup2 != null)
                canvasGroup2.alpha = 1f;
        }
    }
}
