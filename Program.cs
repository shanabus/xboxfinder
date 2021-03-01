using System;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.PageObjects;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace XboxFinder
{
    class Program
    {

        [FindsBy(How = How.CssSelector, Using = ".fulfillment-add-to-cart-button button")]
        public IWebElement PurchaseButton { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Checking for Xbox availability");
            int tryCount = 24;

            while(tryCount > 0)
            {
                var driver = new ChromeDriver();
                
                var url = "https://www.bestbuy.com/site/microsoft-xbox-series-x-1tb-console-black/6428324.p?skuId=6428324";
                //var url = "https://www.bestbuy.com/site/microsoft-xbox-series-s-512-gb-all-digital-console-disc-free-gaming-white/6430277.p?skuId=6430277";

                driver.Navigate().GoToUrl(url);

                Thread.Sleep(2000);

                var button = driver.FindElementByCssSelector(".fulfillment-add-to-cart-button button");

                // button.Click();

                var text = button.Text;

                Console.WriteLine(text);

                if (text == "Add to Cart")
                {
                    Console.WriteLine("Its in stock!");
                    PlaySound("xboxfound.wav");

                    var emailTask = Task.Run(() => SendEmailAsync("Found!", text));
                    emailTask.Wait();                
                }
                else
                {
                    Console.WriteLine("No bueno");     
                    PlaySound("noxbox.wav"); 

                    var emailTask = Task.Run(() => SendEmailAsync("Not Available", text));
                    emailTask.Wait();
                }
                                        
                driver.Close();
                tryCount--;
                Thread.Sleep(1798000);
            }            
        }

        private static async Task SendEmailAsync(string status, string text)
        {
            // create email message
            //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
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("me@shanesievers.com", "Shane Sievers");
            var subject = "Xbox Finder Alert";
            var to = new EmailAddress("shanabus@gmail.com", "Shane Sievers");
            var plainTextContent = $"{status}\r\nWe checked at {DateTime.Now.ToString()}";
            var htmlContent = $"{status}<br /><strong>We checked at {DateTime.Now.ToString()}</strong>";
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
    }
}
