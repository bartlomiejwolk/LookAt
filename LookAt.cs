using UnityEngine;
using System.Collections;

namespace LookAtEx {
    /// Just like Unity's LookAt but with much more options.
    ///
    /// Options available:
    /// - Standard
    /// - YAxisOnly
    /// - RotWithSlerp
    /// - RotThreshold
    /// - RotWithSDA
    public class LookAt : MonoBehaviour {

        public enum Options {
            Standard,
            YAxisOnly,
            RotWithSlerp,
            RotThreshold, 
            RotWithSDA }

        private Transform _transform;

        [SerializeField]
        /// How LookAt controller should work.
        private Options _option;
        public Options Option {
            get { return _option; }
            set { _option = value; }
        }

        // TODO Add public Vector3 field for selecting
        // axis to lock.
        
        /// Object to look at.
        [SerializeField]
        private Transform _target;
        public Transform Target {
            get { return _target; }
            set { _target = value; }
        }

        /// Dead zone angle where rotation don't accours.
        [SerializeField]
        private float _thresholdAngle;
        public float ThresholdAngle {
            get { return _thresholdAngle; }
            set { _thresholdAngle = value; }
        }

        /// Rotation speed.
        [SerializeField]
        private float _speed;
        public float Speed {
            get { return _speed; }
            set { _speed = value; }
        }

        /// Maximum rotation speed.
        [SerializeField]
        private float _maxRotSpeed;
        public float MaxRotSpeed {
            get { return _maxRotSpeed; }
            set { _maxRotSpeed = value; }
        }

        /// Minimum time to reach target.
        [SerializeField]
        private float _minTimeToReach;
        public float MinTimeToReach {
            get { return _minTimeToReach; }
            set { _minTimeToReach = value; }
        }

        /// When this is enabled, on mouse click a full rotation
        /// is applied.
        [SerializeField]
        private bool _clickInstantRot;
        public bool ClickInstantRot {
            get { return _clickInstantRot; }
            set { _clickInstantRot = value; }
        }

        /// Transform used to calculate angle between himself 
        /// and the target.
        ///
        /// Use it if you need sth. else that root transform.
        [SerializeField]
        private Transform _overrideRoot;
        public Transform OverrideRoot {
            get { return _overrideRoot; }
            set { _overrideRoot = value; }
        }

        /// Variable required by SmoothDampAngle().
        private float _velocity;

        /// GUIStyle options for scene view labels.
        [SerializeField]
        private GUIStyle _labelStyle;
        public GUIStyle LabelStyle {
            get { return _labelStyle; }
            set { _labelStyle = value; }
        }

        private void Awake() {
            if (!_target) {
                // todo Add Utilities class
                //MissingReference("_target", InfoType.Error);
            }
        }

        private void Start () {
            _transform = GetComponent<Transform>();
        }
        
        private void Update () {
            if (!_target) {
                return;
            }

            switch (_option) {
                case Options.Standard:
                    _transform.LookAt(_target);
                    break;
                case Options.YAxisOnly:
                    Vector3 v = _target.position - _transform.position;
                    v.x = v.z = 0.0f;
                    _transform.LookAt(_target.transform.position - v); 
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

        private void RotateWithSlerp() {
            // Rotation to be applied.
            Vector3 newRotation;
            // Direction to the target.
            Vector3 dir;

            dir = _target.position - _transform.position;
            // Calculate rotation to the target.
            newRotation = Quaternion.LookRotation(dir).eulerAngles;
            // Rotate only around y axis.
            newRotation.x = 0;
            newRotation.z = 0;
            // Apply rotation.
            _transform.rotation = Quaternion.Slerp(
                    _transform.rotation,
                    Quaternion.Euler(newRotation),
                    Time.deltaTime * _speed);
        }

        private void RotateWithSDA() {
            // Direction to the target.
            Vector3 dir;
            // Rotation to be applied.
            Vector3 newRotation;
            // Transform current rotation.
            Vector3 angles;

            // Calculate direction to the target.
            dir = _target.position - _transform.position;
            // Calculate rotation to the target.
            newRotation = Quaternion.LookRotation(dir).eulerAngles;
            // Remember current rotation.
            angles = _transform.rotation.eulerAngles;
            // Rotate only around y axis.
            angles.x = 0;
            angles.z = 0;
            // Apply rotation.
            _transform.rotation = Quaternion.Euler(
                    angles.x,
                    Mathf.SmoothDampAngle(
                        angles.y,
                        newRotation.y,
                        ref _velocity,
                        _minTimeToReach,
                        _maxRotSpeed),
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
                    _transform.forward,
                    _target.position - _transform.position,
                    Vector3.up);

            if (_overrideRoot) {
                hAngleCustom = AngleAroundAxis(
                        _overrideRoot.forward,
                        _target.position - _overrideRoot.position,
                        Vector3.up);
            }
            else {
                hAngleCustom = AngleAroundAxis(
                        _transform.forward,
                        _target.position - _transform.position,
                        Vector3.up);
            }

            // On click, omit all the smoothing code and rotate
            // transform immediately.
            if (Input.GetKey(KeyCode.Mouse0) && _clickInstantRot) {
                _transform.Rotate(0, hAngleCustom, 0);
                return;
            }

            // If target is within specified threshold, return 0
            // which means, that there's no reason to rotate.
            hAngleThr = Mathf.Max(
                    0,
                    Mathf.Abs(hAngle) - _thresholdAngle);
            // If target is beyond specified threshold, add a sign
            // to know which direction to rotate.
            hAngleThr *= Mathf.Sign(hAngle);

            // Rotation to be applied.
            newRotation = Mathf.SmoothDampAngle(
                    newRotation,
                    hAngleThr,
                    ref _velocity,
                    _minTimeToReach,
                    _maxRotSpeed);

            // Apply rotation to the transform.
            _transform.Rotate (0, newRotation, 0);

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
    }
}
