using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NAudio.Wave;
using OpenQA.Selenium.Chrome;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace XboxFinder
{
    class Program
    {
        public static IConfiguration Configuration { get; set; }

        static void Main(string[] args)
        {
            var arguments = SplitArgs(args);
            var playSound = arguments.ContainsKey("sound")? bool.Parse(arguments["sound"]) : true;
            var sendEmail = arguments.ContainsKey("email")? bool.Parse(arguments["email"]) : true;

            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())                
                .AddUserSecrets<Program>()
                .AddJsonFile("appsettings.json");
            
            Configuration = builder.Build();

            Console.WriteLine("Checking for Xbox availability");

            int tryCount = arguments.ContainsKey("retries")? int.Parse(arguments["retries"]) : 24;

            #region Do the work

            while(tryCount > 0)
            {
                var driver = new ChromeDriver();
                
                var bestBuyUrl = "https://www.bestbuy.com/site/microsoft-xbox-series-x-1tb-console-black/6428324.p?skuId=6428324";

                if (arguments.ContainsKey("series") && arguments["series"] == "S")
                {
                    bestBuyUrl = "https://www.bestbuy.com/site/microsoft-xbox-series-s-512-gb-all-digital-console-disc-free-gaming-white/6430277.p?skuId=6430277";
                }                

                driver.Navigate().GoToUrl(bestBuyUrl);

                Thread.Sleep(2000);

                var button = driver.FindElementByCssSelector(".fulfillment-add-to-cart-button button");

                var inStockBestBuy = button.Text == "Add to Cart";
                                
                driver.Navigate().GoToUrl("https://www.target.com/p/xbox-series-x-console/-/A-80790841");

                var targetStatusElement = driver.FindElementByXPath("//*[@id='viewport']/div[4]/div/div[2]/div[3]/div[1]/div/div/div");
                var inStockTarget = targetStatusElement.Text != "Sold out";
                
                if (inStockBestBuy || inStockTarget)
                {
                    Console.WriteLine("Its in stock!");
                    
                    if (playSound)
                    {
                        PlaySound("xboxfound.wav");
                    }

                    if (sendEmail)
                    {
                        var emailTask = Task.Run(() => SendEmailAsync("Found!", inStockBestBuy, inStockTarget));
                        emailTask.Wait();                
                    }                    
                }
                else
                {
                    Console.WriteLine("Out of stock");     

                    if (playSound)
                    {
                        PlaySound("noxbox.wav"); 
                    }

                    if (sendEmail)
                    {
                        var emailTask = Task.Run(() => SendEmailAsync("Not Available", inStockBestBuy, inStockTarget));
                        emailTask.Wait();
                    }
                }

                driver.Close();
                tryCount--;

                var sleep = arguments.ContainsKey("timeBetween") ? int.Parse(arguments["timeBetween"]) : 1800000;
                Thread.Sleep(sleep);
            }         

            #endregion
        }

        private static async Task SendEmailAsync(string status, bool bestBuyHasIt, bool targetHasIt)
        {
            // create email message
            var apiKey = Configuration["SendGrid.ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = Configuration.GetSection("From").Get<EmailAddress>();
            var to = Configuration.GetSection("To").Get<EmailAddress>(); 
            
            var subject = $"Xbox Finder Alert - {status}";
            var plainTextContent = $"We checked at {DateTime.Now.ToString()}\r\nBest Buy: {bestBuyHasIt}\r\nTarget: {targetHasIt}";
            var htmlContent = $"<strong>We checked at {DateTime.Now.ToString()}</strong><br>Best Buy: {bestBuyHasIt}<br>Target: {targetHasIt}";
            
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

        private static void PlaySound(string file)
        {
            using(var audioFile = new AudioFileReader(file))
            using(var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }
            }        
        }

        private static Dictionary<string, string> SplitArgs(string[] args)
        {
            var arguments = new Dictionary<string, string>();

            foreach (string argument in args)
            {
                string[] splitted = argument.Split('=');

                if (splitted.Length == 2)
                {
                    arguments[splitted[0]] = splitted[1];
                }
            }

            return arguments;
        }
    }
}
