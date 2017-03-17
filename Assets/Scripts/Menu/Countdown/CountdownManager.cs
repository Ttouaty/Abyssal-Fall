using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CountdownManager : GenericSingleton<CountdownManager>
{
    public Image[] Images = new Image[4];

    public override void Init()
    {
        for(int i = 0; i < Images.Length; ++i)
        {
            Images[i].gameObject.SetActive(false);
        }
    }

    public IEnumerator Countdown ()
    {
        TimeManager.Pause();

        for (int i = 0; i < Images.Length; ++i)
        {
            Images[i].gameObject.SetActive(true);
            RectTransform transform = Images[i].GetComponent<RectTransform>();
            yield return StartCoroutine(Countdown_Implementation(i, transform, Images[i]));
            Images[i].gameObject.SetActive(false);
        }

        TimeManager.Resume();
        MenuPauseManager.Instance.CanPause = true;
    }

    IEnumerator Countdown_Implementation (int index, RectTransform transform, Image image)
    {
        float initScale = 3.0f;
        float initAlpha = 1.0f;
        float timer     = 1.0f;
        float delta     = 0.0f;

        transform.localScale.Set(initScale, initScale, initScale);

        Color color = image.color;
        color.a = initAlpha;
        image.color = color;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            delta = 1.0f - timer;
            float scale = Mathf.Lerp(initScale, 0.0f, delta);
            float alpha = Mathf.Lerp(initAlpha, 0.0f, delta);

            transform.sizeDelta.Set(scale, scale);
            color.a = alpha;
            image.color = color;

            yield return null;
        }
    }
}
