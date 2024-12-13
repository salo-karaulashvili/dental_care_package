using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class bacteriaManager : MonoBehaviour
{
    [SerializeField] Camera mainCam;
    private static float IDLE_TIME=5f;
    private static float COOLDOWN_TIME=1f;
    private static float colliderRadius=0.9f;
    private static float colliderOffsety=1.63f;
    private bool neg;
    public bool gameOn;
    private float totaladded;
    public int deadBacteriaCount;
    private float cooldown=COOLDOWN_TIME;
    private static List<String> trigNames=new List<string>{"orange","green","pink","purple"};
    private static List<String> trigNamesDeath=new List<string>{"odie","gdie","pdie","pudie"};
    private int deathAnimIndx;
    public void init(int index,int length){
        gameObject.SetActive(true);
        if(length%2==0) neg=index<(length/2);
        else neg=index<((length+1)/2);
        gameOn=true;
        totaladded=0;
        Vector2 scale=transform.localScale;
        if(neg){
             scale.y=-1;
             scale.x=-1;
        }else{
             scale.y=1;
                scale.x=1;
        }
        transform.localScale=scale;
        int skinNum=UnityEngine.Random.Range(0,4);
        transform.GetComponent<Animator>().SetTrigger(trigNames[skinNum]);
        deathAnimIndx=skinNum;
        
    }
    void Update(){
        if(gameOn){
            if (Input.touchCount > 0){
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began){
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                    if(hit){
                        if (hit.collider.gameObject.transform == transform){
                            if(cooldown>=COOLDOWN_TIME){
                                Invoke("falseGameon",1f);
                                transform.GetComponent<Animator>().SetTrigger(trigNamesDeath[deathAnimIndx]);
                                deadBacteriaCount++;
                                cooldown=0;
                            }
                        }
                    }
                }
            }
        totaladded+=Time.deltaTime;
        if(totaladded>IDLE_TIME){
            transform.GetComponent<Animator>().SetTrigger(trigNamesDeath[deathAnimIndx]);
            Invoke("falseGameon",1f);
            totaladded=0f;
        }
        }else totaladded=0;
        cooldown+=Time.deltaTime;
    }

    bool almostThere(Vector2 p1, Vector2 d,float threshold){ //p=pos1, d=destination
        return math.abs(d.y+colliderOffsety-p1.y)<threshold&&math.abs(d.x-p1.x)<threshold;
    }

    void OnMouseDown()
    {
        /*if(cooldown>=COOLDOWN_TIME){
            Invoke("falseGameon",1f);
            transform.GetComponent<Animator>().SetTrigger(trigNamesDeath[deathAnimIndx]);
            deadBacteriaCount++;
            cooldown=0;
        }*/
    }
    void falseGameon(){gameOn=false;}
}
