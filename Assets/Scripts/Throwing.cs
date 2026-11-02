using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class Throwing : MonoBehaviour
{
    public float shootSpeed = 12f;
    public bool useGravity = true;

    private XRGrabInteractable grab;
    private Rigidbody rb;
    private bool fired;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        fired = false;
        rb.useGravity = true;          
        rb.isKinematic = false;
        grab.activated.AddListener(OnActivated);
    }

    void OnDisable()
    {
        grab.activated.RemoveListener(OnActivated);
        StopAllCoroutines();
    }

    void OnActivated(ActivateEventArgs args)
    {
        if (fired) return;
        fired = true;

        Vector3 direction = args.interactorObject.transform.forward;

        var interactor = grab.firstInteractorSelecting;
        if (interactor != null)
            grab.interactionManager.SelectExit(interactor, grab);

        StartCoroutine(Fire(direction));
    }

    IEnumerator Fire(Vector3 direction)
    {
        yield return new WaitForFixedUpdate();   

        transform.forward = direction;
        rb.useGravity = useGravity;
        rb.linearVelocity = direction.normalized * shootSpeed;   
        rb.angularVelocity = Vector3.zero;
    }
}