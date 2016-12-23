using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace WEAK.Communication
{
    public sealed partial class Publisher
    {
        public static class InnerPublisher<T>
        {
            #region Fields

            private static readonly List<StrongBox<Action<object>[]>> _relays;

            public static readonly StrongBox<Action<object>[]> Actions;

            #endregion

            #region Initialisation

            static InnerPublisher()
            {
                _relays = new List<StrongBox<Action<object>[]>>();

                Actions = new StrongBox<Action<object>[]>(new Action<object>[_idDispenser.LastInt + 1]);

                foreach (Type type in GetTypes(typeof(T)))
                {
                    _addRelay.GetOrAdd(type, GetAddRelay)(Actions);
                }
            }

            #endregion

            #region Methods

            public static void AddRelay(StrongBox<Action<object>[]> actions)
            {
                lock (_relays)
                {
                    _relays.Add(actions);

                    if (typeof(T) == typeof(object))
                    {
                        Array.Copy(Actions.Value, actions.Value, Math.Min(actions.Value.Length, actions.Value.Length));
                    }
                }
            }

            public static void Subscribe(int id, Action<object> action, bool isFirst)
            {
                lock (_relays)
                {
                    foreach (StrongBox<Action<object>[]> actions in _relays)
                    {
                        lock (actions)
                        {
                            Action<object> newAction = action;
                            Action<object> temp = actions.Value[id];

                            if (temp != null)
                            {
                                if (isFirst)
                                {
                                    newAction = (Action<object>)Delegate.Combine(newAction, temp);
                                }
                                else
                                {
                                    newAction = (Action<object>)Delegate.Combine(temp, newAction);
                                }
                            }

                            actions.Value[id] = newAction;
                        }
                    }
                }
            }

            public static void Unsubscribe(int id, Action<object> action)
            {
                lock (_relays)
                {
                    foreach (StrongBox<Action<object>[]> actions in _relays)
                    {
                        lock (actions)
                        {
                            actions.Value[id] -= action;
                        }
                    }
                }
            }

            public static void Clear(int id)
            {
                lock (_relays)
                {
                    foreach (StrongBox<Action<object>[]> actions in _relays)
                    {
                        lock (actions)
                        {
                            actions.Value[id] = null;
                        }
                    }
                }
            }

            public static void Add(int id)
            {
                lock (_relays)
                {
                    foreach (StrongBox<Action<object>[]> actions in _relays)
                    {
                        if (actions.Value.Length < id + 1)
                        {
                            lock (actions)
                            {
                                if (actions.Value.Length < id + 1)
                                {
                                    Action<object>[] newActions = new Action<object>[(id + 1) * 2];
                                    Array.Copy(actions.Value, newActions, actions.Value.Length);
                                    actions.Value = newActions;
                                }
                            }
                        }
                    }
                }
            }

            #endregion
        }
    }
}
