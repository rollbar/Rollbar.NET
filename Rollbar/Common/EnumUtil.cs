namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    public static class EnumUtil
    {
        public static TEnum[] GetAllValues<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
        }
    }
}
