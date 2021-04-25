using System;
using System.Collections.Generic;

namespace Valheim.WhereAmI.Startup
{
    public static class StateResetter
    {
        private static HashSet<Action> OnResetActions = new HashSet<Action>();

        public static void Subscribe(Action onReset)
        {
            OnResetActions.Add(onReset);
        }

        public static void Unsubscribe(Action onReset)
        {
            OnResetActions.Remove(onReset);
        }

        internal static void Reset()
        {
            foreach (var onReset in OnResetActions)
            {
                onReset.Invoke();
            }
        }
    }
}
