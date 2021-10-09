using System.Collections;
using UnityEngine;

public class DisableAfterSeconds : MonoBehaviour
{
    [SerializeField] float delayBeforeDisabling = 5;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(delayBeforeDisabling);
        gameObject.SetActive(false);
    }
}
