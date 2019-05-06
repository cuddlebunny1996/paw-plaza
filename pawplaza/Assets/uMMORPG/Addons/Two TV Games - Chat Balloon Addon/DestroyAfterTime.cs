// Destroys the GameObject after a certain time.
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float time = 1;

    void Start()
    {
        Destroy(gameObject, time);
    }
}
