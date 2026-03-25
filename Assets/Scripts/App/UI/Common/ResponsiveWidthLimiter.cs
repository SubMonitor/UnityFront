using UnityEngine;

namespace SubMonitor.App.UI.Common
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class ResponsiveWidthLimiter : MonoBehaviour
    {
        [SerializeField] private float maxContentWidth = 1440f;

        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;
        private bool _baselineCaptured;
        private Vector2 _baselineOffsetMin;
        private Vector2 _baselineOffsetMax;
        private Vector2 _lastParentSize = new Vector2(-1f, -1f);
        private int _lastScreenWidth = -1;
        private int _lastScreenHeight = -1;
        private bool _isApplying;

        public void Configure(float newMaxContentWidth)
        {
            maxContentWidth = Mathf.Max(0f, newMaxContentWidth);
            Apply();
        }

        [ContextMenu("Reset Baseline")]
        private void ResetBaseline()
        {
            _baselineCaptured = false;
            CaptureBaseline();
            Apply();
        }

        private void Awake()
        {
            EnsureReferences();
            CaptureBaseline();
            Apply();
        }

        private void OnEnable()
        {
            EnsureReferences();
            CaptureBaseline();
            Apply();
        }

        private void OnTransformParentChanged()
        {
            _parentRectTransform = null;
            _baselineCaptured = false;
            EnsureReferences();
            CaptureBaseline();
            Apply();
        }

        private void Update()
        {
            if (HasLayoutChanged())
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

        private void EnsureReferences()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (_parentRectTransform == null && _rectTransform != null)
            {
                _parentRectTransform = _rectTransform.parent as RectTransform;
            }
        }

        private void CaptureBaseline()
        {
            if (_baselineCaptured || _rectTransform == null)
            {
                return;
            }

            _baselineOffsetMin = _rectTransform.offsetMin;
            _baselineOffsetMax = _rectTransform.offsetMax;
            _baselineCaptured = true;
        }

        private bool HasLayoutChanged()
        {
            EnsureReferences();

            Vector2 parentSize = _parentRectTransform != null ? _parentRectTransform.rect.size : Vector2.zero;
            return parentSize != _lastParentSize ||
                   _lastScreenWidth != Screen.width ||
                   _lastScreenHeight != Screen.height;
        }

        private void Apply()
        {
            if (_isApplying)
            {
                return;
            }

            EnsureReferences();
            CaptureBaseline();

            if (_rectTransform == null || _parentRectTransform == null || !_baselineCaptured)
            {
                return;
            }

            if (!Mathf.Approximately(_rectTransform.anchorMin.x, 0f) || !Mathf.Approximately(_rectTransform.anchorMax.x, 1f))
            {
                return;
            }

            float parentWidth = _parentRectTransform.rect.width;
            if (parentWidth <= 0f || maxContentWidth <= 0f)
            {
                return;
            }

            float baseLeft = _baselineOffsetMin.x;
            float baseRight = -_baselineOffsetMax.x;
            float availableWidth = parentWidth - baseLeft - baseRight;
            float extraInset = Mathf.Max(0f, availableWidth - maxContentWidth) * 0.5f;
            Vector2 nextOffsetMin = new Vector2(baseLeft + extraInset, _baselineOffsetMin.y);
            Vector2 nextOffsetMax = new Vector2(-(baseRight + extraInset), _baselineOffsetMax.y);

            _isApplying = true;
            try
            {
                if (_rectTransform.offsetMin != nextOffsetMin)
                {
                    _rectTransform.offsetMin = nextOffsetMin;
                }

                if (_rectTransform.offsetMax != nextOffsetMax)
                {
                    _rectTransform.offsetMax = nextOffsetMax;
                }

                _lastParentSize = _parentRectTransform.rect.size;
                _lastScreenWidth = Screen.width;
                _lastScreenHeight = Screen.height;
            }
            finally
            {
                _isApplying = false;
            }
        }
    }
}
