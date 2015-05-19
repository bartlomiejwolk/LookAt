// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the LookAt extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using UnityEngine;
using System.Collections;
using UnityEditor;

namespace LookAtEx {

    [CustomEditor(typeof(LookAt))]
    public class LookAtEditor : Editor {

        #region PROPERTIES 

        LookAt Script { get; set; }

        #endregion

        #region SERIALIZED PROPERTIES
        private SerializedProperty labelStyle;
        private SerializedProperty targetTransform;
        #endregion

        #region UNITY MESSAGES
        public void OnSceneGUI() {
            LookAt script = (LookAt)target;

            switch (script.Option) {
                case Options.RotThreshold:
                    Handles.color = Color.blue;
                    Handles.Label(
                        // TODO Add param for label position.
                            script.transform.position + Vector3.up * 1.5f,
                            "ThresholdAngle: " + script.ThresholdAngle.ToString(),
                            script.LabelStyle);
                    Handles.DrawWireArc(
                            script.transform.position,
                            script.transform.up,
                        // Make the arc simetrical on the left and right
                        // side of the player.
                            Quaternion.AngleAxis(
                                -script.ThresholdAngle / 2,
                                Vector3.up) * script.transform.forward,
                            script.ThresholdAngle,
                            2);
                    script.ThresholdAngle = Handles.ScaleValueHandle(
                            script.ThresholdAngle,
                            script.transform.position + script.transform.forward * 2,
                            script.transform.rotation,
                            1,
                            Handles.ConeCap,
                            1);
                    break;
            }
        }


        public void OnEnable() {
            Script = (LookAt) target;

            labelStyle = serializedObject.FindProperty("labelStyle");
            targetTransform = serializedObject.FindProperty("targetTransform");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawTargetTransformField();
            DrawLabelStyleControls();
            DrawOptionPopup();
            DrawOverrideRootField();
            DrawInstantRotationToggle();

            HandleOptionPopup();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region INSPECTOR METHODS
        private void HandleOptionPopup() {
            switch (Script.Option) {
                case Options.Standard:
                    break;
                case Options.YAxisOnly:
                    break;
                case Options.RotWithSlerp:
                    DrawSpeedField();
                    break;
                case Options.RotThreshold:
                    DrawMaxRotSpeedField();
                    DrawMinTimeToReachField();
                    DrawThresholdAngleField();
                    break;
                case Options.RotWithSDA:
                    DrawMaxRotSpeedField();
                    DrawMinTimeToReachField();
                    break;
            }
        }

        private void DrawThresholdAngleField() {
            Script.ThresholdAngle = EditorGUILayout.FloatField(
                "Threshold angle",
                Script.ThresholdAngle);
        }

        private void DrawMinTimeToReachField() {
            Script.MinTimeToReach = EditorGUILayout.FloatField(
                "Min Time to Reach",
                Script.MinTimeToReach);
        }

        private void DrawMaxRotSpeedField() {
            Script.MaxRotSpeed = EditorGUILayout.FloatField(
                "Max Rot. Speed",
                Script.MaxRotSpeed);
        }

        private void DrawSpeedField() {
            Script.Speed = EditorGUILayout.FloatField(
                "Speed",
                Script.Speed);
        }

        private void DrawInstantRotationToggle() {
            Script.ClickInstantRot = EditorGUILayout.Toggle(
                "Instant Rotation",
                Script.ClickInstantRot);
        }

        private void DrawOverrideRootField() {
            Script.OverrideRoot = (Transform) EditorGUILayout.ObjectField(
                "Transform",
                Script.OverrideRoot,
                typeof (Transform),
                true);
        }

        private void DrawOptionPopup() {
            Script.Option = (Options) EditorGUILayout.EnumPopup(
                "Option",
                Script.Option);
        }

        private void DrawLabelStyleControls() {
            EditorGUILayout.PropertyField(
                labelStyle,
                new GUIContent(
                    "Label Style",
                    "Style of the 'on scene' info label"),
                true);
        }

        private void DrawTargetTransformField() {
            EditorGUILayout.PropertyField(targetTransform);
        }
        #endregion
        #region METHODS

        [MenuItem("Component/LookAt")]
        private static void AddLookAtComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(LookAt));
            }
        }

        #endregion METHODS
    }
}
