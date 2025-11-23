using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace BingoGame.Network
{
    // Displays the target Bingo pattern (5x5 grid)
    // All players see the same pattern
    public class PatternDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform imagesParent;

        [Header("Colors")]
        [SerializeField] private Color requiredColor = Color.red;
        [SerializeField] private Color notRequiredColor = Color.gray;

        private Image[] patternCells = new Image[24];

        private void Awake()
        {
            GetImagesFromParent();
        }

        private void Start()
        {
            // Wait a bit for BingoManager to initialize
            Invoke(nameof(UpdatePatternDisplay), 0.5f);
        }

        private void GetImagesFromParent()
        {
            if (imagesParent == null)
            {
                Debug.LogError("[PatternDisplay] Images parent not assigned!");
                return;
            }

            // Get all Image components from children (excluding parent itself)
            Image[] allImages = imagesParent.GetComponentsInChildren<Image>();
            List<Image> childImages = new List<Image>();

            // Filter out the parent's own Image component if it has one
            foreach (Image img in allImages)
            {
                if (img.transform != imagesParent)
                {
                    childImages.Add(img);
                }
            }

            Image[] images = childImages.ToArray();

            if (images.Length != 24)
            {
                Debug.LogError($"[PatternDisplay] Expected 24 images, found {images.Length}! Make sure only child objects have Image components.");
                return;
            }

            // Assign images to array
            for (int i = 0; i < 24; i++)
            {
                patternCells[i] = images[i];
            }

        }

        public void UpdatePatternDisplay()
        {
            if (BingoManager.Instance == null)
            {
                Debug.LogWarning("[PatternDisplay] BingoManager not found!");
                Invoke(nameof(UpdatePatternDisplay), 0.5f);
                return;
            }

            BingoPattern pattern = BingoManager.Instance.CurrentPattern;
            if (pattern == null)
            {
                Debug.LogWarning("[PatternDisplay] No pattern selected!");
                return;
            }

            if (pattern.pattern == null || pattern.pattern.Length != 24)
            {
                Debug.LogError($"[PatternDisplay] Invalid pattern array! Expected 24, got {pattern.pattern?.Length}");
                return;
            }

            // Verify we have valid pattern cells
            if (patternCells == null || patternCells.Length != 24)
            {
                Debug.LogError("[PatternDisplay] Pattern cells array not properly initialized!");
                return;
            }

            // Update grid colors (24 cells)
            for (int i = 0; i < 24; i++)
            {
                if (patternCells[i] != null)
                {
                    patternCells[i].color = pattern.pattern[i] ? requiredColor : notRequiredColor;
                }
                else
                {
                    Debug.LogWarning($"[PatternDisplay] Pattern cell at index {i} is null!");
                }
            }
        }
    }
}
