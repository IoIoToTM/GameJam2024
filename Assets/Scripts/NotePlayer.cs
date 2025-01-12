using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class NotePlayer : MonoBehaviour
{

    //note class that holds the note and the time it should be played
    public class Note
    {
        public int note;
        public float time;
        public float duration;

        //image for the note
        public Sprite sprite;

        public Note (int note, float time, PSMoveButton button, float duration)
        {
            this.note = note;
            this.time = time;
            this.button = button;
            this.duration = duration;

            //load the sprite from the resources folder
            //depending on the button, load a different sprite
            //sprite names are in the format PSMove_Circle.png, PSMove_Cross.png, etc. in the Controller folder of Resources
            string spriteName = "PSMove_" + button.ToString();
            //Debug.Log(spriteName);




            this.sprite = Resources.Load<Sprite>("Controller/PSMove_Move");
        }

        //button combination that needs to be pressed to play the note
        public PSMoveButton button;
    }

    //list of notes
    public List<Note> notes = new List<Note>();

    public GameObject notePrefab;

    public bool isPlaying = false;
    
    float time = 0;

    public static float songSpeed = 6.0f;

    AudioSource audioSource;



    // Start is called before the first frame update
    void Start()
    {


        var midiFile = MidiFile.Read("Assets/StreamingAssets/careless_chiken.mid");

        var tempoMap = midiFile.GetTempoMap();

        //long ticks = 123;

        //MetricTimeSpan metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(ticks, tempoMap);

        


        foreach (var trackChunk in midiFile.GetTrackChunks())
        {
            using (var notesManager = trackChunk.ManageNotes())
            {
                foreach (var note in notesManager.Objects)
                {

                    //Debug.Log(note.NoteNumber + " " + note.Time + " " + note.Length + " " + note.Velocity + " " + note.NoteName +note.Octave);
                    //Debug.Log(note.NoteName + " " + note.Octave);
                    //get the time by using metric time span
                    float time = (float)TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap).TotalSeconds;

                    float duration = (float)note.Length * 60.0f / 120.0f;

                    //add the note to the list
                    notes.Add(new Note (note.NoteNumber-48, time, PSMoveButton.Circle, duration));
                }
            }
        }

        //print all the notes plus index
        foreach (Note note in notes)
        {
            Debug.Log(notes.IndexOf(note) + " " + note.note + " " + note.time + " " + note.duration);   
        }



        ////create a simple song with 4 notes
        //notes.Add(new Note (1, 0.0f, PSMoveButton.Circle,1));
        //notes.Add(new Note (8, 0.71f, PSMoveButton.Cross,1));
        //notes.Add(new Note (13, 1.43f, PSMoveButton.Square, 1));
        //notes.Add(new Note (20, 2.15f, PSMoveButton.Triangle, 1));

        //notes.Add(new Note(3, 2.89f, PSMoveButton.Circle, 1));
        //notes.Add(new Note(12, 3.60f, PSMoveButton.Cross, 1));
        //notes.Add(new Note(20, 4.32f, PSMoveButton.Square, 1));
        //notes.Add(new Note(24, 5.04f, PSMoveButton.Triangle, 1));

        //notes.Add(new Note(5, 5.78f, PSMoveButton.Circle, 1));
        //notes.Add(new Note(8, 6.50f, PSMoveButton.Cross, 1));
        //notes.Add(new Note(17, 7.21f, PSMoveButton.Square, 1));
        //notes.Add(new Note(29, 7.93f, PSMoveButton.Triangle, 1));



    }

    public double DSPTime;

    // Update is called once per frame
    void Update()
    {

        DSPTime = AudioSettings.dspTime;
        
        //to start, press enter
        if (Input.GetKeyDown(KeyCode.Return) && !isPlaying)
        {

            //start music as well
            audioSource = GetComponent<AudioSource>();
            audioSource.PlayScheduled(DSPTime + 2.5f);

            isPlaying = true;
            StartCoroutine(PlayNotes(0));

            

        }

    }

    IEnumerator PlayNotes(float delay)
    {

       

        time = 0;

        //loop through all the notes

        float nextSpawnTime = notes[0].time;

        yield return new WaitForSeconds(delay);


        while (true) {
            //Debug.Log("Time is " + time);

            if (time >= nextSpawnTime)
            {
                //spawn the note

                //depending on the note, change the y position between -3 and 3
                //notes are from 1 to 33
                //use range mapper 
                float ypos = TestPlaySound.RangeMapper.Map(notes[0].note, 1, 33, -3, 3);

                GameObject note = Instantiate(notePrefab, transform.position, Quaternion.identity);
                note.GetComponent<NoteObject>().noteInfo = notes[0];
                //set the sprite of the note
                
                //note.GetComponent<SpriteRenderer>().sprite = notes[0].sprite;

                //set the y position
                note.transform.position = new Vector3(note.transform.position.x, ypos, note.transform.position.z);

                

                notes.RemoveAt(0);
                if (notes.Count > 0)
                {
                    nextSpawnTime = notes[0].time;

                    //Debug.Log("Next note at " + nextSpawnTime);
                }
                else
                {
                    nextSpawnTime = 1000000;
                }
            }
            
            time += Time.deltaTime;

            yield return null;


        }
    
    }
}
