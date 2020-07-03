using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameObjectComparer : IComparer
{
    public int Compare(object x, object y)
    {
        String nx = ((GameObject)x).name;
        String ny = ((GameObject)y).name;
        return nx.CompareTo(ny);
    }
}

public class GameObjectPhotonComparer : IComparer
{
    public int Compare(object x, object y)
    {
        int nx = ((GameObject)x).GetComponent<PhotonView>().viewID;
        int ny = ((GameObject)y).GetComponent<PhotonView>().viewID;
        return nx.CompareTo(ny);
    }
}

public class ObjectManager : Photon.PunBehaviour
{
    private GameObject[] throwableObjs;
    private ObjectStateServer[] throwables;
    private GameObject[] leverObjs;
    private LeverState_Client[] levers;
    private GameObject[] controllers;
    private Hashtable playerForceCache;
    private Hashtable controllerIndexMap;
    private List<int>[] toSeparate;
    private string gameVersion = "0.1";
    private GameObjectComparer comp;
    private GameObjectPhotonComparer photonComp;
    private Boolean resolvedControllers;
    public float networked_force_factor;

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = true;
        PhotonNetwork.automaticallySyncScene = true;
    }

    void Start()
    {
        resolvedControllers = false;
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.ConnectUsingSettings(gameVersion);
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions() { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");

        PhotonNetwork.Instantiate("PlayerPrefab", new Vector3(PhotonNetwork.player.ID, 2, 2), Quaternion.identity, 0);

        comp = new GameObjectComparer();
        photonComp = new GameObjectPhotonComparer();
        RefreshControllers();
        
        throwableObjs = GameObject.FindGameObjectsWithTag("Throwable");
        Array.Sort(throwableObjs, comp);
        throwables = new ObjectStateServer[throwableObjs.Length];
        int id = 10000;
        for (int i = 0; i < throwableObjs.Length; i++)
        {
            // Add PhotonTransformView
            PhotonView pv = throwableObjs[i].AddComponent<PhotonView>();
            PhotonTransformView ptv = throwableObjs[i].AddComponent<PhotonTransformView>();
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
            // Modify ObjectStates
            throwables[i] = throwableObjs[i].GetComponent<ObjectStateServer>();
            throwables[i].SetObjectIndex(i);
        }

        leverObjs = GameObject.FindGameObjectsWithTag("Lever");
        Array.Sort(leverObjs, comp);
        levers = new LeverState_Client[leverObjs.Length];
        for (int i = 0; i < leverObjs.Length; i++)
        {
            // Modify LeverStates
            levers[i] = leverObjs[i].GetComponent<LeverState_Client>();
            levers[i].SetObjectIndex(throwableObjs.Length + i);
        }

        if (PhotonNetwork.isMasterClient == false)
        {
            return;
        }

        playerForceCache = new Hashtable();
    }

    private void RefreshControllers()
    {
        Debug.Log("RefreshControllers");
        List<GameObject> controllerList = new List<GameObject>();
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            int id = PhotonNetwork.MAX_VIEW_IDS * player.ID + 5;
            if (PhotonView.Find(id) != null)
            {
                GameObject handObj = PhotonView.Find(id).gameObject;
                controllerList.Add(handObj);
                if (!player.IsMasterClient)
                {
                    handObj.GetComponent<PlayerForce>().force_factor = networked_force_factor;
                }
            }
            if (PhotonView.Find(id + 1) != null)
            {
                GameObject handObj = PhotonView.Find(id + 1).gameObject;
                controllerList.Add(PhotonView.Find(id + 1).gameObject);
                if (!player.IsMasterClient)
                {
                    handObj.GetComponent<PlayerForce>().force_factor = networked_force_factor;
                }
            }
        }
        resolvedControllers = controllerList.Count == 2 * PhotonNetwork.playerList.Length;
        if (!resolvedControllers)
        {
            return;
        }
        controllers = controllerList.ToArray();
        Array.Sort(controllers, photonComp);
        controllerIndexMap = new Hashtable();
        toSeparate = new List<int>[controllers.Length];
        for (int i = 0; i < controllers.Length; i++)
        {
            Controller_State_Client client = controllers[i].GetComponent<Controller_State_Client>();
            PhotonView targetView = client.photonView;
            if (PhotonNetwork.isMasterClient)
                targetView.RPC("SetObjectIndex", PhotonTargets.All, i);
            controllerIndexMap.Add(controllers[i], i);
            toSeparate[i] = new List<int>();
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        RefreshControllers();
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer newPlayer)
    {
        RefreshControllers();
    }

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

    void Update()
    {
        if (!resolvedControllers)
        {
            RefreshControllers();
        }
    }

    void LateUpdate()
    {
        if (PhotonNetwork.isMasterClient == false)
        {
            return;
        }

        // 1. Apply pull force to throwable interactees
        for (int i = 0; i < throwables.Length; i++)
        {
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
            GameObject leverObj = leverObjs[i];
            LeverState_Client leverState = levers[i];
            Vector3 leverPos = leverObj.transform.position;
            foreach (GameObject hand in leverState.GetInteractors())
            {
                Vector3 pull = hand.transform.position - leverObj.transform.position;
                if (pull.magnitude > GetPlayerForce(hand).separation_threshold)
                {
                    toSeparate[(int)controllerIndexMap[hand]].Add(leverState.GetObjectIndex());
                }
                leverObj.GetComponent<LeverState_Client>().UpdateActivation(hand.transform.position - leverPos);
            }
        }
        // 3. Release any interactees marked for separation
        for (int j = 0; j < toSeparate.Length; j++)
        {
            if (toSeparate[j].Count > 0)
            {
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
                toSeparate[j].Clear();
            }
        }
    }

    public GameObject GetController(int index)
    {
        return controllers[index];
    }

    public GameObject GetThrowableObj(int index)
    {
        if (index >= throwableObjs.Length)
        {
            return GetLeverObj(index);
        }
        return throwableObjs[index];
    }

    public GameObject GetLeverObj(int index)
    {
        return leverObjs[index - throwableObjs.Length];
    }

}
