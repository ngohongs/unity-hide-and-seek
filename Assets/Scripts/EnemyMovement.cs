using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [HideInInspector]
    public Transform Player;
    [Tooltip("Behind what layer can the agent hide")]
    public LayerMask HidableLayers;
    [Tooltip("Line of sight checker")]
    public OnRangeChecker LineOfSightChecker;
    public NavMeshAgent Agent;
    [Tooltip("Avoid walls that are in the direction of the player. Defines the angle between the direction to the wall and the opponent")]
    [Range(0, 180)]
    public float AngleToPlayer = 30;
    [Range(-1, 1)]
    [Tooltip("Lower is a better hiding spot")]
    public float HideSensitivity = 0;
    [Range(1, 10)]
    [Tooltip("Avoid walls close to the opponent")]
    public float MinPlayerDistance = 5f;
    [Tooltip("Minimal height of the wall to hide behind")]
    [Range(0, 5f)]
    public float MinObstacleHeight = 1.25f;
    [Range(0.01f, 1f)]
    public float UpdateFrequency = 0.25f;

    private Coroutine MovementCoroutine;
    private Collider[] Colliders = new Collider[10]; // more is less performant, but more options

    public Animator Animator;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        LineOfSightChecker.OnRangeEnter += HandleGainSight;
        LineOfSightChecker.OnRangeExit += HandleLoseSight;
    }

    private void Update()
    { 
        Animator.SetFloat("Velocity", Agent.velocity.normalized.magnitude);
    }

    private void HandleGainSight(Transform Target)
    {
        if (MovementCoroutine != null)
        {
            StopCoroutine(MovementCoroutine);
        }
        Player = Target;
        MovementCoroutine = StartCoroutine(Hide(Target));
    }

    private void HandleLoseSight(Transform Target)
    {
        if (MovementCoroutine != null)
        {
            StopCoroutine(MovementCoroutine);
        }
        Player = null;
    }

    private IEnumerator Hide(Transform Target)
    {
        int d = 0;
        int a = 0;
        WaitForSeconds Wait = new WaitForSeconds(UpdateFrequency);
        while (true)
        {
            for (int i = 0; i < Colliders.Length; i++)
            {
                Colliders[i] = null;
            }

            int hits = Physics.OverlapSphereNonAlloc(Agent.transform.position, LineOfSightChecker.Collider.radius, Colliders, HidableLayers);
            int orig = hits;
            int hitReduction = 0;
            for (int i = 0; i < hits; i++)
            {
                Vector3 directionToCollider = (Colliders[i].transform.position - Agent.transform.position).normalized;
                Vector3 directionToPlayer = (Player.transform.position - Agent.transform.position).normalized;

                if (Vector3.Distance(Colliders[i].transform.position, Target.position) < MinPlayerDistance)
                    d++;
                if (Vector3.Dot(directionToCollider, directionToPlayer) > Mathf.Cos(Mathf.Deg2Rad * AngleToPlayer))
                    a++;

                if (Vector3.Distance(Colliders[i].transform.position, Target.position) < MinPlayerDistance || Vector3.Dot(directionToCollider, directionToPlayer) > Mathf.Cos(Mathf.Deg2Rad * AngleToPlayer))
                {
                    Colliders[i] = null;
                    hitReduction++;
                }


            }
            hits -= hitReduction;

            System.Array.Sort(Colliders, ColliderArraySortComparer);

            for (int i = 0; i < hits; i++)
            {
                if (NavMesh.SamplePosition(Colliders[i].transform.position + (Agent.transform.position - Player.position).normalized * 2, out NavMeshHit hit, 2 * Agent.height, Agent.areaMask))
                {
                    if (!NavMesh.FindClosestEdge(hit.position, out hit, Agent.areaMask))
                    {
                        Debug.LogError($"Unable to find edge close to {hit.position}");
                    }

                    if (Vector3.Dot(hit.normal, (Target.position - hit.position).normalized) < HideSensitivity)
                    {
                        Agent.SetDestination(hit.position);
                        // Debug.Log("Setting dest 1: " + hit.position);
                        break;
                    }
                    else
                    {
                        // Since the previous spot wasn't facing "away" enough from teh target, we'll try on the other side of the object
                        if (NavMesh.SamplePosition(Colliders[i].transform.position - (Target.position - hit.position).normalized * 2, out NavMeshHit hit2, 2 * Agent.height, Agent.areaMask))
                        {
                            if (!NavMesh.FindClosestEdge(hit2.position, out hit2, Agent.areaMask))
                            {
                                Debug.LogError($"Unable to find edge close to {hit2.position} (second attempt)");
                            }

                            if (Vector3.Dot(hit2.normal, (Target.position - hit2.position).normalized) < HideSensitivity)
                            {
                                Agent.SetDestination(hit2.position);
                                // Debug.Log("Setting dest 2: " + hit.position);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Unable to find NavMesh near object {Colliders[i].name} at {Colliders[i].transform.position}");
                }
            }
            // Debug.Log("Wait orig: " + orig + " red: " + hits + " dis: " + a + " dot: " + a); ;
            a = 0;
            d= 0;
            yield return Wait;
        }
    }

    public int ColliderArraySortComparer(Collider A, Collider B)
    {
        if (A == null && B != null)
        {
            return 1;
        }
        else if (A != null && B == null)
        {
            return -1;
        }
        else if (A == null && B == null)
        {
            return 0;
        }
        else
        {
            return -Vector3.Distance(Player.transform.position, A.transform.position).CompareTo(Vector3.Distance(Player.transform.position, B.transform.position));
        }
    }
}
