using UnityEngine;

namespace AnythingWorld.Animation
{
    public class HelicoperController : MonoBehaviour
    {
        public bool controlThisVehicle = true;
        public bool rootMovement = true;
        private PropellorVehicleAnimator animator;
        private BoxCollider helicopterCollider;
        private Rigidbody helicopterRigidBody;
        public float effectiveHeight = 100f;
        public bool onGround = false;

        private void Start()
        {
            MakeRigidbody();

            TryGetComponent<PropellorVehicleAnimator>(out animator);

            MakeCollider();
        }

        private void MakeRigidbody()
        {
            if (helicopterRigidBody == null)
            {
                if (!TryGetComponent<Rigidbody>(out helicopterRigidBody))
                {
                    helicopterRigidBody = gameObject.AddComponent<Rigidbody>();
                }
            }
            helicopterRigidBody.mass = 500;
        }

        private void MakeCollider()
        {
            if (helicopterCollider == null) helicopterCollider = gameObject.AddComponent<BoxCollider>();
            var bounds = new Bounds();
            foreach (var rend in gameObject.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(rend.bounds);
            }

            helicopterCollider.size = bounds.size;
            helicopterCollider.center = bounds.center;

        }

        // Update is called once per frame
        void Update()
        {
            if (controlThisVehicle && animator != null)
            {

                //Acceleration
                if (Input.GetKey(KeyCode.W))
                {
                }
                else if (Input.GetKey(KeyCode.S))
                {

                }

                //Turning
                if (Input.GetKey(KeyCode.D))
                {
                }
                else if (Input.GetKey(KeyCode.A))
                {
                }
                else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
                {
                }


                if (Input.GetKey(KeyCode.Space))
                {
                    animator.Accelerate();
                }
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    animator.Deceleration();
                }


                if (rootMovement) HelicopterRootMotion.VerticalMovement(helicopterRigidBody, effectiveHeight, animator.engineForce / 100);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 9)
            {
                Debug.Log("Hit ground");
                onGround = true;
            }
        }
        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.layer == 9)
            {
                Debug.Log("Left ground");
                onGround = false;
            }
        }
    }
}
