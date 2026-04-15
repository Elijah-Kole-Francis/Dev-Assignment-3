using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;

public class WitchStateMachine : MonoBehaviour
{
    //States
    public enum State { Search, Investigate, Flee, Teleport, Stun }
    public State state;

    //Searchpoint array, will see if I can randomize
    public Transform[] searchPoints;
    public float searchPointThreshold = 1.0f;
    public int searchPointIndex = 0;
    public float searchTime = 0f;

    //investigation variables, might add a lookaround/check 
    public float investigateThreshold = 5f;
    public float investigateDistance = 5f;
    public float investigateTime = 0.0f;
    public float timeElapsed = 0f;
    public float checkAngle = 20.0f;

    //found that reusing this variable for a
    //
    public float stateStartTime;

    //variables for senses
    public float viewCircleRadius = 5f;
    public float viewConeLength = 8f;
    Vector3 soundLocation = Vector3.zero;
    Vector3 currentTarget;

    //Flee variables
    public float fleeDistance = 10f;
    public float fleeTime = 6.0f;

    //Variables to be used for teleport, might need to rename Y to Z
    //Assign either in engine or in struct
    public float teleportDistanceX;
    public float teleportDistanceZ;
    public float lastTeleportTime;
    public float teleportCooldown = 10f;

    //time player has reduced or null movement
    public float stunTime = 5.0f;

    //Character references
    public NavMeshAgent witch;
    public Character Character;

    //Bools 
    bool viewConeEnable = false;
    bool viewCircleEnable = false;
    bool canSeePlayer = false;
    bool soundHeard = false;
    bool teleportComplete = false;

    //Get AI and set it to search
    private void Awake()
    {
        witch = GetComponent<NavMeshAgent>();

        state = State.Search;
        searchTime = Time.time;

    }

    //set searchpoint on start since it wasn't pathing
    private void Start()
    {
        if (searchPoints.Length > 0)
        {
            currentTarget = searchPoints[0].position;
            witch.SetDestination(currentTarget);
        }
    }

    //State machine breakdown
    private void Update()
    {
        //AI was having trouble recalling seeing player for state changes
        //so if it does it continues flee
        canSeePlayer = InViewCone();

        //remember to put a break after each case or unity gets angry
        switch (state)
        {
            case State.Search:
                Search();
                break;
            case State.Investigate:
                Investigate();
                break;
            case State.Flee:
                Flee();
                break;
            case State.Teleport:
                Teleport();
                break;
            case State.Stun:
                Stun();
                break;
        }
    }

    //Sound sensory
    //likely not used
    public void SoundRecieve(Sound Sound)
    {
        soundHeard = true;
        soundLocation = Sound.transform.position;

    }

    //Randomize the search point transforms so the witch will go anywhere
    //forcing the player to chase them
    /*public Transform GetRandomSearchPoint()
    {
        int index = Random.Range(0, searchPoints.Length);
        return searchPoints[index];
    }
    */

    //amend this to utilize the randomization of the above SearchPointChoice
    void Search()
    {
        Vector3 searchPoint = searchPoints[searchPointIndex].position;

        viewConeEnable = true;
        viewCircleEnable = true;

        canSeePlayer = InViewCone();

        witch.SetDestination(currentTarget);

        float distance = Vector3.Distance(transform.position, searchPoint);
        if (distance < searchPointThreshold)
        {
            searchPointIndex++;
            if (searchPointIndex >= searchPoints.Length)
            {
                searchPointIndex = 0;
            }

            state = State.Investigate;
            investigateTime = Time.time;
        }

        // this should change which point it goes to
        //pathpending was a hassle to figure out

        /// There was an attempt at randomizing searchpoints but it would cycle through them constantly leading to the AI doing nothing
        
        /*if (!witch.pathPending && witch.remainingDistance < searchPointThreshold)
        {
            int newSearchIndex = Random.Range(0, searchPoints.Length);

            // prevent repeating same point
            while (newSearchIndex == searchPointIndex && searchPoints.Length > 1)
            {
                newSearchIndex = Random.Range(0, searchPoints.Length);
            }

            searchPointIndex = newSearchIndex;
        }
        */
        
        //break down of options the AI has and how it chooses them
        if (canSeePlayer)
        {
            float distanceD = Vector3.Distance(transform.position, Character.transform.position);

            bool canTeleport = Time.time - lastTeleportTime >= teleportCooldown;

            if (canTeleport)
            {
                state = State.Teleport;
                teleportComplete = false;
            }
            else if (distance < 3f)
            {
                state = State.Stun;
            }
            else
            {
                state = State.Flee;
            }

            stateStartTime = Time.time;
        }
    }

