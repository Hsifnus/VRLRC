using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    private GameObject[] throwableObjs;
    private ObjectState[] throwables;
    private GameObject[] controllers;
    private Hashtable playerForceCache;
    private Hashtable controllerIndexMap;
    private List<int>[] toSeparate;
    // Photon view
    private PhotonView photonView;

    void OnJoinedRoom()
    {
        photonView = PhotonView.Get(this);
        throwableObjs = GameObject.FindGameObjectsWithTag("Throwable");
        throwables = new ObjectState[throwableObjs.Length];
        for (int i = 0; i < throwables.Length; i++)
        {
            throwables[i] = throwableObjs[i].GetComponent<ObjectState>();
            throwables[i].SetObjectIndex(i);
        }
        
        controllers = GameObject.FindGameObjectsWithTag("GameController");
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
        throwables[obj].OnTriggerPress(controllers[ctrl]);
    }

    [PunRPC]
    void RelayOnTriggerRelease(int ctrl, int obj)
    {
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
                    photonView.RPC("ControllerRemoveInteractee", PhotonTargets.All, j, toSeparate[j].ToArray());
                    toSeparate[j].Clear();
                }
            }
        }
    }

}
