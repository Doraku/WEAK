using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace WEAK.Communication
{
    public sealed partial class Publisher
    {
        private static class InnerPublisher<T>
        {
            #region Fields

            private static readonly List<StrongBox<Action<object>[]>> _relays;

            private static Action<object>[] _ownActions;

            public static readonly StrongBox<Action<object>[]> Actions;

            #endregion

            #region Initialisation

            static InnerPublisher()
            {
                _relays = new List<StrongBox<Action<object>[]>>();
                _ownActions = new Action<object>[_idDispenser.LastInt + 1];

                Actions = new StrongBox<Action<object>[]>(new Action<object>[0]);

                _relays = new List<StrongBox<Action<object>[]>> { Actions };

                foreach (Type type in GetBaseTypes(typeof(T)).Concat(typeof(T).GetInterfaces()))
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
                    if (actions.Value.Length == 0)
                    {
                        actions.Value = new Action<object>[Actions.Value.Length];
                        Array.Copy(Actions.Value, actions.Value, Actions.Value.Length);
                    }
                    else if (_relays.Count == 1)
                    {
                        for (int i = 0; i < _ownActions.Length; ++i)
                        {
                            if (_ownActions[i] != null)
                            {
                                foreach (Action<object> action in _ownActions[i].GetInvocationList())
                                {
                                    actions.Value[i] += action;
                                }
                            }
                        }
                    }

                    _relays.Add(actions);
                }
            }

            public static void Subscribe(int id, Action<object> action)
            {
                lock (_relays)
                {
                    _ownActions[id] += action;

                    foreach (StrongBox<Action<object>[]> actions in _relays)
                    {
                        lock (actions)
                        {
                            actions.Value[id] += action;
                        }
                    }
                }
            }

            public static void Unsubscribe(int id, Action<object> action)
            {
                lock (_relays)
                {
                    _ownActions[id] -= action;

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
                    _ownActions[id] = null;

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

                                    Action<object>[] newOwnActions = new Action<object>[(id + 1) * 2];
                                    Array.Copy(_ownActions, newOwnActions, _ownActions.Length);
                                    _ownActions = newOwnActions;
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
