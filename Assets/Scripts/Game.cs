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

        public Hero Hero;
        public List<Transform> FormationSlots;
        public ObservableTrigger2DTrigger PenCollisionTrigger;
        public Rect SpawnArea = new Rect(0,0,1,1);
        public AnimalPool WhiteSheepPool = new AnimalPool();
        public float SpeedIncreaseCooldownSeconds = 10f;
        public int MaxFollowers = 5;
        public int MaxAnimalsOnField = 10;
        public float LevelDurationSeconds = 30f;
        public ReactiveProperty<int> TotalScore = new ReactiveProperty<int>(0);
        public ReactiveProperty<float> LevelTimeLeftSeconds = new ReactiveProperty<float>(0);
        public ReactiveProperty<bool> IsPlaying = new ReactiveProperty<bool>(false);
        public float FakeDropDelaySeconds = 0.5f; 
        private List<Animal> Followers = new List<Animal>();
        private List<Animal> FreeAnimals = new List<Animal>();

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

            var heroNavigator = new Navigator(Hero.Speed, Hero.transform, WhenUserClicks);
            this.formationNavigator = new FormationNavigator(
                FreeAnimals,
                Followers, 
                FormationSlots, 
                heroNavigator);

            this.WhenHeroWillDropFollowersAtPen = 
                this.PenCollisionTrigger.OnTriggerEnter2DAsObservable()
                    .Where ( collider => collider.gameObject == Hero.gameObject )
                    .Where ( _ => Followers.Count > 0 && IsPlaying.Value == true )
                    .Select( _ => Followers);

            this.WhenHeroCanGatherAnimal = 
                Observable.EveryUpdate()
                    .Where( _ => Followers.Count < MaxFollowers && IsPlaying.Value == true)
                    .Select( _ => FreeAnimals.FirstOrDefault( Hero.IsAnimalInGatherRange ))
                    .Where ( animal => animal != null);

             
            this.WhenAnimalSpawns = 
                Observable.Interval(TimeSpan.FromSeconds(1))
                          .Where( _ => FreeAnimals.Count < MaxAnimalsOnField && IsPlaying.Value == true)
                          .Select( _ => WhiteSheepPool.Retain());

            WhenHeroCanGatherAnimal.Subscribe( formationNavigator.AddAnimalToFollowers);
            WhenAnimalSpawns.Subscribe(AddToFieldInRandomPosition);
            WhenHeroWillDropFollowersAtPen.Select( followers => DropAnimalsInPen(followers))
                                          .Switch()
                                          .Subscribe( _ => ReleaseFollowers());
            StartLevel();
        }

        public void StartLevel()
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

            this.countDownSubscription = levelCountDown.Subscribe( _ => 
            {
                LevelTimeLeftSeconds.Value -= timeTickSeconds;
                if(Mathf.Abs( LevelTimeLeftSeconds.Value ) < 0.001 )
                {
                    LevelTimeLeftSeconds.Value = 0;
                    this.IsPlaying.Value = false;
                }
            });
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

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            this.formationNavigator.Update(deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(this.SpawnArea.center, this.SpawnArea.size);

            if( Application.isPlaying )
            {
                formationNavigator.DrawGizmos();
            }
        }

        private IObservable<Vector3> WhenUserClicks;
        private IObservable<List<Animal>> WhenHeroWillDropFollowersAtPen;
        private IObservable<Animal> WhenHeroCanGatherAnimal;
        private IObservable<Animal> WhenAnimalSpawns;
        private FormationNavigator formationNavigator;

        private bool droppingAnimals;
        private IDisposable countDownSubscription;

        [Serializable]
        public class AnimalPool : Pool<Animal> {}
    }
}



