using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PuzzleManagerServer : Photon.PunBehaviour
{
    // Names for global objects containing puzzle elements
    public string movingPlatformAlias = "Moving Platform";
    public string teleporterAlias = "Teleporters";
    public string leverAlias = "Levers";
    public string pressurePlateAlias = "Pressure Plates";
    // List of condition strings defined in editor that control puzzle element logic
    public string[] conditions;
    // Processed list of puzzle conditions
    private List<PuzzleCondition> puzzleConditions;
    // Current condition states
    private List<bool> puzzleConditionStates;
    // Maps used to store GameObjects and puzzle element types by object name
    private Hashtable activatorTypes;
    private Hashtable activators;
    private Hashtable activateeTypes;
    private Hashtable activatees;
    // Flag that tells manager to update puzzle conditions
    private bool dirty;
    // Minimum time interval between requests
    private float requestInterval;
    // Timer that enforces the request time interval
    private float requestTimer;
    // Flag that tells manager that there is a deferred request incoming
    private bool deferRequest;

    public class PuzzleCondition
    {
        GameObject activator;
        string activatorType;
        string operand;
        float threshold;

        List<GameObject> activatees;
        List<string> activateeTypes;
        List<string> activateeStates;

        // Example condition string: "if Lever1 > 0.5 then activate Teleporter:on Platform2:moveToPos1"

        // Activatee States:
        // Moving Platform: on, off, moveToPos1, moveToPos2
        // Teleporter: on, off

        public PuzzleCondition(PuzzleManagerServer manager, string condition)
        {
            // Break condition string down into tokens and then convert tokens into condition logic
            activatees = new List<GameObject>();
            activateeTypes = new List<string>();
            activateeStates = new List<string>();
            string[] tokens = Regex.Split(condition, @"\s+");
            if (tokens.Length < 7 || tokens[0] != "if")
            {
                throw new UnityException("Invalid condition string: " + condition);
            }
            activator = manager.GetActivator(tokens[1]);
            if (activator == null)
            {
                throw new UnityException("Activator is null in condition: " + condition);
            }
            activatorType = manager.GetActivatorType(tokens[1]);
            operand = tokens[2];
            if (operand != "<" && operand != ">")
            {
                throw new UnityException("Operand is not > or < in condition: " + condition);
            }
            threshold = float.Parse(tokens[3]);
            if (threshold < 0 || threshold > 1)
            {
                throw new UnityException("Threshold is not between 0 and 1 in conditon: " + condition);
            }
            if (tokens[4] != "then" || tokens[5] != "activate")
            {
                throw new UnityException("Invalid condition string: " + condition);
            }
            for (int i = 6; i < tokens.Length; i++)
            {
                string[] pair = tokens[i].Split(new char[] { ':' });
                if (pair.Length != 2)
                {
                    throw new UnityException("Invalid condition activatee pair string: " + tokens[i]);
                }
                GameObject activatee = manager.GetActivatee(pair[0]);
                if (activatee == null)
                {
                    throw new UnityException("Activatee " + pair[0] + " is null in condition: " + condition);
                }
                activatees.Add(activatee);
                activateeTypes.Add(manager.GetActivateeType(pair[0]));
                activateeStates.Add(pair[1]);
            }
        }

        // Gets the condition activator's activation
        private float GetActivation()
        {
            if (activatorType == "Lever")
            {
                return activator.GetComponent<LeverState_Client>().GetActivation();
            } else if (activatorType == "Pressure Plate")
            {
                return activator.GetComponent<PressurePlateState_Client>().GetActivation();
            }
            throw new UnityException("Invalid activator type: " + activatorType);
        }

        // Does the condition evaluate to true?
        public bool Evaluate()
        {
            float activation = GetActivation();
            if (operand == ">")
            {
                return activation > threshold;
            } else
            {
                return activation < threshold;
            }
        }

        // Set states of activatees to that defined in the condition
        public void UpdateState()
        {
            for (int i = 0; i < activatees.Count; i++)
            {
                if (activateeTypes[i] == "Moving Platform")
                {
                    activatees[i].GetComponent<PlatformMovement_Client>().SetState(activateeStates[i], true);
                }
                else if (activateeTypes[i] == "Teleporter")
                {
                    activatees[i].GetComponent<Teleporter_Client>().SetState(activateeStates[i], true);
                } else
                {
                    throw new UnityException("Invalid activatee type: " + activateeTypes[i]);
                }
            }
        }
    }

    private void GatherObjects(string alias, Hashtable target, Hashtable targetType, string type)
    {
        GameObject aliasObj = GameObject.Find(alias);
        if (aliasObj)
        {
            for (int i = 0; i < aliasObj.transform.childCount; i++)
            {
                GameObject child = aliasObj.transform.GetChild(i).gameObject;
                target[child.name] = child;
                targetType[child.name] = type;
            }
        }
    }

    public GameObject GetActivator(string name)
    {
        return (GameObject) activators[name];
    }

    public string GetActivatorType(string name)
    {
        return (string) activatorTypes[name];
    }

    public GameObject GetActivatee(string name)
    {
        return (GameObject) activatees[name];
    }

    public string GetActivateeType(string name)
    {
        return (string) activateeTypes[name];
    }

    void Start()
    {
        activatorTypes = new Hashtable();
        activators = new Hashtable();
        activateeTypes = new Hashtable();
        activatees = new Hashtable();

        dirty = true;
        requestInterval = 0.2f;
        requestTimer = -1f;
        deferRequest = false;
        // Add levers and pressure plates to activators
        // Add moving platforms and teleporters to activatees
        GameObject levers = GameObject.Find(leverAlias);
        if (levers)
        {
            for (int i = 0; i < levers.transform.childCount; i++)
            {
                GameObject child1 = levers.transform.GetChild(i).gameObject;
                GameObject child2 = child1.transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
                activators[child1.name] = child2;
                activatorTypes[child1.name] = "Lever";
            }
        }
        GameObject plates = GameObject.Find(pressurePlateAlias);
        if (plates)
        {
            for (int i = 0; i < plates.transform.childCount; i++)
            {
                GameObject child1 = plates.transform.GetChild(i).gameObject;
                GameObject child2 = child1.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                activators[child1.name] = child2;
                activatorTypes[child1.name] = "Pressure Plate";
            }
        }
        GatherObjects(movingPlatformAlias, activatees, activateeTypes, "Moving Platform");
        GatherObjects(teleporterAlias, activatees, activateeTypes, "Teleporter");
        // Parse puzzle conditions and initialize puzzle condition states to false
        puzzleConditions = new List<PuzzleCondition>();
        puzzleConditionStates = new List<bool>();
        for (int i = 0; i < conditions.Length; i++)
        {
            puzzleConditions.Add(new PuzzleCondition(this, conditions[i]));
            puzzleConditionStates.Add(false);
        }
    }

    // Requests an update on the PuzzleManager
    [PunRPC]
    public void RequestUpdate(bool master)
    {
        if (master && PhotonNetwork.isMasterClient)
        {
            dirty = true;
        } else if (!master)
        {
            if (requestTimer <= 0)
            {
                photonView.RPC("RequestUpdate", PhotonTargets.All, true);
                requestTimer = requestInterval;
            } else
            {
                deferRequest = true;
            }
        }
    }
    
    void Update()
    {
        if (requestTimer > 0)
        {
            requestTimer -= Time.deltaTime;
            if (requestTimer <= 0 && deferRequest)
            {
                deferRequest = false;
                requestTimer = requestInterval;
                photonView.RPC("RequestUpdate", PhotonTargets.All, true);
            }
        }
        if (dirty)
        {
            for (int i = 0; i < puzzleConditions.Count; i++)
            {
                bool eval = puzzleConditions[i].Evaluate();
                Debug.Log(conditions[i] + " = " + eval + ", prior state = " + puzzleConditionStates[i]);
                if (eval && !puzzleConditionStates[i])
                {
                    puzzleConditionStates[i] = true;
                    puzzleConditions[i].UpdateState();
                } else if (!eval && puzzleConditionStates[i])
                {
                    puzzleConditionStates[i] = false;
                }
            }
            dirty = false;
        }
    }
}
