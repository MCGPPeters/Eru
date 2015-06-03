using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Eru.Tests
{
    public abstract class Primitive<TNext>
    {
        protected Primitive() { }
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

    public static class PrimitiveExtension
    {
        public static Primitive<TB> Select<TA, TB>(this Primitive<TA> primitive, Func<TA, TB> function)
        {
            //if (primitive is Create<TA>)
            //{
            //    return new Create<TB>(primitive.Behavior, function(primitive.Value.Get));
            //}
            return null;
        }
    }

    public class Behavior
    {
    }

   

  public class Address
    {
    }

    public class Send<TResult> { }

    public class Pure<TResult>
    {
    }

    public class Operation<TResult>
    {

    }
}
