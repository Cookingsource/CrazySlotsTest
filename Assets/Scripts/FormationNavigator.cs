namespace CrazySlots
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using UniRx;
    using System;

    public class FormationNavigator 
    {
        private Navigator heroNavigator;
        private List<Navigator> followerNavigators;
        private Vector3 target;
        private List<Animal> followers;
        private List<Animal> freeAnimals;

        public FormationNavigator(List<Animal> freeAnimals, List<Animal> followers, List<Transform> formationSlots, Navigator lead)
        {
            this.Lead = lead;
            this.followers = followers;
            this.freeAnimals = freeAnimals;
            this.followerNavigators = 
                formationSlots.Select( 
                    slot => new Navigator(0, null, Observable.EveryUpdate().Select( _=> slot.position)))
                .ToList();
            this.heroNavigator = lead;
        }

        public void AddAnimalToFollowers(Animal animal)
        {
            freeAnimals.Remove(animal);
            followers.Add(animal);
            for( int i = 0; i < followerNavigators.Count; i++)
            {
                var navigator = followerNavigators[i];
                if( i < followers.Count)
                {
                    navigator.Speed = followers[i].Speed;
                    navigator.Mobile = followers[i].transform;
                }
                else
                {
                    navigator.Speed = 0;
                    navigator.Mobile = null;
                }
            }
        }

        /* public Navigator( float initialSpeed, Transform mobile, IObservable<Vector3> WhenTargetUpdates)
        {
            this.Mobile = mobile;
            this.Speed = initialSpeed;
            WhenTargetUpdates.Subscribe( newTarget => target = newTarget);
        }
*/
        public float LeadSpeed { get; set; }
        public Transform Mobile {get; set;}
        public Vector3 Target { get { return target; } }
        public Vector3 CurrentPosition { get { return Mobile.position;}}

        public Navigator Lead { get; set; }

        public void Update(float deltaTime)
        {
            //this.heroNavigator.Speed = HeroSpeed;
            this.heroNavigator.Update(deltaTime);
            for( int i = 0; i < followers.Count ; i++)
            {
                var navigator = followerNavigators[i];
                navigator.Update(deltaTime);
            }
        }

        public void DrawGizmos()
        {
            Gizmos.DrawWireSphere(heroNavigator.Target, .1f);
            for( int i = 0; i < followers.Count ; i++)
            {
                Gizmos.DrawWireSphere(followerNavigators[i].Target, .1f);
            }
        }
    }
    
}

