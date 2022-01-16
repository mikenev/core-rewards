using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;

namespace Rewards
{
    class Program
    {
        static Random random = new Random();

        static void Main(string[] args)
        {
            var env = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var mobileAgent = Configuration.GetSection("MobileAgentString").Get<string>();
            var desktopAgent = Configuration.GetSection("DesktopAgentString").Get<string>();

            var profileDir = Configuration.GetSection("ProfileDirectory").Get<string>();
            var profiles = Configuration.GetSection("Profiles").Get<string[]>();

            var desktopCount = Configuration.GetSection("NumberDesktopSearches").Get<int>();
            var mobileCount = Configuration.GetSection("NumberMobileSearches").Get<int>();

            foreach (var profile in profiles)
            {
                Console.WriteLine(profile);

                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--headless");
                options.AddArgument("--remote-debugging-port=9222");
                options.AddArgument($"user-data-dir={profileDir}/{profile}");
                options.AddArgument($"--user-agent={mobileAgent}");
                RunSearches(options, GetSearchTerms(mobileCount));

                options = new ChromeOptions();
                options.AddArgument("--headless");
                options.AddArgument($"user-data-dir={profileDir}/{profile}");
                options.AddArgument($"--user-agent={desktopAgent}");
                RunSearches(options, GetSearchTerms(desktopCount));
            }
        }

        static void RunSearches(ChromeOptions options, List<string> items)
        {
            using (var driver = new ChromeDriver(options))
            {
                driver.Navigate().GoToUrl($"http://www.bing.com");
                Thread.Sleep(5 * 1000);

                foreach (var item in items)
                {
                    Console.WriteLine($"Searching: {item}");
                    driver.Navigate().GoToUrl($"http://bing.com/search?q={item}");
                    Thread.Sleep(random.Next(2, 9) * 1000);
                }

                driver.Close();
                //driver.Quit();
            }
        }

        static List<string> GetSearchTerms(int count)
        {
            var result = new List<string>();

            for (int i = 0; i < count; i++)
            {
                result.Add(WordList.SearchDictionary[random.Next(0, WordList.SearchDictionary.Count)]);
            }

            return result;
        }
    }
}
