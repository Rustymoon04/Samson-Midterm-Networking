using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnHandler : NetworkBehaviour
{
    [ClientRpc]
    public void SetSpawnPositionClientRpc(
        Vector3 position,
        Quaternion rotation,
        ClientRpcParams rpcParams = default)
    {
        if (!IsOwner)
            return;

        CharacterController controller =
            GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        transform.SetPositionAndRotation(position, rotation);

        Physics.SyncTransforms();

        if (controller != null)
            controller.enabled = true;
    }
}