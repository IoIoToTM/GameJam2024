using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool hopping = false;

    //using lerping, this ienumerator hops the chicken
    IEnumerator HopCoroutine()
    {

        if(hopping) { yield break; }

        //set hopping to true
        hopping = true;



        //get the starting position
        Vector3 startPos = transform.position;
        //get the target position
        Vector3 targetPos = transform.position + new Vector3(Random.Range(-1.0f, 1.0f), 1.0f, 0);
        //lerp the chicken from the start position to the target position over 0.5 seconds
        for(float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, t / 0.5f);
            yield return null;
        }
        //set the chicken's position to the target position
        transform.position = targetPos;
        //lerp the chicken from the target position to the start position over 0.5 seconds
        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(targetPos, startPos, t / 0.5f);
            yield return null;
        }
        //set the chicken's position to the start position
        transform.position = startPos;

        //set hopping to false
        hopping = false;
    }

    // Update is called once per frame
    void Update()
    {
        
        //start the hop coroutine if the chicken is not already hopping
        if(!hopping)
        {
            StartCoroutine(HopCoroutine());
        }


    }
}
