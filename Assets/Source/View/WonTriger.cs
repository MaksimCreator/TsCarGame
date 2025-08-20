using System;
using UnityEngine;

public class WonTriger : MonoBehaviour
{
    public event Action onWon;

    private void OnTriggerEnter(Collider other)
    {
       if (other.gameObject.layer == Config.LAYER_CAR_PLAYER)
            onWon.Invoke();
    }
}