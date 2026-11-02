using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class TrashItem : MonoBehaviour
{
    public Action<TrashItem> OnThrown;      
    public Action<TrashItem> OnDespawned;   
    public float despawnDelay = 3f;

    private bool thrown;
    private XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectExited.AddListener(OnReleased);
    }

    void OnDestroy()
    {
        if (grab != null)
            grab.selectExited.RemoveListener(OnReleased);

        OnDespawned?.Invoke(this);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (thrown) return;
        thrown = true;

        OnThrown?.Invoke(this);
        Destroy(gameObject, despawnDelay);
    }
}