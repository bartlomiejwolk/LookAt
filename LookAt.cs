// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the LookAt extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using UnityEngine;
using System.Collections;

namespace LookAtEx {
    /// Just like Unity's LookAt but with more options.
    ///
    /// Options available:
    /// - Standard
    /// - YAxisOnly
    /// - RotWithSlerp
    /// - RotThreshold
    /// - RotWithSDA
    public class LookAt : MonoBehaviour {

        #region FIELDS
        #endregion

        #region INSPECTOR FIELDS
        /// Variable required by SmoothDampAngle().
        // todo encapsulate
        private float velocity;

        /// Transform used to calculate angle between himself 
        /// and the target.
        ///
        /// Use it if you need sth. else that root transform.
        [SerializeField]
        private Transform overrideRoot;

        /// When this is enabled, on mouse click a full rotation
        /// is applied.
        [SerializeField]
        private bool clickInstantRot;

        /// Minimum time to reach target.
        [SerializeField]
        private float minTimeToReach;

        /// Maximum rotation speed.
        [SerializeField]
        private float maxRotSpeed;

        /// Rotation speed.
        [SerializeField]
        private float speed;

        /// Dead zone angle where rotation don't accours.
        [SerializeField]
        private float thresholdAngle;

        /// Object to look at.
        [SerializeField]
        private Transform targetTransform;

        [SerializeField]
        // How LookAt controller should work.
        private Options option;

        /// GUIStyle options for scene view labels.
        [SerializeField]
        private GUIStyle labelStyle;

        #endregion

        #region PROPERTIES
        public Options Option {
            get { return option; }
            set { option = value; }
        }

        // TODO Add public Vector3 field for selecting
        // axis to lock.
        public Transform TargetTransform {
            get { return targetTransform; }
            set { targetTransform = value; }
        }
        public float ThresholdAngle {
            get { return thresholdAngle; }
            set { thresholdAngle = value; }
        }
        public float Speed {
            get { return speed; }
            set { speed = value; }
        }
        public float MaxRotSpeed {
            get { return maxRotSpeed; }
            set { maxRotSpeed = value; }
        }
        public float MinTimeToReach {
            get { return minTimeToReach; }
            set { minTimeToReach = value; }
        }
        public bool ClickInstantRot {
            get { return clickInstantRot; }
            set { clickInstantRot = value; }
        }
        public Transform OverrideRoot {
            get { return overrideRoot; }
            set { overrideRoot = value; }
        }
        public GUIStyle LabelStyle {
            get { return labelStyle; }
            set { labelStyle = value; }
        }

        private Transform MyTransform { get; set; }

        #endregion

        #region UNITY MESSAGES
        private void Awake() {
            if (!targetTransform) {
                // todo Add Utilities class
                //MissingReference("_target", InfoType.Error);
            }
        }

        private void Start () {
            MyTransform = GetComponent<Transform>();
        }
        
        private void Update () {
            if (!targetTransform) {
                return;
            }

            switch (option) {
                case Options.Standard:
                    MyTransform.LookAt(targetTransform);
                    break;
                case Options.YAxisOnly:
                    Vector3 v = targetTransform.position - MyTransform.position;
                    v.x = v.z = 0.0f;
                    MyTransform.LookAt(targetTransform.transform.position - v); 
                    break;
                case Options.RotWithSlerp:
                    RotateWithSlerp();
                    break;
                case Options.RotThreshold:
                    RotateWithinThreshold();
                    break;
                case Options.RotWithSDA:
                    RotateWithSDA();
                    break;
            }
        }
        #endregion

        #region METHODS
        private void RotateWithSlerp() {
            // Rotation to be applied.
            Vector3 newRotation;
            // Direction to the target.
            Vector3 dir;

            dir = targetTransform.position - MyTransform.position;
            // Calculate rotation to the target.
            newRotation = Quaternion.LookRotation(dir).eulerAngles;
            // Rotate only around y axis.
            newRotation.x = 0;
            newRotation.z = 0;
            // Apply rotation.
            MyTransform.rotation = Quaternion.Slerp(
                    MyTransform.rotation,
                    Quaternion.Euler(newRotation),
                    Time.deltaTime * speed);
        }

