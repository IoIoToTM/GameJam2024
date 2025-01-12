using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrangleVibrate : MonoBehaviour
{


    Vector3 originalPosition;

    public static bool makeVisible = false;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        //make the image vibrate
        transform.position = originalPosition + new Vector3(Mathf.Sin(Time.time * 100), Mathf.Cos(Time.time * 100), 0) * 0.1f;


        //set the visibility to makeVisible
        GetComponent<SpriteRenderer>().enabled = makeVisible;
    }
}
