using System.Collections;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float timeToFade = 3f;
    public float fadeDuration = 2f;
    private bool collided = false;

    void Start()
    {
        StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        yield return new WaitForSeconds(timeToFade);

        Renderer r = GetComponent<Renderer>();
        if (r == null) yield break;

        Material mat = r.material;
        Color startColor = mat.color;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            mat.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t / fadeDuration));
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collided) return;
        collided = true;

        NPCGroupController npcGroup = FindObjectOfType<NPCGroupController>();
        if (npcGroup != null)
        {
            Vector3 collisionPoint = collision.contacts[0].point;
            npcGroup.groupCheckNoise(collisionPoint);
        }

        Debug.Log("Colidiu com " + collision.gameObject.name);
    }
}
