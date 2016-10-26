using System;
using System.Runtime.CompilerServices;

namespace Eru.Control
{
    public static class State
    {
        public static StateFunc<TSource, TState> Return<TSource, TState>(this TSource value)
            =>
                state => Tuple.Create(value, state);

        public static StateFunc<TSource, TState> Return<TSource, TState>(this TSource source, Func<TState, TState> determineInitialState)
            =>
                state => Tuple.Create(source, determineInitialState(state));

        public static StateFunc<Unit, TState> Return<TState>(Unit unit)
            =>
                unit.Return<Unit, TState>();

        public static StateFunc<TResult, TState> Bind<TSource, TState, TResult>(this StateFunc<TSource, TState> stateFunc, Func<TSource, StateFunc<TResult, TState>> function)
            =>
                previousState =>
                {
                    var parameterWithNewState = stateFunc(previousState);
                    var newState = parameterWithNewState.Item2;

                    var parameter = parameterWithNewState.Item1;
                    var result = function(parameter);

                    return result(newState);
                };

        public static StateFunc<TOutput, TState> SelectMany<TSource, TState, TResult, TOutput>(
            this StateFunc<TSource, TState> stateFunc, Func<TSource, StateFunc<TResult, TState>> function,
            Func<TResult, TOutput> map)
            =>
                stateFunc.Bind(source => function(source).Bind(result => map(result).Return<TOutput,TState>()));

       

        public static StateFunc<TState, TState> Get<TState>()
            =>
                state => Tuple.Create(state, state);

        public static StateFunc<Unit, TState> Set<TState>(TState newState)
            =>
                oldState => Tuple.Create(Unit.Instance, newState);

        public static StateFunc<TResult, TState> Select<TSource, TState, TResult>(this StateFunc<TSource, TState> stateFunc, Func<TSource, TResult> map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            return state =>
            {
                var resT = stateFunc(state);
                return Tuple.Create(map(resT.Item1), resT.Item2);
            };
        }

        public static Tuple<TResult, TState> For<TSource, TState, TResult>(this TSource source, TState initialState,
            Func<TState, bool> continueWhile, Func<TState, TState> updateState, Func<TSource, TResult> function)
        {
            var stateFunc = source.Return<TSource, TState>();

            StateFunc<TResult, TState> loop = null;

            loop = state =>
            {
                var m = stateFunc
                    .Bind(x => Get<TState>()
                        .Bind(y => Set(updateState(y))
                            .Bind(z => Return<TResult, TState>(function(x)))));

                if (continueWhile(state))
                {
                    return m.Bind(result => loop)(state);
                }
                ;

                return m(state);
            };

            return loop(initialState);
        }
    }
}