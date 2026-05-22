using UnityEngine;

namespace Elyqara.Game
{
    public enum PanelId { Inventory, Status, Skill }
    public enum SlideFrom { Left, Top, Right }

    // 토글 패널 1개. 화면 밖(slideFrom 방향) ↔ 제자리 슬라이드 + 종이 펼침/접힘 애니 동시.
    // ★ Update 구동 (코루틴 X) — stuck state 불가.
    // 슬라이드 = 2D Elyqara: offset ±700(X)/+1200(Y), 열기 EaseOutBack(오버슈트) / 닫기 EaseInCubic, 0.6초.
    public sealed class UIPanel : MonoBehaviour
    {
        private const float SlideOffsetX = 700f;
        private const float SlideOffsetY = 1200f;
        private const float SlideDuration = 0.6f;

        [SerializeField] private PanelId id;
        [SerializeField] private SlideFrom slideFrom;
        [SerializeField] private AnimatedPanelUI paper;

        public PanelId Id => id;
        public bool IsOpen { get; private set; }

        private RectTransform _rt;
        private Vector2 _homePos;
        private Vector2 _offPos;
        private bool _sliding;
        private bool _slideIn;
        private float _slideTime;

        private void Awake()
        {
            _rt = (RectTransform)transform;
            _homePos = _rt.anchoredPosition;
            _offPos = _homePos + OffsetVector();
            _rt.anchoredPosition = _offPos;   // 시작 = 화면 밖 (닫힘)
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            if (paper != null) paper.Open();
            _slideIn = true;
            _slideTime = 0f;
            _sliding = true;
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (paper != null) paper.Close();
            _slideIn = false;
            _slideTime = 0f;
            _sliding = true;
        }

        private void Update()
        {
            if (!_sliding) return;

            _slideTime += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(_slideTime / SlideDuration);
            Vector2 from = _slideIn ? _offPos : _homePos;
            Vector2 to = _slideIn ? _homePos : _offPos;
            float eased = _slideIn ? EaseOutBack(k) : EaseInCubic(k);
            _rt.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);

            if (k >= 1f)
            {
                _rt.anchoredPosition = to;
                _sliding = false;
            }
        }

        // slideFrom 방향의 화면 밖 오프셋 (제자리 기준 상대)
        private Vector2 OffsetVector()
        {
            switch (slideFrom)
            {
                case SlideFrom.Left:  return new Vector2(-SlideOffsetX, 0f);
                case SlideFrom.Right: return new Vector2( SlideOffsetX, 0f);
                case SlideFrom.Top:   return new Vector2(0f, SlideOffsetY);
                default:              return Vector2.zero;
            }
        }

        // 오버슈트 이징 — 도착지를 살짝 넘었다 돌아옴 (2D Elyqara 동일)
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseInCubic(float t) => t * t * t;
    }
}
