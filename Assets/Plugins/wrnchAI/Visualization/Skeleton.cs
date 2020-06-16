﻿/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using System.Collections.Generic;
using UnityEngine;

using wrnchAI.Core;
using wrnchAI.wrAPI;

namespace wrnchAI.Visualization
{
    /// <summary>
    /// Class representing a skeleton for visualization.
    /// </summary>
    class Skeleton : MonoBehaviour
    {
        //Id of the person tracked by this skeleton
        private int m_id;
        public int Id { get { return m_id; } set { m_id = value; } }

        private Color m_color;
        public Color color
        {
            get { return m_color; }
            set
            {
                m_color = value;
            }
        }

        //Frame space to texture space transform (Handled by SkeletonVisualizer).
        private Matrix4x4 m_jointToVideoQuad;
        public Matrix4x4 JointToVideoQuad { get { return m_jointToVideoQuad; } set { m_jointToVideoQuad = value; } }

        private Joint[] m_debugJoints;
        private List<LineRenderer> m_debugBones;

        private Vector2 m_jointScaleOffset = new Vector2(1f, 1f);
        public Vector2 JointScaleOffset
        {
            get { return m_jointScaleOffset; }
            set
            {
                if (m_initialized)
                {
                    foreach (var j in m_debugJoints)
                    {
                        if (j == null)
                            continue;
                        var localScale = j.transform.localScale;
                        localScale.x *= value.x / m_jointScaleOffset.x;
                        localScale.y *= value.y / m_jointScaleOffset.y;
                        j.transform.localScale = localScale;
                    }
                }
                m_jointScaleOffset = value;
            }
        }


        //Last update time for tracking timeout
        private float m_lastUpdate;

        [SerializeField]
        private GameObject m_jointPrefab;
        public GameObject JointPrefab { get { return m_jointPrefab; } set { m_jointPrefab = value; } }

        [SerializeField]
        private GameObject m_bonePrefab;
        public GameObject BonePrefab { get { return m_bonePrefab; } set { m_bonePrefab = value; } }

        [SerializeField]
        private bool m_isUI;
        public bool IsUI { get { return m_isUI; } set { m_isUI = value; } }

        private List<int[]> m_boneMap;

        private int m_jointsNumber;

        private bool m_initialized = false;
        public bool HasBeenInitialized { get { return m_initialized; } }

        private static readonly List<string> m_jointsToDisplay = new List<string> {
            "RANKLE",//0
            "RKNEE",//2
            "RHIP",//3
            "LHIP",//4
            "LKNEE",//5
            "LANKLE",//6
            "PELV",  //7
            "NECK", //8
            "HEAD", //9
            "RWRIST",//10
            "RELBOW",//11
            "RSHOULDER",//12
            "LSHOULDER",//13
            "LELBOW",//14
            "LWRIST",//15
            "RTOE",//16
            "LTOE",//17
            };

        private static readonly List<Color> m_colorToDisplay = new List<Color> {
            Color.black, //"RANKLE",//0
            Color.black,//"RKNEE",//2
            Color.black,//"RHIP",//3
            Color.white,//"LHIP",//4
            Color.white,//"LKNEE",//5
            Color.white,//"LANKLE",//6
            Color.green,//"PELV",  //7
            Color.green,//"NECK", //8
            Color.green,//"HEAD", //9
            Color.black,//"RWRIST",//10
            Color.black,//"RELBOW",//11
            Color.black,//"RSHOULDER",//12
            Color.white,//"LSHOULDER",//13
            Color.white,//"LELBOW",//14
            Color.white,//"LWRIST",//15
            Color.black,//"RTOE",//16
            Color.white,//"LTOE",//17
            };


