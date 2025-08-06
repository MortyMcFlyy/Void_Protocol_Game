using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform doorToOpen;
    [SerializeField] private float openHeight = 3f;
    [SerializeField] private float openSpeed = 2f;

    private bool isOpen = false;
    private bool isMoving = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    private void Start()
    {
        if (doorToOpen != null)
        {
            closedPosition = doorToOpen.position;
            openPosition = closedPosition + Vector3.up * openHeight;
        }
        else
        {
            Debug.LogError("Tür wurde nicht zugewiesen!", this);
        }
    }

    public string GetPrompt()
    {
        return isOpen ? "" : "F: Tür öffnen";
    }

    public void Interact()
    {
        if (isOpen || isMoving || doorToOpen == null) return;
        doorToOpen.gameObject.SetActive(true); // falls deaktiviert
        StartCoroutine(OpenDoor());
    }

    public void OpenExternally()
    {
        if (isOpen || isMoving || doorToOpen == null) return;
        StartCoroutine(OpenDoor());
    }


    private System.Collections.IEnumerator OpenDoor()
    {
        isMoving = true;

        float t = 0f;
        Vector3 start = doorToOpen.position;
        Vector3 end = openPosition;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            doorToOpen.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        doorToOpen.position = end;
        isOpen = true;
        isMoving = false;
    }
}

