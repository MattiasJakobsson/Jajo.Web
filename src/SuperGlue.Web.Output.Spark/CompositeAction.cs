using System;
using System.Collections.Generic;

namespace SuperGlue.Web.Output.Spark
{
    public class CompositeAction<T>
    {
        private readonly List<Action<T>> _actions = new List<Action<T>>();

        public static CompositeAction<T> operator +(CompositeAction<T> actions, Action<T> action)
        {
            actions._actions.Add(action);
            return actions;
        }

        public void Do(T target)
        {
            _actions.ForEach(x => x(target));
        }
    }
}