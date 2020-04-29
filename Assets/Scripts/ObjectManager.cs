using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : Photon.PunBehaviour
{
    private GameObject[] throwableObjs;
    private ObjectState[] throwables;
    private GameObject[] controllers;
    private Hashtable playerForceCache;
    private Hashtable controllerIndexMap;
    private List<int>[] toSeparate;
    private string gameVersion = "0.1";

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = true;
        PhotonNetwork.automaticallySyncScene = true;
    }

    void Start()
    {
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.ConnectUsingSettings(gameVersion);
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions() { MaxPlayers = 2 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");

        PhotonNetwork.Instantiate("PlayerPrefab", new Vector3(1, 2, 2), Quaternion.identity, 0);
        throwableObjs = GameObject.FindGameObjectsWithTag("Throwable");
        throwables = new ObjectState[throwableObjs.Length];
        int id = 10000;
        for (int i = 0; i < throwables.Length; i++)
        {
            // Add PhotonTransformView
            PhotonView pv = throwableObjs[i].AddComponent<PhotonView>();
            PhotonTransformView ptv = throwableObjs[i].AddComponent<PhotonTransformView>();
            ptv.m_PositionModel.SynchronizeEnabled = true;
            ptv.m_RotationModel.SynchronizeEnabled = true;
            pv.ObservedComponents = new List<Component>();
            pv.ObservedComponents.Add(ptv);
            pv.viewID = id++;
            // Modify ObjectStates
            throwables[i] = throwableObjs[i].GetComponent<ObjectState>();
            throwables[i].SetObjectIndex(i);
        }
        
        controllers = GameObject.FindGameObjectsWithTag("Hand");
        controllerIndexMap = new Hashtable();
        toSeparate = new List<int>[controllers.Length];
        for (int i = 0; i < controllers.Length; i++)
        {
            Controller_State_Client client = controllers[i].GetComponent<Controller_State_Client>();
            client.SetObjectIndex(i);
            controllerIndexMap.Add(controllers[i], i);
            toSeparate[i] = new List<int>();
        }

        if (PhotonNetwork.isMasterClient == false)
        {
            return;
        }

        playerForceCache = new Hashtable();
    }

    private void RefreshControllers()
    {
        controllers = GameObject.FindGameObjectsWithTag("Hand");
        controllerIndexMap = new Hashtable();
        toSeparate = new List<int>[controllers.Length];
        for (int i = 0; i < controllers.Length; i++)
        {
            Controller_State_Client client = controllers[i].GetComponent<Controller_State_Client>();
            PhotonView targetView = client.photonView;
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
        Debug.Log("RelayOnTriggerPress: " + ctrl + ", " + obj);
        if (PhotonNetwork.isMasterClient == false)
        {
            return;
        }
        throwables[obj].OnTriggerPress(controllers[ctrl]);
    }

    [PunRPC]
    void RelayOnTriggerRelease(int ctrl, int obj)
    {
        Debug.Log("RelayOnTriggerRelease: " + ctrl + ", " + obj);
        if (PhotonNetwork.isMasterClient == false)
        {
            return;
        }
        throwables[obj].OnTriggerRelease(controllers[ctrl]);
        PlayerForce force = GetPlayerForce(controllers[ctrl]);
        if (force != null)
        {
            force.ApplyThrowForce(throwableObjs[obj]);
        }
    }

    void LateUpdate()
    {
        if (PhotonNetwork.isMasterClient == false)
        {
            return;
        }

        // 1. Apply pull force to interactees
        for (int i = 0; i < throwables.Length; i++)
        {
            GameObject throwableObj = throwableObjs[i];
            ObjectState throwableState = throwables[i];
            int j = 0;
            foreach (GameObject hand in throwableState.GetInteractors())
            {
                if (!GetPlayerForce(hand).ApplyForce(throwableObj))
                {
                    toSeparate[(int) controllerIndexMap[hand]].Add(j);
                }
                j++;
            }
            // 2. Release any interactees marked for separation
            for (j = 0; j < toSeparate.Length; j++)
            {
                if (toSeparate[j].Count > 0)
                {
                    PhotonView targetView = controllers[j].GetComponent<PhotonView>();
                    int[] sepVals = toSeparate[j].ToArray();
                    targetView.RPC("RemoveInteractee", PhotonTargets.All, j, sepVals);
                    for (int k = 0; k < sepVals.Length; k++)
                    {
                        throwables[sepVals[k]].OnTriggerRelease(controllers[j]);
                    }
                    toSeparate[j].Clear();
                }
            }
        }
    }

    public GameObject GetController(int index)
    {
        return controllers[index];
    }

}
