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
        [Property(Verbose = true)]
        public void True_returns_first_parameter_when_this_value_represents_the_boolean_value_of_true(int @true, object @false)
        {
            Assert.Equal(@true, Bool.True(@true)(@false));
        }

        [Property(Verbose = true)]
        public void False_returns_the_second_parameter_when_this_value_represents_the_boolean_value_of_true(int @true, string @false)
        {
            Assert.Equal(@false, Bool.False(@true)(@false));
        }
    }
}
