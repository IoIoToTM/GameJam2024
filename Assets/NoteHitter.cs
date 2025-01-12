using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteHitter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject heartObject;




    bool alreadyHit = false;
    

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(TestPlaySound.triggerValue > 0.1f)
        {

            if(TestPlaySound.angerMode)
            {
                //if in anger mode, destroy the note
                Destroy(collision.gameObject);
                return;
            }

           // Debug.Log("Hit");

            //caluclate what percentage the two collision capsules are overlapping
            float overlap = collision.bounds.size.y - Mathf.Abs(collision.transform.position.y - transform.position.y);

            //calculate the percentage of the note that was hit
            float percentage = overlap / collision.bounds.size.y;

            //Debug.Log(percentage);

            //add to score
            TestPlaySound.score += 100 * percentage;

            //get the Chickens UI text element, and set it to "Chickens: " + score divided by 100 (rounded down)
            //it's text mesh pro
            GameObject.Find("Chickens").GetComponent<TMPro.TextMeshProUGUI>().text = "Chickens: " + Mathf.FloorToInt(TestPlaySound.score / 500.0f);


            //if number of new chickens is different that the math floor, set new chicken to true
            if (TestPlaySound.numberOfNewChickens != Mathf.FloorToInt(TestPlaySound.score / 500.0f))
            {
                TestPlaySound.newChicken = true;
            }

            //save the number of chickens
            TestPlaySound.numberOfNewChickens = Mathf.FloorToInt(TestPlaySound.score / 500.0f);

            //add to anger meter as well
            TestPlaySound.angerMeter += 4 * (1 - percentage);


            //40% chance to spawn a heart   
            //random x position between -7 and 7
            //and a random y position between -4 and -2
            if (Random.Range(0, 100) < 40)
            {
                Instantiate(heartObject, new Vector3(Random.Range(-7.0f, 7.0f), Random.Range(-4.0f, -2.0f), 0.0f), Quaternion.identity);
            }

            //desrtoy note
            Destroy(collision.gameObject);
        }
        else
        {

            if (alreadyHit == true)
            {
                return;
            }
            else if (alreadyHit == false)
            {
                alreadyHit = true;
            }

            //Debug.Log("Miss");

            //add to anger meter
            TestPlaySound.angerMeter += 10;

            

            //get the Anger AU image object and increase the fill amount
            

        }
    }
}
