﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class MathHelper : MonoBehaviour
{
    public static MathHelper instance;
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

    //Convert Radian Angle to Degree
    public float ToDegree(float x)
    {
        double pi = Math.PI;
        float degree = (float)((x * 180) / pi);
        return degree;
    }


    public float CalculateSlope2D(float x1, float y1, float x2, float y2)
    {
        return (y2 - y1) / (x2 - x1);
    }

    public float GetUserOrientation(Vector3 rightHip, Vector3 leftHip)
    {
        float slope = CalculateSlope2D(rightHip.x, rightHip.z, leftHip.x, leftHip.z);
        float angle = ToDegree((float)Math.Atan(slope));
        return angle;
    }

    public float GetMean(float[] lst)
    {
        float sum = 0;
        foreach (float item in lst)
        {
            sum += item;
        }

        return sum / lst.Length;
    }

    public float GetPositiveMean(float[] lst)
    {
        for (int i = 0; i < lst.Length; i++)
        {
            lst[i] = Math.Abs(lst[i]);
        }

        return GetMean(lst);
    }

    public float GetEuclideanDistance(Vector3 point1, Vector3 point2)
    {
        return (float)Math.Sqrt(Math.Pow(point2.x - point1.x, 2) + Math.Pow(point2.y - point1.y, 2) + Math.Pow(point2.z - point1.z, 2));
    }


    public bool CheckForNegative(Vector3 listOfTuples)
    {
        if (listOfTuples.x <0)
        {
            return true;
        }
        else if (listOfTuples.y < 0)
        {
            return true;
        }
        else if (listOfTuples.z < 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }


    public float GetAngle(Vector3 joint1, Vector3 joint2, Vector3 joint3)
    {
        // The angle you want is located at joint2
        float a = GetEuclideanDistance(joint1, joint2);
        float b = GetEuclideanDistance(joint2, joint3);
        float c = GetEuclideanDistance(joint1, joint3);

        float aSqaured = (float)Math.Pow(a, 2);
        float bSqaured = (float)Math.Pow(b, 2);
        float cSqaured = (float)Math.Pow(c, 2);

        float C = (aSqaured + bSqaured - cSqaured) / (2 * a * b);
        float angle = ToDegree((float)Math.Acos(C));

        return angle;
    }

    public float GetTorsoAngleWithStraightLeg(Vector3 shoulder, Vector3 hip, Vector3 ankle)
    {
        // This only works for movements where the user is standing upright

        float a = GetEuclideanDistance(shoulder, hip);
        Vector3 straightLeg = new Vector3 (hip.x, ankle.y, hip.z);
        float b = GetEuclideanDistance(hip, straightLeg);
        float c = GetEuclideanDistance(straightLeg, shoulder);

        float aSqaured = (float)Math.Pow(a, 2);
        float bSqaured = (float)Math.Pow(b, 2);
        float cSqaured = (float)Math.Pow(c, 2);

        float C = (aSqaured + bSqaured - cSqaured) / (2 * a * b);
        float angle = ToDegree((float)Math.Acos(C));

        return angle;
    }


    public float GetKneeAngleWithStraightShin(Vector3 hip, Vector3 knee, Vector3 ankle)
    {
        // Here we simplify the knee joint by assuming a straight shin

        float a = GetEuclideanDistance(hip, knee);
        Vector3 straightShinAnkle = new Vector3(knee.x, ankle.y, knee.z);
        float b = GetEuclideanDistance(knee, straightShinAnkle);
        float c = GetEuclideanDistance(hip, straightShinAnkle);

        float aSqaured = (float)Math.Pow(a, 2);
        float bSqaured = (float)Math.Pow(b, 2);
        float cSqaured = (float)Math.Pow(c, 2);

        float C = (aSqaured + bSqaured - cSqaured) / (2 * a * b);
        float angle = ToDegree((float)Math.Acos(C));

        return angle;
    }

    public bool IsSideLeft(Vector3 leftHip, Vector3 pelvis, Vector3 rightHip)
    {
        float left = GetEuclideanDistance(leftHip, pelvis);
        float right = GetEuclideanDistance(rightHip, pelvis);

        if (left < right)
            return true;
        else
            return false;
    }



    public Feetdata FeetAreShoulderWidth(Vector3 rightShoulder, Vector3 leftShoulder, Vector3 rightHeel, Vector3 leftHeel)
    {
        float rightUpperBoundary = rightShoulder.x * 1.05f;  // Increase by 5%
        float rightLowerBoundary = rightShoulder.x * 0.8f;  // Decrease by 20%

        float leftUpperBoundary = leftShoulder.x * 1.2f;
        float leftLowerBoundary = leftShoulder.x * 0.95f;

        int rightFoot = 0;
        int leftFoot = 0;

        if (rightHeel.x < rightLowerBoundary)
        {
            //Right foot too wide
            rightFoot = -1;
        }

        else if (rightHeel.x > rightUpperBoundary)
        {
            // Right foot too narrow
            rightFoot = 0;
        }

        else
        {
            //Foot is shoulder width
            rightFoot = 1;
        }


        if (leftHeel.x > leftUpperBoundary)
        {
            // Left foot too wide
            leftFoot = -1;
        }

        else if (leftHeel.x < leftLowerBoundary)
        {
            // Left foot too narrow
            leftFoot = 0;
        }

        else
        {
            // Foot is shoulder width
            leftFoot = 1;
        }

        Feetdata feetdata = new Feetdata();
        if (leftFoot + rightFoot == 2)
        {
            feetdata.state = true;
            feetdata.leftFoot = leftFoot;
            feetdata.rightFoot = rightFoot;
            return feetdata;
        }

        else
        {
            feetdata.state = false;
            feetdata.leftFoot = leftFoot;
            feetdata.rightFoot = rightFoot;
            return feetdata;
        }
        
    }

    public Feetdata FeetAreHipWidth(Vector3 rightHip, Vector3 leftHip, Vector3 rightHeel, Vector3 leftHeel)
    {
        float rightUpperBoundary = rightHip.x * 1.07f;  // Increase by 15%
        float rightLowerBoundary = rightHip.x * 0.85f;  // Decrease by 20%

        float leftUpperBoundary = leftHip.x * 1.15f;
        float leftLowerBoundary = leftHip.x * 0.93f;

        int rightFoot = 0;
        int leftFoot = 0;

    
        if (rightHeel.x < rightLowerBoundary)
        {
            // Right foot too wide
            rightFoot = -1;
        }

        else if (rightHeel.x > rightUpperBoundary)
        {
            // Right foot too narrow
            rightFoot = 0;
        }

        else
        {
            // Foot is shoulder width
            rightFoot = 1;
        }


        if (leftHeel.x > leftUpperBoundary)
        {
            // Left foot too wide
            leftFoot = -1;
        }

        else if (leftHeel.x < leftLowerBoundary)
        {
            // Left foot too narrow
            leftFoot = 0;
        }

        else
        {
            //Foot is shoulder width
            leftFoot = 1;
        }

        Feetdata feetdata = new Feetdata();
        if (leftFoot + rightFoot == 2)
        {
            feetdata.state = true;
            feetdata.leftFoot = leftFoot;
            feetdata.rightFoot = rightFoot;
            return feetdata;
        }

        else
        {
            feetdata.state = false;
            feetdata.leftFoot = leftFoot;
            feetdata.rightFoot = rightFoot;
            return feetdata;
        }
    }



}

public class Feetdata
{
    public bool state;
    public int leftFoot;
    public int rightFoot;
}


