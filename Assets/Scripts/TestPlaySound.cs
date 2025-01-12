using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.U2D;

public class TestPlaySound : MonoBehaviour
{
    public AudioClip chickensound;
    public AudioSource source;

    public GameObject circle;

    public SpriteShapeController controllerOfSprites;

    // We save a list of Move controllers.
    List<UniMoveController> moves = new List<UniMoveController>();

    // Start is called before the first frame update
    void Start()
    {

      

        Time.maximumDeltaTime = 0.1f;

        int count = UniMoveController.GetNumConnected();

        // Iterate through all connections (USB and Bluetooth)
        for (int i = 0; i < count; i++)
        {
            UniMoveController move = gameObject.AddComponent<UniMoveController>();  // It's a MonoBehaviour, so we can't just call a constructor

            // Remember to initialize!
            if (move.Init(i) != PSMove_Connect_Status.MoveConnect_OK)
            {
                Destroy(move);  // If it failed to initialize, destroy and continue on
                continue;
            }

            // This example program only uses Bluetooth-connected controllers
            PSMoveConnectionType conn = move.ConnectionType;
            if (conn == PSMoveConnectionType.Unknown || conn == PSMoveConnectionType.USB)
            {
                Destroy(move);
            }
            else
            {
                moves.Add(move);

                move.OnControllerDisconnected += HandleControllerDisconnected;

                // Start all controllers with a white LED
                move.SetLED(Color.white);

                //move.EnableTracking();

            }
        }

        



    }

    void HandleControllerDisconnected(object sender, EventArgs e)
    {
        // TODO: Remove this disconnected controller from the list and maybe give an update to the player
    }

    public float smoothing = 8.0f; // Adjust this value to control the smoothing level
    private float smoothInputY = 0f;
    private float smoothInputX = 0f;

    public class RangeMapper
    {
        // Map a value from the input range to the output range
        public static float Map(float value, float inMin, float inMax, float outMin, float outMax)
        {
            // Clamp the input value to be within the input range
            value = Mathf.Clamp(value, inMin, inMax);

            // Map the input value to the output range
            float mappedValue = outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);

            // Clamp the result to be within the output range
            mappedValue = Mathf.Clamp(mappedValue, outMin, outMax);

