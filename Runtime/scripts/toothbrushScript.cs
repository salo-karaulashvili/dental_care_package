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
    void Start(){
        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
        initOffset1=colliders[0].offset;
        initOffset2=colliders[1].offset;
        flippedOffset1=new Vector2(initOffset1.x,-initOffset1.y);
        flippedOffset2=new Vector2(initOffset2.x,-initOffset2.y);
    }
    void Update(){
        if(Input.touchCount>0&&gameon){
            Touch touch=Input.GetTouch(0);
            Vector2 mousePos=touch.position;
            Vector2 screenPoint=mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCam.nearClipPlane));
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePos), Vector2.zero);
            if(touch.phase==TouchPhase.Began) {
                if(hit){
                    if(hit.collider.gameObject.tag=="toothbrush"){
                        began=true;
                        delta=new Vector2(transform.position.x-screenPoint.x,transform.position.y-screenPoint.y);
                    }
                }
            }
            else if(touch.phase==TouchPhase.Moved&&began){
                transform.position=new Vector2(screenPoint.x+delta.x,screenPoint.y+delta.y);
            }
            else if(touch.phase==TouchPhase.Ended) began=false;
        }
    }
    private void OnTriggerStay2D(Collider2D other){
        if(other.gameObject.tag=="lowertooth") {
            GetComponent<SpriteRenderer>().flipY=true;
            BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
            colliders[0].offset=flippedOffset1;
            colliders[1].offset=flippedOffset2;
        }
        else if(other.gameObject.tag=="uppertooth"){
            GetComponent<SpriteRenderer>().flipY=false;
            BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
            colliders[0].offset=initOffset1;
            colliders[1].offset=initOffset2;
        }
        SpriteResolver sp=other.GetComponent<SpriteResolver>();
        if(sp!=null){
            if(timer>0.5f&&!other.GetComponent<SpriteResolver>().GetLabel().Contains("clean")){
                timer=0;
                if(other.GetComponent<SpriteResolver>().GetLabel().Contains("broken")){
                    other.GetComponent<SpriteResolver>().SetCategoryAndLabel("textures","broken_clean");
                }else{
                    other.GetComponent<SpriteResolver>().SetCategoryAndLabel("textures","clean");
                    other.GetComponentInChildren<Animator>().SetTrigger("happy");
                }
                cleanteeth++;
                Transform foamChild=other.gameObject.transform.Find("Foam").transform.GetChild(0);
                var main1=foamChild.GetComponent<ParticleSystem>().main;
                var main2=foamChild.GetChild(0).GetComponent<ParticleSystem>().main;
                main1.loop=false;
                main2.loop=false;
                main1.startLifetime=0.5f;
                main2.startLifetime=0.5f;
            }
            else{
                timer+=Time.deltaTime;
                if(!sp.GetLabel().Contains("clean")) {
                    other.gameObject.transform.Find("Foam").gameObject.SetActive(true);
                }
            } 
        }
    }
    private void OnTriggerExit2D(Collider2D other){
        timer=0f;
    }
}
