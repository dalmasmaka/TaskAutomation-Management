using System;
using System.Collections.Generic;
using System.Text;

namespace TA_Application.Configurations
{
    public class JwtSettings
    {
        public string Secret { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpiryHours { get; set; }
    }

}
