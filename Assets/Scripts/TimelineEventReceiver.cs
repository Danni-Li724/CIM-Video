using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TimelineEventReceiver : MonoBehaviour
{
    [Header("Post Processing Volume")]
    public Volume volume;

    [Header("Chromatic Aberration Fade")]
    public float chromaticStart = 0f;
    public float chromaticEnd = 1f;
    public float chromaticDuration = 1f;

    [Header("Vignette Fade")]
    public float vignetteStart = 0f;
    public float vignetteEnd = 0.5f;
    public float vignetteDuration = 1f;

    [Header("Tentacle Effect")]
    public TentacleEffectManager tentacleEffectManager;

    private ChromaticAberration chromaticAberration;
    private Vignette vignette;

    private Coroutine chromaticCoroutine;
    private Coroutine vignetteCoroutine;

    private void Awake()
    {
        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out chromaticAberration);
            volume.profile.TryGet(out vignette);
        }
    }
    

    // fade in chromatic aberration
    public void TriggerChromaticAberrationFade()
    {
        if (chromaticAberration == null)
        {
            return;
        }

        if (chromaticCoroutine != null)
        {
            StopCoroutine(chromaticCoroutine);
        }

        chromaticCoroutine = StartCoroutine(FadeChromaticAberration(
            chromaticStart,
            chromaticEnd,
            chromaticDuration
        ));
    }

    // fade vignette intensity
    public void TriggerVignetteFade()
    {
        if (vignette == null)
        {
            return;
        }

        if (vignetteCoroutine != null)
        {
            StopCoroutine(vignetteCoroutine);
        }

        vignetteCoroutine = StartCoroutine(FadeVignette(
            vignetteStart,
            vignetteEnd,
            vignetteDuration
        ));
    }

    // spawn and animate tentacles
    public void TriggerTentacles()
    {
        if (tentacleEffectManager != null)
        {
            tentacleEffectManager.SpawnTentacles();
        }
    }

    // call from Timeline to clear tentacles
    public void ClearTentacles()
    {
        if (tentacleEffectManager != null)
        {
            tentacleEffectManager.ClearTentacles();
        }
    }
    

    private IEnumerator FadeChromaticAberration(float from, float to, float duration)
    {
        chromaticAberration.active = true;

        float time = 0f;
        chromaticAberration.intensity.Override(from);

        if (duration <= 0f)
        {
            chromaticAberration.intensity.Override(to);
            yield break;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float value = Mathf.Lerp(from, to, t);
            chromaticAberration.intensity.Override(value);
            yield return null;
        }

        chromaticAberration.intensity.Override(to);
    }

    private IEnumerator FadeVignette(float from, float to, float duration)
    {
        vignette.active = true;

        float time = 0f;
        vignette.intensity.Override(from);

        if (duration <= 0f)
        {
            vignette.intensity.Override(to);
            yield break;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float value = Mathf.Lerp(from, to, t);
            vignette.intensity.Override(value);
            yield return null;
        }

        vignette.intensity.Override(to);
    }
}

