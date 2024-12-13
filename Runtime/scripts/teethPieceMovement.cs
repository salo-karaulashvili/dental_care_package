using DG.Tweening;
using UnityEngine;

public class teethPieceMovement : MonoBehaviour
{
    [SerializeField] Transform destination;
    public bool isThere;
    private bool beagn=false;
    Vector2 correctPosition;
    private DragAndDrop dragAndDrop;


    private void OnEnable(){
        dragAndDrop.OnIncorrectSnap += incorrectAnimation;
    }

    private void OnDisable(){
        dragAndDrop.OnIncorrectSnap -= incorrectAnimation;
    }

    private void Awake(){
        dragAndDrop = GetComponent<DragAndDrop>();
    }

    public void init(Vector2 correctPosition){
        isThere=false;
        this.correctPosition=correctPosition;
        transform.DOMove(destination.position,0.5f);
        Invoke("trueisThere",0.7f);
        
    }
    void trueisThere(){
        isThere=true;
        dragAndDrop.TargetPositions.Add(correctPosition);
        dragAndDrop.StartPosition=destination.position;
        dragAndDrop.enabled=true;
    }

    private void incorrectAnimation(int index){

        transform.DORotate(new Vector3(0,0,-12),0.2f).OnComplete(()=>{
            transform.DORotate(new Vector3(0,0,12),0.2f).OnComplete(()=>{
                transform.DORotate(new Vector3(0,0,0),0.2f);
            });
        });
    }

}
