using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Proxies;
using System.Collections;

namespace Intelligent.OTC.Common.Utils
{
    /// <summary>
    /// assert utils
    /// </summary>
    public sealed class AssertUtils
    {
        /// <summary> 
        ///  Checks, whether <paramref name="method"/> may be invoked on <paramref name="target"/>. 
        ///  Supports testing transparent proxies.
        /// </summary>
        /// <param name="target">the target instance or <c>null</c></param>
        /// <param name="targetName">the name of the target to be used in error messages</param>
        /// <param name="method">the method to test for</param>
        /// <exception cref="ArgumentNullException">
        ///  if <paramref name="method"/> is <c>null</c>
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///  if it is not possible to invoke <paramref name="method"/> on <paramref name="target"/>
        /// </exception>
        public static void Understands(object target, string targetName, MethodBase method)
        {
            ArgumentNotNull(method, "method");

            if (target == null)
            {
                if (method.IsStatic)
                {
                    return;
                }

                Exception ex = new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Target '{0}' is null and target method '{1}.{2}' is not static.", targetName, method.DeclaringType.FullName, method.Name));

                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            Understands(target, targetName, method.DeclaringType);
        }

        /// <summary>
        /// checks, whether <paramref name="target"/> supports the methods of <paramref name="requiredType"/>.
        /// Supports testing transparent proxies.
        /// </summary>
        /// <param name="target">the target instance or <c>null</c></param>
        /// <param name="targetName">the name of the target to be used in error messages</param>
        /// <param name="requiredType">the type to test for</param>
        /// <exception cref="ArgumentNullException">
        /// if <paramref name="requiredType"/> is <c>null</c>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// if it is not possible to invoke methods of 
        /// type <paramref name="requiredType"/> on <paramref name="target"/>
        /// </exception>
        public static void Understands(object target, string targetName, Type requiredType)
        {
            ArgumentNotNull(requiredType, "requiredType");

            if (target == null)
            {
                Exception ex = new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Target '{0}' is null.", targetName));
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }

            Type targetType;
            if (RemotingServices.IsTransparentProxy(target))
            {
                RealProxy rp = RemotingServices.GetRealProxy(target);
                IRemotingTypeInfo rti = rp as IRemotingTypeInfo;
                if (rti != null)
                {
                    if (rti.CanCastTo(requiredType, target))
                    {
                        return;
                    }

                    Exception ex = new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Target '{0}' is a transparent proxy that does not support methods of '{1}'.", targetName, requiredType.FullName));
                    Helper.Log.Error(ex.Message, ex);
                    throw ex;
                }

                targetType = rp.GetProxiedType();
            }
            else
            {
                targetType = target.GetType();
            }

            if (!requiredType.IsAssignableFrom(targetType))
            {
                Exception ex = new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Target '{0}' of type '{1}' does not support methods of '{2}'.", targetName, targetType, requiredType.FullName));
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Checks the value of the supplied <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/>.
        /// </summary>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        public static void ArgumentNotNull(object argument, string name)
        {
            if (argument == null)
            {
                Exception ex = new ArgumentNullException(name, string.Format(CultureInfo.InvariantCulture, "Argument '{0}' cannot be null.", name));
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Checks the value of the supplied <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/>.
        /// </summary>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentNullException"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        public static void ArgumentNotNull(object argument, string name, string message)
        {
            if (argument == null)
            {
                Exception ex = new ArgumentNullException(name, message);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Checks the value of the supplied string <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </summary>
        /// <param name="argument">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public static void ArgumentHasText(string argument, string name)
        {
            if (string.IsNullOrEmpty(argument))
            {
                Exception ex = new ArgumentNullException(name,
                    string.Format(CultureInfo.InvariantCulture, "Argument '{0}' cannot be null or resolve to an empty string : '{1}'.", name, argument));
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Checks the value of the supplied string <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </summary>
        /// <param name="argument">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentNullException"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/> or
        /// contains only whitespace character(s).
        /// </exception>
        public static void ArgumentHasText(string argument, string name, string message)
        {
            if (string.IsNullOrEmpty(argument))
            {
                Exception ex = new ArgumentNullException(name, message);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Checks the value of the supplied <see cref="ICollection"/> <paramref name="argument"/> and throws
        /// an <see cref="ArgumentNullException"/> if it is <see langword="null"/> or contains no elements.
        /// </summary>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/> or
        /// contains no elements.
        /// </exception>
        public static void ArgumentHasLength(ICollection argument, string name)
        {
            if (!ArrayUtils.HasLength(argument))
            {
                Exception ex = new ArgumentNullException(name,
                    string.Format(CultureInfo.InvariantCulture, "Argument '{0}' cannot be null or resolve to an empty array", name));
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Checks the value of the supplied <see cref="ICollection"/> <paramref name="argument"/> and throws
        /// an <see cref="ArgumentNullException"/> if it is <see langword="null"/> or contains no elements.
        /// </summary>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.ArgumentNullException"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/> or
        /// contains no elements.
        /// </exception>
        public static void ArgumentHasLength(ICollection argument, string name, string message)
        {
            if (!ArrayUtils.HasLength(argument))
            {
                Exception ex = new ArgumentNullException(name, message);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Checks the value of the supplied <see cref="ICollection"/> <paramref name="argument"/> and throws
        /// an <see cref="ArgumentException"/> if it is <see langword="null"/>, contains no elements or only null elements.
        /// </summary>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>, 
        /// contains no elements or only null elements.
        /// </exception>
        public static void ArgumentHasElements(ICollection argument, string name)
        {
            if (!ArrayUtils.HasElements(argument))
            {
                Exception ex = new ArgumentException(name,
                    string.Format(CultureInfo.InvariantCulture, "Argument '{0}' must not be null or resolve to an empty collection and must contain non-null elements", name));
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Checks whether the specified <paramref name="argument"/> can be cast 
        /// into the <paramref name="requiredType"/>.
        /// </summary>
        /// <param name="argument">
        /// The argument to check.
        /// </param>
        /// <param name="argumentName">
        /// The name of the argument to check.
        /// </param>
        /// <param name="requiredType">
        /// The required type for the argument.
        /// </param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentException"/>.
        /// </param>
        public static void AssertArgumentType(object argument, string argumentName, Type requiredType, string message)
        {
            if (argument != null && requiredType != null && !requiredType.IsAssignableFrom(argument.GetType()))
            {
                Exception ex = new ArgumentNullException(message, argumentName);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        ///  Assert a boolean expression, throwing <code>ArgumentException</code>
        ///  if the test result is <code>false</code>.
        /// </summary>
        /// <param name="expression">a boolean expression.</param>
        /// <param name="message">The exception message to use if the assertion fails.</param>
        /// <exception cref="ArgumentException">
        /// if expression is <code>false</code>
        /// </exception>
        public static void IsTrue(bool expression, string message)
        {
            if (!expression)
            {
                Exception ex = new ArgumentNullException(message);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        ///  Assert a boolean expression, throwing <code>ArgumentException</code>
        ///  if the test result is <code>false</code>.
        /// </summary>
        /// <param name="expression">a boolean expression.</param>
        /// <exception cref="ArgumentException">
        /// if expression is <code>false</code>
        /// </exception>
        public static void IsTrue(bool expression)
        {
            IsTrue(expression, "[Assertion failed] - this expression must be true");
        }

        /// <summary>
        /// Assert a boolean expression, throwing <code>InvalidOperationException</code>
        /// if the expression is <code>false</code>.
        /// </summary>
        /// <param name="expression">a boolean expression.</param>
        /// <param name="message">The exception message to use if the assertion fails</param>
        /// <exception cref="InvalidOperationException">if expression is <code>false</code></exception>
        public static void State(bool expression, string message)
        {
            if (!expression)
            {
                Exception ex = new InvalidOperationException(message);
                Helper.Log.Error(ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// array utils class
        /// </summary>
        public sealed class ArrayUtils
        {
            /// <summary>
            /// Checks if the given array or collection has elements and none of the elements is null.
            /// </summary>
            /// <param name="collection">the collection to be checked.</param>
            /// <returns>true if the collection has a length and contains only non-null elements.</returns>
            public static bool HasElements(ICollection collection)
            {
                if (!HasLength(collection))
                {
                    return false;
                }

                IEnumerator it = collection.GetEnumerator();
                while (it.MoveNext())
                {
                    if (it.Current == null)
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Checks if the given array or collection is null or has no elements.
            /// </summary>
            /// <param name="collection">collection object</param>
            /// <returns>return value</returns>
            public static bool HasLength(ICollection collection)
            {
                return !((collection == null) || (collection.Count == 0));
            }
        }
    }
}
