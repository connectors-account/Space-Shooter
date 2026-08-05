using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Utilities;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Small helper for building legacy-uGUI elements at runtime, used by the
    /// menu controllers so scenes are playable without hand-wiring a canvas.
    /// </summary>
    public static class UIFactory
    {
        public static Font DefaultFont()
        {
            var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return f;
        }

        public static Canvas CreateCanvas(string name, Transform parent, int sortingOrder = 50)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static Image CreatePanel(Transform parent, Color colour)
        {
            var go = new GameObject("Panel");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = SpriteGenerator.CreateSquareSprite();
            img.color = colour;
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return img;
        }

        public static Text CreateText(Transform parent, string content, int fontSize,
            Vector2 anchoredPos, Vector2 size, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = DefaultFont();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var rt = text.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            return text;
        }

        public static Button CreateButton(Transform parent, string label, Vector2 anchoredPos, Vector2 size,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Button_" + label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = SpriteGenerator.CreateSquareSprite();
            img.color = new Color(0.2f, 0.4f, 0.7f, 0.95f);
            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var button = go.AddComponent<Button>();
            var colours = button.colors;
            colours.normalColor = new Color(0.2f, 0.4f, 0.7f, 0.95f);
            colours.highlightedColor = new Color(0.3f, 0.55f, 0.9f, 1f);
            colours.pressedColor = new Color(0.15f, 0.3f, 0.55f, 1f);
            colours.selectedColor = colours.highlightedColor;
            button.colors = colours;
            if (onClick != null) button.onClick.AddListener(onClick);

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<Text>();
            text.font = DefaultFont();
            text.text = label;
            text.fontSize = 30;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            var trt = text.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            return button;
        }

        public static Slider CreateLabelledSlider(Transform parent, string label, Vector2 anchoredPos,
            float value, UnityEngine.Events.UnityAction<float> onChanged)
        {
            var container = new GameObject("Slider_" + label);
            container.transform.SetParent(parent, false);
            var crt = container.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0.5f);
            crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(420f, 60f);
            crt.anchoredPosition = anchoredPos;

            var labelText = CreateText(container.transform, label, 22, new Vector2(-150f, 0f), new Vector2(120f, 40f), TextAnchor.MiddleRight);

            var sliderGo = new GameObject("Slider");
            sliderGo.transform.SetParent(container.transform, false);
            var srt = sliderGo.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(240f, 18f);
            srt.anchoredPosition = new Vector2(80f, 0f);

            var slider = sliderGo.AddComponent<Slider>();

            var bg = new GameObject("Background");
            bg.transform.SetParent(sliderGo.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.sprite = SpriteGenerator.CreateSquareSprite();
            bgImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            var bgRt = bgImg.rectTransform;
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            var faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one; faRt.offsetMin = Vector2.zero; faRt.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.AddComponent<Image>();
            fillImg.sprite = SpriteGenerator.CreateSquareSprite();
            fillImg.color = new Color(0.3f, 0.7f, 1f, 1f);
            var fillRt = fillImg.rectTransform;
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;

            var handle = new GameObject("Handle");
            handle.transform.SetParent(sliderGo.transform, false);
            var handleImg = handle.AddComponent<Image>();
            handleImg.sprite = SpriteGenerator.CreateSquareSprite();
            handleImg.color = Color.white;
            var handleRt = handleImg.rectTransform;
            handleRt.sizeDelta = new Vector2(16f, 26f);

            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            if (onChanged != null) slider.onValueChanged.AddListener(onChanged);

            return slider;
        }
    }
}
