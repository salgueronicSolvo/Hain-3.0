using UnityEngine;

namespace AnythingWorld.Animation
{
    public static class HelicopterRootMotion
    {
        //public static void VerticalMovement(Rigidbody rb, float speed, float downforce, bool onGround)
        //{
        //var force = speed - downforce;
        //if (onGround)
        //{
        // force = Mathf.Max(speed-downforce, 0);
        //}

        //transform.position = transform.position + (transform.up *force * Time.deltaTime );
        //}


        public static void VerticalMovement(Rigidbody rb, float effectiveHeight, float engineForce)
        {
            var upForce = 1 - Mathf.Clamp(rb.transform.position.y / effectiveHeight, 0, 1);
            upForce = Mathf.Lerp(0f, engineForce, upForce) * rb.mass;
            rb.AddRelativeForce(Vector3.up * upForce);
        }
    }
}
