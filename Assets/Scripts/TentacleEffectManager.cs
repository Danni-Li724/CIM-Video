using System.Collections.Generic;
using UnityEngine;

public class TentacleEffectManager : MonoBehaviour
{
    [System.Serializable]
    public class TentacleSettings
    {
        public Transform rootTransform;  // Place around screen border
        public float thickness = 0.25f;  // Base thickness at the root
    }

    [Header("Tentacle Prefab")]
    public LineRenderer tentaclePrefab;     // Needs a black material

    [Header("Tentacle Shape")]
    public int segmentCount = 24;
    public float minLength = 2f;
    public float maxLength = 3f;

    [Header("Wave Random Ranges")]
    public float minWaveAmplitude = 0.2f;
    public float maxWaveAmplitude = 0.6f;

    public float minWaveFrequency = 1.5f;
    public float maxWaveFrequency = 3.5f;

    public float minWaveSpeed = 1.0f;
    public float maxWaveSpeed = 2.5f;

    [Header("Growth")]
    public float growthDuration = 1.0f;     // Time to reach full length

    [Header("Tentacles")]
    public TentacleSettings[] tentacles;

    private class TentacleRuntimeData
    {
        public LineRenderer lineRenderer;
        public TentacleSettings settings;

        public float waveAmplitude;
        public float waveFrequency;
        public float waveSpeed;
        public float length; 
    }

    private readonly List<TentacleRuntimeData> activeTentacles = new List<TentacleRuntimeData>();

    private float globalWaveTime;
    private float growthTime;

    private void Update()
    {
        AnimateTentacles();
    }

    // Called by TimelineEventReceiver
    public void SpawnTentacles()
    {
        ClearTentacles();

        if (tentaclePrefab == null)
        {
            return;
        }

        if (tentacles == null || tentacles.Length == 0)
        {
            return;
        }

        int i;
        for (i = 0; i < tentacles.Length; i++)
        {
            TentacleSettings settings = tentacles[i];
            if (settings == null || settings.rootTransform == null)
            {
                continue;
            }

            LineRenderer newTentacle = Instantiate(
                tentaclePrefab,
                settings.rootTransform.position,
                Quaternion.identity,
                transform
            );

            SetupTentacleRenderer(newTentacle, settings.thickness);

            TentacleRuntimeData data = new TentacleRuntimeData();
            data.lineRenderer = newTentacle;
            data.settings = settings;

            // RANDOMISE length & wave values per tentacle (runtime only)
            data.length = Random.Range(minLength, maxLength);
            data.waveAmplitude = Random.Range(minWaveAmplitude, maxWaveAmplitude);
            data.waveFrequency = Random.Range(minWaveFrequency, maxWaveFrequency);
            data.waveSpeed = Random.Range(minWaveSpeed, maxWaveSpeed);

            activeTentacles.Add(data);
        }

        globalWaveTime = 0f;
        growthTime = 0f;
    }

    public void ClearTentacles()
    {
        int i;
        for (i = 0; i < activeTentacles.Count; i++)
        {
            TentacleRuntimeData data = activeTentacles[i];
            if (data != null && data.lineRenderer != null)
            {
                Destroy(data.lineRenderer.gameObject);
            }
        }

        activeTentacles.Clear();
    }

    private void SetupTentacleRenderer(LineRenderer lineRenderer, float baseThickness)
    {
        lineRenderer.positionCount = segmentCount;

        // Thick at root, thin & pointy at tip
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, baseThickness);
        widthCurve.AddKey(1f, 0.01f);

        lineRenderer.widthCurve = widthCurve;
        lineRenderer.useWorldSpace = true;
    }

    private void AnimateTentacles()
    {
        if (activeTentacles.Count == 0)
        {
            return;
        }

        // Advance time for waving
        globalWaveTime += Time.deltaTime;

        // Growth factor 0 → 1 over growthDuration
        float growthFactor = 1f;
        if (growthDuration > 0f)
        {
            growthTime += Time.deltaTime;
            growthFactor = Mathf.Clamp01(growthTime / growthDuration);
        }

        int i;
        for (i = 0; i < activeTentacles.Count; i++)
        {
            TentacleRuntimeData data = activeTentacles[i];
            if (data == null || data.lineRenderer == null)
            {
                continue;
            }

            TentacleSettings settings = data.settings;
            Transform rootTransform = settings.rootTransform;

            Vector3 rootPosition = rootTransform.position;

            // Direction the tentacle grows in – rotate the root transform to aim
            Vector3 direction = rootTransform.up.normalized;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

            // Current length (growing over time)
            float currentLength = data.length * growthFactor;

            int s;
            for (s = 0; s < segmentCount; s++)
            {
                float t = (float)s / (float)(segmentCount - 1);

                float distance = currentLength * t;
                Vector3 basePosition = rootPosition + direction * distance;

                float phase = (t * data.waveFrequency * Mathf.PI * 2f) +
                              (globalWaveTime * data.waveSpeed);

                float wave = Mathf.Sin(phase);
                float scaledWave = wave * data.waveAmplitude * (1f - t);

                Vector3 finalPosition = basePosition + perpendicular * scaledWave;

                data.lineRenderer.SetPosition(s, finalPosition);
            }
        }
    }
}

