﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Lime.Protocol
{
    public static class UtilExtensions
    {
        /// <summary>
        /// Gets the Uri address
        /// of a command resource
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Uri GetResourceUri(this Command command)
        {
            if (command.Uri == null)
            {
                throw new ArgumentException("The command 'uri' value is null");
            }

            if (command.Uri.IsRelative)
            {
                if (command.From == null)
                {
                    throw new ArgumentException("The command 'from' value is null");
                }

                return command.Uri.ToUri(command.From);
            }
            else
            {
                return command.Uri.ToUri();
            }            
        }


        /// <summary>
        /// Disposes an object if it is not null
        /// and it implements IDisposable interface
        /// </summary>
        /// <param name="source"></param>
        public static void DisposeIfDisposable<T>(this T source) where T : class
        {
            if (source != null &&
                source is IDisposable)
            {
                ((IDisposable)source).Dispose();
            }
        }

        /// <summary>
        /// Checks if an event handles is not null and
        /// raise it if is the case
        /// </summary>
        /// <param name="event"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void RaiseEvent(this EventHandler @event, object sender, EventArgs e)
        {
            if (@event != null)
                @event(sender, e);
        }

        /// <summary>
        /// Checks if an event handles is not null and
        /// raise it if is the case
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void RaiseEvent<T>(this EventHandler<T> @event, object sender, T e)
            where T : EventArgs
        {
            if (@event != null)
                @event(sender, e);
        }

        /// <summary>
        /// Allow cancellation of non-cancellable tasks        
        /// </summary>
        /// <a href="http://blogs.msdn.com/b/pfxteam/archive/2012/10/05/how-do-i-cancel-non-cancelable-async-operations.aspx"/>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(
                        s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            return await task;
        }

        /// <summary>
        /// Allow cancellation of non-cancellable tasks        
        /// </summary>
        /// <a href="http://blogs.msdn.com/b/pfxteam/archive/2012/10/05/how-do-i-cancel-non-cancelable-async-operations.aspx"/>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(
                        s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            await task;
        }

        /// <summary>
        /// Gets a long running task (a task that runs in a thread out of the thread pool)
        /// for the specified task func.
        /// </summary>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task AsLongRunningTask(this Func<Task> func, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(
                func,
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Current)
                .Unwrap();
        }
#if !PCL
        /// <summary>
        /// Converts a SecureString to a regular, unsecure string.        
        /// </summary>
        /// <a href="http://blogs.msdn.com/b/fpintos/archive/2009/06/12/how-to-properly-convert-securestring-to-string.aspx"/>
        /// <param name="securePassword"></param>
        /// <returns></returns>
        public static string ToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
            {
                throw new ArgumentNullException("securePassword");
            }

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        /// <summary>
        /// Converts a regular string to a SecureString
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SecureString ToSecureString(this string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var secureString = new SecureString();
            foreach (var c in value)
            {
                secureString.AppendChar(c);
            }
            return secureString;
        }