        public void Init(List<int[]> boneMap)
        {
            m_debugBones = new List<LineRenderer>();

            m_debugJoints = new Joint[PoseManager.Instance.JointDefinition2D.NumJoints];


            //Spawn all joints 

            foreach (string name in m_jointsToDisplay)
            {
                var jointIdx = PoseManager.Instance.JointDefinition2D.GetJointIndex(name);
                var go = Instantiate(m_jointPrefab);
                go.transform.SetParent(transform, false);

                var visualJoint = go.GetComponent<Joint>();
                visualJoint.JointId = jointIdx;
                visualJoint.Color = m_color;
                visualJoint.ScaleJoint(m_jointScaleOffset);
                m_debugJoints[jointIdx] = visualJoint;

            }

            Debug.Log("Total Bones: " + boneMap.Count);

            //Spawn all bones
            for (int i = 0; i < boneMap.Count; i++)
            {
                var bone = Instantiate(m_bonePrefab);
                var boneRenderer = bone.GetComponent<LineRenderer>();
                bone.transform.SetParent(transform, false);
                boneRenderer.startColor = boneRenderer.endColor = boneRenderer.material.color = m_color;
                var ar = m_jointScaleOffset.y / m_jointScaleOffset.x;
                boneRenderer.endWidth = boneRenderer.startWidth *= ar;
                m_debugBones.Add(boneRenderer);
            }

            m_boneMap = boneMap;
            m_initialized = true;
        }

        private void Start()
        {
            gameObject.name = "Skeleton";
        }

        /// <summary>
        /// Update the joints and bones positions/visibility with a Person returned by the PoseWorker
        /// </summary>
        /// <param name="person"></param>
        public void UpdateSkeleton(Person person)
        {
            var joints = person.Pose2d.Joints;


            // Send The Person to JointDataDisplay So we can use it for further calculations ;
            DataManager.instance.person = person;


            for (int i = 0; i < m_jointsToDisplay.Count; i++)
            {
                if (m_debugJoints[i] != null)
                {
                    Vector4 position = new Vector4(joints[i * 2], joints[2 * i + 1], 0, 1);
                    if (position.x > 0 && position.x < 1.0f && position.y > 0 && position.y < 1.0f)
                    {
                        m_debugJoints[i].gameObject.SetActive(true);
                        Vector3 pos = (m_jointToVideoQuad * position);
                        pos.z = -0.02f; //-0.02f to avoid clipping between linerenderers and the texture
                        m_debugJoints[i].SetPosition(pos);
                        m_debugJoints[i].enabled = true;
                    }
                    else
                    {
                        m_debugJoints[i].gameObject.SetActive(false);
                    }
                }
            }

            for (int i = 0; i < m_boneMap.Count; ++i)
            {
                var j0 = m_debugJoints[m_boneMap[i][0]];
                var j1 = m_debugJoints[m_boneMap[i][1]];

                if (j0 != null && j1 != null)
                {
                    if (j0.gameObject.activeSelf && j1.gameObject.activeSelf)
                    {
                        m_debugBones[i].gameObject.SetActive(true);
                        m_debugBones[i].SetPosition(0, j0.GetPosition());
                        m_debugBones[i].SetPosition(1, j1.GetPosition());

                        if (i == 6 || i == 5 || i == 12 || i == 13)
                        {
                            m_debugBones[i].gameObject.SetActive(false);
                        }


                        if (i == 6)
                        {
                            var j2 = m_debugJoints[6];
                            var j3 = m_debugJoints[8];

                            if (j2 != null && j3 != null)
                            {
                                if (j2.gameObject.activeSelf && j3.gameObject.activeSelf)
                                {
                                    m_debugBones[i].gameObject.SetActive(true);
                                    m_debugBones[i].SetPosition(0, j2.GetPosition());
                                    m_debugBones[i].SetPosition(1, j3.GetPosition());

                                }
                                else
                                {
                                    m_debugBones[i].gameObject.SetActive(false);
                                }
                            }
                        }
                    }
                    else
                    {
                        m_debugBones[i].gameObject.SetActive(false);
                    }
                }
            }


        }



        private void OnDestroy()
        {
            foreach (var j in m_debugJoints)
            {
                if (j != null)
                    Destroy(j);
            }
            foreach (var b in m_debugBones)
            {
                if (b != null && b.gameObject != null)
                    Destroy(b);
            }
        }
    }


}