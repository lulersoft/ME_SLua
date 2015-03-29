using System;
using System.Collections.Generic;
using UnityEngine;

namespace BestHTTP
{
    /// <summary>
    /// Delegates some U3D calls to the HTTPManager.
    /// </summary>
    sealed class HTTPUpdateDelegator : MonoBehaviour
    {
        private static HTTPUpdateDelegator instance;
        private static bool IsCreated;

        public static void CheckInstance()
        {
            try
            {
                if (!IsCreated)
                {
                    instance = UnityEngine.Object.FindObjectOfType(typeof(HTTPUpdateDelegator)) as HTTPUpdateDelegator;

                    if (instance == null)
                    {
                        GameObject go = new GameObject("HTTP Update Delegator");
                        go.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                        UnityEngine.Object.DontDestroyOnLoad(go);

                        instance = go.AddComponent<HTTPUpdateDelegator>();
                    }
                    IsCreated = true;
                }
            }
            catch
            {
                Debug.LogError("Please call the BestHTTP.HTTPManager.Setup() from one of Unity's event(eg. awake, start) before you send any request!");
            }
        }

        void Awake()
        {
            Caching.HTTPCacheService.SetupCacheFolder();
            Cookies.CookieJar.SetupFolder();
            Cookies.CookieJar.Load();
        }

        void Update()
        {
            HTTPManager.OnUpdate();
        }

#if UNITY_EDITOR
        void OnDisable()
        {
            HTTPManager.OnQuit();
        }
#else
        void OnApplicationQuit()
        {
            HTTPManager.OnQuit();
        }
#endif
    }
}