using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        if (!player) return;
        transform.LookAt(player);
    }
}