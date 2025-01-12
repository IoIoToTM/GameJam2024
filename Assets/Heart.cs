using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{
   //once it spawns , float up and randomly flutter, destroy itself after 2 seconds
   float speed = 0.01f;
    float flutterSpeed = 0.1f;
    float flutterAmount = 0.1f;
    float flutterTimer = 0.0f;
    float flutterTime = 0.1f;
    float timer = 0.0f;
    float time = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        //set the timer to 0
        timer = 0.0f;
    }
    // Update is called once per frame
    void Update()
    {
        //add to the timer
        timer += Time.deltaTime;
        //if the timer is greater than the time, destroy the heart
        if(timer > time)
        {
            Destroy(gameObject);
        }
        //add to the flutter timer
        flutterTimer += Time.deltaTime;
        //if the flutter timer is greater than the flutter time, reset the flutter timer and change the flutter amount
        if(flutterTimer > flutterTime)
        {
            flutterTimer = 0.0f;
            flutterAmount = Random.Range(-0.1f, 0.1f);
        }
        //move the heart up
        transform.position += new Vector3(0.0f, speed, 0.0f);
        //rotate the heart
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, flutterAmount * 100.0f);
    }
}
