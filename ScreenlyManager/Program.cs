using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ScreenlyManager
{
    public static class Program
    {
        // this is a value to represent that no date was specified for logic purposes
        private const int NoValueDays = 3650;

#pragma warning disable S1075 // URIs should not be hardcoded
        // we are hardcoding the default URI, it can be overridden via environment variable
        private const string DefaultAPIUrl = "http://{0}/api/v1/assets";
#pragma warning restore S1075 // URIs should not be hardcoded

        private const string EnvAddress = "SCREENLY_ADDRESS";
        private const string EnvApi = "SCREENLY_API";
        private const string EnvUser = "SCREENLY_USER";
        private const string EnvPassword = "SCREENLY_PASSWORD";
        private const string EnvList = "SCREENLY_LIST";
        private const string EnvRemove = "SCREENLY_REMOVE";

        private const string HelpAddress
            = "Comma separated addresses of Screenly OSE system, or use environment variable {0}";

        private const string HelpUser
            = "User name to log in (if configured) or use environment variable {0}";

        private const string HelpPassword
            = "Password to log in (if configured) or use environment variable {0}";

        private const string HelpList
            = "List items older than <days> or use environment variable {0}";

        private const string HelpRemove
            = "Remove items older than <days> or use environment variable {0}";

        private const string PromptAddress
            = "Addresses of Screenly OSE system (comma separated)?";

        private const string SuccessDeleted = "Deleted {0} assets from {1}.";
        private const string SuccessMatching = "{0} matching assets.";
        private const string SuccessFound = "Found {0} assets on {1}:";

        private const string InvalidEnvVar = "Invalid {0} environment variable: {1}";

        private const string InvalidHttp
            = "Do not include 'http://' or 'https://' in the remote address; just the IP address or hostname";

        private const string InvalidAssets = "Unable to fetch assets, error: {0}";
        private const string InvalidDeletion = "Error deleting asset {0}: {1}";
        private const string InvalidNoAddress = "No addresses specified.";
        private const string InvalidWebRequest = "A problem happened querying the API: {0}";

        private const string HttpValue = "http://";
        private const string HttpsValue = "https://";

        public static int Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption("-h|--help");

            app.VersionOption("-v|--version", new Version().AssemblyVersion);

            var address = app.Option("-a|--address <address>",
                string.Format(HelpAddress, EnvAddress),
                CommandOptionType.SingleOrNoValue);

            var user = app.Option("-u|--user <user>",
                string.Format(HelpUser, EnvUser),
                CommandOptionType.SingleOrNoValue);

            var password = app.Option("-p|--password <password>",
                string.Format(HelpPassword, EnvPassword),
                CommandOptionType.SingleOrNoValue);

            var ls = app.Option<int>("-ls|--list <days>",
                string.Format(HelpList, EnvList),
                CommandOptionType.SingleOrNoValue);

            var rm = app.Option<int>("-rm|--remove <days>",
                string.Format(HelpRemove, EnvRemove),
                CommandOptionType.SingleOrNoValue);

            app.OnExecute(async () =>
            {
                string addressValue = address.HasValue()
                    ? address.Value()
                    : Environment.GetEnvironmentVariable(EnvAddress);

                if (string.IsNullOrWhiteSpace(addressValue))
                {
                    addressValue = Prompt.GetString(PromptAddress);
                }

                if (string.IsNullOrEmpty(addressValue))
                {
                    Console.WriteLine(InvalidNoAddress);
                    return 1;
                }

                int lsValue = ls.HasValue() ? int.Parse(ls.Value()) : NoValueDays;
                int rmValue = rm.HasValue() ? int.Parse(rm.Value()) : NoValueDays;

                if (lsValue == NoValueDays && rmValue == NoValueDays)
                {
                    var lsEnvVar = Environment.GetEnvironmentVariable(EnvList);
                    if (!string.IsNullOrEmpty(lsEnvVar) && !int.TryParse(lsEnvVar, out lsValue))
                    {
                        Console.WriteLine(string.Format(InvalidEnvVar, EnvList, lsEnvVar));
                    }

                    if (lsValue == NoValueDays)
                    {
                        var rmEnvVar = Environment.GetEnvironmentVariable(EnvRemove);
                        if (!string.IsNullOrEmpty(rmEnvVar)
                            && !int.TryParse(rmEnvVar, out rmValue))
                        {
                            Console.WriteLine(string.Format(InvalidEnvVar, EnvRemove, rmEnvVar));
                        }
                    }
                }

                bool lsHasValue = lsValue != NoValueDays;
                bool rmHasValue = rmValue != NoValueDays;

                string userValue = user.HasValue()
                    ? user.Value()
                    : Environment.GetEnvironmentVariable(EnvUser);

                string passwordValue = password.HasValue()
                    ? password.Value()
                    : Environment.GetEnvironmentVariable(EnvPassword);

                bool hasAuthentication = !string.IsNullOrEmpty(userValue)
                    && !string.IsNullOrEmpty(passwordValue);

                string authentication = hasAuthentication
                        ? Convert.ToBase64String(Encoding
                            .ASCII
                            .GetBytes($"{userValue}:{passwordValue}"))
                        : null;

                foreach (string remoteAddress in addressValue.Split(','))
                {
                    if (remoteAddress.StartsWith(HttpValue)
                        || remoteAddress.StartsWith(HttpsValue))
                    {
                        Console.WriteLine(InvalidHttp);
                        return 1;
                    }

                    string apiUrl =
                        string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvApi))
                        ? DefaultAPIUrl
                        : Environment.GetEnvironmentVariable(EnvApi);

                    var url = string.Format(apiUrl, remoteAddress);

                    using (var client = new HttpClient())
                    {
                        if (!string.IsNullOrEmpty(authentication))
                        {
                            client.DefaultRequestHeaders.Authorization
                                = new AuthenticationHeaderValue("Basic", authentication);
                        }

                        client.Timeout = TimeSpan.FromSeconds(30);

                        HttpResponseMessage response = null;
                        try
                        {
                            response = await client.GetAsync(url);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format(InvalidWebRequest, ex.Message));
                            continue;
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine(string.Format(InvalidAssets, response.StatusCode));
                            return 1;
                        }

                        var assets = (JArray)JsonConvert
                            .DeserializeObject(await response.Content.ReadAsStringAsync());

                        if (rmHasValue || lsHasValue)
                        {
                            DateTime cutoffDate = lsHasValue
                                ? DateTime.Now.AddDays(lsValue * -1)
                                : DateTime.Now.AddDays(rmValue * -1);

                            var assetsToRemove = new List<string>();
                            var assetsToList = new List<string>();
                            foreach (var asset in assets)
                            {
                                var endDate = DateTime.Parse(asset["end_date"].ToString());

                                if (endDate < cutoffDate)
                                {
                                    if (lsHasValue)
                                    {
                                        assetsToList.Add($"{endDate}\t{asset["name"]}");
                                    }
                                    else
                                    {
                                        assetsToRemove.Add(asset["asset_id"].ToString());
                                    }
                                }
                            }
                            foreach (var assetId in assetsToRemove)
                            {
                                var deleteResponse = await client.DeleteAsync($"{url}/{assetId}");
                                if (!deleteResponse.IsSuccessStatusCode)
                                {
                                    Console.WriteLine(string.Format(InvalidDeletion,
                                        assetId,
                                        deleteResponse.StatusCode));
                                }
                                Thread.Sleep(200);
                            }
                            if (assetsToList.Count > 0)
                            {
                                Console.WriteLine(string.Format(SuccessMatching,
                                    assetsToList.Count));
                                Console.WriteLine("End Date\t\tName");
                                Console.WriteLine("=============================================");
                                foreach (var asset in assetsToList)
                                {
                                    Console.WriteLine($"{asset}");
                                }
                                Console.WriteLine();
                            }

                            if (rmHasValue)
                            {
                                Console.WriteLine(string.Format(SuccessDeleted,
                                    assetsToRemove.Count,
                                    remoteAddress));
                            }
                        }
                        else
                        {
                            Console.WriteLine(string.Format(SuccessFound,
                                assets.Count,
                                remoteAddress));

                            if (assets.Count > 0)
                            {
                                Console.WriteLine("End Date\t\tName");
                                Console.WriteLine("=============================================");
                            }

                            foreach (var asset in assets)
                            {
                                var name = asset["name"].ToString();
                                var endDate = DateTime.Parse(asset["end_date"].ToString());
                                Console.WriteLine($"{endDate}\t{name}");
                            }
                            Console.WriteLine();
                        }
                    }
                }

#if DEBUG
                Prompt.GetString("Press enter to exit...");
#endif
                return 0;
            });

            return app.Execute(args);
        }
    }
}
