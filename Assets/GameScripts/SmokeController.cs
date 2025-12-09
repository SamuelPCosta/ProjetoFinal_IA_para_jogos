using System.Collections;
using UnityEngine;

public class SmokeController : MonoBehaviour
{
    public float timeToRelease = 2f;
    public float timeToDisapear = 6f;

    void Start()
    {
        StartCoroutine(Process());
    }

    IEnumerator Process()
    {
        yield return new WaitForSeconds(timeToRelease);
        if (transform.childCount > 0) Destroy(transform.GetChild(0).gameObject);

        yield return new WaitForSeconds(timeToDisapear);

        var child = transform.childCount > 0 ? transform.GetChild(0) : null;
        var renderer = child ? child.GetComponent<Renderer>() : null;

        if (renderer)
        {
            float duration = 1.8f;
            float elapsed = 0f;
            var color = renderer.material.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / duration);
                renderer.material.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}