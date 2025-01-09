using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class gameController : MonoBehaviour
{
    [SerializeField] Camera maincam;
    [SerializeField] GameObject bacteria,toothbrush;
    //human
    [SerializeField] GameObject[] humanTeeth,humanToothPieces;
    [SerializeField] GameObject happyFacehuman,humanMoutharea,humanBrokenToothArea;
    //crocodile
    [SerializeField] GameObject[] crocTeeth,crocToothPieces;
    [SerializeField] GameObject crocMoutharea,crocBrokenToothArea,crocHappyFace;
    //shark
    [SerializeField] GameObject[] sharkTeeth,sharkToothPieces;
    [SerializeField] GameObject sharkMouthArea,sharkBrokenToothArea,sharkHappyFace;
    //ui
    [SerializeField] Button humanButton,sharkButton,crocodileButton,exitButton;
    [SerializeField] GameObject humanMouth,crocodileMouth,sharkMouth;
    [SerializeField] GameObject brokenTooth,buttons;
    [SerializeField] toothbrushScript tbs;
    private List<Vector2> teethCorrectPositions;
    private GameObject curBacteria,curTooth;
    private Vector2 ToothBrushInitPosition;
    private int index;
    private int brokenTeethIndex;
    private bool human,crocodile,shark;
    private GameObject[] teeth,toothPieces;
    private GameObject happyFace,brokenToothArea,mouthArea,curMouth;
    private Color32 backColor;
    private bool gameon;
    private static int BACTERIA_TO_KILL=6;
    private DentalCareEntryPoint _entryPoint;
    private SpriteResolver sr;
    Tween toothbrushMove;
    private List<Vector2> humanCorrect,sharkCorrect,crocodileCorrect;


    void Awake(){
        Application.targetFrameRate=120;
        
    }
    void Start(){
        humanCorrect=new List<Vector2>();
        crocodileCorrect=new List<Vector2>();
        sharkCorrect=new List<Vector2>();
        humanButton.onClick.AddListener(humantrue);
        crocodileButton.onClick.AddListener(crocodiletrue);
        sharkButton.onClick.AddListener(sharktrue);
        exitButton.onClick.AddListener(reload);

        gameon=false;
        sr=toothbrush.GetComponent<SpriteResolver>();
        for(int i=0;i<humanToothPieces.Length;i++){humanCorrect.Add(humanToothPieces[i].transform.position); }
        for(int i=0;i<crocToothPieces.Length;i++){crocodileCorrect.Add(crocToothPieces[i].transform.position);}
        for(int i=0;i<sharkToothPieces.Length;i++){sharkCorrect.Add(sharkToothPieces[i].transform.position);}
        teethCorrectPositions=new List<Vector2>();
    }


    public void finishGame()
    {
        _entryPoint.InvokeGameFinished();
    }

    public void SetEntryPoint(DentalCareEntryPoint entryPoint){_entryPoint=entryPoint;}

    private void humantrue(){
        human=true;
        humanMouth.SetActive(true);
        curMouth=humanMouth;
        sr.SetCategoryAndLabel("textures","human");
        init();
    }

    private void crocodiletrue(){
        crocodile=true;
        crocodileMouth.SetActive(true);
        curMouth=crocodileMouth;
        sr.SetCategoryAndLabel("textures","crocodile");
        init();
    }

    private void sharktrue(){
        shark=true;
        sharkMouth.SetActive(true);
        curMouth=sharkMouth;
        sr.SetCategoryAndLabel("textures","shark");
        init();
    }

    void init(){
        buttons.gameObject.SetActive(false);
        curBacteria=bacteria;
        brokenTooth.gameObject.SetActive(false);
        if(human){
            backColor=new Color32(255,204,171,255);
            teeth=humanTeeth;
            teethCorrectPositions=humanCorrect;
            brokenTeethIndex=7;
            toothPieces=new GameObject[humanToothPieces.Length];
            toothPieces=humanToothPieces;
            happyFace=happyFacehuman;
            mouthArea=humanMoutharea;
            brokenToothArea=humanBrokenToothArea;
            ToothBrushInitPosition=new Vector3(6.88999987f,0.610000014f,0);
        }
        else if (crocodile){
            curBacteria.transform.localScale=new Vector3(curBacteria.transform.localScale.x/2,curBacteria.transform.localScale.y/2,curBacteria.transform.localScale.z/2);
            backColor=new Color32(79,131,62,255);
            teeth=crocTeeth;
            teethCorrectPositions=crocodileCorrect;
            brokenTeethIndex=8;
            toothPieces=new GameObject[crocToothPieces.Length];
            toothPieces=crocToothPieces;
            happyFace=crocHappyFace;
            mouthArea=crocMoutharea;
            brokenToothArea=crocBrokenToothArea;
            ToothBrushInitPosition=new Vector3(6.42999983f,0.310000002f,0);
        }
        else if(shark){
            curBacteria.transform.localScale=new Vector3(curBacteria.transform.localScale.x/2,curBacteria.transform.localScale.y/2,curBacteria.transform.localScale.z/2);
            backColor=new Color32(68,127,195,255);
            teeth=sharkTeeth;
            teethCorrectPositions=sharkCorrect;
            brokenTeethIndex=1;
            toothPieces=new GameObject[sharkToothPieces.Length];
            toothPieces=sharkToothPieces;
            happyFace=sharkHappyFace;
            mouthArea=sharkMouthArea;
            brokenToothArea=sharkBrokenToothArea;
            ToothBrushInitPosition=new Vector3(6.36000013f,0.0399999917f,0);
        }
        mouthArea.gameObject.SetActive(true);
        spawnBacteria();
        curTooth=null;
        index=0;
        maincam.backgroundColor=backColor;
        gameon=true;
        tbs.cleanteeth=0;
        toothbrush.transform.position=new Vector3(11.5699997f,ToothBrushInitPosition.y,0);

    }

    // Update is called once per frame
    void Update(){
        if(gameon){
            if(!curBacteria.GetComponent<bacteriaManager>().gameOn&&curBacteria.GetComponent<bacteriaManager>().deadBacteriaCount<BACTERIA_TO_KILL){
                spawnBacteria();
            }
            else if(!curBacteria.GetComponent<bacteriaManager>().gameOn&&curBacteria.GetComponent<bacteriaManager>().deadBacteriaCount==BACTERIA_TO_KILL){
                toothbrush.gameObject.SetActive(true);
                toothbrush.transform.DOMove(ToothBrushInitPosition,0.3f).SetEase(Ease.Linear).OnComplete(()=>{
                    toothbrush.transform.DOMoveX(ToothBrushInitPosition.x+0.7f,0.3f).SetEase(Ease.Linear).OnComplete(()=>{
                        toothbrush.transform.DOMoveX(ToothBrushInitPosition.x,0.3f).SetEase(Ease.Linear).OnComplete(()=>tbs.gameon=true);
                    });
                });
                curBacteria.GetComponent<bacteriaManager>().deadBacteriaCount++;
            }
            if(tbs.cleanteeth==teeth.Length){
                tbs.gameon=false;
                Collider2D[] col=toothbrush.GetComponents<Collider2D>();
                for(int i=0;i<col.Length;i++) col[i].enabled=false;
                tbs.cleanteeth++;
                toothbrushMove=toothbrush.transform.DOMove(new Vector3(11.5699997f,ToothBrushInitPosition.y),0.5f).SetEase(Ease.Linear).OnComplete(()=>{toothbrushMove=null;});
                Invoke("repairBrokenTooth",1f);
            }
            if(curTooth!=null&&index<5&&index>0){
                if(curTooth.GetComponent<teethPieceMovement>().isThere){
                    curTooth=toothPieces[index];
                    curTooth.GetComponent<teethPieceMovement>().init(teethCorrectPositions[index]);
                    index++;
                }
            }else if(index==5){
                bool correct=true;
                for(int i=0;i<toothPieces.Length;i++){
                    correct=correct&&toothPieces[i].GetComponent<DragAndDrop>().IsSnapped;
                }
                if(correct) {
                    index++;
                    happyFace.gameObject.SetActive(true);
                    Invoke("showPrettyTeeth",2f);
                }
            }
        }
    }

    private void showPrettyTeeth(){
        happyFace.gameObject.SetActive(false);
        brokenToothArea.gameObject.SetActive(false);
        brokenTooth.gameObject.SetActive(false);
        maincam.backgroundColor=backColor;
        mouthArea.gameObject.SetActive(true);
        teeth[brokenTeethIndex].GetComponent<SpriteResolver>().SetCategoryAndLabel("textures","clean");
        for(int i=0;i<teeth.Length;i++){
            teeth[i].transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
            teeth[i].GetComponentInChildren<Animator>().SetTrigger("happy");
        }
        Invoke("reload",3f);
    }

    void reload(){
        if(human||crocodile||shark){
            for(int i=0;i<teeth.Length;i++){
                if(i!=brokenTeethIndex) teeth[i].GetComponent<SpriteResolver>().SetCategoryAndLabel("textures","dirty");
                else teeth[i].GetComponent<SpriteResolver>().SetCategoryAndLabel("textures","broken_dirty");
                teeth[i].GetComponentInChildren<Animator>().ResetTrigger("happy");
                teeth[i].transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
            }
            happyFace.gameObject.SetActive(false);
            brokenToothArea.gameObject.SetActive(false);
            brokenTooth.gameObject.SetActive(false);
            mouthArea.gameObject.SetActive(false);
            curMouth.SetActive(false);
            buttons.SetActive(true);
            if(toothbrushMove!=null) toothbrushMove.Kill();
            toothbrush.gameObject.SetActive(false);
            toothbrush.transform.position=new Vector3(11.5699997f,-0.569999993f,0);
            bacteria.GetComponent<bacteriaManager>().deadBacteriaCount=0;
            tbs.cleanteeth=0;
            maincam.backgroundColor=new Color32(95,156,246,255);
            for(int i=0;i<toothPieces.Length;i++) {
                DragAndDrop tmp=toothPieces[i].GetComponent<DragAndDrop>(); 
                tmp.IsSnapped=false;
                tmp.CanDrag=true;
                Vector2 pos=teethCorrectPositions[i];
                toothPieces[i].transform.position=pos;
            }
            Collider2D[] col=toothbrush.GetComponents<Collider2D>();
            for(int i=0;i<col.Length;i++)col[i].enabled=true;
            CancelInvoke();
            gameon=false;
            index=0;
            curTooth=null;
            human=false;
            crocodile=false;
            shark=false;
            brokenTooth.gameObject.SetActive(true);
        }else finishGame();
    }


    private void repairBrokenTooth(){
        if(gameon){
            toothbrush.gameObject.SetActive(false);
            mouthArea.SetActive(false);
            maincam.backgroundColor=new Color32(95,156,246,255);
            brokenToothArea.gameObject.SetActive(true);
            brokenTooth.gameObject.SetActive(true);
            curTooth=toothPieces[index];
            for(int i=0;i<toothPieces.Length;i++) {
                toothPieces[i].transform.position=teethCorrectPositions[i];}
            Invoke("movePieces",1f);
        }
    }
    void movePieces(){
        curTooth.GetComponent<teethPieceMovement>().init(teethCorrectPositions[index]);
        index++;
    }

    private void spawnBacteria(){
        int teethnum=UnityEngine.Random.Range(0,teeth.Length);
        if(crocodile){
            while(new List<int>{0,5,6,11}.Contains(teethnum)) teethnum=UnityEngine.Random.Range(0,teeth.Length);
        }
        else if(shark){
            while(new List<int>{2,5,11,12}.Contains(teethnum)) teethnum=UnityEngine.Random.Range(0,teeth.Length);
        }
        Quaternion rot=teeth[teethnum].transform.rotation;
        curBacteria.transform.localScale=new Vector3(0.5f,0.5f,0.5f);
        curBacteria.transform.parent=teeth[teethnum].transform;
        curBacteria.transform.localPosition=new Vector2(0,0);
        curBacteria.transform.rotation=rot;
        curBacteria.GetComponent<bacteriaManager>().init(teethnum,teeth.Length);
    }
}
