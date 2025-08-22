using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Infrastructure.Security.ExternalAuth.Facebook
{
    /// <summary>
    /// Options cấu hình cho Facebook Authentication
    /// </summary>
    public class FacebookAuthOptions
    {
        /// <summary>
        /// Facebook App ID
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Facebook App Secret
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// Facebook Graph API Version
        /// </summary>
        public string ApiVersion { get; set; } = "v18.0";

        /// <summary>
        /// Facebook Graph API Base URL
        /// </summary>
        public string GraphApiBaseUrl { get; set; } = "https://graph.facebook.com";
    }
}
