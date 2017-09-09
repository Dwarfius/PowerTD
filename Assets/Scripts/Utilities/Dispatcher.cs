using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

// a small utility class that executes callbacks on the main thread
// rewrite using the "lock" keyword
public class Dispatcher : MonoBehaviour
{
    public static Dispatcher Instance { get; private set; }
    Queue<Action> callbacks = new Queue<Action>();

    void Awake()
    {
        Instance = this;
        // this seems to give the smallest delay in general, compared to Update and FixedUpdate
        InvokeRepeating("ProcessCallbacks", 1, 0.000011f); // 0.00001+f is the minimum accepted repeat rate
    }

    void ProcessCallbacks()
    {
        if (callbacks.Count > 0)
        {
            lock (callbacks)
            {
                while (callbacks.Count > 0)
                {
                    Action callback = callbacks.Dequeue();
                    callback();
                }
            }
        }
    }

    // thread safe schedule of the callback
    public void Add(Action callback)
    {
        lock (callbacks)
        {
            callbacks.Enqueue(callback);
        }
    }
}
