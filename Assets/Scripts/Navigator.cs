﻿namespace CrazySlots
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UniRx;

    public class Navigator 
    {
        private Transform mobile;
        private Vector3 target;

        public Navigator(float initialSpeed, Transform mobile, IObservable<Vector3> WhenTargetUpdates)
        {
            this.mobile = mobile;
            this.Speed = initialSpeed;
            WhenTargetUpdates.Subscribe( newTarget => target = newTarget);
        }

        public float Speed { get; set; }
        public Vector3 Target { get { return target; } }
        public Vector3 CurrentPosition { get { return mobile.position;}}

        public void Update(float deltaTime)
        {
            float distanceUpdate = Speed * deltaTime;
            Vector3 vectorToTarget = target - CurrentPosition;
            float distanceToTarget = vectorToTarget.magnitude;
            Vector3 vectorToTargetNormalized = vectorToTarget / distanceToTarget;
            
            if( distanceToTarget > distanceUpdate)
            {
                mobile.position += vectorToTargetNormalized * distanceUpdate;
                mobile.up = vectorToTargetNormalized;
            }
            else
                mobile.position = target;
        }
    }
    
}

