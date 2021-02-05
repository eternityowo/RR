using System;
using TMPro;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using System.Runtime.CompilerServices; 
using System.Threading.Tasks;
using RR.Scripts.Common;

namespace RR.Scripts.Extension 
{
    public static class RxExtensions
    {
        public static IDisposable SubscribeToText(this UniRx.IObservable<string> source, TextMeshProUGUI text)
        {
            return source.SubscribeWithState<string, TextMeshProUGUI>(text,
                (Action<string, TextMeshProUGUI>) ((x, t) => t.text = x));
        }

        public static IDisposable SubscribeToText<T>(this UniRx.IObservable<T> source, TextMeshProUGUI text)
        {
            return source.SubscribeWithState<T, TextMeshProUGUI>(text,
                (Action<T, TextMeshProUGUI>) ((x, t) => t.text = x.ToString()));
        }

        public static IDisposable SubscribeToImage(this UniRx.IObservable<Sprite> source, Image image)
        {
            return source.SubscribeWithState(image, (x, i) => image.sprite = x);
        }
    }

    public static class UnityExtensions 
    {
        public static T EnsureGetComponent<T> (this GameObject go) where T : Component 
        {
            if (go == null) 
            {
                return null;
            }
            var c = go.GetComponent<T> ();
            if (c == null) 
            {
                c = go.AddComponent<T> ();
            }
            return c;
        }
    }
    
    public static class TaskExtension
    {
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }
    
    public static class Math
    {
        public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = uu * p0; 
            p += 2 * u * t * p1; 
            p += tt * p2;

            return p;
        }
        
        public static Vector3 CalculateCubicBezierPoint(float t, Arc3 arc)
        {
            return CalculateCubicBezierPoint(t, arc.p0, arc.p1, arc.p2);
        }
    }
}