#endif
        /// <summary>
        /// Creates a CancellationToken
        /// with the specified delay
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <returns></returns>
        public static CancellationToken ToCancellationToken(this TimeSpan delay)
        {
            var cts = new CancellationTokenSource(delay);
            return cts.Token;
        }

        /// <summary>
        /// Gets the identity
        /// associated to the URI 
        /// authority
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Identity GetIdentity(this Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.HostNameType != UriHostNameType.Dns)
            {
                throw new ArgumentException("The uri hostname must be a dns value");
            }

            if (string.IsNullOrWhiteSpace(uri.UserInfo))
            {
                throw new ArgumentException("The uri user info is empty");
            }

            return new Identity()
            {
                Name = uri.UserInfo,
                Domain = uri.Host
            };
        }

        /// <summary>
        /// Transform to a flat string
        /// with comma sepparate values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ToCommaSepparate(this IEnumerable<string> values)
        {
            return values.Aggregate((a, b) => string.Format("{0},{1}", a, b)).TrimEnd(',');
        }

        private static Regex formatRegex = new Regex(@"({)([^}]+)(})", RegexOptions.IgnoreCase);

        /// <summary>
        /// Format the string using
        /// the source object to populate
        /// the named formats.
        /// http://www.hanselman.com/blog/CommentView.aspx?guid=fde45b51-9d12-46fd-b877-da6172fe1791
        /// </summary>
        /// <param name="format"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string NamedFormat(this string format, object source)
        {
            return NamedFormat(format, source, null);
        }

        /// <summary>
        /// Format the string using
        /// the source object to populate
        /// the named formats.
        /// http://www.hanselman.com/blog/CommentView.aspx?guid=fde45b51-9d12-46fd-b877-da6172fe1791
        /// </summary>
        /// <param name="format"></param>
        /// <param name="source">The format names source object.</param>
        /// <param name="formatProvider">The format provider for the ToString method.</param>
        /// <returns></returns>
        public static string NamedFormat(this string format, object source, IFormatProvider formatProvider)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            StringBuilder sb = new StringBuilder();
            Type type = source.GetType();
            MatchCollection mc = formatRegex.Matches(format);
            int startIndex = 0;
            foreach (Match m in mc)
            {
                Group g = m.Groups[2]; //it's second in the match between { and }  
                int length = g.Index - startIndex - 1;
                sb.Append(format.Substring(startIndex, length));

                string toGet = String.Empty;
                string toFormat = String.Empty;
                int formatIndex = g.Value.IndexOf(":"); //formatting would be to the right of a :  
                if (formatIndex == -1) //no formatting, no worries  
                {
                    toGet = g.Value;
                }
                else //pickup the formatting  
                {
                    toGet = g.Value.Substring(0, formatIndex);
                    toFormat = g.Value.Substring(formatIndex + 1);
                }

                //first try properties  
                var retrievedProperty = type.GetRuntimeProperty(toGet);
                Type retrievedType = null;
                object retrievedObject = null;
                if (retrievedProperty != null)
                {
                    retrievedType = retrievedProperty.PropertyType;
                    retrievedObject = retrievedProperty.GetValue(source, null);
                }
                else //try fields  
                {
                    var retrievedField = type.GetRuntimeField(toGet);
                    if (retrievedField != null)
                    {
                        retrievedType = retrievedField.FieldType;
                        retrievedObject = retrievedField.GetValue(source);
                    }
                }

                if (retrievedType != null) //Cool, we found something  
                {
                    string result = String.Empty;
                    if (toFormat == String.Empty) {//no format info  
                        var method = retrievedType.GetRuntimeMethod("ToString", new Type [] {});
                        result =  method.Invoke(retrievedObject, null) as string;

                    }
                    else //format info  
                    {
                        var formatType = formatProvider != null ? formatProvider.GetType() : null;
                        var method = retrievedType.GetRuntimeMethod("ToString", new Type[] { toFormat.GetType(), formatType});
                        result = method.Invoke(retrievedObject, new object[] { toFormat, formatProvider }) as string;
                        result =  method.Invoke(retrievedObject, null) as string;
                    }
                    sb.Append(result);
                }
                else //didn't find a property with that name, so be gracious and put it back  
                {
                    sb.Append("{");
                    sb.Append(g.Value);
                    sb.Append("}");
                }
                startIndex = g.Index + g.Length + 1;
            }
            if (startIndex < format.Length) //include the rest (end) of the string  
            {
                sb.Append(format.Substring(startIndex));
            }
            return sb.ToString();
        }

        public static MethodInfo GetRuntimeMethodsExt(this Type type, string name, params Type[] types)
        {
            // Find potential methods with the correct name and the right number of parameters
            // and parameter names
            var potentials = (from ele in type.GetRuntimeMethods()
                              where ele.Name.Equals(name)
                              let param = ele.GetParameters()
                              where param.Length == types.Length
                              && param.Select(p => p.ParameterType.Name).SequenceEqual(types.Select(t => t.Name))
                              select ele);

            // Maybe check if we have more than 1? Or not?
            return potentials.FirstOrDefault();
        }
    }
}
