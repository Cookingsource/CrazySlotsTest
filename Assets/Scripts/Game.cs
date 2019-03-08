namespace CrazySlots
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Game : MonoBehaviour
    {
        public Vector2 HeroPosition;
        public Vector2 HeroTarget;
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
    }
}



