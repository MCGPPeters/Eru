using System;

namespace Eru
{
    public delegate (T value, TState state) StatefulComputation<T, TState>(TState state);

    public static partial class _
    {
        public static StatefulComputation<T, TState> AsStateful<T, TState>(this T value)
            =>
                state => (value, state);

        public static StatefulComputation<T, TState> AsStateful<T, TState>(this T source,
            Func<TState, TState> determineInitialState)
            =>
                state => (source, determineInitialState(state));

        public static StatefulComputation<Unit, TState> AsStateful<TState>(Unit unit)
            =>
                AsStateful<Unit, TState>(unit);

        public static StatefulComputation<TResult, TState> Bind<T, TState, TResult>(
            this StatefulComputation<T, TState> statefulComputation,
            Func<T, StatefulComputation<TResult, TState>> function)
            =>
                previousState =>
                {
                    var parameterWithNewState = statefulComputation(previousState);
                    var n = parameterWithNewState.state;

                    var parameter = parameterWithNewState.value;
                    var result = function(parameter);

                    return result(n);
                };

        public static StatefulComputation<TOutput, TState> SelectMany<T, TState, TResult, TOutput>(
            this StatefulComputation<T, TState> statefulComputation,
            Func<T, StatefulComputation<TResult, TState>> function,
            Func<TResult, TOutput> map)
            =>
                statefulComputation.Bind(source => function(source)
                    .Bind(result => AsStateful<TOutput, TState>(map(result))));

        public static StatefulComputation<TState, TState> Get<TState>()
            =>
                state => (state, state);

        public static StatefulComputation<Unit, TState> Set<TState>(TState newState)
            =>
                oldState => (Unit.Instance, newState);

        public static StatefulComputation<TResult, TState> Select<T, TState, TResult>(
            this StatefulComputation<T, TState> statefulComputation, Func<T, TResult> map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            return state =>
            {
                var statefulComputationResult = statefulComputation(state);
                return (map(statefulComputationResult.value), statefulComputationResult.state);
            };
        }

        public static (TResult value, TState state) For<T, TState, TResult>(this T source,
            TState initialState,
            Func<TState, bool> continueWhile, Func<TState, TState> updateState, Func<T, TResult> function)
        {
            var stateFunc = AsStateful<T, TState>(source);

            (TResult value, TState state) Loop(TState state)
            {
                var m = stateFunc.Bind(x => Get<TState>()
                    .Bind(y => Set(updateState(y))
                        .Bind(z => AsStateful<TResult, TState>(function(x)))));

                return continueWhile(state)
                    ? m.Bind<TResult, TState, TResult>(result => Loop)(state)
                    : m(state);
            }

            return Loop(initialState);
        }
    }
}