using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private float interactRadius = 1f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private TMPro.TextMeshProUGUI promptText;
    [SerializeField] private Transform originTransform; // z. B. Kamera

    private IInteractable currentInteractable;

    void Update()
    {
        CheckForInteractable();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.F))
        {
            currentInteractable.Interact();
        }
    }

    void CheckForInteractable()
    {
        Vector3 origin = originTransform.position;
        Vector3 direction = originTransform.forward;

        Ray ray = new Ray(origin, direction);
        Debug.DrawRay(origin, direction * interactRange, Color.red);

        if (Physics.SphereCast(ray, interactRadius, out RaycastHit hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                promptText.text = interactable.GetPrompt();
                promptText.enabled = true;
                return;
            }
        }

        currentInteractable = null;
        promptText.enabled = false;
    }
}
