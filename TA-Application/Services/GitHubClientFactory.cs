using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.Interfaces.Services;

namespace TA_Application.Services
{
    public class GitHubClientFactory : IGitHubClientFactory
    {
        public GitHubClient Create(string token)
        {
            var client = new GitHubClient(new ProductHeaderValue("TaskAutomationApp"));
            client.Connection.Credentials = new Credentials(token);
            return client;
        }
    }
}
