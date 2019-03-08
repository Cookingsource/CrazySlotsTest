namespace CrazySlots
{
    using System;
    using UnityEngine;
    using Zenject;

    public class GameInstaller : MonoInstaller
    {
        public Game gameController;
        public AnimalPool WhiteSheepPool = new AnimalPool();

        public override void InstallBindings()
        {
            Container.Bind<Game>().FromInstance(gameController);
            Container.Bind<AnimalPool>().FromInstance(WhiteSheepPool);
        }
    }
    
    [Serializable]
    public class AnimalPool : Pool<Animal> {}
}
