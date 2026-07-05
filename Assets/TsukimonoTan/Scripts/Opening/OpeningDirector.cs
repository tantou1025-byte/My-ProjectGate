using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TsukimonoTan.Opening
{
    /// <summary>
    /// オープニング演出「白符に墨が滲んで文字が浮かび上がり、やがてタイトルへ」のシーケンス制御。
    /// UIは全てコードで生成するため、シーンにはこのコンポーネントを持つGameObjectが1つあれば動く。
    ///
    /// タイムライン（約30秒）
    ///   0-2s   完全な黒。静寂
    ///   2-5s   白符フェードイン（わずかに揺れる）
    ///   5-10s  符の中心から墨パーティクル拡散
    ///   10-20s ナレーション6行が順に墨書き風フェードイン
    ///   20-25s 「憑物譚」タイトルが符の上に浮かび上がる
    ///   25-30s 霧のようにフェードアウト → TitleScene へ遷移
    /// </summary>
    public class OpeningDirector : MonoBehaviour
    {
        [Header("遷移先シーン名")]
        [SerializeField] private string titleSceneName = "TitleScene";

        [Header("日本語フォント（FontSetupToolで生成したNoto Sans JPを割り当て。未設定ならResources/Fonts/NotoSansJP SDFを探す）")]
        [SerializeField] private TMP_FontAsset japaneseFont;

        [Header("クリック/キー入力でスキップ可能にする")]
        [SerializeField] private bool skippable = true;

        private static readonly string[] NarrationLines =
        {
            "──想いは、死なない。",
            "人が死んでも、",
            "忘れても、",
            "想いだけが夜に残る。",
            "それを郷のひとは、こう呼んだ。",
            "憑物、と。",
        };

        // 和風ダークファンタジーの確定パレット（深紫黒・金・紅）
        private static readonly Color BgColor    = new Color32(0x0a, 0x0e, 0x27, 0xff);
        private static readonly Color Crimson    = new Color32(0xc4, 0x1e, 0x3a, 0xff);
        private static readonly Color WashiWhite = new Color32(0xf5, 0xf0, 0xe6, 0xff);
        private static readonly Color FogColor   = new Color32(0xc9, 0xc4, 0xd4, 0x00);

        private CanvasGroup _talisman;
        private RectTransform _talismanRect;
        private OpeningInkParticles _ink;
        private TextMeshProUGUI[] _narration;
        private TextMeshProUGUI _title;
        private Image _fog;
        private Image _blackout;
        private bool _transitionStarted;
        private float _swayTime;

        private void Awake()
        {
            if (japaneseFont == null)
                japaneseFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP SDF");
            if (japaneseFont == null)
                Debug.LogWarning("[Opening] 日本語フォント未設定。TMPデフォルトでは日本語が□になります。OpeningDirectorにフォントを割り当ててください。");
            BuildUI();
        }

        private void Start() => StartCoroutine(Run());

        private void Update()
        {
            // 白符のゆらぎ（表示中のみ）
            if (!_transitionStarted && _talisman.alpha > 0.01f)
            {
                _swayTime += Time.deltaTime;
                _talismanRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(_swayTime * 0.8f) * 1.2f);
                _talismanRect.anchoredPosition = new Vector2(0f, Mathf.Sin(_swayTime * 0.5f) * 6f);
            }

            if (skippable && !_transitionStarted && SkipPressed())
            {
                StopAllCoroutines();
                StartCoroutine(Transition(fast: true));
            }
        }

        private static bool SkipPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
#else
            return Input.anyKeyDown;
#endif
        }

        private IEnumerator Run()
        {
            yield return new WaitForSeconds(2f);                    // 0-2s  完全な黒・静寂

            yield return FadeGroup(_talisman, 0f, 1f, 3f);          // 2-5s  白符フェードイン

            _ink.Play(5f);                                          // 5-10s 墨パーティクル拡散
            yield return new WaitForSeconds(5f);

            // 紙を背景へ退かせて本文の可読性を確保する
            yield return FadeGroup(_talisman, 1f, 0.25f, 1f);
            for (int i = 0; i < _narration.Length; i++)             // 10-20s ナレーション
            {
                StartCoroutine(FadeInText(_narration[i], 1.2f));
                yield return new WaitForSeconds(1.4f);
            }
            yield return new WaitForSeconds(1.2f);

            foreach (var line in _narration)                        // 20-25s 本文退場→タイトル
                StartCoroutine(FadeOutText(line, 0.8f));
            yield return FadeGroup(_talisman, 0.25f, 1f, 1f);
            yield return FadeInText(_title, 2.2f);
            yield return new WaitForSeconds(1.8f);

            yield return Transition(fast: false);                   // 25-30s 霧フェード→遷移
        }

        private IEnumerator Transition(bool fast)
        {
            _transitionStarted = true;

            yield return FadeImageAlpha(_fog, 0f, 0.55f, fast ? 0.6f : 2.5f);
            yield return FadeImageAlpha(_blackout, 0f, 1f, fast ? 0.4f : 1.5f);

            if (Application.CanStreamedLevelBeLoaded(titleSceneName))
                SceneManager.LoadScene(titleSceneName);
            else
                Debug.LogWarning($"[Opening] シーン'{titleSceneName}'がBuild Settingsに無いため遷移をスキップしました。");
        }

        // ---- UI構築 ----

        private void BuildUI()
        {
            var canvasGo = new GameObject("OpeningCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var root = (RectTransform)canvasGo.transform;

            CreateImage(root, "Background", BgColor, stretch: true);

            // 白符（中央の縦長の札）
            var talismanImg = CreateImage(root, "Talisman", WashiWhite, stretch: false);
            _talismanRect = talismanImg.rectTransform;
            _talismanRect.sizeDelta = new Vector2(300f, 640f);
            _talisman = talismanImg.gameObject.AddComponent<CanvasGroup>();
            _talisman.alpha = 0f;

            // 契約印（札の下部の紅い菱形）
            var seal = CreateImage(_talismanRect, "Seal", Crimson, stretch: false);
            seal.rectTransform.sizeDelta = new Vector2(36f, 36f);
            seal.rectTransform.anchoredPosition = new Vector2(0f, -240f);
            seal.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            // 墨パーティクル（符の中心=画面中央から拡散。札より前・文字より後ろ）
            var inkGo = new GameObject("InkParticles", typeof(RectTransform));
            inkGo.transform.SetParent(root, false);
            _ink = inkGo.AddComponent<OpeningInkParticles>();

            // ナレーション（画面中央に縦積み）
            _narration = new TextMeshProUGUI[NarrationLines.Length];
            const float topY = 190f;
            const float lineHeight = 76f;
            for (int i = 0; i < NarrationLines.Length; i++)
            {
                var t = CreateText(root, $"Narration{i}", NarrationLines[i], 40f, WashiWhite);
                t.rectTransform.anchoredPosition = new Vector2(0f, topY - i * lineHeight);
                _narration[i] = t;
            }

            // タイトル「憑物譚」— 御札の墨書きに見えるよう縦書き（1文字ずつ改行）
            _title = CreateText(root, "Title", "憑\n物\n譚", 120f, Crimson);
            _title.lineSpacing = -20f;
            _title.fontStyle = FontStyles.Bold;

            // 霧・暗転オーバーレイ（最前面）
            _fog = CreateImage(root, "Fog", FogColor, stretch: true);
            _blackout = CreateImage(root, "Blackout", new Color(0f, 0f, 0f, 0f), stretch: true);
        }

        private static Image CreateImage(RectTransform parent, string name, Color color, bool stretch)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            if (stretch)
            {
                var rect = img.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            return img;
        }

        private TextMeshProUGUI CreateText(RectTransform parent, string name, string content, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            if (japaneseFont != null) t.font = japaneseFont;
            t.text = content;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.alpha = 0f;
            t.rectTransform.sizeDelta = new Vector2(1600f, size * 6f);
            return t;
        }

        // ---- フェード補助 ----

        private static IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
        {
            for (float e = 0f; e < duration; e += Time.deltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, Smooth(e / duration));
                yield return null;
            }
            group.alpha = to;
        }

        private static IEnumerator FadeImageAlpha(Image img, float from, float to, float duration)
        {
            var c = img.color;
            for (float e = 0f; e < duration; e += Time.deltaTime)
            {
                c.a = Mathf.Lerp(from, to, Smooth(e / duration));
                img.color = c;
                yield return null;
            }
            c.a = to;
            img.color = c;
        }

        // 墨書き風：わずかに下から浮かび上がりつつ滲むように現れる
        private static IEnumerator FadeInText(TMP_Text text, float duration)
        {
            var rect = text.rectTransform;
            Vector2 basePos = rect.anchoredPosition;
            Vector2 startPos = basePos + new Vector2(0f, -10f);
            for (float e = 0f; e < duration; e += Time.deltaTime)
            {
                float k = Smooth(e / duration);
                text.alpha = k;
                rect.anchoredPosition = Vector2.Lerp(startPos, basePos, k);
                yield return null;
            }
            text.alpha = 1f;
            rect.anchoredPosition = basePos;
        }

        private static IEnumerator FadeOutText(TMP_Text text, float duration)
        {
            float from = text.alpha;
            for (float e = 0f; e < duration; e += Time.deltaTime)
            {
                text.alpha = Mathf.Lerp(from, 0f, e / duration);
                yield return null;
            }
            text.alpha = 0f;
        }

        private static float Smooth(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }
    }
}
