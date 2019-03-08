namespace CrazySlots
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Hero : MonoBehaviour 
    {
        public float Speed = 10f;
        public float GatherRadious = 1f;

        public bool IsAnimalInGatherRange(Animal animal)
        {
            Vector3 distanceVector = animal.transform.position - this.transform.position;
            distanceVector.z = 0;
            return distanceVector.magnitude < GatherRadious;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(this.transform.position, GatherRadious);
        }
    }
}

