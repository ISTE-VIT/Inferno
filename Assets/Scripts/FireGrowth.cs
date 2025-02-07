using UnityEngine;

public class FireGrowth : MonoBehaviour
{
    public float growthRate = 0.1f; // Rate at which fire grows
    public float shrinkRate = 0.2f; // Rate at which fire shrinks when extinguished
    public float maxScale = 3f; // Maximum scale limit for fire
    public float minScale = 0.1f; // Minimum scale before fire disappears

    public bool inExtinguisherZone = false;

    void Update()
    {
        if (inExtinguisherZone)
        {
            // Shrink the fire when in the extinguisher zone
            transform.localScale -= Vector3.one * shrinkRate * Time.deltaTime;
        }
        else
        {
            // Grow the fire when not being extinguished
            transform.localScale += Vector3.one * growthRate * Time.deltaTime;
        }

        // Clamp the scale within min and max limits
        float clampedScale = Mathf.Clamp(transform.localScale.x, minScale, maxScale);
        transform.localScale = new Vector3(clampedScale, clampedScale, clampedScale);

        // Destroy fire object if it becomes too small
        if (transform.localScale.x <= minScale)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Extinguisher"))
        {
            inExtinguisherZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Extinguisher"))
        {
            inExtinguisherZone = false;
        }
    }
}
