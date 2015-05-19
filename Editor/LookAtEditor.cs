// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
// 
// This file is part of the LookAt extension for Unity. Licensed under the MIT
// license. See LICENSE file in the project root folder.

using UnityEditor;
using UnityEngine;

namespace LookAtEx {

    [CustomEditor(typeof (LookAt))]
    public class LookAtEditor : Editor {
        #region PROPERTIES

        private LookAt Script { get; set; }

        #endregion PROPERTIES

        #region SERIALIZED PROPERTIES

        private SerializedProperty labelStyle;
        private SerializedProperty targetTransform;
        private SerializedProperty description;

        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public void OnEnable() {
            Script = (LookAt) target;

            labelStyle = serializedObject.FindProperty("labelStyle");
            targetTransform = serializedObject.FindProperty("targetTransform");
            description = serializedObject.FindProperty("description");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawVersionLabel();
            DrawDescriptionTextArea();

            EditorGUILayout.Space();

            DrawTargetTransformField();
            DrawLabelStyleControls();
            DrawOptionPopup();
            DrawOverrideRootField();
            DrawInstantRotationToggle();

            HandleOptionPopup();

            serializedObject.ApplyModifiedProperties();
        }

        // todo refactor
        public void OnSceneGUI() {
            switch (Script.Option) {
                case Options.RotThreshold:
                    Handles.color = Color.blue;
                    Handles.Label(
                        // TODO Add param for label position.
                        Script.transform.position + Vector3.up * 1.5f,
                        "ThresholdAngle: " + Script.ThresholdAngle,
                        Script.LabelStyle);
                    Handles.DrawWireArc(
                        Script.transform.position,
                        Script.transform.up,
                        // Make the arc simetrical on the left and right side
                        // of the player.
                        Quaternion.AngleAxis(
                            -Script.ThresholdAngle / 2,
                            Vector3.up) * Script.transform.forward,
                        Script.ThresholdAngle,
                        2);
                    Script.ThresholdAngle = Handles.ScaleValueHandle(
                        Script.ThresholdAngle,
                        Script.transform.position + Script.transform.forward * 2,
                        Script.transform.rotation,
                        1,
                        Handles.ConeCap,
                        1);
                    break;
            }
        }

        #endregion UNITY MESSAGES

        #region INSPECTOR METHODS

        private void DrawInstantRotationToggle() {
            Script.ClickInstantRot = EditorGUILayout.Toggle(
                new GUIContent(
                    "Instant Rotation",
                    ""),
                Script.ClickInstantRot);
        }

        private void DrawLabelStyleControls() {
            EditorGUILayout.PropertyField(
                labelStyle,
                new GUIContent(
                    "Label Style",
                    "Style of the 'on scene' info label"),
                true);
        }

        private void DrawMaxRotSpeedField() {
            Script.MaxRotSpeed = EditorGUILayout.FloatField(
                new GUIContent(
                    "Max Rot. Speed",
                    ""),
                Script.MaxRotSpeed);
        }

        private void DrawMinTimeToReachField() {
            Script.MinTimeToReach = EditorGUILayout.FloatField(
                new GUIContent(
                    "Min Time to Reach",
                    ""),
                Script.MinTimeToReach);
        }

        private void DrawOptionPopup() {
            Script.Option = (Options) EditorGUILayout.EnumPopup(
                new GUIContent(
                    "Option",
                    ""),
                Script.Option);
        }

        private void DrawOverrideRootField() {
            Script.OverrideRoot = (Transform) EditorGUILayout.ObjectField(
                new GUIContent(
                    "Transform",
                    ""),
                Script.OverrideRoot,
                typeof (Transform),
                true);
        }

        private void DrawSpeedField() {
            Script.Speed = EditorGUILayout.FloatField(
                new GUIContent(
                    "Speed",
                    ""),
                Script.Speed);
        }

        private void DrawTargetTransformField() {
            EditorGUILayout.PropertyField(
                targetTransform,
                new GUIContent(
                    "Target Transform",
                    ""));
        }

        private void DrawThresholdAngleField() {
            Script.ThresholdAngle = EditorGUILayout.FloatField(
                new GUIContent(
                    "Threshold Angle",
                    ""),
                Script.ThresholdAngle);
        }

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

        private void DrawVersionLabel() {
            EditorGUILayout.LabelField(
                string.Format(
                    "{0} ({1})",
                    LookAt.Version,
                    LookAt.Extension));
        }

        private void DrawDescriptionTextArea() {
            description.stringValue = EditorGUILayout.TextArea(
                description.stringValue);
        }
        #endregion INSPECTOR METHODS

        #region METHODS

        [MenuItem("Component/LookAt")]
        private static void AddLookAtComponent() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof (LookAt));
            }
        }

        #endregion METHODS
    }

}