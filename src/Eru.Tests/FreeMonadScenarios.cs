using System;
using Xunit;

namespace Eru.Tests
{
 
    /*
          C# does not have proper sum types. They must be emulated.

          This data type is one of 4 possible values:
          - Create, being pure pair of pure string and A
          - WriteErr, being pure pair of pure string and A
          - readLine, being pure function from string to A
          - read, being pure function from int to A

          It gives rise to pure functor. See `Select` method.

          The Fold function deconstructs the data type into its one of 4 possibilities.
          The 4 static functions construct into one of the possibilities.
        */

    public abstract class Primitive<A>
    {
        public FreePrimitive<A> Lift
        {
            get { return FreePrimitive<A>.More(this.Select(FreePrimitive<A>.Done)); }
        }

        public abstract X Fold<X>(
            Func<Behavior, Func<Address, A>, X> create
            , Func<Address, object, A, X> send
            );

        internal class Send : Primitive<A>
        {
            private readonly A _a;
            private readonly Address _address;
            private readonly object _message;

            public Send(Address address, object message, A a)
            {
                _address = address;
                _message = message;
                _a = a;
            }

            public override X Fold<X>(
                Func<Behavior, Func<Address, A>, X> create
                , Func<Address, object, A, X> send
                )
            {
                return send(_address, _message, _a);
            }
        }

        internal class Create : Primitive<A>
        {
            private readonly Behavior _behavior;
            private readonly Func<Address, A> _f;

            public Create(Behavior behavior, Func<Address, A> f)
            {
                _behavior = behavior;
                _f = f;
            }

            public override X Fold<X>(Func<Behavior, Func<Address, A>, X> create, Func<Address, object, A, X> send)
            {
                return create(_behavior, _f);
            }
        }
    }

    public class Address
    {
    }

    public class Behavior
    {
    }

    public static class PrimitiveFunctor
    {
        public static Primitive<B> Select<A, B>(this Primitive<A> o, Func<A, B> f)
        {
            return o.Fold<Primitive<B>>(
                (behavior, g) => new Primitive<B>.Create(behavior, address => f(g(address))),
                (address, message, a) => new Primitive<B>.Send(address, message, f(a))
                );
        }
    }

    public abstract class FreePrimitive<A>
    {
        public abstract X Fold<X>(
            Func<A, X> done
            , Func<Primitive<FreePrimitive<A>>, X> more
            );

        public static FreePrimitive<A> Done(A pure)
        {
            return new Pure(pure);
        }

        public static FreePrimitive<A> More(Primitive<FreePrimitive<A>> free)
        {
            return new Free(free);
        }

        private class Pure : FreePrimitive<A>
        {
            private readonly A _a;

            public Pure(A a)
            {
                _a = a;
            }

            public override X Fold<X>(
                Func<A, X> done
                , Func<Primitive<FreePrimitive<A>>, X> more
                )
            {
                return done(_a);
            }
        }

        private class Free : FreePrimitive<A>
        {
            private readonly Primitive<FreePrimitive<A>> _a;

            public Free(Primitive<FreePrimitive<A>> a)
            {
                _a = a;
            }

            public override X Fold<X>(
                Func<A, X> done
                , Func<Primitive<FreePrimitive<A>>, X> more
                )
            {
                return more(_a);
            }
        }
    }

    public static class Actor
    {
        public static FreePrimitive<Unit> Send<T>(Address address, T message)
        {
            return new Primitive<Unit>.Send(address, message, Unit.Instance).Lift;
        }

        public static FreePrimitive<Address> Create(Behavior behavior)
        {
            return new Primitive<Address>.Create(behavior, address => address).Lift;
        }

        public static FreePrimitive<B> Select<A, B>(this FreePrimitive<A> t, Func<A, B> f)
        {
            return t.Fold(
                a => FreePrimitive<B>.Done(f(a))
                , a => FreePrimitive<B>.More(a.Select(k => k.Select(f)))
                );
        }

        public static FreePrimitive<B> SelectMany<A, B>(this FreePrimitive<A> t, Func<A, FreePrimitive<B>> f)
        {
            return t.Fold(
                f
                , a => FreePrimitive<B>.More(a.Select(k => k.SelectMany(f)))
                );
        }

        public static FreePrimitive<C> SelectMany<A, B, C>(this FreePrimitive<A> t, Func<A, FreePrimitive<B>> u,
            Func<A, B, C> f)
        {
            return SelectMany(t, a => Select(u(a), b => f(a, b)));
        }
    }

    public static class PrimitiveInterpreter
    {
        public static A Interpret<A>(this FreePrimitive<A> t)
        {
            return t.Fold(
                a => a
                , a => a.Fold
                (
                    (behavior, func) =>
                    {
                        var address = new Address(); //create actor using the behavior, return the address
                        return Interpret(func(address));
                    }
                    ,
                    (address, message, next) =>
                    {
                        //send the message to the address
                        Actor.Send(address, message);
                        return Interpret(next);
                    }
                )
            );
        }
    }

    public class Demonstration
    {
        public static Unit Main()
        {
            var Program =
                from cell in Actor.Create(new Behavior())
                from _ in Actor.Send(cell, "foo")
                select _;

            return Program.Interpret();
        }
    }
}