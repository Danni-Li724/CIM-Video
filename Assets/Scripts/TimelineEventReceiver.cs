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
    public GameObject mouth;
    public GameObject girl1;
    public GameObject border;
    public GameObject messball;
    public GameObject smallEyeParent;
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
    
    [Header("Fade Children Settings")]
    public float childFadeInDuration = 1.0f;
    public float childActivationInterval = 1.5f;
    
    [Header("Children Scale Settings")]
    public Vector3 childrenTargetScale = new Vector3(3.0f, 3.0f, 3.0f);
    public float childrenScaleDuration = 4f;
    private Coroutine childrenScaleCoroutine;
    
    [Header("Step Fade Settings")]
    public float fadeToHalfDuration = 2.0f;
    public float fadeToAlmostOpaqueDuration = 2.0f;

    private Coroutine sequentialActivationCoroutine;

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
    
    public void ShowMouth()
    {
        mouth.SetActive(true);
    }
    
    public void HideMouth()
    {
        mouth.SetActive(false);
    }
    
    public void ShowGirl1()
    {
        FadeInSpriteObject(girl1);
    }
    
    public void ShowMessBall()
    {
        FadeInSpriteObject(messball);
    }
    
    public void HideMessBall()
    {
        messball.SetActive(false);
    }
    
    public void FadeInBorderToHalf()
    {
        FadeSpriteToHalf(border);
    }
    
    public void FadeInBorderToFull()
    {
        FadeSpriteToAlmostOpaque(border);
    }

    public void ActivateSmallEyes()
    {
        ActivateChildrenSequentially(smallEyeParent);
    }

    public void ScaleSmallEyes()
    {
        ScaleAllChildrenOverDuration(smallEyeParent);
    }

    public void KillSmallEyeParent()
    {
        smallEyeParent.SetActive(false);
    }
    
    public void HideIntro()
    {
        blackness.SetActive(false);
        hand.SetActive(false);
    }
    
    public void ScaleAllChildrenOverDuration(GameObject parentObject)
{
    if (parentObject == null)
    {
        return;
    }

    Transform parentTransform = parentObject.transform;
    if (parentTransform.childCount == 0)
    {
        return;
    }

    if (childrenScaleDuration <= 0f)
    {
        int childCountInstant = parentTransform.childCount;
        for (int i = 0; i < childCountInstant; i++)
        {
            Transform child = parentTransform.GetChild(i);
            if (child != null)
            {
                child.localScale = childrenTargetScale;
            }
        }

        return;
    }
    if (childrenScaleCoroutine != null)
    {
        StopCoroutine(childrenScaleCoroutine);
    }

    childrenScaleCoroutine = StartCoroutine(ScaleAllChildrenCoroutine(parentTransform));
}

private IEnumerator ScaleAllChildrenCoroutine(Transform parentTransform)
{
    int childCount = parentTransform.childCount;
    Vector3[] originalScales = new Vector3[childCount];
    for (int i = 0; i < childCount; i++)
    {
        Transform child = parentTransform.GetChild(i);
        if (child != null)
        {
            originalScales[i] = child.localScale;
        }
    }

    float elapsedTime = 0f;

    while (elapsedTime < childrenScaleDuration)
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / childrenScaleDuration);

        for (int i = 0; i < childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            if (child != null)
            {
                Vector3 startScale = originalScales[i];
                Vector3 targetScale = childrenTargetScale;
                child.localScale = Vector3.Lerp(startScale, targetScale, t);
            }
        }

        yield return null;
    }
    for (int i = 0; i < childCount; i++)
    {
        Transform child = parentTransform.GetChild(i);
        if (child != null)
        {
            child.localScale = childrenTargetScale;
        }
    }
}
    
    public void ActivateChildrenSequentially(GameObject parentObject)
    {
        if (parentObject == null)
        {
            Debug.LogWarning("TimelineEventReceiver: ActivateChildrenSequentially called with null parentObject.");
            return;
        }

        Transform parentTransform = parentObject.transform;
        if (parentTransform.childCount == 0)
        {
            return;
        }
        if (sequentialActivationCoroutine != null)
        {
            StopCoroutine(sequentialActivationCoroutine);
        }

        sequentialActivationCoroutine = StartCoroutine(ActivateChildrenSequentiallyCoroutine(parentTransform));
    }

    private IEnumerator ActivateChildrenSequentiallyCoroutine(Transform parentTransform)
    {
        int childCount = parentTransform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform childTransform = parentTransform.GetChild(i);
            if (childTransform != null)
            {
                childTransform.gameObject.SetActive(true);
            }
            if (childActivationInterval > 0f && i < childCount - 1)
            {
                float elapsedTime = 0f;
                while (elapsedTime < childActivationInterval)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }
    }
    public void FadeSpriteToHalf(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("TimelineEventReceiver: FadeSpriteToHalf called with null target.");
            return;
        }

        SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("TimelineEventReceiver: No SpriteRenderer found on targetObject.");
            return;
        }

        targetObject.SetActive(true);
        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;

        StartCoroutine(FadeSpriteAlpha(spriteRenderer, 0f, 0.5f, fadeToHalfDuration));
    }
    public void FadeSpriteToAlmostOpaque(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("TimelineEventReceiver: FadeSpriteToAlmostOpaque called with null target.");
            return;
        }

        SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("TimelineEventReceiver: No SpriteRenderer found on targetObject.");
            return;
        }

        targetObject.SetActive(true);
        Color color = spriteRenderer.color;
        color.a = 0.5f;
        spriteRenderer.color = color;

        StartCoroutine(FadeSpriteAlpha(spriteRenderer, 0.5f, 0.9f, fadeToAlmostOpaqueDuration));
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
    
    public void FadeInTransparentObject(GameObject targetObject)
    {
        SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        targetObject.SetActive(true); 
        StartCoroutine(FadeSpriteAlpha(spriteRenderer, 0f, .5f, fadeInDuration));
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

