using UnityEngine;

[RequireComponent(typeof(Light))]
public class Flackerlicht : MonoBehaviour
{
    public Renderer emissionRenderer;
    public Color emissionColor = Color.white;
    public float minIntensity = 0f;
    public float maxIntensity = 1f;
    public float flickerSpeed = 0.1f;

    private Light flackerLight;
    private Material emissionMaterial;
    private bool emissionOn = true;

    void Start()
    {
        if (!CompareTag("Flackerlicht"))
        {
            Debug.LogWarning("Dieses GameObject hat nicht den Tag 'Flackerlicht'!");
            return;
        }

        flackerLight = GetComponent<Light>();

        if (emissionRenderer != null)
        {
            emissionMaterial = emissionRenderer.material;
            emissionMaterial.EnableKeyword("_EMISSION");
        }
        else
        {
            Debug.LogWarning("Kein Emission Renderer zugewiesen!");
        }

        InvokeRepeating(nameof(Flicker), 0f, flickerSpeed);
    }

    void Flicker()
    {
        float randomIntensity = Random.Range(minIntensity, maxIntensity);
        flackerLight.intensity = randomIntensity;

        if (emissionMaterial != null)
        {
            if (emissionOn)
            {
                emissionMaterial.SetColor("_EmissionColor", emissionColor);
            }
            else
            {
                emissionMaterial.SetColor("_EmissionColor", Color.black);
            }
            emissionOn = !emissionOn;
        }
    }
}
