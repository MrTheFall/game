using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AimSync : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform weaponAngleObject;
    public Transform weaponParent;
    private float aimAngle;

    public void Start()
    {
        weaponParent = gameObject.transform.root.Find("Camera Holder/Recoil Camera/RotationSyncObject").transform;
    }

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
        targetRotation = new Quaternion(targetRotation.x, targetRotation.y, 0, targetRotation.w);
        weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 4f);
        var angles = weaponParent.rotation.eulerAngles;
        angles.z = 0;
        weaponParent.transform.eulerAngles = angles;

        Vector3 finalRotation = weaponParent.localEulerAngles;
        finalRotation.y = cacheEulY;

        weaponParent.localEulerAngles = finalRotation;
    }


    public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
    {
        if (p_stream.IsWriting)
        {
            p_stream.SendNext((float)weaponAngleObject.localEulerAngles.x);
        }
        else
        {
            aimAngle = (float)p_stream.ReceiveNext();
        }
    }
}
