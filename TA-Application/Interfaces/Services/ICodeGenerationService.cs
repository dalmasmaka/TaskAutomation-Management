using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface ICodeGenerationService
    {
        Task<GeneratedFolderDTO> GenerateAsync(string description);
    }
}