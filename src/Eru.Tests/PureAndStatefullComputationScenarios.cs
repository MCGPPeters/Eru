//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Net.NetworkInformation;
//using FsCheck;
//using Xunit;

//namespace Eru.Tests
//{
//    public class PureAndStatefullComputationScenarios
//    {
//        [Fact]
//        public void f()
//        {
//            Assert.Equal(10, 1.For(0, x => x < 9, x => x++, i => i++));
//        }

//    }

//    //State is a family of functions that consume a state and produce both a result and an updated state
//    public delegate Tuple<T, TState> StateFunc<T, TState>(TState state);

//    public static class Control
//    {
//        public static TResult For<TSource, TState, TResult>(this TSource source, TState initialState, Func<TState, bool> finalState, Func<TState, TState> updateState, Func<TSource, TResult> function)
//        {
//            return MoveNext(source, finalState, updateState, function)(initialState).Item1.FirstOrDefault();
//        }

//        public static StateFunc<IEnumerable<TResult>, TState> MoveNext<TSource, TState, TResult>(this TSource source, Func<TState, bool> predicate, Func<TState, TState> updateState,
//            Func<TSource, TResult> function)
//        {
//            return state =>
//            {
//                var result = predicate(state) 
//                    ? new Tuple<IEnumerable<TResult>, TState>(Enumerable.Repeat(function(source), 1), updateState(state)) 
//                    : new Tuple<IEnumerable<TResult>, TState>(Enumerable.Empty<TResult>(), state);
//                return result;
//            };
//        }
//    }
//    //state =>
//    //{

//    //    if (predicate(stateTransition => stateTransition(state)))
//    //    {
//    //        return ;
//    //    }
//    //    return While(source, predicate, function);
//    //}

//    public static class StateMachineExtensions
//    {
//        public static StateFunc<TSource, TState> AsState<TSource, TState>(this TSource value)
//            =>
//                state => new Tuple<TSource, TState>(value, state);

//        public static StateFunc<TSource, TState> AsState<TSource, TState>(this TSource source, Func<TState, TState> determineInitialState)
//            =>
//                state => new Tuple<TSource, TState>(source, determineInitialState(state));

//        public static StateFunc<Unit, TState> AsState<TState>(Unit unit)
//            =>
//                unit.AsState<Unit, TState>();

//        //public static TState State<TSource, TState>(this StateFunc<TSource, TState> source, TState state) 
//        //    =>
//        //        source(state).Item2;

//        public static StateFunc<TResult, TState> Bind<TSource, TState, TResult>(this StateFunc<TSource, TState> stateFunc, Func<TSource, StateFunc<TResult, TState>> function)
//            =>
//                previousState =>
//                {
//                    var parameterWithNewState = stateFunc(previousState);
//                    var newState = parameterWithNewState.Item2;

//                    var parameter = parameterWithNewState.Item1;
//                    var result = function(parameter);

//                    return result(newState);
//                };

//        public static StateFunc<TState, TState> Get<TState>()
//            =>
//                state => new Tuple<TState, TState>(state, state);

//        public static StateFunc<TState, TState> Set<TState>(TState newState)
//            =>
//                oldState => new Tuple<TState, TState>(oldState, newState);
//    }
//}
