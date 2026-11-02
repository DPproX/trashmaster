using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class TrashItem : MonoBehaviour
{
    public float despawnDelay = 3f;   
    public float respawnDelay = 5f;   

    static readonly List<TrashItem> all = new List<TrashItem>();
    static Runner runner;

    class Runner : MonoBehaviour { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { all.Clear(); runner = null; }

    Vector3 homePos;
    Quaternion homeRot;
    XRGrabInteractable grab;
    Rigidbody rb;
    bool thrown, gone;
    Coroutine timer;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        homePos = transform.position;   
        homeRot = transform.rotation;

        all.Add(this);
        grab.selectExited.AddListener(OnReleased);
    }

    void OnDestroy()
    {
        all.Remove(this);
        if (grab != null) grab.selectExited.RemoveListener(OnReleased);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (thrown || gone) return;
        thrown = true;
        timer = StartCoroutine(DespawnAfterDelay());
    }

    IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(despawnDelay);
        Despawn();
    }

    public void Despawn()
    {
        if (gone) return;
        gone = true;

        if (timer != null) StopCoroutine(timer);

        if (grab.isSelected)
            grab.interactionManager.SelectExit(grab.firstInteractorSelecting, grab);

        gameObject.SetActive(false);

        if (all.TrueForAll(t => t.gone))
        {
            if (runner == null)
                runner = new GameObject("TrashRunner").AddComponent<Runner>();
            runner.StartCoroutine(RespawnAll(respawnDelay));
        }
    }

    static IEnumerator RespawnAll(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var t in all) t.ResetToHome();
    }

    void ResetToHome()
    {
        transform.SetPositionAndRotation(homePos, homeRot);
        gameObject.SetActive(true);

        rb.linearVelocity = Vector3.zero;     
        rb.angularVelocity = Vector3.zero;

        thrown = false;
        gone = false;
    }
}