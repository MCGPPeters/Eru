using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eru.Lambda;
using FsCheck.Xunit;
using Xunit;

namespace Eru.Tests.Lambda
{
    public class BooleanProperties
    {
        public static readonly Eru.Lambda.Boolean True = Bool.True;
        public static readonly Eru.Lambda.Boolean False = Bool.False;

        [Property(Verbose = true)]
        public void True_returns_first_parameter_when_this_value_represents_the_boolean_value_of_true(int @true, string @false)
        {
            Assert.Equal(@true, True(@true)(@false));
        }

        [Property(Verbose = true)]
        public void False_returns_the_second_parameter_when_this_value_represents_the_boolean_value_of_true(int @true, string @false)
        {
            Assert.Equal(@false, False(@true)(@false));
        }
    }
}
