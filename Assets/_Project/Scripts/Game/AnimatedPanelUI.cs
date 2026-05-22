using UnityEngine;
using UnityEngine.UI;

namespace Elyqara.Game
{
    // 종이 펼침/접힘 프레임 애니 오버레이. 2D Elyqara AnimatedPanelUI 패턴.
    // ★ Update 구동 (코루틴 X) — 코루틴은 GameObject 비활성/StopCoroutine 에 취약해
    //   stuck state (Play 가 끝까지 안 돌아 isAnimating 고착) 발생. 타이머+bool 은 그게 불가능.
    // 열기 = frame 1→N (종이가 가운데부터 사라지며 내용 노출), 닫기 = N→1 (종이 다시 덮임).
    [RequireComponent(typeof(Image))]
    public sealed class AnimatedPanelUI : MonoBehaviour
    {
        private const float Duration = 0.6f;   // 36프레임 / 60fps

        [SerializeField] private string frameFolder = "UI/MiddleUi";

        private Sprite[] _frames;
        private Image _image;
        private bool _playing;
        private bool _forward;
        private float _time;

        public bool IsAnimating => _playing;
        public event System.Action OnOpenComplete;
        public event System.Action OnCloseComplete;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.raycastTarget = false;
            _image.color = Color.white;
            _image.enabled = false;
            LoadFrames();
        }

        // 종이 펼침 (frame 1→N).
        public void Open() => StartAnim(true);

        // 종이 접힘 (frame N→1).
        public void Close() => StartAnim(false);

        private void StartAnim(bool forward)
        {
            if (_frames == null || _frames.Length == 0) return;
            _forward = forward;
            _time = 0f;
            _playing = true;
            _image.enabled = true;
            _image.sprite = forward ? _frames[0] : _frames[_frames.Length - 1];
        }

        private void Update()
        {
            if (!_playing) return;

            _time += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(_time / Duration);

            int last = _frames.Length - 1;
            int step = Mathf.Clamp(Mathf.RoundToInt(p * last), 0, last);
            int idx = _forward ? step : last - step;
            if (_frames[idx] != null) _image.sprite = _frames[idx];

            if (p >= 1f)
            {
                _playing = false;
                _image.enabled = false;   // 애니 끝 = 오버레이 숨김
                if (_forward) OnOpenComplete?.Invoke();
                else OnCloseComplete?.Invoke();
            }
        }

        private void LoadFrames()
        {
            var loaded = Resources.LoadAll<Sprite>(frameFolder);
            if (loaded == null || loaded.Length == 0)
            {
                _frames = System.Array.Empty<Sprite>();
                Debug.LogWarning($"[AnimatedPanelUI] Resources/{frameFolder} 에 스프라이트 없음");
                return;
            }

            int max = 0;
            foreach (var s in loaded)
            {
                int n = ParseFrameNumber(s.name);
                if (n > max) max = n;
            }
            _frames = new Sprite[max];
            foreach (var s in loaded)
            {
                int n = ParseFrameNumber(s.name);
                if (n >= 1 && n <= max) _frames[n - 1] = s;
            }
        }

        // "1" → 1, "1_0" → 1
        private static int ParseFrameNumber(string spriteName)
        {
            if (int.TryParse(spriteName, out int direct)) return direct;
            int u = spriteName.IndexOf('_');
            if (u > 0 && int.TryParse(spriteName.Substring(0, u), out int prefix)) return prefix;
            return 0;
        }
    }
}
