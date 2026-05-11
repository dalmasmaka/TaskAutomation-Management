using Octokit;
using System;
using System.Collections.Generic;
using System.Text;

namespace TA_Application.Interfaces.Services
{
    public interface IGitHubClientFactory
    {
        GitHubClient Create(string token);
    }
}
