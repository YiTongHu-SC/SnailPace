using System;
using System.Collections;
using Core;
using DG.Tweening;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class RunPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI ShowCurrentTimeText;
        [SerializeField] private TextMeshProUGUI ShowStartCountDownText;
        [SerializeField] private TextMeshProUGUI AddRunTimeText;

        private void Start()
        {
            AddRunTimeText.text = "+10";
            AddRunTimeText.color = Color.magenta;
            var color = AddRunTimeText.color;
            color.a = 0;
            AddRunTimeText.color = color;
        }

        private void Update()
        {
            ShowCurrentTimeText.text = GameManager.Instance.RunClock;
        }

        private void ShowCountDown(int count)
        {
            ShowStartCountDownText.text = count.ToString();
            ShowStartCountDownText.fontSize = 10;
            ShowStartCountDownText.color = Color.white;
            ShowStartCountDownText.DOFade(0, 0.8f);
            DOTween.To(() => ShowStartCountDownText.fontSize,
                (x) => ShowStartCountDownText.fontSize = x,
                5, 0.4f);
        }

        private void OnWinning()
        {
            ShowCurrentTimeText.text = GameManager.Instance.RunClock;
        }

        private void OnAddRunTime()
        {
            var color = AddRunTimeText.color;
            color.a = 1;
            AddRunTimeText.color = color;
            AddRunTimeText.transform.localPosition = Vector3.zero;
            AddRunTimeText.transform.DOLocalMoveY(0.5f, 1f);
            StartCoroutine(FadeDelay(2f));
        }

        IEnumerator FadeDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            AddRunTimeText.DOFade(0, 0.5f);
        }

        private void OnEnable()
        {
            GameEventManager.Instance.OnGameWinning += OnWinning;
            GameEventManager.Instance.OnStartCountDown += ShowCountDown;
            GameEventManager.Instance.OnRunReward += OnAddRunTime;
        }

        private void OnDisable()
        {
            GameEventManager.Instance.OnGameWinning -= OnWinning;
            GameEventManager.Instance.OnStartCountDown -= ShowCountDown;
            GameEventManager.Instance.OnRunReward -= OnAddRunTime;
        }
    }
}