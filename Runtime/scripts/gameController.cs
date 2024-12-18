using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class gameController : MonoBehaviour
{
    [SerializeField] Camera maincam;
    [SerializeField] GameObject bacteria,toothbrush,background;
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
    private GameObject happyFace,brokenToothArea,mouthArea;
    private Color32 backColor;
    private bool gameon;
    private static int BACTERIA_TO_KILL=6;
    private DentalCareEntryPoint _entryPoint;
    private SpriteResolver sr;


    
    void Start(){
        humanButton.onClick.AddListener(humantrue);
        crocodileButton.onClick.AddListener(crocodiletrue);
        sharkButton.onClick.AddListener(sharktrue);

        gameon=false;
        sr=toothbrush.GetComponent<SpriteResolver>();
    }

    public void finishGame()
    {
        _entryPoint.InvokeGameFinished();
    }

    public void SetEntryPoint(DentalCareEntryPoint entryPoint){_entryPoint=entryPoint;}

    private void humantrue(){
        human=true;
        humanMouth.SetActive(true);
        sr.SetCategoryAndLabel("textures","human");
        init();
    }

    private void crocodiletrue(){
        crocodile=true;
        crocodileMouth.SetActive(true);
        sr.SetCategoryAndLabel("textures","crocodile");
        init();
    }

    private void sharktrue(){
        shark=true;
        sharkMouth.SetActive(true);
        sr.SetCategoryAndLabel("textures","shark");
        init();
    }

    void init(){
        buttons.gameObject.SetActive(false);
        teethCorrectPositions=new List<Vector2>();
        curBacteria=bacteria;
        if(human){
            backColor=new Color32(255,204,171,255);
            teeth=humanTeeth;
            for(int i=0;i<humanToothPieces.Length;i++){teethCorrectPositions.Add(humanToothPieces[i].gameObject.transform.position);}
            brokenTeethIndex=7;
            toothPieces=humanToothPieces;
            happyFace=happyFacehuman;
            mouthArea=humanMoutharea;
            brokenToothArea=humanBrokenToothArea;
        }
        else if (crocodile){
            curBacteria.transform.localScale=new Vector3(curBacteria.transform.localScale.x/2,curBacteria.transform.localScale.y/2,curBacteria.transform.localScale.z/2);
            backColor=new Color32(79,131,62,255);
            teeth=crocTeeth;
            for(int i=0;i<crocToothPieces.Length;i++){teethCorrectPositions.Add(crocToothPieces[i].gameObject.transform.position);}
            brokenTeethIndex=8;
            toothPieces=crocToothPieces;
            happyFace=crocHappyFace;
            mouthArea=crocMoutharea;
            brokenToothArea=crocBrokenToothArea;
        }
        else if(shark){
            curBacteria.transform.localScale=new Vector3(curBacteria.transform.localScale.x/2,curBacteria.transform.localScale.y/2,curBacteria.transform.localScale.z/2);
            backColor=new Color32(68,127,195,255);
            teeth=sharkTeeth;
            for(int i=0;i<sharkToothPieces.Length;i++){teethCorrectPositions.Add(sharkToothPieces[i].gameObject.transform.position);}
            brokenTeethIndex=1;
            toothPieces=sharkToothPieces;
            happyFace=sharkHappyFace;
            mouthArea=sharkMouthArea;
            brokenToothArea=sharkBrokenToothArea;
        }
        mouthArea.gameObject.SetActive(true);
        spawnBacteria();
        ToothBrushInitPosition=toothbrush.transform.position;
        curTooth=null;
        index=0;
        maincam.backgroundColor=backColor;
        background.GetComponent<SpriteRenderer>().color=backColor;
        gameon=true;
    }

    // Update is called once per frame
    void Update(){
        if(gameon){
            if(!curBacteria.GetComponent<bacteriaManager>().gameOn&&curBacteria.GetComponent<bacteriaManager>().deadBacteriaCount<BACTERIA_TO_KILL){
                spawnBacteria();
            }
            else if(!curBacteria.GetComponent<bacteriaManager>().gameOn&&curBacteria.GetComponent<bacteriaManager>().deadBacteriaCount==BACTERIA_TO_KILL){
                toothbrush.gameObject.SetActive(true);
                tbs.gameon=true;
                curBacteria.GetComponent<bacteriaManager>().deadBacteriaCount++;
            }
            if(tbs.cleanteeth==teeth.Length){
                tbs.gameon=false;
                toothbrush.transform.position=Vector2.Lerp(toothbrush.transform.position,ToothBrushInitPosition,2f*Time.deltaTime);
                if(almostThere(toothbrush.transform.position,ToothBrushInitPosition,0.2f)){
                    tbs.cleanteeth++;
                    toothbrush.transform.position=ToothBrushInitPosition;
                    Invoke("repairBrokenTooth",5f);//aq xma ismis romelic instruqcias idzleva
                }
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
        background.GetComponent<SpriteRenderer>().color=backColor;
        mouthArea.gameObject.SetActive(true);
        teeth[brokenTeethIndex].GetComponent<SpriteResolver>().SetCategoryAndLabel("textures","clean");
        for(int i=0;i<teeth.Length;i++){
            teeth[i].gameObject.transform.GetChild(1).gameObject.SetActive(false);
            teeth[i].GetComponentInChildren<Animator>().SetTrigger("happy");
        }
        Invoke("reload",3f);
    }

    void reload(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    bool almostThere(Vector2 p1, Vector2 d,float threshold){ //p=pos1, d=destination
        return math.abs(d.y-p1.y)<threshold&&math.abs(d.x-p1.x)<threshold;
    }


    private void repairBrokenTooth(){
        toothbrush.gameObject.SetActive(false);
        mouthArea.SetActive(false);
        maincam.backgroundColor=new Color32(95,156,246,255);
        background.GetComponent<SpriteRenderer>().color=new Color32(95,156,246,255);
        brokenToothArea.gameObject.SetActive(true);
        brokenTooth.gameObject.SetActive(true);
        curTooth=toothPieces[index];
        Invoke("movePieces",2f);

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