            return mappedValue;
        }
    }

    bool fadingOut = false;

    IEnumerator fadeSoundOut()
    {

        if(fadingOut)
        {
            yield break;
        }

        fadingOut  = true;

        Debug.Log("Starting dafe out");

        while (source.volume >= 0.0f)
        {
            //this will fade out the source
            source.volume -= 0.1f;
            
            yield return new WaitForSeconds(0.1f);
        }

        source.volume = 1;
        source.Stop();

    }

    public static float triggerValue;

    bool haveReleasedTriggerToBreathe = true;

    public static float angerMeter = 0;

    public static float score = 0;

    public static int numberOfNewChickens = 1;

    public static bool newChicken = false;

    public GameObject chickenPrefab;


    public static bool angerMode = false;

    public static bool moveButton = false;

    // Update is called once per frame
    void Update()
    {

        GameObject.Find("Anger").GetComponent<UnityEngine.UI.Image>().fillAmount = TestPlaySound.angerMeter / 100.0f;


        foreach (UniMoveController move in moves)
        {

            //depending on the y accelerometer data, change the position of the circle in the y direction
            //Debug.Log(move.Acceleration.z);

            Transform circleTransform = circle.GetComponent<Transform>();
            if (circleTransform != null)
            {
                Vector3 localPos = circleTransform.localPosition;

                //

                localPos.y = 0;
                localPos.z = 0;

                smoothInputY = Mathf.Lerp(smoothInputY, move.Acceleration.y, smoothing * Time.deltaTime);
                smoothInputX = Mathf.Lerp(smoothInputX, move.Acceleration.x, smoothing * Time.deltaTime);

                //Debug.Log("Smooth input is " + smoothInputY);

                localPos.y = RangeMapper.Map(smoothInputY,0.3f,0.55f,-3f,3f);
                //localPos.x = -smoothInputX;

                //update the transform
                circleTransform.localPosition = localPos;



            }

            if(newChicken)
            {
                //spawn a new chicken in a random x position between -7 and 7
                //and a random y position between -4 and -2
                Vector3 chickenPosition = new Vector3(UnityEngine.Random.Range(-7f, 7f), UnityEngine.Random.Range(-4f, -2f), 0);
                GameObject newChickenObject = Instantiate(chickenPrefab, chickenPosition, Quaternion.identity);
                newChicken = false;


            }    


            if(angerMeter > 100)
            {
                angerMode = true;
                StrangleVibrate.makeVisible = true;

            }

            if(angerMeter < 0)
            {
                angerMode = false;
                StrangleVibrate.makeVisible=false;
            }


            //depending on the position of the circle, play the pitch shifted sound
            //y position is from -3 to 3
            //pitch shift is from 1 to 2
            //so we need to map the y position to the pitch shift
            float pitchShift = RangeMapper.Map(circleTransform.localPosition.y, -3f, 3f, 0.3f, 2f);

            

            //if in anger mode, you have to strangle the controller, to furiously shake it up and down for anger to go down
            if (angerMode)
            {

                Debug.Log("Gyro is "+move.Gyro.y + " ");

                //trigger has to be pressed
                if (triggerValue > 0.7f)
                {

                    //the more you move the circle transorm, the more anger goes down
                    //if the controller is being shaken up and down
                    //use the Gyro 
                    if (move.Gyro.y > 1f || move.Gyro.y < -1f) 
                    {
                        angerMeter -= 0.4f;
                    }

                }



            }


            //change the sprite spline to match the pitch shift
            //move the second point  left or right depending on the pitch shift
            //the second point is the one that is moved
            //the first point is the one that is not moved


            //because the pitch shift cant go lower than 0.5, I use two pitch shifts chained
            //while the value is between 0.5 and 1, it uses the first pitch shift to 1, and the second is the value
            //if the value dips below 0.5, it sets the second pitch shift to 0.5, and the first is calculated by value/0.5
            if (pitchShift < 0.5f)
            {
                source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 0.5f);
                source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch2", pitchShift/0.5f);
            }
            else
            {
                source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", pitchShift);
                source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch2", 1);
            }

            //set the pitch to the pitch shift
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", pitchShift);


            //Debug.Log(move.Trigger);

            triggerValue = move.Trigger;

            if(triggerValue == 0)
            {
                haveReleasedTriggerToBreathe = true;
            }
            else 
            {
                
                
            }

            moveButton = move.GetButton(PSMoveButton.Move);


            //if trigger is pressed, the trigger value is greater than 0
            //when you first press it and get it to 1, it playes the chicken sound
            //then, while the value is between 0 and 1, it sets the volume to that, but when it hits 0, it stops the sound
            if(move.Trigger>=0)
            {

                

                //set the LED color based on how hard you're squeezing, from yellow to red
                Color color = Color.Lerp(Color.yellow, Color.red, move.Trigger);
                move.SetLED(color);

                //move.SetRumble(RangeMapper.Map(move.Trigger, 0f, 1f, 0f, 0.3f));

                float squeeze = RangeMapper.Map(move.Trigger, 0f, 1f, -2.5f, -2.1f);

                //flip the range 
                //squeeze +=0.4f;

                //Debug.Log("Sqeeze" + squeeze);

                controllerOfSprites.spline.SetPosition(0, new Vector3(squeeze, 0, 0));

               

                //if it just got to 1, play the sound
                if (move.Trigger > 0.4f && !source.isPlaying && haveReleasedTriggerToBreathe)
                {
                    source.PlayOneShot(chickensound);
                    haveReleasedTriggerToBreathe = false;
                }
                //set the volume to the trigger value

                //make the trigger value from 0 to 1 map to volume of 0.5 to 1
                float volume = RangeMapper.Map(move.Trigger, 0f, 1f, 0.5f, 1f);
                source.volume = volume;
                //if the trigger value is 0, stop the sound
                if(move.Trigger == 0)
                {
                    source.Stop();
                }


            }


            //if (move.GetButton(PSMoveButton.Trigger))
            //{


                
            //    //source.Play();
            //    //play the sound

            //    if (!source.isPlaying)
            //    {
            //        Debug.Log("Stopping all coroutines");


            //        fadingOut = false;

            //        StopAllCoroutines();
            //        source.PlayOneShot(chickensound);
            //        source.volume = 1;
            //    }
            //}
            //else if(source.isPlaying) {

               
            //    StartCoroutine(fadeSoundOut()); 
            
            //}
           


            //depending on the y accelerometer data, change the LED green channel of the move controller
            //Debug.Log(move.Acceleration.z);

            //the acceleration is a number between -1 and 1
            //so we need to convert it to a number between 0 and 1
            
            

           //set the LED color to be green
           //move.SetLED(new Color(0, greenValue, 0));

            //// Instead of this somewhat kludge-y check, we'd probably want to remove/destroy
            //// the now-defunct controller in the disconnected event handler below.
            //if (move.Disconnected) continue;

            //// Button events. Works like Unity's Input.GetButton
            //if (move.GetButtonDown(PSMoveButton.Circle))
            //{
            //    Debug.Log("Circle Down");
            //}
            //if (move.GetButtonUp(PSMoveButton.Circle))
            //{
            //    Debug.Log("Circle UP");
            //}

            //// Change the colors of the LEDs based on which button has just been pressed:
            //if (move.GetButtonDown(PSMoveButton.Circle)) move.SetLED(Color.cyan);
            //else if (move.GetButtonDown(PSMoveButton.Cross)) move.SetLED(Color.red);
            //else if (move.GetButtonDown(PSMoveButton.Square)) move.SetLED(Color.yellow);
            //else if (move.GetButtonDown(PSMoveButton.Triangle)) move.SetLED(Color.magenta);
            //else if (move.GetButtonDown(PSMoveButton.Move)) move.SetLED(Color.black);

            //// Set the rumble based on how much the trigger is down
            //move.SetRumble(move.Trigger);
        }

        //the chicken sound is in C5
        //keyboard is like a piano, and the a key is c5
        //so when you press a, it plays the chicken sound
        if (Input.GetKeyDown(KeyCode.A))
        {
            //set the pitch to 1
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1);

            source.PlayOneShot(chickensound);
        }
        
        //w is c#5, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 1.05946, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.W))
        {
            //set the pitch to 1.05946
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1.05946f);
            source.PlayOneShot(chickensound);
        }

        //s is d5, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 1.12246, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.S))
        {
            //set the pitch to 1.12246
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1.12246f);
            source.PlayOneShot(chickensound);
        }

        //e is d#5, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 1.18921, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.E))
        {
            //set the pitch to 1.18921
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1.18921f);
            source.PlayOneShot(chickensound);
        }

        //d is e5, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 1.25992, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.D))
        {
            //set the pitch to 1.25992
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1.25992f);
            source.PlayOneShot(chickensound);
        }

        //f is f5, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 1.33483, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.F))
        {
            //set the pitch to 1.33483
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1.33483f);
            source.PlayOneShot(chickensound);
        }

        //t is f#5, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 1.41421, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.T))
        {
            //set the pitch to 1.41421
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1.41421f);
            source.PlayOneShot(chickensound);
        }

        //g is g5, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 1.49831, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.G))
        {
            //set the pitch to 1.49831
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1.49831f);
            source.PlayOneShot(chickensound);
        }

        //y is g#5, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 1.58740, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.Y))
        {
            //set the pitch to 1.58740
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1.58740f);
            source.PlayOneShot(chickensound);
        }

        //h is a5, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 1.68179, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.H))
        {
            //set the pitch to 1.68179
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1.68179f);
            source.PlayOneShot(chickensound);
        }

        //u is a#5, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 1.78180, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.U))
        {
            //set the pitch to 1.78180
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1.78180f);
            source.PlayOneShot(chickensound);
        }

        //j is b5, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 1.88775, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.J))
        {
            //set the pitch to 1.88775
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1.88775f);
            source.PlayOneShot(chickensound);
        }

        //k is c6, so it must pitch shift it using the audio mixer group effect "Pitch"
        //the pitch shift is 2, so it must be multiplied by that
        if (Input.GetKeyDown(KeyCode.K))
        {
            //set the pitch to 2
            source.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 2);
            source.PlayOneShot(chickensound);
        }


        
    }
}
