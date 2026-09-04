using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Utilities
{
    public class ObjectUtils
    {
        #region properties
        #endregion properties

        #region data
        #endregion data

        #region ctor
        #endregion ctor

        #region private
        #endregion private

        #region public

        /// <summary>
        /// Returns a shallow copy of the <paramref name="source"/> argument.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static object? CloneObject(object source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var type = source.GetType();
            var method = type.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            return method?.Invoke(source, null);
        }


        #endregion public
    }
}
