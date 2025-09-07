using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        if (!player) return;

        float fixedY = transform.position.y;
        if (Vector3.Distance(transform.position, player.position) > 0.01f)
            transform.LookAt(player, Vector3.up);
        transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);
    }
}