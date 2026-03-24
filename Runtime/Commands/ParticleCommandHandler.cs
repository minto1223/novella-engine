using System;
using System.Collections.Generic;
using Novella.Core;
using UnityEngine;

namespace Novella.Commands
{
    /// <summary>
    /// パーティクルエフェクトを再生する。
    /// JSON: { "type": "play_particle", "value": "sakura" }
    /// - value: プリセット名 (sakura / snow / rain / firefly / dust) またはResources/Particles/内のPrefab名
    /// - duration: 持続時間（秒、0=無限、デフォルト: 0）
    /// </summary>
    public class PlayParticleCommandHandler : ICommandHandler
    {
        public string CommandType => "play_particle";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            string effectName = command.Value ?? "sakura";
            float duration = command.Duration;

            ParticleEffectManager.Instance.Play(effectName, duration, engine);
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// パーティクルエフェクトを停止する。
    /// JSON: { "type": "stop_particle", "value": "sakura" }
    /// - value: 停止するエフェクト名（省略で全停止）
    /// - duration: フェードアウト時間（秒、デフォルト: 1.0）
    /// </summary>
    public class StopParticleCommandHandler : ICommandHandler
    {
        public string CommandType => "stop_particle";

        public void Execute(ScriptCommand command, NovellaEngine engine, Action onComplete)
        {
            string effectName = command.Value;
            float fadeDuration = command.Duration > 0 ? command.Duration : 1f;

            if (string.IsNullOrEmpty(effectName))
                ParticleEffectManager.Instance.StopAll(fadeDuration, engine);
            else
                ParticleEffectManager.Instance.Stop(effectName, fadeDuration, engine);

            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// パーティクルエフェクトの管理クラス。プリセットとカスタムPrefabの両方に対応。
    /// </summary>
    public class ParticleEffectManager
    {
        private static ParticleEffectManager _instance;
        public static ParticleEffectManager Instance => _instance ??= new ParticleEffectManager();

        private readonly Dictionary<string, GameObject> _activeEffects = new Dictionary<string, GameObject>();

        public void Play(string effectName, float duration, NovellaEngine engine)
        {
            // 既存エフェクトがあれば停止
            if (_activeEffects.ContainsKey(effectName))
                StopImmediate(effectName);

            // カスタムPrefabを試みる
            var prefab = Resources.Load<GameObject>($"Particles/{effectName}");
            GameObject effectGo;

            if (prefab != null)
            {
                effectGo = UnityEngine.Object.Instantiate(prefab);
            }
            else
            {
                // プリセット生成
                effectGo = CreatePresetEffect(effectName);
            }

            if (effectGo == null)
            {
                Debug.LogWarning($"[Novella] play_particle: Unknown effect '{effectName}'");
                return;
            }

            effectGo.name = $"ParticleEffect_{effectName}";

            // カメラの前に配置（ParticleSystemはCanvas UI描画では表示されない）
            var cam = Camera.main;
            if (cam != null)
            {
                // カメラの少し前に配置
                float zPos = cam.nearClipPlane + 10f;
                effectGo.transform.position = cam.transform.position + cam.transform.forward * zPos;
                effectGo.transform.rotation = cam.transform.rotation;
            }
            else
            {
                effectGo.transform.position = new Vector3(0, 5f, 0);
            }

            _activeEffects[effectName] = effectGo;

            // duration > 0 なら自動停止
            if (duration > 0)
                engine.StartCoroutine(AutoStop(effectName, duration));

            Debug.Log($"[Novella] Particle started: {effectName}");
        }

        public void Stop(string effectName, float fadeDuration, NovellaEngine engine)
        {
            if (!_activeEffects.TryGetValue(effectName, out var go)) return;
            engine.StartCoroutine(FadeAndDestroy(effectName, go, fadeDuration));
        }

        public void StopAll(float fadeDuration, NovellaEngine engine)
        {
            var keys = new List<string>(_activeEffects.Keys);
            foreach (var key in keys)
                Stop(key, fadeDuration, engine);
        }

        private void StopImmediate(string effectName)
        {
            if (_activeEffects.TryGetValue(effectName, out var go))
            {
                UnityEngine.Object.Destroy(go);
                _activeEffects.Remove(effectName);
            }
        }

        private System.Collections.IEnumerator AutoStop(string effectName, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_activeEffects.TryGetValue(effectName, out var go))
            {
                var ps = go.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.rateOverTime = 0;
                }
                // パーティクルが消えるまで待つ
                yield return new WaitForSeconds(3f);
                StopImmediate(effectName);
            }
        }

