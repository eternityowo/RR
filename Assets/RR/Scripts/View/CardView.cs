using System;
using DG.Tweening;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RR.View
{
    // public interface ICardView
    // {
    //     TextMeshProUGUI GetStatView(CardParamType type);
    // }

    public class CardView : MonoBehaviour//, ICardView
    {
        public TextMeshProUGUI Hp;
        public TextMeshProUGUI Mana;
        public TextMeshProUGUI Damage;

        public TextMeshProUGUI Title;
        public TextMeshProUGUI Desc;

        public Image CardArt;

        [Inject]
        public void Ctor(Sprite cardArt, string title, string desc)
        {
            CardArt.sprite = cardArt;
            
            Title.text = title;
            Desc.text = desc;
        }

        void Start()
        {
            gameObject.AddComponent<ObservableBeginDragTrigger>()
                .OnBeginDragAsObservable()
                .Select(_ => Input.mousePosition)
                .Subscribe(v => Begin())
                .AddTo(this);

            gameObject.AddComponent<ObservableDragTrigger>()
                .OnDragAsObservable()
                .Select(_ => Input.mousePosition)
                .Subscribe(v => Drag(v))
                .AddTo(this);
        
            gameObject.AddComponent<ObservableEndDragTrigger>()
                .OnEndDragAsObservable()
                .Select(_ => Input.mousePosition)
                .Subscribe(v => End())
                .AddTo(this);
        }

        private void Begin()
        {
            Debug.Log("begin");
            transform.DOScale(Vector3.one * 2.5f, 0.15f);
        }

        private void Drag(Vector3 mousePosition)
        {
            transform.position = mousePosition;
        }

        private void End()
        {
            Debug.Log("end");
            transform.DOKill();
            transform.DOScale(Vector3.one, 0.15f);
        }

        // public TextMeshProUGUI GetStatView(CardParamType type)
        // {
        //     switch (type)
        //     {
        //         case CardParamType.Hp:
        //             return Hp;
        //             break;
        //         case CardParamType.Mana:
        //             return Mana;
        //             break;
        //         case CardParamType.Damage:
        //             return Damage;
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException(nameof(type), type, null);
        //     }
        // }
    }
}
