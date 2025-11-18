
using UnityEngine;
using UnityEngine.AI;

public class AIObedienceState : AIState
{
    private AIStateHandler stateHandler;
    private AIChaseState chaseState;
    private AIPeacefulState peaceState;
    private Rigidbody rb;
    public MinionCommands currentCommand;
    public Vector3 commandLoc { private set; get; }
    public LivingBeing commandTarget { private set; get; }
    private MinionStats minionStats;
    //can be command + game object, command + location
    private NavMeshAgent navMesh;

    public void Awake()
    {
        stateHandler = GetComponent<AIStateHandler>();
        chaseState = GetComponent<AIChaseState>();
        peaceState = GetComponent<AIPeacefulState>();
        minionStats = GetComponent<MinionStats>();
        navMesh = GetComponent<NavMeshAgent>();

        rb = GetComponent<Rigidbody>();
    }

    public override AIState RunCurrentState()
    {
        if (stateHandler.minionStats != null) { Debug.Log($"{stateHandler.minionStats.Name} is obeying"); }

        currentCommand = minionStats.CurrentCommand;
        Debug.Log($"current command  is {currentCommand}");
        States state = ObeyCommand(currentCommand);

        if (state == States.Obedience) return this;
        if (state == States.Peace) return peaceState;
        if (state == States.Chase) return chaseState;

        else return this;
    }

    private States ObeyCommand(MinionCommands command)
    {
        States state = States.Obedience;
        if (command == MinionCommands.GoTo)
        {
            state = GoToLocation();
            return state;
        }

        if (command == MinionCommands.FocusTarget && commandTarget != null)
        {
            stateHandler.SetTarget(commandTarget);
            state = GoToLocation();
        }
        else
        {
            currentCommand = MinionCommands.None;
            minionStats.SetCommand(MinionCommands.None);

            return States.Peace;
        }
        return state;


    }
    public void SetCommandLoc(Vector3 loc)
    {
        //Instantiate(minionStats.locSphere, loc, Quaternion.identity);
        commandLoc = loc;
    }
    public void SetCommandTarget(LivingBeing target)
    {
        commandTarget = target;
    }

    private States GoToLocation()
    {
        Vector3 direction = commandLoc - transform.position;
        if (currentCommand == MinionCommands.FocusTarget) commandLoc = commandTarget.transform.position;
        if (direction.magnitude > stateHandler.navAgent.stoppingDistance)
        {
            Debug.Log($"traveling to commanded loc {commandLoc}. distance to target = {direction.sqrMagnitude}");
            navMesh.SetDestination(commandLoc);
            //rb.linearVelocity = direction * 5;
            return States.Obedience;
        }
        else
        {
            Debug.Log("no longer setting destination to the commanded loc because sufficiently close");
            navMesh.ResetPath();
            minionStats.SetCommand(MinionCommands.None);
            if (currentCommand == MinionCommands.GoTo) stateHandler.SetTarget(null); // is this good?
            return States.Peace; // Is this good?...
        }

    }
    private enum States
    {
        None,
        Obedience,
        Peace,
        Chase,
    }

}