        private void RotateWithSDA() {
            // Direction to the target.
            Vector3 dir;
            // Rotation to be applied.
            Vector3 newRotation;
            // Transform current rotation.
            Vector3 angles;

            // Calculate direction to the target.
            dir = targetTransform.position - MyTransform.position;
            // Calculate rotation to the target.
            newRotation = Quaternion.LookRotation(dir).eulerAngles;
            // Remember current rotation.
            angles = MyTransform.rotation.eulerAngles;
            // Rotate only around y axis.
            angles.x = 0;
            angles.z = 0;
            // Apply rotation.
            MyTransform.rotation = Quaternion.Euler(
                    angles.x,
                    Mathf.SmoothDampAngle(
                        angles.y,
                        newRotation.y,
                        ref velocity,
                        minTimeToReach,
                        maxRotSpeed),
                    angles.z);
        }

        private void RotateWithinThreshold() {
            // Angle between character forword vector and the target.
            float hAngle;
            // Angle between custom selected transform and the target.
            // You can select custom transtorm in the inspector.
            float hAngleCustom;
            // How much the target is beyond the specified threshold
            // (in degrees).
            float hAngleThr;
            // Rotation that will be applied to the transform.
            // When rotation is finished, should always be 0.
            float newRotation = 0;


            hAngle = AngleAroundAxis(
                    MyTransform.forward,
                    targetTransform.position - MyTransform.position,
                    Vector3.up);

            if (overrideRoot) {
                hAngleCustom = AngleAroundAxis(
                        overrideRoot.forward,
                        targetTransform.position - overrideRoot.position,
                        Vector3.up);
            }
            else {
                hAngleCustom = AngleAroundAxis(
                        MyTransform.forward,
                        targetTransform.position - MyTransform.position,
                        Vector3.up);
            }

            // On click, omit all the smoothing code and rotate
            // transform immediately.
            if (Input.GetKey(KeyCode.Mouse0) && clickInstantRot) {
                MyTransform.Rotate(0, hAngleCustom, 0);
                return;
            }

            // If target is within specified threshold, return 0
            // which means, that there's no reason to rotate.
            hAngleThr = Mathf.Max(
                    0,
                    Mathf.Abs(hAngle) - thresholdAngle);
            // If target is beyond specified threshold, add a sign
            // to know which direction to rotate.
            hAngleThr *= Mathf.Sign(hAngle);

            // Rotation to be applied.
            newRotation = Mathf.SmoothDampAngle(
                    newRotation,
                    hAngleThr,
                    ref velocity,
                    minTimeToReach,
                    maxRotSpeed);

            // Apply rotation to the transform.
            MyTransform.Rotate (0, newRotation, 0);

            // TODO Remove debug lines.
            /*Debug.DrawLine(
                    _overrideRoot.position,
                    _overrideRoot.position + _overrideRoot.forward * 20,
                    Color.green);*/
            /*Debug.DrawLine(
                    _transform.position,
                    _transform.position + _transform.forward * 20,
                    Color.yellow);
            MeasureIt_ODG.Set("_transform.rotation", _transform.rotation.eulerAngles);
            MeasureIt_ODG.Set("Cursor abs angle", AngleAroundAxis(
                        Vector3.forward,
                        _target.position,
                        Vector3.up));
            MeasureIt_ODG.Set("hAngle", hAngle);
            MeasureIt_ODG.Set("hAngleThr", hAngleThr);
            MeasureIt_ODG.Set("AngleAxis", Quaternion.AngleAxis(hAngleThr, Vector3.up).eulerAngles);*/
        }
     
        /// The angle between dirA and dirB around axis
        // TODO Create static class TransformTools and move it there.
        public static float AngleAroundAxis (Vector3 dirA, Vector3 dirB, Vector3 axis) {
            // Project A and B onto the plane orthogonal _target axis
            dirA = dirA - Vector3.Project(dirA, axis);
            dirB = dirB - Vector3.Project(dirB, axis);
     
            // Find (positive) angle between A and B
            float angle = Vector3.Angle(dirA, dirB);
     
            // Return angle multiplied with 1 or -1
            return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
        }
        #endregion
    }

}
