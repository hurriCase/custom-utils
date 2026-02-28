using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CustomUtils.Runtime
{
    internal static class StaticResetter
    {
        private static readonly List<Action> _resetActions = new();
        private static readonly object _lock = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAllSingletons()
        {
            lock (_lock)
            {
                foreach (var resetAction in _resetActions)
                {
                    try
                    {
                        resetAction();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError("[StaticResetter::ResetAllSingletons] " +
                                       $"Failed to reset singleton: {e.Message}");
                    }
                }

                _resetActions.Clear();
            }
        }

        [Conditional("UNITY_EDITOR")]
        internal static void RegisterResetAction(Action resetAction)
        {
            lock (_lock)
            {
                _resetActions.Add(resetAction);
            }
        }
    }
}