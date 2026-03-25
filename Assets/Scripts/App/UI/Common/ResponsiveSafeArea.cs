using UnityEngine;

namespace SubMonitor.App.UI.Common
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class ResponsiveSafeArea : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private int _lastScreenWidth = -1;
        private int _lastScreenHeight = -1;
        private bool _isApplying;

        private void Awake()
        {
            EnsureRectTransform();
            Apply();
        }

        private void OnEnable()
        {
            EnsureRectTransform();
            Apply();
        }

        private void Update()
        {
            if (HasScreenChanged())
            {
                Apply();
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (Application.isPlaying)
            {
                Apply();
            }
        }

        private void EnsureRectTransform()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
        }

        private bool HasScreenChanged()
        {
            return _lastScreenWidth != Screen.width ||
                   _lastScreenHeight != Screen.height ||
                   _lastSafeArea != Screen.safeArea;
        }

        private void Apply()
        {
            if (_isApplying)
            {
                return;
            }

            EnsureRectTransform();

            if (_rectTransform == null)
            {
                return;
            }

            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            _isApplying = true;
            try
            {
                Rect safeArea = Screen.safeArea;
                Vector2 min = safeArea.position;
                Vector2 max = safeArea.position + safeArea.size;
                min.x /= Screen.width;
                min.y /= Screen.height;
                max.x /= Screen.width;
                max.y /= Screen.height;

                if (_rectTransform.anchorMin != min)
                {
                    _rectTransform.anchorMin = min;
                }

                if (_rectTransform.anchorMax != max)
                {
                    _rectTransform.anchorMax = max;
                }

                if (_rectTransform.offsetMin != Vector2.zero)
                {
                    _rectTransform.offsetMin = Vector2.zero;
                }

                if (_rectTransform.offsetMax != Vector2.zero)
                {
                    _rectTransform.offsetMax = Vector2.zero;
                }

                _lastScreenWidth = Screen.width;
                _lastScreenHeight = Screen.height;
                _lastSafeArea = Screen.safeArea;
            }
            finally
            {
                _isApplying = false;
            }
        }
    }
}
