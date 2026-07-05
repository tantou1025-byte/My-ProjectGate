using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TsukimonoTan.Opening
{
    /// <summary>
    /// 白符の中心から墨が滲み広がる演出。uGUIのImageベースの簡易パーティクル。
    /// ParticleSystemはScreen Space OverlayのCanvasと描画順が競合するため、
    /// Canvas階層内で完結するこの方式を採る。
    /// </summary>
    public class OpeningInkParticles : MonoBehaviour
    {
        [SerializeField] private float emissionPerSecond = 26f;
        [SerializeField] private Vector2 lifeRange = new Vector2(1.6f, 3.2f);
        [SerializeField] private Vector2 speedRange = new Vector2(30f, 170f);
        [SerializeField] private Vector2 sizeRange = new Vector2(10f, 48f);

        private static readonly Color InkColor = new Color32(0x14, 0x12, 0x1e, 0xff);

        private class Particle
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Velocity;
            public float Life;
            public float MaxLife;
            public float SpinSpeed;
        }

        private readonly List<Particle> _alive = new List<Particle>();
        private readonly Stack<Particle> _pool = new Stack<Particle>();
        private Sprite _inkSprite;
        private float _emitRemaining;
        private float _emitAccumulator;

        private void Awake()
        {
            _inkSprite = CreateInkSprite();
        }

        /// <summary>指定秒数のあいだ墨を放出する。</summary>
        public void Play(float duration) => _emitRemaining = duration;

        private void Update()
        {
            float dt = Time.deltaTime;

            if (_emitRemaining > 0f)
            {
                _emitRemaining -= dt;
                _emitAccumulator += emissionPerSecond * dt;
                while (_emitAccumulator >= 1f)
                {
                    _emitAccumulator -= 1f;
                    Emit();
                }
            }

            for (int i = _alive.Count - 1; i >= 0; i--)
            {
                var p = _alive[i];
                p.Life -= dt;
                if (p.Life <= 0f)
                {
                    p.Image.enabled = false;
                    _pool.Push(p);
                    _alive.RemoveAt(i);
                    continue;
                }

                p.Velocity *= 1f - 0.9f * dt;                 // 墨が水に広がるような減速
                p.Rect.anchoredPosition += p.Velocity * dt;
                p.Rect.Rotate(0f, 0f, p.SpinSpeed * dt);

                float t = p.Life / p.MaxLife;                 // 1 → 0
                var c = InkColor;
                c.a = Mathf.Clamp01(t * 1.4f) * 0.85f;        // 終端でふっと消える
                p.Image.color = c;
            }
        }

        private void Emit()
        {
            Particle p = _pool.Count > 0 ? _pool.Pop() : Create();
            p.Image.enabled = true;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(speedRange.x, speedRange.y);
            p.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            p.Life = p.MaxLife = Random.Range(lifeRange.x, lifeRange.y);
            p.SpinSpeed = Random.Range(-90f, 90f);

            float size = Random.Range(sizeRange.x, sizeRange.y);
            p.Rect.sizeDelta = new Vector2(size, size);
            p.Rect.anchoredPosition = Vector2.zero;           // 自身（=符の中心）から放出
            p.Image.color = InkColor;

            _alive.Add(p);
        }

        private Particle Create()
        {
            var go = new GameObject("Ink", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.sprite = _inkSprite;
            img.raycastTarget = false;
            return new Particle { Rect = (RectTransform)go.transform, Image = img };
        }

        /// <summary>中心から外へ柔らかく減衰する円スプライトを実行時生成する（外部アセット不要）。</summary>
        private static Sprite CreateInkSprite()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[size * size];
            const float half = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(half, half)) / half;
                    float a = Mathf.Pow(Mathf.Clamp01(1f - d), 2f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
