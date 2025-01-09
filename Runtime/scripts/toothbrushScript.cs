using UnityEngine;
using UnityEngine.U2D.Animation;

public class toothbrushScript : MonoBehaviour
{
    [SerializeField] Camera mainCam;
    private float timer=0f;
    public int cleanteeth=0;
    private Vector2 delta;
    private bool began=false;
    public bool gameon;
    Vector2 initOffset1,initOffset2;
    Vector2 flippedOffset1,flippedOffset2;
    Collider2D[] col;
    void Start(){
        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
        initOffset1=colliders[0].offset;
        initOffset2=colliders[1].offset;
        flippedOffset1=new Vector2(initOffset1.x,-initOffset1.y);
        flippedOffset2=new Vector2(initOffset2.x,-initOffset2.y);
        col=GetComponents<Collider2D>();
        //for(int i=0;i<col.Length;i++) col[i].enabled=false;
    }
    void Update(){
        if(Input.touchCount>0&&gameon){
            Touch touch=Input.GetTouch(0);
            Vector2 mousePos=touch.position;
            Vector2 screenPoint=mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCam.nearClipPlane));
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero);
            if(touch.phase==TouchPhase.Began) {
                if(hit){
                    if(hit.collider.transform==this.transform){
                        began=true;
                        delta=new Vector2(transform.position.x-screenPoint.x,transform.position.y-screenPoint.y);
                        
                        //for(int i=0;i<col.Length;i++) col[i].enabled=true;
                    }
                }
            }
            else if(touch.phase==TouchPhase.Moved&&began){
                transform.position=new Vector2(screenPoint.x+delta.x,screenPoint.y+delta.y);
            }
            else if(touch.phase==TouchPhase.Ended) {
                began=false;
                //for(int i=0;i<col.Length;i++) col[i].enabled=false;
            }
        }
    }
    private void OnTriggerStay2D(Collider2D other){
        if(began){
            if(other.transform.parent.gameObject.name=="lowerTeeth") {
                GetComponent<SpriteRenderer>().flipY=true;
                col[0].offset=flippedOffset1;
                col[1].offset=flippedOffset2;
            }
            else if(other.transform.parent.gameObject.name=="upperTeeth"){
                GetComponent<SpriteRenderer>().flipY=false;
                col[0].offset=initOffset1;
                col[1].offset=initOffset2;
            }
            //Debug.Log(other.transform.parent.gameObject.name);
            SpriteResolver sp=other.GetComponent<SpriteResolver>();
            if(sp!=null){
                if(timer>0.1f&&!other.GetComponent<SpriteResolver>().GetLabel().Contains("clean")){
                    timer=0;
                    if(other.GetComponent<SpriteResolver>().GetLabel().Contains("broken")){
                        other.GetComponent<SpriteResolver>().SetCategoryAndLabel("textures","broken_clean");
                    }else{
                        other.GetComponent<SpriteResolver>().SetCategoryAndLabel("textures","clean");
                        other.GetComponentInChildren<Animator>().SetTrigger("happy");
                    }
                    cleanteeth++;
                }
                else{
                    timer+=Time.deltaTime;
                    if(!sp.GetLabel().Contains("clean")) {
                        Transform foam=other.gameObject.transform.GetChild(1);
                        if(!foam.GetChild(0).gameObject.activeSelf){
                            foam.GetChild(0).gameObject.SetActive(true);
                            foam.GetChild(0).GetChild(0).gameObject.SetActive(true);
                            //Debug.Log("reset");
                        }
                    }
                } 
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other){
        timer=0f;
    }
    // void  OnMouseDown(){
    //     Vector2 mousePos=Input.mousePosition;
    //     Vector2 screenPoint=mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCam.nearClipPlane));
    //     began=true;
    //     delta=new Vector2(transform.position.x-screenPoint.x,transform.position.y-screenPoint.y);
    // }
    // void OnMouseDrag(){
    //     Vector2 mousePos=Input.mousePosition;
    //     Vector2 screenPoint=mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCam.nearClipPlane));
    //     transform.position=new Vector2(screenPoint.x+delta.x,screenPoint.y+delta.y);
    // }
    void OnMouseUp()=>began=false;
}
