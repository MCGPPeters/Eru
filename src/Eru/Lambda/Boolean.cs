using System;

namespace Eru.Lambda
{
    public delegate Func<object, object> Boolean(object @true);
    public delegate Func<TTrue, TFalse> Boolean<in TTrue, out TFalse>(TTrue @true);

    public static class Bool
    {
        public static Boolean True => @true => @false => @true;
        public static Boolean False => @true => @false => @false;
    }
}
