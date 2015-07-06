using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Eru.Tests
{
    public abstract class Primitive<TNext>
    {
        protected Primitive()
        {
        }
    }

    public sealed class Create<TNext> : Primitive<TNext>
    {
        public Behavior Behavior { get; set; }
        public TNext Next { get; set; }

        public Create(Behavior behavior, TNext next)
        {
            Behavior = behavior;
            Next = next;
        }
    }

    public sealed class Send<TNext> : Primitive<TNext>
    {
        public object Message { get; set; }
        public TNext Next { get; set; }

        public Send(object message, TNext next)
        {
            Message = message;
            Next = next;
        }
    }

    public static class PrimitiveExtension
    {
        public static Either<Exception, Primitive<TB>> Select<TA, TB>(this Primitive<TA> primitive,
            Func<TA, TB> function)
        {
            if (primitive is Create<TA>)
            {
                var create = (Create<TA>) primitive;
                return new Either<Exception, Primitive<TB>>(new Create<TB>(create.Behavior, function(create.Next)));
            }

            if (primitive is Send<TA>)
            {
                var send = (Send<TA>) primitive;
                return new Either<Exception, Primitive<TB>>(new Send<TB>(send.Message, function(send.Next)));
            }
            return new Either<Exception, Primitive<TB>>(new NotSupportedException());
        }
    }

    public class Behavior
    {
    }



    public class Address
    {
    }

    public class Pure<TResult>
    {
    }

    public class Operation<TResult>
    {

    }

    public class FreeMonadScenarios
    {
        [Fact]
        public void Creating()
        {
            
        }
    }
}
