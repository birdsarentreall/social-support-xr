using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractible interactibleInRange = null;
    public GameObject interactionIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactibleInRange?.Interact();
        }
    }

    private void OTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractible interactible) && interactible.CanInteract())
        {
            interactibleInRange = interactible;
            interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractible interactible) && interactible == interactibleInRange)
        {
            interactibleInRange = interactible;
            interactionIcon.SetActive(false);
        }
    }
}