    //a lot like an idle
    void Investigate()
    {
        viewCircleEnable = false;
        if (Time.time - stateStartTime >= investigateTime)
        {
            state = State.Search;
            stateStartTime = Time.time;
        }
    }

    //have to normalize direction when going away from character unless AI implodes and travels to a 5th dimension
    //and set a fleeTarget so it has some place to go
    void Flee()
    {

        Vector3 dir = (transform.position - Character.transform.position).normalized;
        Vector3 fleeTarget = transform.position + dir * fleeDistance;

        witch.SetDestination(fleeTarget);

        //after-timer cooldown options
        if (Time.time - stateStartTime >= fleeTime)
        {
            if (!canSeePlayer)
            {
                state = State.Search;
                stateStartTime = Time.time;
            }
            else
            {
                // refresh flee timer if still threatened
                stateStartTime = Time.time;
            }

        }
    }

    //for some reason this was much harder to impliment thanI thought
    //but logic for teleporting and enables a cooldown
    void Teleport()
    {
        if (!teleportComplete)
        {
            teleportDistanceX = Random.Range(-15, 15);
            teleportDistanceZ = Random.Range(-15, 15);

            witch.transform.position = new Vector3 (teleportDistanceX, transform.position.y, teleportDistanceZ);

            teleportComplete = true;
            lastTeleportTime = Time.time;

            state = State.Search;
            stateStartTime = Time.time;
        }
    }

    void Stun()
    {
        //was tempted to disable or enable the playerinput which could have worked
        //but didn't want to get too bad in case unity got even more mad
        if (Time.time - stateStartTime < stunTime)
        {
            Character.moveSpeed = 0;
        }
        else
        {
            Character.moveSpeed = 1;
            state = State.Flee;
            stateStartTime = Time.time;
        }

    }

    //viewcone logic from example
    bool InViewCone()
    {
        if (Vector3.Distance(transform.position, Character.transform.position) > viewConeLength)
            return false;

        Vector3 npcToCharacter = Character.transform.position - transform.position;
        if (Vector3.Angle(transform.forward, npcToCharacter) > 0.5f * checkAngle)
            return false;

        Vector3 toCharacterDir = npcToCharacter.normalized;
        if (Physics.Raycast(transform.position, toCharacterDir, out RaycastHit ray, viewConeLength))
        {
            return ray.transform == Character.transform;
        }


        return false;
    }

    //The Handles and Gizmos were giving me a great deal of trouble 
    //so somehow reformatting it and using the #if and #endif were the only things to solve its
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (searchPoints != null)
        {
            foreach (Transform searchpoint in searchPoints)
            {
                if (searchpoint != null)
                    Gizmos.DrawWireSphere(searchpoint.position, 0.5f);
            }
        }

#if UNITY_EDITOR
        if (viewConeEnable)
        {
            Handles.color = new Color(0f, 1f, 1f, 0.25f);

            if (canSeePlayer)
                Handles.color = new Color(1f, 0f, 0f, 0.25f);

            Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, checkAngle / 2, checkAngle);
            Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, -checkAngle / 2, checkAngle);
        }
#endif
    }

}
