using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Lime.Client.TestConsole.Utils
{
    public static class X509Certificate2Extensions
    {
        /// <summary>
        /// Gets the identity value from 
        /// the certificate subject
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static Identity GetIdentity(this X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }

            var identityName = certificate.GetNameInfo(
                X509NameType.SimpleName,
                false);

            Identity identity = null;

            if (!string.IsNullOrWhiteSpace(identityName))
            {
                Identity.TryParse(identityName, out identity);
            }

            return identity;
        }
    }
}
