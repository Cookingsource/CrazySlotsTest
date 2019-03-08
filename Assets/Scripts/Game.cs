namespace CrazySlots
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UniRx;
    using UniRx.Triggers;
    using System;

    public class Game : MonoBehaviour
    {
        private const float GroundZ = 0;

        public Transform Hero;
        public List<Transform> FormationSlots;
        public ObservableTrigger2DTrigger PenCollisionTrigger;
        public Rect SpawnArea = new Rect(0,0,1,1);

        public AnimalPool WhiteSheepPool = new AnimalPool();
        
        public float HeroGatherRadious = 1f;
        public float HeroSpeed;
        public List<Animal> Followers;
        public List<Animal> FreeAnimals;
        public float SpeedIncreaseCooldownSeconds;
        public int MaxFollowers = 5;
        public int MaxAnimalsOnField = 10;
        public float LevelDurationSeconds = 30f;
        public ReactiveProperty<int> TotalScore = new ReactiveProperty<int>(0);
        public ReactiveProperty<float> LevelTimeLeftSeconds = new ReactiveProperty<float>(0);
        public ReactiveProperty<bool> IsPlaying = new ReactiveProperty<bool>(false);
        public float FakeDropDelaySeconds = 0.5f; 

        private void OnEnable()
        {
            WhiteSheepPool.Initialize(MaxAnimalsOnField + MaxFollowers);
            WhenUserClicks = 
                Observable.EveryUpdate()
                        .Where( _ => IsPlaying.Value == true && Input.GetMouseButton(0) && !droppingAnimals)
                        .Select( _ => 
                        {
                            Vector3 groundPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            groundPos.z = GroundZ;
                            return groundPos;
                        });

            this.heroNavigator = new Navigator(HeroSpeed, Hero, WhenUserClicks);
            this.followerNavigators = 
                this.FormationSlots.Select( 
                    slot => new Navigator(0, null, Observable.EveryUpdate().Select( _=> slot.position)))
                .ToList();

            this.WhenHeroWillDropFollowersAtPen = 
                this.PenCollisionTrigger.OnTriggerEnter2DAsObservable()
                    .Where ( collider => collider.gameObject == Hero.gameObject )
                    .Where ( _ => Followers.Count > 0 && IsPlaying.Value == true )
                    .Select( _ => Followers);

            this.WhenHeroCanGatherAnimal = 
                Observable.EveryUpdate()
                    .Where( _ => Followers.Count < MaxFollowers && IsPlaying.Value == true)
                    .Select( _ => FreeAnimals.FirstOrDefault( IsAnimalInGatherRange ))
                    .Where ( animal => animal != null);

             
            this.WhenAnimalSpawns = 
                Observable.Interval(TimeSpan.FromSeconds(1))
                          .Where( _ => FreeAnimals.Count < MaxAnimalsOnField && IsPlaying.Value == true)
                          .Select( _ => WhiteSheepPool.Retain());

            WhenHeroCanGatherAnimal.Subscribe(AddAnimalToFollowers);
            WhenAnimalSpawns.Subscribe(AddToFieldInRandomPosition);
            WhenHeroWillDropFollowersAtPen.Select( followers => DropAnimalsInPen(followers))
                                          .Switch()
                                          .Subscribe( _ => ReleaseFollowers());
            StartLevel();
        }

        private void StartLevel()
        {
            this.TotalScore.Value = 0;
            this.LevelTimeLeftSeconds.Value = 0;
            this.WhiteSheepPool.ReleaseAll();
            this.Followers.Clear();
            this.FreeAnimals.Clear();

            if( this.countDownSubscription != null)
                this.countDownSubscription.Dispose();
            float timeTickSeconds = 0.01f;
            this.LevelTimeLeftSeconds.Value = LevelDurationSeconds;
            var levelCountDown = Observable.Interval(TimeSpan.FromSeconds(0.01f))
                                           .Take( Mathf.CeilToInt( LevelDurationSeconds / timeTickSeconds));

            this.countDownSubscription = levelCountDown.Subscribe( _ => LevelTimeLeftSeconds.Value -= timeTickSeconds);
            this.IsPlaying.Value = true;
        }

        private void ReleaseFollowers()
        {
            this.TotalScore.Value += Followers.Sum( animal => animal.Points);
            foreach( var follower in Followers)
                WhiteSheepPool.Release(follower);

            Followers.Clear();
        }

        private IObservable<Unit> DropAnimalsInPen(List<Animal> animals)
        {
            this.droppingAnimals = true;
            return Observable.Interval(TimeSpan.FromSeconds(FakeDropDelaySeconds))
                             .Take(1)
                             .Do( _ => droppingAnimals = false)
                             .Select( _ => Unit.Default);
        }

        private void AddToFieldInRandomPosition(Animal animal)
        {
            float x = UnityEngine.Random.Range(SpawnArea.xMin, SpawnArea.xMax);
            float y = UnityEngine.Random.Range(SpawnArea.yMin, SpawnArea.yMax);
            Vector3 fieldPos = new Vector3(x, y, GroundZ);
            animal.transform.position = fieldPos;
            this.FreeAnimals.Add(animal);
        }

        private void AddAnimalToFollowers(Animal animal)
        {
            FreeAnimals.Remove(animal);
            Followers.Add(animal);
            for( int i = 0; i < followerNavigators.Count; i++)
            {
                var navigator = followerNavigators[i];
                if( i < Followers.Count)
                {
                    navigator.Speed = Followers[i].Speed;
                    navigator.Mobile = Followers[i].transform;
                }
                else
                {
                    navigator.Speed = 0;
                    navigator.Mobile = null;
                }
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            this.heroNavigator.Speed = HeroSpeed;
            this.heroNavigator.Update(deltaTime);
            for( int i = 0; i < Followers.Count ; i++)
            {
                var navigator = followerNavigators[i];
                navigator.Update(deltaTime);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(this.SpawnArea.center, this.SpawnArea.size);

            if( Application.isPlaying )
            {
                Gizmos.DrawWireSphere(heroNavigator.Target, .1f);
                for( int i = 0; i < Followers.Count ; i++)
                {
                    Gizmos.DrawWireSphere(followerNavigators[i].Target, .1f);
                }
                Gizmos.DrawWireSphere(heroNavigator.CurrentPosition, HeroGatherRadious);
            }
        }

        private bool IsAnimalInGatherRange(Animal animal)
        {
            Vector3 distanceVector = animal.transform.position - Hero.transform.position;
            distanceVector.z = 0;
            return distanceVector.magnitude < HeroGatherRadious;
        }

        private IObservable<Vector3> WhenUserClicks;
        private IObservable<List<Animal>> WhenHeroWillDropFollowersAtPen;
        private IObservable<Animal> WhenHeroCanGatherAnimal;
        private IObservable<Animal> WhenAnimalSpawns;
                                                             
        private Navigator heroNavigator;
        private List<Navigator> followerNavigators;
        private bool droppingAnimals;
        private IDisposable countDownSubscription;

        [Serializable]
        public class AnimalPool : Pool<Animal> {}
    }
}



