using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Web.Administration;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace testgetandpost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            string hostUrl = GetHostUrl();
            string macAddress = GetMacAddress();
            string[] siteNames = GetSiteNames();

            return Ok(new
            {
                HostUrl = hostUrl,
                MacAddress = macAddress,
                SiteNames = siteNames
            });
        }

        [HttpGet("SendData")]
        public async Task<IActionResult> SendData()
        {
            string hostUrl = GetHostUrl();
            string macAddress = GetMacAddress();
            string[] siteNames = GetSiteNames();

            var data = new PostDataModel
            {
                HostUrl = hostUrl,
                MacAddress = macAddress,
                SiteNames = siteNames
            };

            await SendDataToUrl(data);

            return Ok("Data sent to the specified URL.");
        }

        private string GetHostUrl()
        {
            string hostName = Dns.GetHostName();
            string hostUrl = "http://" + hostName; // Assuming HTTP, change to HTTPS if needed
            return hostUrl;
        }

        private string[] GetSiteNames()
        {
            var serverManager = new ServerManager();
            List<string> siteNamesList = new List<string>();

            foreach (Site site in serverManager.Sites)
            {
                siteNamesList.Add(site.Name);
            }

            return siteNamesList.ToArray();
        }

        private string GetMacAddress()
        {
            string macAddresses = "";

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Only consider physical interfaces
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break; // Only need the first MAC address
                }
            }

            return macAddresses;
        }

        private async Task SendDataToUrl(PostDataModel data)
        {
            using (var client = new HttpClient())
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                await client.PostAsync("https://licence-management.free.beeceptor.com", content);
            }
        }
    }

    public class PostDataModel
    {
        public string HostUrl { get; set; }
        public string MacAddress { get; set; }
        public string[] SiteNames { get; set; }
    }
}
