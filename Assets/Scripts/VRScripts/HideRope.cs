using UnityEngine;

public class HideRope : MonoBehaviour
{
    [SerializeField] private Transform parentObject;
    private Transform[] pieces;
    [SerializeField] private Transform hiddenPoint;
    [SerializeField] private Transform visiblePoint;

    private void Awake()
    {
        pieces = new Transform[parentObject.childCount];
        for (int i = 0; i < parentObject.childCount; i++)
        {
            pieces[i] = parentObject.GetChild(i);
            
            CheckMesh(pieces[i]);
        }
    }

    private void OnEnable()
    {
        EventsManager.OnUpdateHiddenBar += UpdateRope;
    }

    private void OnDisable()
    {
        EventsManager.OnUpdateHiddenBar -= UpdateRope;
    }
    
    private void OnTriggerExit(Collider _other)
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if(_other.transform == pieces[i])
            {        
                CheckMesh(pieces[i]);
            }
        }        
    }

    private void UpdateRope()
    {
        // for (int i = 0; i < pieces.Length; i++)
        // {
        //     CheckMesh(pieces[i]);
        // }  
    }

    private void CheckMesh(Transform piece)
    {
        if(Vector3.Distance(piece.position, hiddenPoint.position) < Vector3.Distance(piece.position, visiblePoint.position))
        {
            piece.GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            piece.GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
