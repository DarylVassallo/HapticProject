using UnityEngine;
using System.Collections;

public class AnimationFunctions : MonoBehaviour
{
    public void OnDestroyEntity()
    {
        StartCoroutine(DelayDeath());
    }

    IEnumerator DelayDeath()
    {
        yield return new WaitForSeconds(1f);
        Destroy(this.transform.parent.gameObject);
    }
}
