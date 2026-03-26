using System;
using System.Collections.Generic;
using System.Text;

namespace TA_Application.Configurations
{
    public class TaskProvidersSettings
    {
        public string AuthBaseUrl { get; set; } = null!;
        public string AuthorizeEndpoint { get; set; } = null!;
        public string TokenEndpoint { get; set; } = null!;
        public string ApiBaseUrl { get; set; } = null!;
    }
}