        private System.Collections.IEnumerator FadeAndDestroy(string effectName, GameObject go, float fadeDuration)
        {
            if (go == null) yield break;

            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var emission = ps.emission;
                emission.rateOverTime = 0;
            }

            yield return new WaitForSeconds(fadeDuration + 2f);
            if (go != null) UnityEngine.Object.Destroy(go);
            _activeEffects.Remove(effectName);
            Debug.Log($"[Novella] Particle stopped: {effectName}");
        }

        private GameObject CreatePresetEffect(string preset)
        {
            var go = new GameObject("ParticlePreset");
            var ps = go.AddComponent<ParticleSystem>();

            // カメラの視錐台サイズからスケールを算出
            float camHeight = 10f;
            float camWidth = 18f;
            var cam = Camera.main;
            if (cam != null)
            {
                float dist = 10f; // カメラからの距離
                if (cam.orthographic)
                {
                    camHeight = cam.orthographicSize * 2f;
                    camWidth = camHeight * cam.aspect;
                }
                else
                {
                    camHeight = 2f * dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                    camWidth = camHeight * cam.aspect;
                }
            }

            var main = ps.main;
            main.playOnAwake = true;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 300;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(camWidth, 0.1f, 1f);

            var emission = ps.emission;

            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.renderMode = ParticleSystemRenderMode.Billboard;
            // URP対応: Particles/Standard Unlitの代わりにUniversal Render Pipeline/Particles/Unlitを使用
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            var mat = new Material(shader);
            mat.color = Color.white;
            // Additive合成ではなくAlpha Blendで（白い四角にならないように）
            mat.SetFloat("_Surface", 0); // Opaque=0, Transparent=1
            mat.SetFloat("_Blend", 0);
            psr.material = mat;

            // パーティクルサイズはカメラサイズに対する比率で設定
            float sizeUnit = camHeight * 0.02f; // 画面高さの2%を1単位

            switch (preset.ToLower())
            {
                case "sakura":
                    main.startLifetime = 6f;
                    main.startSpeed = camHeight * 0.05f;
                    main.startSize = new ParticleSystem.MinMaxCurve(sizeUnit * 2f, sizeUnit * 5f);
                    main.startColor = new Color(1f, 0.75f, 0.8f, 0.8f);
                    main.gravityModifier = 0.15f;
                    emission.rateOverTime = 15;
                    shape.position = new Vector3(0, camHeight * 0.1f, 0);
                    var velSakura = ps.velocityOverLifetime;
                    velSakura.enabled = true;
                    velSakura.x = new ParticleSystem.MinMaxCurve(-camWidth * 0.03f, camWidth * 0.03f);
                    velSakura.y = new ParticleSystem.MinMaxCurve(0f, 0f);
                    velSakura.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                    var rotSakura = ps.rotationOverLifetime;
                    rotSakura.enabled = true;
                    rotSakura.x = new ParticleSystem.MinMaxCurve(0f, 0f);
                    rotSakura.y = new ParticleSystem.MinMaxCurve(0f, 0f);
                    rotSakura.z = new ParticleSystem.MinMaxCurve(-180f, 180f);
                    break;

                case "snow":
                    main.startLifetime = 8f;
                    main.startSpeed = camHeight * 0.03f;
                    main.startSize = new ParticleSystem.MinMaxCurve(sizeUnit * 0.5f, sizeUnit * 2f);
                    main.startColor = new Color(1f, 1f, 1f, 0.9f);
                    main.gravityModifier = 0.08f;
                    emission.rateOverTime = 30;
                    shape.position = new Vector3(0, camHeight * 0.1f, 0);
                    var velSnow = ps.velocityOverLifetime;
                    velSnow.enabled = true;
                    velSnow.x = new ParticleSystem.MinMaxCurve(-camWidth * 0.02f, camWidth * 0.02f);
                    velSnow.y = new ParticleSystem.MinMaxCurve(0f, 0f);
                    velSnow.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                    break;

                case "rain":
                    main.startLifetime = 1.0f;
                    main.startSpeed = camHeight * 0.8f;
                    main.startSize = new ParticleSystem.MinMaxCurve(sizeUnit * 0.3f, sizeUnit * 0.6f);
                    main.startColor = new Color(0.7f, 0.85f, 1f, 0.5f);
                    main.gravityModifier = 1.5f;
                    emission.rateOverTime = 100;
                    shape.position = new Vector3(0, camHeight * 0.2f, 0);
                    // 縦に引き伸ばす
                    main.startSize3D = true;
                    main.startSizeX = new ParticleSystem.MinMaxCurve(sizeUnit * 0.15f);
                    main.startSizeY = new ParticleSystem.MinMaxCurve(sizeUnit * 2f, sizeUnit * 4f);
                    main.startSizeZ = new ParticleSystem.MinMaxCurve(sizeUnit * 0.15f);
                    break;

                case "firefly":
                    main.startLifetime = 4f;
                    main.startSpeed = camHeight * 0.02f;
                    main.startSize = new ParticleSystem.MinMaxCurve(sizeUnit * 1f, sizeUnit * 2f);
                    main.startColor = new Color(0.8f, 1f, 0.5f, 0.8f);
                    main.gravityModifier = -0.03f;
                    emission.rateOverTime = 10;
                    shape.scale = new Vector3(camWidth * 0.8f, camHeight * 0.6f, 1f);
                    shape.position = new Vector3(0, -camHeight * 0.2f, 0);
                    var velFirefly = ps.velocityOverLifetime;
                    velFirefly.enabled = true;
                    velFirefly.x = new ParticleSystem.MinMaxCurve(-camWidth * 0.03f, camWidth * 0.03f);
                    velFirefly.y = new ParticleSystem.MinMaxCurve(-camHeight * 0.02f, camHeight * 0.02f);
                    velFirefly.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                    var colFirefly = ps.colorOverLifetime;
                    colFirefly.enabled = true;
                    var gradient = new Gradient();
                    gradient.SetKeys(
                        new[] { new GradientColorKey(new Color(0.8f, 1f, 0.5f), 0f), new GradientColorKey(new Color(0.8f, 1f, 0.5f), 1f) },
                        new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.8f, 0.3f), new GradientAlphaKey(0.8f, 0.7f), new GradientAlphaKey(0f, 1f) }
                    );
                    colFirefly.color = gradient;
                    break;

                case "dust":
                    main.startLifetime = 5f;
                    main.startSpeed = camHeight * 0.01f;
                    main.startSize = new ParticleSystem.MinMaxCurve(sizeUnit * 0.3f, sizeUnit * 0.8f);
                    main.startColor = new Color(1f, 1f, 0.9f, 0.4f);
                    main.gravityModifier = -0.02f;
                    emission.rateOverTime = 12;
                    shape.scale = new Vector3(camWidth * 0.8f, camHeight * 0.7f, 1f);
                    shape.position = new Vector3(0, -camHeight * 0.15f, 0);
                    var velDust = ps.velocityOverLifetime;
                    velDust.enabled = true;
                    velDust.x = new ParticleSystem.MinMaxCurve(-camWidth * 0.01f, camWidth * 0.01f);
                    velDust.y = new ParticleSystem.MinMaxCurve(-camHeight * 0.005f, camHeight * 0.005f);
                    velDust.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                    break;

                default:
                    UnityEngine.Object.Destroy(go);
                    return null;
            }

            return go;
        }
    }
}
