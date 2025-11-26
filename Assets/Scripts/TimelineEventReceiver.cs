using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Video;

public class TimelineEventReceiver : MonoBehaviour
{
    [Header("Intro")] 
    public GameObject hand;
    public GameObject blackness;
    public VideoPlayer videoPlayer;

    [Header("Animated Objects")] 
    public GameObject eye1;
    public GameObject spikes1;
    public GameObject girl1;
    public float fadeInDuration;
    
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

    public void ShowBlackness()
    {
        blackness.SetActive(true);
    }
    
    public void ShowEye1()
    {
        eye1.SetActive(true);
    }
    
    public void ShowSpikes1()
    {
        spikes1.SetActive(true);
    }
    
    public void ShowGirl1()
    {
        FadeInSpriteObject(girl1);
    }
    
    public void HideIntro()
    {
        blackness.SetActive(false);
        hand.SetActive(false);
    }
    
    public void PlayVideo()
    {
        if (videoPlayer == null)
            return;
        videoPlayer.time = 0; // start from the beginning
        videoPlayer.Play();
    }
    
    public void StopVideo()
    {
        if (videoPlayer == null)
            return;
        videoPlayer.Stop();

        // Optional: if you want it to stay at first frame instead of clearing:
        // wooliesVideoPlayer.time = 0;
        // wooliesVideoPlayer.Play();
        // wooliesVideoPlayer.Pause();
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
    
    public void FadeInSpriteObject(GameObject targetObject)
    {
        SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        targetObject.SetActive(true); 
        StartCoroutine(FadeSpriteAlpha(spriteRenderer, 0f, 1f, fadeInDuration));
    }

    private IEnumerator FadeSpriteAlpha(SpriteRenderer spriteRenderer, float startAlpha, float endAlpha, float duration)
    {
        if (duration <= 0f)
        {
            Color instantColor = spriteRenderer.color;
            instantColor.a = endAlpha;
            spriteRenderer.color = instantColor;
            yield break;
        }

        Color color = spriteRenderer.color;
        color.a = startAlpha;
        spriteRenderer.color = color;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            spriteRenderer.color = color;

            yield return null;
        }
        // Snap to final value
        color.a = endAlpha;
        spriteRenderer.color = color;
    }
}

