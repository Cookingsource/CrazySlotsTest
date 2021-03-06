﻿namespace CrazySlots
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;
    using Zenject;

    public class GameplayUI : MonoBehaviour
    {
        public Text PointsText;
        public Text TimeSecondsText;
        public Button RestartButton;
        public RectTransform EndGamePanel;

        [Inject]
        Game GameController; 


        private void Start()
        {
            GameController.TotalScore.SubscribeToText(PointsText);
            GameController.LevelTimeLeftSeconds.SubscribeToText(TimeSecondsText, seconds => seconds.ToString("G4"));
            GameController.IsPlaying.DistinctUntilChanged().Subscribe(OnIsPlayingChanged);
            RestartButton.OnClickAsObservable()
                         .Where( _ => GameController.IsPlaying.Value == false)
                         .Subscribe( _ => GameController.StartLevel());
        }

        private void OnIsPlayingChanged(bool isPlaying)
        {
            bool enableEndGamePanel = !isPlaying;
            EndGamePanel.gameObject.SetActive(enableEndGamePanel);
        }
    }
}
