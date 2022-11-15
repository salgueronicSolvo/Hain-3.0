using AnythingWorld.Animation;
using UnityEngine;

namespace AnythingWorld
{
    public class RandomMovement : MonoBehaviour
    {
        //Position variables
        private Vector3 goalPosition;
        private Vector3 directionToGoal;
        //Debug
        public float brakingVariable = 1;
        private float variableSpeed;
        private float distanceToGoal;
        //input
        [Header("Speed")]
        public float maxSpeed = 3;
        public float turnSpeed = 2;
        [Header("Thresholds")]
        [Tooltip("Speed above which walk animation is called.")]
        public float walkThreshold = 0.1f;
        [Tooltip("Speed above which run animation is called.")]
        public float runThreshold = 1.5f;
        [Tooltip("Limit movement speed to the maximum threshold active.")]
        public bool clampToActivatedThresholds = false;
        [Header("Animation States Active")]
        public bool walk = true;
        public bool run = true;
        [Header("Braking")]
        public bool generateNewPoints = true;
        public bool brakeAtDestination = true;
        public float brakeDist = 2;
        [Header("Goal Randomization")]
        public float positionSpawnRadius = 20;
        public float goalRadius = 1;
        public float stopThreshold = 0.1f;
        [Header("Set Manual Goal")]
        public bool manualGoal = false;
        public Transform manualGoalTransform;
        [Header("Spawn Around Point")]
        [Tooltip("If false, spawn radius will around the model position.")]
        public bool spawnAroundPoint = false;
        public Vector3 inputSpawnCentroid = Vector3.zero;
        private Vector3 spawnCentroid = Vector3.zero;
        public RunWalkIdleController animationController;
        public bool showGizmos = true;
        public bool stopMovement = false;
        public void Start()
        {
            if (GetComponentInChildren<LegacyAnimationController>())
            {
                animationController = GetComponentInChildren<RunWalkIdleController>();
                animationController.crossfadeTime = 0.5f;
            }

        }

        public void Update()
        {
            variableSpeed = Mathf.Lerp(maxSpeed * brakingVariable, variableSpeed, Time.deltaTime);
            if (variableSpeed < stopThreshold) variableSpeed = 0;
            distanceToGoal = Vector3.Distance(goalPosition, transform.position);
            //Set center point
            if (spawnAroundPoint) spawnCentroid = inputSpawnCentroid;
            else spawnCentroid = transform.position;
           
            if (manualGoal)
            {
                goalPosition = manualGoalTransform.position;
            }
            else
            {
                if (distanceToGoal <= goalRadius && generateNewPoints)
                {
                    goalPosition = GetRandomPositionInsideSphere(spawnCentroid);

                }
            }

            //Brake when close to target
            if (brakeAtDestination) { brakingVariable = Mathf.Clamp(distanceToGoal - (goalRadius / 2) - brakingVariable, 0, 1); } else { brakingVariable = 1; };
            //Calculate vector to goal
            directionToGoal = new Vector3(goalPosition.x, transform.position.y, goalPosition.z) - transform.position;


            if(walk && run)
            {

                //Blend animation
                animationController?.BlendAnimationOnSpeed(variableSpeed, walkThreshold,runThreshold);
            }
            else
            {
                variableSpeed = Mathf.Min(variableSpeed, runThreshold);
                animationController?.BlendAnimationOnSpeed(variableSpeed, walkThreshold);
            }


            if (stopMovement) return;
            TurnTowardsTarget(directionToGoal);
            MoveTowardsTarget();

        }
        public void MoveTowardsTarget()
        {
            transform.position = Vector3.Lerp(transform.position, transform.position + (transform.forward), variableSpeed * Time.deltaTime);

        }

        public void TurnTowardsTarget(Vector3 directionToTarget)
        {
            // Turn towards the target


            var normalizedLookDirection = directionToTarget.normalized;
            var m_LookRotation = Quaternion.LookRotation(normalizedLookDirection);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, m_LookRotation, Time.deltaTime * turnSpeed);
        }
        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (!showGizmos) return;
            try
            {
                GUI.color = Color.white;
                UnityEditor.Handles.Label(goalPosition + (Vector3.left * goalRadius), new GUIContent("Goal"));
                Gizmos.color = Color.white;
                var midpoint = (transform.position + goalPosition) * 0.5f;
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, directionToGoal);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, transform.forward * distanceToGoal / 2);

                UnityEditor.Handles.DrawWireDisc(goalPosition, Vector3.up, goalRadius);
                UnityEditor.Handles.DrawWireDisc(spawnCentroid, Vector3.up, positionSpawnRadius);
                UnityEditor.Handles.Label(spawnCentroid + (Vector3.left * (positionSpawnRadius) + Vector3.left), "Spawn Radius");
                var angle = Vector3.SignedAngle(transform.forward, directionToGoal, Vector3.up);
                var re = Vector3.Cross(transform.forward, directionToGoal);
                UnityEditor.Handles.Label(Vector3.Lerp(transform.position, goalPosition, 0.5f), angle.ToString("F2") + "°");
                UnityEditor.Handles.DrawWireArc(transform.position, transform.up, transform.forward, angle, distanceToGoal * 0.5f);
                GUI.color = Color.white;
            }
            catch
            {

            }
#endif

        }

        private Vector3 GetRandomPositionInsideSphere(Vector3 spawnCentroid)
        {
            var randomPosition = Random.insideUnitSphere * positionSpawnRadius;
            randomPosition = new Vector3(randomPosition.x, 0, randomPosition.z);
            randomPosition = spawnCentroid + randomPosition;
            return randomPosition;

        }
    }
}
