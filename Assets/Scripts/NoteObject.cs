using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteObject : MonoBehaviour
{

    public NotePlayer.Note noteInfo;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //move to the left at a constant speed
        transform.position += Vector3.left * Time.deltaTime * NotePlayer.songSpeed;

        //if the note is past the left edge of the screen, destroy it
        if (transform.position.x < -7)
        {
            TestPlaySound.angerMeter += 5;
            Destroy(gameObject);
        }


        
    }
}
