using System.Data;
using System.Reflection;

namespace AsyncCache.Extensions
{
    internal static class OtherExt
    {
        public static void ThrowIfReadOnly(this PropertyInfo prop, string objectName)
        {
            if (!prop.CanWrite)
            {
                throw new InvalidConstraintException($"{objectName}.{prop.Name} is read-only");
            }
        }
    }
}