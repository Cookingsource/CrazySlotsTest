namespace CrazySlots
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UniRx;
    using UniRx.Triggers;

    public class Game : MonoBehaviour
    {
        private const float GroundZ = 0;

        public Transform Hero;
        public ObservableTrigger2DTrigger PenCollisionTrigger;
        
        //public Vector2 HeroPosition;
        //public Vector2 HeroTarget;
        public float HeroGatherRadious = 1f;
        public float HeroSpeed;
        public List<Animal> Followers;
        public List<Animal> FreeAnimals;
        public float SpeedIncreaseCooldownSeconds;
        public int MaxFollowers = 5;
        public int MaxAnimalsOnField = 10;
        public int ScorePerAnimal = 1;
        public float LevelDurationSeconds = 30f;
        public int TotalScore = 0;
        public float LevelTimeLeft = 0;
        public float FakeDropDelaySeconds = 0.5f; 

        private void OnEnable()
        {
            this.heroNavigator = new Navigator(HeroSpeed, Hero, WhenUserClicks);
            this.WhenHeroDropsFollowersAtPen = 
                this.PenCollisionTrigger.OnTriggerEnter2DAsObservable()
                    .Where ( collider => collider.gameObject == Hero.gameObject )
                    .Where ( _ => Followers.Count > 0 )
                    .Select( _ => Followers);
                    
            this.WhenHeroCanGatherAnimal = 
                Observable.EveryUpdate()
                    .Where( _ => Followers.Count < MaxFollowers)
                    .Select( _ => FreeAnimals.FirstOrDefault( IsAnimalInGatherRange ))
                    .Where ( animal => animal != null);

        }

        private void Update()
        {
            this.heroNavigator.Speed = HeroSpeed;
            this.heroNavigator.Update(Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            if( Application.isPlaying )
            {
                Gizmos.DrawWireSphere(heroNavigator.Target, .1f);
                Gizmos.DrawWireSphere(heroNavigator.CurrentPosition, HeroGatherRadious);
            }
        }

        private bool IsAnimalInGatherRange(Animal animal)
        {
            Vector3 distanceVector = animal.transform.position - Hero.transform.position;
            distanceVector.z = 0;
            return distanceVector.magnitude < HeroGatherRadious;
        }

        private IObservable<Vector3> WhenUserClicks = 
            Observable.EveryUpdate()
                      .Where( _ => Input.GetMouseButton(0))
                      .Select( _ => 
                      {
                          Vector3 groundPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                          groundPos.z = GroundZ;
                          return groundPos;
                      });

        private IObservable<List<Animal>> WhenHeroDropsFollowersAtPen;
        private IObservable<Animal> WhenHeroCanGatherAnimal;
        private Navigator heroNavigator;
    }
}



