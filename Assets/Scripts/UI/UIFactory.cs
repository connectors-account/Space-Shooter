using System;
using SpaceShooter.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Small helper utility for constructing Unity UI elements (text, images, panels and buttons)
    /// in code. Keeps <see cref="UIManager"/> and the main-menu bootstrap concise and consistent.
    /// </summary>
    public static class UIFactory
    {
        /// <summary>
        /// Ensures exactly one <see cref="EventSystem"/> exists in the scene so UI buttons receive input.
        /// </summary>
        public static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        /// <summary>
        /// Creates a <see cref="Text"/> element.
        /// </summary>
        public static Text CreateText(Transform parent, Font font, string content, int fontSize, TextAnchor alignment,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            var rt = text.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPosition;
            return text;
        }

        /// <summary>
        /// Creates an <see cref="Image"/> element backed by a generated white sprite (tinted by colour).
        /// </summary>
        public static Image CreateImage(Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPosition, Vector2 size, TextAnchor pivotHint)
        {
            var go = new GameObject("Image");
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.sprite = SpriteFactory.CreateWhitePixel();
            image.color = color;
            image.type = Image.Type.Simple;

            var rt = image.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = PivotFor(pivotHint);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPosition;
            return image;
        }

        private static Vector2 PivotFor(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.LowerLeft: return new Vector2(0f, 0f);
                case TextAnchor.MiddleLeft: return new Vector2(0f, 0.5f);
                case TextAnchor.UpperLeft: return new Vector2(0f, 1f);
                case TextAnchor.LowerRight: return new Vector2(1f, 0f);
                case TextAnchor.UpperRight: return new Vector2(1f, 1f);
                default: return new Vector2(0.5f, 0.5f);
            }
        }

        /// <summary>
        /// Creates a full-screen panel that stretches to fill its parent canvas.
        /// </summary>
        public static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.sprite = SpriteFactory.CreateWhitePixel();
            image.color = color;
            image.raycastTarget = true;

            var rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        /// <summary>
        /// Creates a clickable button with a label, centred horizontally within its parent.
        /// </summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="font">Font for the label.</param>
        /// <param name="label">Button caption.</param>
        /// <param name="anchoredPosition">Position relative to the parent centre.</param>
        /// <param name="onClick">Callback invoked on click.</param>
        public static Button CreateButton(Transform parent, Font font, string label, Vector2 anchoredPosition, Action onClick)
        {
            var go = new GameObject($"Button_{label}");
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.sprite = SpriteFactory.CreateWhitePixel();
            image.color = new Color(0.2f, 0.45f, 0.8f, 1f);

            var rt = image.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(360f, 90f);
            rt.anchoredPosition = anchoredPosition;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.45f, 0.8f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.6f, 1f, 1f);
            colors.pressedColor = new Color(0.15f, 0.35f, 0.65f, 1f);
            colors.selectedColor = new Color(0.25f, 0.5f, 0.9f, 1f);
            button.colors = colors;
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            Text text = CreateText(go.transform, font, label, 34, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            return button;
        }
    }
}
