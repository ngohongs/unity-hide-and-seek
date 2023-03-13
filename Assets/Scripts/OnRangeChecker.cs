using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SphereCollider))]
public class OnRangeChecker : MonoBehaviour
{
    [HideInInspector]
    public SphereCollider Collider;
    [Range(0, 360)]
    public float FieldOfView = 90f;
    [Tooltip("Check line of sight with this layer")]
    public LayerMask OnRangeCheckerLayer;

    [Tooltip("Called whenever an object gets in range of vision")]
    public delegate void OnRangeEnterCall(Transform Target);
    [Tooltip("Called whenever an object gets in range of vision")]
    public OnRangeEnterCall OnRangeEnter;
    [Tooltip("Called whenever an object exits the range of vision")]
    public delegate void OnRangeExitCall(Transform Target);
    [Tooltip("Called whenever an object exits the range of vision")]
    public OnRangeExitCall OnRangeExit;

    private Coroutine RepeatUntilVisibleCouritne;

    private void Awake()
    {
        Collider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!CheckLineOfSight(other.transform))
        {
            RepeatUntilVisibleCouritne = StartCoroutine(CheckForLineOfSight(other.transform));
        }        
    }

    private void OnTriggerExit(Collider other)
    {
        OnRangeExit?.Invoke(other.transform);
        if (RepeatUntilVisibleCouritne != null)
        {
            StopCoroutine(RepeatUntilVisibleCouritne);
        }
    }

    private bool CheckLineOfSight(Transform Target)
    {
        Vector3 direction = (Target.transform.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, direction);
        if (dotProduct >= Mathf.Cos(Mathf.Deg2Rad * FieldOfView / 2))
        {
            Vector3 headPosition = transform.position;
            headPosition.y = 1.5f;
            if (Physics.Raycast(headPosition, direction, out RaycastHit hit, Collider.radius, OnRangeCheckerLayer))
            {
                if (hit.transform.gameObject.name == "Player")
                {
                    OnRangeEnter?.Invoke(Target);
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator CheckForLineOfSight(Transform Target)
    {
        while (!CheckLineOfSight(Target))
        {
            yield return new WaitForSeconds(0.5f);
        }
    }
}
