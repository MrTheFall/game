using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AimSync : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform weaponParent;
    private float aimAngle;

    public void Update()
    {
        if (!photonView.IsMine)
        {
            RefreshMultiplayerState();
            return;
        }
    }
    private void RefreshMultiplayerState()
    {
        float cacheEulY = weaponParent.localEulerAngles.y;

        Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
        weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

        Vector3 finalRotation = weaponParent.localEulerAngles;
        finalRotation.y = cacheEulY;

        weaponParent.localEulerAngles = finalRotation;
    }


    public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
    {
        if (p_stream.IsWriting)
        {
            p_stream.SendNext((float)weaponParent.localEulerAngles.x);
        }
        else
        {
            aimAngle = (float)p_stream.ReceiveNext();
        }
    }
}
