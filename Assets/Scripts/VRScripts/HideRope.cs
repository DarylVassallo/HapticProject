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
    private void OnTriggerStay(Collider _other)
    {
        Debug.Log(this.gameObject + " : Stay Collided with : " + _other.gameObject);       
    }

    private void OnTriggerEnter(Collider _other)
    {
        Debug.Log(this.gameObject + " : Enter Collided with : " + _other.gameObject);     
    }
    
    private void OnTriggerExit(Collider _other)
    {
        Debug.Log(this.gameObject + " : Exit Collided with : " + _other.gameObject); 
        for (int i = 0; i < pieces.Length; i++)
        {
            if(_other.transform == pieces[i])
            {        
                Debug.Log(this.gameObject + " : piece : " + pieces[i]);        
                CheckMesh(pieces[i]);
            }
        }        
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
