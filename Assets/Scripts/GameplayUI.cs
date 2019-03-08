namespace CrazySlots
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;

    public class GameplayUI : MonoBehaviour
    {
        public Text PointsText;
        public Text TimeSecondsText;
        public Button RestartButton;

        public Game gameController; 
        

        private void Start()
        {
            gameController.TotalScore.SubscribeToText(PointsText);
            gameController.LevelTimeLeftSeconds.SubscribeToText(PointsText, seconds => seconds.ToString("G4"));
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
