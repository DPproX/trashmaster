using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class Throwing : MonoBehaviour
{
    [Header("Throw power")]
    public float minSpeed = 4f;         
    public float maxSpeed = 20f;        
    public float maxChargeTime = 1.5f;  
    public bool useGravity = true;

    private XRGrabInteractable grab;
    private Rigidbody rb;
    private bool charging;
    private bool fired;
    private float chargeStart;

    public float Charge01 =>
        charging ? Mathf.Clamp01((Time.time - chargeStart) / maxChargeTime) : 0f;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        fired = false;
        charging = false;
        rb.useGravity = true;
        rb.isKinematic = false;

        grab.activated.AddListener(OnActivated);
        grab.deactivated.AddListener(OnDeactivated);
        grab.selectExited.AddListener(OnSelectExited);
    }

    void OnDisable()
    {
        grab.activated.RemoveListener(OnActivated);
        grab.deactivated.RemoveListener(OnDeactivated);
        grab.selectExited.RemoveListener(OnSelectExited);
        StopAllCoroutines();
    }

    void OnActivated(ActivateEventArgs args)
    {
        if (fired) return;
        charging = true;
        chargeStart = Time.time;
    }

    void OnDeactivated(DeactivateEventArgs args)
    {
        if (!charging || fired || !grab.isSelected) return;

        float power = Charge01;
        charging = false;
        fired = true;

        Vector3 direction = args.interactorObject.transform.forward;
        float speed = Mathf.Lerp(minSpeed, maxSpeed, power);

        var interactor = grab.firstInteractorSelecting;
        if (interactor != null)
            grab.interactionManager.SelectExit(interactor, grab);

        StartCoroutine(Fire(direction, speed));
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        if (!fired) charging = false;
    }

    IEnumerator Fire(Vector3 direction, float speed)
    {
        yield return new WaitForFixedUpdate();

        transform.forward = direction;
        rb.useGravity = useGravity;
        rb.linearVelocity = direction.normalized * speed;  
        rb.angularVelocity = Vector3.zero;
    }
}