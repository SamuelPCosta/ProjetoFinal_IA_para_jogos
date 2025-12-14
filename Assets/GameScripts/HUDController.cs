using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour{

    //##########
    [SerializeField] private Slider energy;
    [SerializeField] private Image dash;
    [SerializeField] private Image smoke;
    [SerializeField] private Image projectile;
    [SerializeField] private TextMeshProUGUI projectileAmount;
    [SerializeField] private Color disabledColor;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Coroutine energyRoutine;

    public void setEnergy(int value)
    {
        if (energyRoutine != null) StopCoroutine(energyRoutine);
        energyRoutine = StartCoroutine(SmoothEnergy(value));
    }

    IEnumerator SmoothEnergy(int target)
    {
        float start = energy.value;
        float diff = Mathf.Abs(start - target);
        float t = 0f;
        float duration = Mathf.Clamp(diff / 50f, 0.1f, 0.5f);
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            energy.value = Mathf.Lerp(start, target, t);
            yield return null;
        }
        energy.value = target;
    }

    public void SetImageState(bool enabled, ABILITIES img){
        Image image = null;
        switch (img){
            case ABILITIES.DASH: image = dash; break;
            case ABILITIES.SMOKE: image = smoke; break;
            case ABILITIES.PROJECTILE: image = projectile; break;
        }
        if (image != null)
            image.color = enabled ? Color.white : disabledColor;
    }

    public void SetProjectileAmount(int value){
        projectileAmount.text = "" + value;
    }
}

public enum ABILITIES {SMOKE, PROJECTILE, DASH}