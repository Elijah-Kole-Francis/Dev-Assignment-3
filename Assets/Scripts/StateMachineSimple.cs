
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class StateMachineSimple : MonoBehaviour
{

    public enum State { Idle, Patrol, Search, Chase, Investigate }

    [Header ("Scene References")]
    public Transform[] waypoints;
    public float waypointThreshold = 0.5f;

    [Header ("Config Values")]
    public float idleThreshold = 1.0f;
    public float idletime = 0.0f;
    public float viewRadius = 10f;
    public float viewangle = 60.0f;
    public float timeelapsed = 0.0f;
    public float searchtime = 0;
    public float searchthreshold = 0f;
    public float investigationThreshold = 5f;
    public float investigateDistance = 2f;
    public float investigateTime = 0.0f;
   
    public float lookAroundAngle = 30.0f;

    int waypointIndex = 0;
    Vector3 soundLocation = Vector3.zero;


    NavMeshAgent agent;
    public Character character;

    bool viewEnabled = false;
    bool canSeePlayer = false;
    bool soundheard = false;

    State state;
    private void Awake()
    {

        agent = GetComponent<NavMeshAgent>();

        state = State.Idle;
        idletime = Time.time;

    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                Idle();
                break;
            case State.Patrol:
                Patrol();
                break;
            case State.Search:
                Search();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Investigate:
                Investigate();
                break;
        }
    }

    public void SoundRecieve(Sound Sound)
    {
        soundheard = true;
        soundLocation = Sound.transform.position;

    }

    void Idle()
    {

        viewEnabled = false;
        //what's implied here is that ile time is set
        //when the idle state is ENTERED
        float timeElapsed = Time.time - idletime;
        if (timeElapsed >= idleThreshold)
        {
            state = State.Patrol;
        }
    }

    void Patrol()
    {
        Vector3 waypoint = waypoints[waypointIndex].position;

        agent.SetDestination(waypoint);

        viewEnabled = true;
        canSeePlayer = InViewCone();

        float distance = Vector3.Distance(transform.position, waypoint);
        if ( distance < waypointThreshold)
        {
            waypointIndex++;
            if (waypointIndex >= waypoints.Length) waypointIndex = 0;

            //ex. of leaky state code
            //idle needs the time when it is entered
            //but we must set the time EVERY time the state changes
            state = State.Idle;
            idletime = Time.time;
        }

        //float distanceToWaypoint

        if (canSeePlayer)
        {
            state = State.Search;
            searchtime = Time.time;
        }

        if (soundheard)
        {
            EnterInvestigate();
        }    

    }

    void EnterInvestigate()
    {
        state = State.Investigate;
        investigateTime = Time.time;
    }

    void Search()
    {
        // agent.SetDestination(transform.forward + transform.right);
        Lookaround();


        float timeElaspsed = Time.time - searchtime;
        if (timeelapsed >= searchthreshold)
        {
            state = State.Patrol;
        }

        canSeePlayer= InViewCone();
        if (canSeePlayer)
        { 
            state = State.Chase;
        }

        if (soundheard)
        {
            EnterInvestigate();
        }

    }

    void Chase()
    {
        agent.SetDestination(character.transform.position);

        canSeePlayer = InViewCone();

        if(!canSeePlayer)
        {
            searchtime = Time.time;
            state = State.Search;
        }
    }


    void Investigate()
    {
        agent.SetDestination(soundLocation);

        float distance = Vector3.Distance(transform.position, soundLocation);

        if (distance <= investigateDistance)
        {
            Lookaround();
            float timeElapsed = Time.time - investigateTime;
            if(timeElapsed >= investigationThreshold)
            {
                soundheard = false;
                state = State.Patrol;
            }
        }

        else
        {
            investigateTime = Time.time;
        }

        viewEnabled = true;
        canSeePlayer = InViewCone();

        if (canSeePlayer)
        {
            soundheard = false;
            state = State.Chase;
        }

    }

    //UTILITIES


    void Lookaround()
    {
        float angle = Mathf.Sin(Time.time) * lookAroundAngle * Time.deltaTime;
        transform.Rotate(Vector3.up, angle);
    }

    bool InViewCone()
    {
        if (Vector3.Distance(transform.position, character.transform.position) > viewRadius)
        return false;

        Vector3 npcToCharacter = character.transform.position - transform.position;
        if (Vector3.Angle(transform.forward, npcToCharacter) > 0.5f * viewangle)
            return false;

        Vector3 toCharacterDir = npcToCharacter.normalized;
        if (Physics.Raycast(transform.position, toCharacterDir, out RaycastHit ray, viewRadius))
        {
            return ray.transform == character.transform;
        }


        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Transform waypoint in waypoints)
        {
            Gizmos.DrawWireSphere(waypoint.position, 0.5f); 
        }

        if (viewEnabled)
        {
            Handles.color = new Color(0f, 1f, 1f, 0.25f);

            if (canSeePlayer) Handles.color = new Color(1f, 0f, 0f, 0.25f);

            Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, viewangle/2, viewRadius);
            Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, -viewangle / 2, viewRadius);

        }

    }

}
