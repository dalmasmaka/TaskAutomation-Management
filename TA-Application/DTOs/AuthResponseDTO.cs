using System;
using System.Collections.Generic;
using System.Text;

namespace TA_Application.DTOs
{
    public class AuthResponseDTO
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
    }
}
