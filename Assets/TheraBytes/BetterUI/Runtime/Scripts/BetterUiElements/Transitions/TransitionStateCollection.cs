using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheraBytes.BetterUi
{
    //
    // GENERIC CLASS
    //
    public abstract class TransitionStateCollection<T> : TransitionStateCollection
    {
        [Serializable]
        public abstract class TransitionState : TransitionStateBase
        {
            public T StateObject;

            public TransitionState(string name, T stateObject)
                : base(name)
            {
                this.StateObject = stateObject;
            }
        }

        protected TransitionStateCollection(string[] stateNames)
        {
            foreach (string name in stateNames)
            {
                AddStateObject(name);
            }
        }

        public IEnumerable<TransitionState> GetStates()
        {
            foreach (var s in GetTransitionStates())
            {
                yield return s;
            }
        }

        public override void Apply(string stateName, bool instant)
        {
            var s = GetTransitionStates().FirstOrDefault((o) => o.Name == stateName);
            if (s != null)
            {
                ApplyState(s, instant);
            }
        }

        protected abstract IEnumerable<TransitionState> GetTransitionStates();
        protected abstract void ApplyState(TransitionState state, bool instant);
        internal abstract void AddStateObject(string stateName);

    }

    //
    // NON GENERIC CLASS
    //
    [Serializable]
    public abstract class TransitionStateCollection
    {
        public abstract UnityEngine.Object Target { get; }

        [Serializable]
        public abstract class TransitionStateBase
        {
            public string Name;
            public TransitionStateBase(string name)
            {
                this.Name = name;
            }
        }

        public abstract void Apply(string stateName, bool instant);

        internal abstract void SortStates(string[] sortedOrder);


        protected void SortStatesLogic<T>(List<T> states, string[] sortedOrder)
            where T : TransitionStateBase
        {
            states.Sort((a, b) =>
            {
                int idxA = -1;
                int idxB = -1;

                for (int i = 0; i < sortedOrder.Length; i++)
                {
                    if (sortedOrder[i] == a.Name)
                        idxA = i;

                    if (sortedOrder[i] == b.Name)
                        idxB = i;
                }

                return idxA.CompareTo(idxB);
            });
        }
    }

}
