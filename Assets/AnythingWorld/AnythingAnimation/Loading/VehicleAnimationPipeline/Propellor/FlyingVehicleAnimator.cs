using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnythingWorld.Animation
{
    public abstract class FlyingVehicleAnimator : MonoBehaviour
    {
        public abstract void Accelerate();
        public abstract void Deceleration();
        public abstract void Stop();
    }
}
