using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Loads a destination when a configured controller points at this door and presses trigger.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(XRSimpleInteractable))]
public sealed class DoorSceneTransition : MonoBehaviour
{
    [SerializeField] private string destinationScenePath;
    [SerializeField] private XRBaseInputInteractor leftController;
    [SerializeField] private XRBaseInputInteractor rightController;

    private XRSimpleInteractable interactable;
    private static bool transitionInProgress;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetTransitionState() => transitionInProgress = false;

    private void Awake() => interactable = GetComponent<XRSimpleInteractable>();

    // LateUpdate reads hover after XRI's dynamic interaction update. Selection/grip is not required.
    private void LateUpdate()
    {
        if (transitionInProgress ||
            (!HasTriggerRequest(leftController) && !HasTriggerRequest(rightController)))
            return;

        var buildIndex = string.IsNullOrEmpty(destinationScenePath)
            ? -1
            : SceneUtility.GetBuildIndexByScenePath(destinationScenePath);
        if (buildIndex < 0)
        {
            Debug.LogError($"Door destination is not enabled in Build Settings: {destinationScenePath}", this);
            return;
        }

        transitionInProgress = true;
        try
        {
            var operation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
            if (operation == null)
                transitionInProgress = false;
            else
                operation.completed += _ => transitionInProgress = false;
        }
        catch (System.Exception exception)
        {
            transitionInProgress = false;
            Debug.LogException(exception, this);
        }
    }

    private bool HasTriggerRequest(XRBaseInputInteractor controller)
    {
        return controller != null && controller.isActiveAndEnabled &&
               (controller == leftController || controller == rightController) &&
               interactable != null && interactable.isActiveAndEnabled &&
               interactable.interactorsHovering.Contains(controller) &&
               controller.activateInput.ReadWasPerformedThisFrame();
    }
}
