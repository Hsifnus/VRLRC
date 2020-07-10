using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// This lets GameObjects be sorted in ascending name order by default.
public class GameObjectComparer : IComparer
{
    public int Compare(object x, object y)
    {
        String nx = ((GameObject)x).name;
        String ny = ((GameObject)y).name;
        return nx.CompareTo(ny);
    }
}

// This lets GameObjects be sorted in ascending Photon view ID order by default
public class GameObjectPhotonComparer : IComparer
{
    public int Compare(object x, object y)
    {
        int nx = ((GameObject)x).GetComponent<PhotonView>().viewID;
        int ny = ((GameObject)y).GetComponent<PhotonView>().viewID;
        return nx.CompareTo(ny);
    }
}

// Manages the various dynamic objects in the networked VR LRC environment.
public class ObjectManager : Photon.PunBehaviour
{
    // List of throwable GameObjects for easy transform manipulations, sorted by object name.
    private GameObject[] throwableObjs;
    // List of ObjectStateServer scripts corresponding one-to-one with throwable GameObjects
    // The two lists are of the same size,
    // and a throwable script is always the same index as its corresponding GameObject.
    private ObjectStateServer[] throwables;
    // Same thing as with throwables above, but with lever objects instead.
    // This is done to be able to enforce hand-lever interactions in multiplayer.
    private GameObject[] leverObjs;
    private LeverState_Client[] levers;
    // List of all player hands, sorted by Photon view ID.
    private GameObject[] controllers;
    // Hashmap that stores all retrieved PlayerForce scripts so far. This minimizes GetComponent<PlayerForce> calls.
    private Hashtable playerForceCache;
    // Hashmap that binds controller objects to their assigned indexes, also saving GetComponent<Controller_State_Client> calls
    private Hashtable controllerIndexMap;
    // List of indices indicating objects a controller should disconnect from. This is cleared and reused for each controller.
    private List<int>[] toSeparate;
    // Game version.
    private string gameVersion = "0.1";
    // Instances of the sorting functions above.
    private GameObjectComparer comp;
    private GameObjectPhotonComparer photonComp;
    // Flag that determines whether controllers have been initialized properly in the scene yet.
    private Boolean resolvedControllers;
    // Affects the strength of pull forces for non-local players.
    public float networked_force_factor;
    // List of spawn points for different players, listed in increasing order of player number.
    public GameObject[] playerLocations;

    // We automatically join the multiplayer lobby and sync scene data over the network.
    void Awake()
    {
        PhotonNetwork.autoJoinLobby = true;
        PhotonNetwork.automaticallySyncScene = true;
    }

