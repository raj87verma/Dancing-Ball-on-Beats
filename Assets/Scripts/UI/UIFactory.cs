using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DancingBallOnBeats.UI
{
    /// <summary>
    /// Small helper library of static methods for building uGUI elements at runtime (Canvas,
    /// buttons, text, panels). Every screen in this project (MainMenu, SongSelect, Settings, HUD,
    /// GameOver) is assembled purely in C# using these helpers instead of hand-authored prefabs,
    /// which keeps the project readable and avoids fragile hand-edited .prefab/.unity YAML.
    /// </summary>
    public static class UIFactory
    {
        public static Canvas CreateCanvas(string name, out GameObject rootObject)
        {
            rootObject = new GameObject(name, typeof(RectTransform));
            var canvas = rootObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = rootObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            rootObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        public static TextMeshProUGUI CreateText(
            Transform parent,
            string name,
            string content,
            int fontSize,
            Color color,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin = default,
            Vector2 offsetMax = default)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateButton(
            Transform parent,
            string name,
            string label,
            Color backgroundColor,
            Color textColor,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin = default,
            Vector2 offsetMax = default,
            int fontSize = 42)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var image = go.AddComponent<Image>();
            image.color = backgroundColor;

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = backgroundColor * 1.15f;
            colors.pressedColor = backgroundColor * 0.85f;
            button.colors = colors;

            CreateText(
                go.transform, "Label", label, fontSize, textColor, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one);

            return button;
        }

        public static Slider CreateSlider(
            Transform parent,
            string name,
            float value01,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var slider = go.AddComponent<Slider>();

            var background = new GameObject("Background", typeof(RectTransform));
            background.transform.SetParent(go.transform, false);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.25f);
            bgRect.anchorMax = new Vector2(1, 0.75f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(1f, 1f, 1f, 0.15f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.5f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.8f, 1f);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = Vector2.zero;
            handleAreaRect.offsetMax = Vector2.zero;

            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(30, 0);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value01;

            return slider;
        }
    }
}
