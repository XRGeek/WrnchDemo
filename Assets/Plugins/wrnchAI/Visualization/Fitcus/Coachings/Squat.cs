﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Squat : Coaching
{

    public static Squat instance;
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    /// <summary>
    ///  """
    //The class used to track a normal Squat

    //Attributes
    //----------
    //  depth1 : Int
    //       How deep should the user squat for a rep to be counted.
    //        Value represents the angle of knee(assuming a straight shin).
    //  depth2 : Int
    //        How high should the user rise out of the squat for a rep to be counted.
    //       Value represents the angle of knee(assuming a straight shin).
    //  kneeAngleCutoff: Int
    //      If the knee angle doesnt get below this point, the coach is triggered.
    //    Knee angle is measured between the thigh and the verticle plane(or straight shin)
    //  torsoAngleCutoff: Int
    //How low of an angle can the torso get before triggering the coach.
    //Torso angle is measured between torso and the verticle plane

    /// </summary>

    public List<float> knee_angle_list = new List<float>();
    public List<float> torso_angle_list = new List<float>();

    public List<float> torso_angles_of_current_rep = new List<float>();
    public List<float> knee_angles_of_current_rep = new List<float>();
    public List<float> user_rotations_of_current_rep = new List<float>();

    private float depth1 = 130f;
    private float depth2 = 140f;
    private float kneeAngleCutoff = 110f;
    private float torsoAngleCutoff = 115f;
    private bool feetAreShoulderWidth = false;
    private bool turnedToSide = false;
    private bool isfeetAreShoulderWidthInstructionRunning = false;


    private System.DateTime sideTimer1;
    private System.DateTime sideTimer2;

    private bool isTimerRunning = false;

    /// Used for rep counting
    public bool trackingBegan = false;
    public bool thresholdReached = false;
    public CoachingOneEuroFilter one_euro_filter_r_heel ;
    public CoachingOneEuroFilter one_euro_filter_r_shoulder ;
    public CoachingOneEuroFilter one_euro_filter_l_heel ;
    public CoachingOneEuroFilter one_euro_filter_l_shoulder ;
    public CoachingOneEuroFilter one_euro_filter_userRotation;
    public int frame_no = 0;
    public int frame_no_stance_check = 0;
    public int frame_no_shoulder_check = 0;




        //Analyses squat mechanics given a single frame of joints.

        //This function will trigger count reps functionality and detect correct movement patterns.

        //Parameters
        //----------
        //frame : list
        //    A list of 25 joints that have been extracted from a video frame.
        //    Joints are (x, y, z) coordinates



    public override void AnalyseFrame( JointData[] frame)
    {
         if (feetAreShoulderWidth == false) {
             // Debug.Log("---------- FEET -------------");
            GuideFeetToShoulderWidth(frame);
            frame_no_stance_check += 1;
        } else if (feetAreShoulderWidth && turnedToSide == false) {
           // Debug.Log("---------- SHOULDER -------------");
            GuideBodyToSidePosition(frame);
            frame_no_shoulder_check += 1;
        } else {
            //Debug.Log("---------- COACHING -------------");
            DoCoaching(frame);
        }

    }

    public void GuideBodyToSidePosition( JointData[] frame) {
        
        Vector3 r_heel = frame[23].jointposition;
        Vector3 r_ankle = frame[0].jointposition;
        Vector3 r_knee = frame[1].jointposition;
        Vector3 r_hip = frame[2].jointposition;
        Vector3 r_shoulder = frame[12].jointposition;

        Vector3 l_heel = frame[24].jointposition;
        Vector3 l_ankle = frame[5].jointposition;
        Vector3 l_knee = frame[4].jointposition;
        Vector3 l_hip = frame[3].jointposition;
        Vector3 l_shoulder = frame[13].jointposition;

        int right = 0;
        int left = 0;

        float userRotation = MathHelper.instance.GetUserOrientation(r_shoulder, l_shoulder);

        if(sideTimer1 == null ) {
            // Init times
            sideTimer1 = System.DateTime.Now;
            sideTimer2 = System.DateTime.Now;
        }

        if (frame_no_shoulder_check == 0) {
            // Init Euro Filters(
            one_euro_filter_userRotation = new CoachingOneEuroFilter(frame_no_shoulder_check, userRotation);
                      
        } else {
            l_shoulder.x = one_euro_filter_userRotation.ApplyFilter(frame_no_shoulder_check, userRotation);  
        }

        if (!float.IsNaN(userRotation)) {

            if (r_heel.z > l_heel.z) { right += 1;  } else { left += 1; }
            if (r_ankle.z > l_ankle.z) { right += 1;  } else { left += 1; }
            if (r_knee.z > l_knee.z) { right += 1;  } else { left += 1; }
            if (r_hip.z > l_hip.z) { right += 1;  } else { left += 1; }
            if (r_shoulder.z > l_shoulder.z) { right += 1;  } else { left += 1; }


            if ( right > left && Mathf.Abs(userRotation) >= 60 )  {
                DataManager.currentSkeleton.ResetGlowValues();
                Debug.Log("--------------- Correct ---------- " + userRotation);
                double diffInSeconds = (sideTimer2 - sideTimer1).TotalSeconds;
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 0,1,2,3,4,6,7,8,9,10,11 } , GlowColor.Green);
                if (diffInSeconds >= 3) {

                    VoiceManager.instance.PlayInstructionSound(8);  
                    turnedToSide = true;
                    DataManager.currentSkeleton.ResetGlowValues();
                    // Reset timers
                    sideTimer1 = System.DateTime.Now;
                    sideTimer2 = System.DateTime.Now;
                } else {
                    sideTimer2 = System.DateTime.Now;
                }
            } else {
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 0,1,2,3,4,6,7,8,9,10,11 } , GlowColor.Red);
                // Reset timers
                sideTimer1 = System.DateTime.Now;
                sideTimer2 = System.DateTime.Now;
                Debug.Log("--------------- Incorrect ---------- " + userRotation);
            }
            
        } else {
            // DataManager.currentSkeleton.SetRedGlowValues(new int[] { 6 });
        }

    }


   

    public void GuideFeetToShoulderWidth( JointData[] frame) {

        Vector3 r_heel = frame[23].jointposition;
        Vector3 r_shoulder = frame[12].jointposition;
        Vector3 l_heel = frame[24].jointposition;
        Vector3 l_shoulder = frame[13].jointposition;

        if (frame_no_stance_check == 0) {
            // Init Euro Filters(
            one_euro_filter_r_heel = new CoachingOneEuroFilter(frame_no_stance_check, r_heel.x, 0.0f, 0.01f, 0.0f, 1.0f);
            one_euro_filter_r_shoulder = new CoachingOneEuroFilter(frame_no_stance_check, r_shoulder.x, 0.0f, 0.01f, 0.0f, 1.0f);
            one_euro_filter_l_heel = new CoachingOneEuroFilter(frame_no_stance_check, l_heel.x, 0.0f, 0.01f, 0.0f, 1.0f);
            one_euro_filter_l_shoulder = new CoachingOneEuroFilter(frame_no_stance_check, l_shoulder.x, 0.0f, 0.01f, 0.0f, 1.0f);
                      
        } else {
            r_heel.x = one_euro_filter_r_heel.ApplyFilter(frame_no_stance_check, r_heel.x);
            r_shoulder.x = one_euro_filter_r_shoulder.ApplyFilter(frame_no_stance_check, r_shoulder.x);
            l_heel.x = one_euro_filter_l_heel.ApplyFilter(frame_no_stance_check, l_heel.x);
            l_shoulder.x = one_euro_filter_l_shoulder.ApplyFilter(frame_no_stance_check, l_shoulder.x);  
        }

            

        Feetdata feetdata = MathHelper.instance.FeetAreShoulderWidth(r_shoulder, l_shoulder, r_heel, l_heel);
        // If feet are shoulder width
        if (feetdata.state)
        {
            if (!isTimerRunning)
            {
                StartCoroutine(ShoulderWidthCompleteTimer());
                Debug.Log("Timer Started");
                isTimerRunning = true;
            }
            //ShoulderWidthComplete();
            DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 0,2 }, GlowColor.Green);
            // Debug.Log("--------------- FEET ARE SHOULDER WIDTH ---------");

        // If feet are NOT shoulder width
        } else 
        {

            isTimerRunning = false;
            StopAllCoroutines();
            Debug.Log("Timer Stoped");

            if (!isfeetAreShoulderWidthInstructionRunning)
            {
                // Please place your feet shoulder width apart
                VoiceManager.instance.PlayInstructionSound(6);
                isfeetAreShoulderWidthInstructionRunning = true;
                Invoke("MakeFeetAreShoulderWidthInstructionRunningInstructionFalse", 5);
            }


            if (feetdata.leftFoot != 1) {
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 0 }, GlowColor.Red);
            } else {
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 2 }, GlowColor.Green);
            }
            
            if (feetdata.rightFoot != 1) {
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 2 }, GlowColor.Red);
            } else {
                DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 2 }, GlowColor.Green);
            }
            

        }

    }
     void MakeFeetAreShoulderWidthInstructionRunningInstructionFalse()
    {
        isfeetAreShoulderWidthInstructionRunning = false;
    }


    IEnumerator ShoulderWidthCompleteTimer()
    {
        yield return new WaitForSeconds(3);
        float length = VoiceManager.instance.PlayInstructionSound(7,true);
        Debug.Log("ShoulderWidthComplete");
        // StartCoroutine(MoveToSidePosition(length));
        DataManager.currentSkeleton.ResetGlowValues();
        feetAreShoulderWidth = true;


    }

    // IEnumerator MoveToSidePosition(float delay)
    // {
    //     yield return new WaitForSeconds(delay + 1);
    //     VoiceManager.instance.PlayInstructionSound(21);
    //     Debug.Log("ShoulderWidthComplete now  MoveToSidePosition");
    // }


    public void DoCoaching( JointData[] frame) {
        
        Vector3 r_heel = frame[23].jointposition;
        Vector3 r_ankle = frame[0].jointposition;
        Vector3 r_knee = frame[1].jointposition;
        Vector3 r_hip = frame[2].jointposition;
        Vector3 r_shoulder = frame[12].jointposition;

        Vector3 l_heel = frame[24].jointposition;
        Vector3 l_ankle = frame[5].jointposition;
        Vector3 l_knee = frame[4].jointposition;
        Vector3 l_hip = frame[3].jointposition;
        Vector3 l_shoulder = frame[13].jointposition;

        if ( r_heel.x <= 0 || r_ankle.x <= 0 || r_knee.x <= 0 || r_hip.x <= 0 || r_shoulder.x <= 0 || l_heel.x <= 0 || l_ankle.x <= 0 || l_knee.x <= 0 || l_hip.x <= 0 || l_shoulder.x <= 0)
        {
            return;
        }
       

        float torso_angle  = MathHelper.instance.GetTorsoAngleWithStraightLeg(r_shoulder, r_hip, r_ankle);
        float knee_angle   = MathHelper.instance.GetKneeAngleWithStraightShin(r_hip, r_knee, r_ankle); // This is for right side of body
        // float userRotation = MathHelper.instance.GetUserOrientation(r_hip, l_hip);


        torso_angles_of_current_rep.Add(torso_angle);
        knee_angles_of_current_rep.Add(knee_angle);
        // user_rotations_of_current_rep.Add(userRotation);

        bool audioPlayed = false;
        if (RepCounter(knee_angle, depth1, depth2))
        {
            Debug.Log("Rep on frame: " + frame_no);

            // Check Torso angle
            if ( torso_angles_of_current_rep.Min() < torsoAngleCutoff)
            {
                //Debug.Log("Sound on: Keep your chest up!");
                if (!audioPlayed)
                {
                    VoiceManager.instance.PlayInstructionSound(10);
                    audioPlayed = true;

                    // Make Spine Red
                    if (DataManager.currentSkeleton != null)
                    {
                        DataManager.currentSkeleton.ResetGlowValues();
                        DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 6 } , GlowColor.Red);
                    }
                }

                
            
            }

            else if ( knee_angles_of_current_rep.Min() > kneeAngleCutoff)
            {
                if (!audioPlayed)
                {
                    //Debug.Log("Sound on: Try to get a bit lower!");
                    VoiceManager.instance.PlayInstructionSound(4);
                    audioPlayed = true;

                    // Make Thighs Red
                    if (DataManager.currentSkeleton != null)
                    {
                        DataManager.currentSkeleton.ResetGlowValues();
                        DataManager.currentSkeleton.SetBoneGlowValues(new int[] { 1,3 } , GlowColor.Red);
                    }
                }

            }

            // Play Audio Count
            else if (!audioPlayed)
            {
                reps += 1;
                VoiceManager.instance.PlayInstructionSound(1); // index of rep sound 
                audioPlayed = true;
                if (DataManager.currentSkeleton != null)
                {
                    DataManager.currentSkeleton.ResetGlowValues();
                }
            }
                
            //Debug.Log("Rep " + reps);


            //// Empty All the list for new data

            torso_angles_of_current_rep.Clear();
            knee_angles_of_current_rep.Clear();
            user_rotations_of_current_rep.Clear();


            print("Array Size: " 
            + knee_angles_of_current_rep.Count
            + torso_angles_of_current_rep.Count
            + user_rotations_of_current_rep.Count
            );

        }
        frame_no += 1;
    }

        
        //Counts reps based on the datapoint and 2 thresholds.

        //The datapoint must hit both thresholds to count the rep.

        //Parameters
        //----------
        //datapoint : Int
        //    A list of 25 joints that have been extracted from a video frame.
        //    Joints are (x, y, z) coordinates

        //threshold1 : Int
        //    First point the datapoint must hit.

        //threshold2 : Int
            //Second point the datapoint must hit.
        
    public bool RepCounter(float datapoint ,float threshold1,float threshold2)
    {

         // This rep counter waits for the user to get below the threshold, then above again
        if ( datapoint > threshold1  && trackingBegan == false) {
            // Possible improvement: Is last 5 points linear and pointing down?

            // Tracking has began
            trackingBegan = true;
            return false;

        }
        else if( datapoint < threshold1 && trackingBegan && thresholdReached == false)
        {

            // Threshold is reach
            thresholdReached = true;
            return false;

        }
        else if ( datapoint > threshold2 && trackingBegan && thresholdReached){
            //Rep is counted now that threshold has returned above threshold2
            trackingBegan = false;
            thresholdReached = false;
            return true;
        }
        else
        {
            return false;
        }
       


     }

}