    // Start resolving controllers, and connect to Photon server if not already connected.
    void Start()
    {
        resolvedControllers = false;
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.ConnectUsingSettings(gameVersion);
        }
    }
    
    // Automatically makes or joins a multiplayer room upon joining the lobby successfully.
    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions() { MaxPlayers = 4 }, TypedLobby.Default);
    }

    // Configures game object management behavior once we successfully join a multiplayer room.
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        // Set spawn point of our player character according to our player number.
        GameObject.Find("[CameraRig]").transform.SetPositionAndRotation(playerLocations[PhotonNetwork.player.ID-1].transform.position, Quaternion.identity);
        // Spawn our player at that spawn point.
        PhotonNetwork.Instantiate("PlayerPrefab", playerLocations[PhotonNetwork.player.ID-1].transform.position, Quaternion.identity, 0);

        // Initialize the sorters
        comp = new GameObjectComparer();
        photonComp = new GameObjectPhotonComparer();
        // Get controller information gathered and initialized.
        RefreshControllers();
        
        // Collect all throwable objects in the scene to add networked object physics and for later
        // hand-throwable interactions.
        throwableObjs = GameObject.FindGameObjectsWithTag("Throwable");
        Array.Sort(throwableObjs, comp);
        throwables = new ObjectStateServer[throwableObjs.Length];
        int id = 10000;
        for (int i = 0; i < throwableObjs.Length; i++)
        {
            // Add a PhotonTransformView to each throwable object.
            // This handles the networked position and rotation updates of each object.
            // Also add a PhotonRigidbodyView to each throwable object.
            // This handles the networked physics of each object.
            // Also add a PhotonView to each throwable object containing the two views above.
            PhotonView pv = throwableObjs[i].AddComponent<PhotonView>();
            PhotonTransformView ptv = throwableObjs[i].AddComponent<PhotonTransformView>();
            // Transform view settings - position and rotational sync across clients
            ptv.m_PositionModel.SynchronizeEnabled = true;
            ptv.m_RotationModel.SynchronizeEnabled = true;
            ptv.m_PositionModel.InterpolateOption = PhotonTransformViewPositionModel.InterpolateOptions.Lerp;
            ptv.m_PositionModel.InterpolateMoveTowardsAcceleration = 0.3f;
            ptv.m_PositionModel.InterpolateMoveTowardsDeceleration = 0.3f;
            ptv.m_PositionModel.InterpolateMoveTowardsSpeed = 1;
            ptv.m_PositionModel.ExtrapolateOption = PhotonTransformViewPositionModel.ExtrapolateOptions.Disabled;
            ptv.m_PositionModel.DrawErrorGizmo = true;
            ptv.m_RotationModel.InterpolateOption = PhotonTransformViewRotationModel.InterpolateOptions.Lerp;
            ptv.m_RotationModel.InterpolateLerpSpeed = 100;
            PhotonRigidbodyView prv = throwableObjs[i].AddComponent<PhotonRigidbodyView>();
            pv.ObservedComponents = new List<Component>
            {
                ptv, prv
            };
            pv.viewID = id++;
            pv.synchronization = ViewSynchronization.Unreliable;
            Debug.Log(throwableObjs[i]);
            // Set the object index of each throwable object
            throwables[i] = throwableObjs[i].GetComponent<ObjectStateServer>();
            throwables[i].SetObjectIndex(i);
        }

        // Collect each lever in the scene for later lever-hand interactions.
        leverObjs = GameObject.FindGameObjectsWithTag("Lever");
        Array.Sort(leverObjs, comp);
        levers = new LeverState_Client[leverObjs.Length];
        for (int i = 0; i < leverObjs.Length; i++)
        {
            // Set the object index of each lever object
            // Lever object indices start above throwable object indices in order to distinguish between the two types
            levers[i] = leverObjs[i].GetComponent<LeverState_Client>();
            levers[i].SetObjectIndex(throwableObjs.Length + i);
        }

        if (PhotonNetwork.isMasterClient == false)
        {
            return;
        }

        // The master client calculates player-object pull physics on an update loop.
        // Thus, it uses an object-to-PlayerForce-script cache to reduce GetComponent calls.
        playerForceCache = new Hashtable();
    }

    // Locates all player hands in scene and adds them into the controller list.
    // Due to network conditions, not all hands may be visible at startup.
    // As such, we have to make sure that the controllers found match up with the
    // number of players in the room before we marked the controller search process
    // as being resolved.
    private void RefreshControllers()
    {
        Debug.Log("RefreshControllers");
        // Initialize controller list.
        List<GameObject> controllerList = new List<GameObject>();
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            // This id is the Photon view ID given to the left hand.
            int id = PhotonNetwork.MAX_VIEW_IDS * player.ID + 5;
            if (PhotonView.Find(id) != null)
            {
                // Finds the controller in the scene for that player's left hand.
                GameObject handObj = PhotonView.Find(id).gameObject;
                // Add controller to the list.
                controllerList.Add(handObj);
                if (!player.IsMasterClient)
                {
                    // Calibrate the force factor of non-master client players to be reasonable
                    handObj.GetComponent<PlayerForce>().force_factor = networked_force_factor;
                }
            }
            // This id is the Photon view ID given to the right hand.
            if (PhotonView.Find(id + 1) != null)
            {
                // Finds the controller in the scene for that player's right hand.
                GameObject handObj = PhotonView.Find(id + 1).gameObject;
                // Add controller to the list.
                controllerList.Add(PhotonView.Find(id + 1).gameObject);
                if (!player.IsMasterClient)
                {
                    // Calibrate the force factor of non-master client players to be reasonable
                    handObj.GetComponent<PlayerForce>().force_factor = networked_force_factor;
                }
            }
        }
        // If we have found exactly double the controllers in our controllers list as the number of players, we can assume that
        // our controller search is now resolved.
        resolvedControllers = controllerList.Count == 2 * PhotonNetwork.playerList.Length;
        if (!resolvedControllers)
        {
            return;
        }
        // Once resolved, we sort the controller list by photon view ID.
        controllers = controllerList.ToArray();
        Array.Sort(controllers, photonComp);
        // Initialize the controller-to-controller-index map for caching purposes.
        controllerIndexMap = new Hashtable();
        // Also initalize the per-controller object separation 2D list.
        toSeparate = new List<int>[controllers.Length];
        for (int i = 0; i < controllers.Length; i++)
        {
            // If current player is the master client, assign each controller their respective indices across all clients.
            Controller_State_Client client = controllers[i].GetComponent<Controller_State_Client>();
            PhotonView targetView = client.photonView;
            if (PhotonNetwork.isMasterClient)
                targetView.RPC("SetObjectIndex", PhotonTargets.All, i);
            // ...and save that index into the controller-to-controller-index map.
            controllerIndexMap.Add(controllers[i], i);
            // Initialize each controller's object separation sublist.
            toSeparate[i] = new List<int>();
        }
    }

    // We lost a player, so we have to refresh the controller list and
    // related data to maintain consistent controller state across clients.
    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        RefreshControllers();
    }

    // We gained a player, so we have to refresh the controller list and
    // related data to maintain consistent controller state across clients.
    public override void OnPhotonPlayerDisconnected(PhotonPlayer newPlayer)
    {
        RefreshControllers();
    }

    // Retrieves the player force script from the given game object
    // If the cache doesn't have a key for the given input game object, this function uses GetComponent
    // and puts the game object and script pair into the cache.
    // Otherwise, the cache is used with the input game object as the key to retrieve the corresponding script.
    private PlayerForce GetPlayerForce(GameObject hand)
    {
        PlayerForce force;
        if (playerForceCache.ContainsKey(hand))
        {
            force = (PlayerForce)playerForceCache[hand];
        }
        else
        {
            force = hand.GetComponent<PlayerForce>();
            playerForceCache.Add(hand, force);
        }
        return force;
    }

    // When a player presses the trigger and it touches a throwable, it relays a signal to this function on the master client
    // to link the throwable object to the hand.
    [PunRPC]
    void RelayOnTriggerPress(int ctrl, int obj)
    {
        if (PhotonNetwork.isMasterClient == false)
        {
            return;
        }
        Debug.Log("RelayOnTriggerPress: " + ctrl + ", " + obj);
        throwables[obj].OnTriggerPress(controllers[ctrl]);
    }

    // When the player releases the trigger while holding a throwable, it relays a signal to this function on the master client
    // to separate and throw the object away from the hand.
    [PunRPC]
    void RelayOnTriggerRelease(int ctrl, int obj)
    {
        if (PhotonNetwork.isMasterClient == false)
        {
            return;
        }
        Debug.Log("RelayOnTriggerRelease: " + ctrl + ", " + obj);
        throwables[obj].OnTriggerRelease(controllers[ctrl]);
        PlayerForce force = GetPlayerForce(controllers[ctrl]);
        if (force != null)
        {
            force.ApplyThrowForce(throwableObjs[obj]);
        }
    }

    // If controller search has not resolved yet, we reattempt it.
    void Update()
    {
        if (!resolvedControllers)
        {
            RefreshControllers();
        }
    }

    // Pull physics loop for the whole scene. This is only done by the master client.
    void LateUpdate()
    {
        if (PhotonNetwork.isMasterClient == false)
        {
            return;
        }

        // 1. Apply pull force to throwable interactees
        for (int i = 0; i < throwables.Length; i++)
        {
            // For each throwable object, check its interactors and apply a force from each interactor to the throwable.
            // If any hand is far enough away from the throwable, add the throwable object to the hand object's separation sublist.
            GameObject throwableObj = throwableObjs[i];
            ObjectStateServer throwableState = throwables[i];
            foreach (GameObject hand in throwableState.GetInteractors())
            {
                if (!GetPlayerForce(hand).ApplyForce(throwableObj))
                {
                    toSeparate[(int)controllerIndexMap[hand]].Add(throwableState.GetObjectIndex());
                }
            }
        }
        // 2. Apply pull force to lever interactees
        for (int i = 0; i < levers.Length; i++)
        {
            // For each lever object, check for separation between each of its interactors and the lever handle. For all interactors that
            // are not being separated, add their pull vectors into a single sum vector and update the lever's activation accordingly.
            GameObject leverObj = leverObjs[i];
            LeverState_Client leverState = levers[i];
            Vector3 leverPos = leverObj.transform.position;
            Vector3 pull = new Vector3();
            foreach (GameObject hand in leverState.GetInteractors())
            {
                Vector3 currentPull = hand.transform.position - leverObj.transform.position;
                if (currentPull.magnitude > GetPlayerForce(hand).separation_threshold)
                {
                    toSeparate[(int)controllerIndexMap[hand]].Add(leverState.GetObjectIndex());
                }
                else
                {
                    pull += currentPull;
                }
            }
            leverObj.GetComponent<LeverState_Client>().UpdateActivation(pull);
        }
        // 3. Release any interactees marked for separation
        for (int j = 0; j < toSeparate.Length; j++)
        {
            if (toSeparate[j].Count > 0)
            {
                // Tells each controller with objects to separate from to remove its link with those objects, while also
                // telling each throwable or lever being separate from to update its state to signify separation.
                int[] sepVals = toSeparate[j].ToArray();
                controllers[j].GetComponent<Controller_State_Client>().RemoveInteractee(j, sepVals);
                for (int k = 0; k < sepVals.Length; k++)
                {
                    if (sepVals[k] < throwableObjs.Length)
                    {
                        throwables[sepVals[k]].OnTriggerRelease(controllers[j]);
                    } else
                    {
                        levers[sepVals[k] - throwableObjs.Length].ChangeInteractor(j, true, true);
                    }
                }
                // This clears out each controller's separation sublist when done being used.
                toSeparate[j].Clear();
            }
        }
    }

    // Gets the controller at the specified index of the controllers list.
    public GameObject GetController(int index)
    {
        return controllers[index];
    }

    // Gets an throwable object corresponding to the specified index of the throwable objects list.
    // If index exceeds the length of the throwables list, fetches from the lever objects list instead.
    public GameObject GetThrowableObj(int index)
    {
        if (index >= throwableObjs.Length)
        {
            return GetLeverObj(index);
        }
        return throwableObjs[index];
    }

    // Gets the lever object corresponding to the specified index of the lever objects list.
    public GameObject GetLeverObj(int index)
    {
        return leverObjs[index - throwableObjs.Length];
    }

}
