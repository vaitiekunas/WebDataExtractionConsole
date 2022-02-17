using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace WebDataExtractionConsole
{
    class Program
    {
        public static string OutDepartFrom { get; set; }
        public static string OutArriveTo { get; set; }


        static void Main(string[] args)
        {
            var departAfter01 = 10;
            var returnAfter01 = 17;
            var departAfter02 = 20;
            var returnAfter02 = 27;

            var departDate01 = CalcDate(departAfter01);
            var returnDate01 = CalcDate(returnAfter01);
            var Url01 = CombineUrl(departDate01, returnDate01);
            GetFromHtml(Url01, departDate01, returnDate01);

            var departDate02 = CalcDate(departAfter02);
            var returnDate02 = CalcDate(returnAfter02);
            var Url02 = CombineUrl(departDate02, returnDate02);
            GetFromHtml(Url02, departDate02, returnDate02);

            //Console.WriteLine($"{departDate01} {returnDate01}");
            //Console.WriteLine($"{departDate02} {returnDate02}");

            Console.ReadLine();
        }
                
        private static string CalcDate(int flightAfter)
        {
            var dateAfter = DateTime.Today.AddDays(flightAfter);
            var yearAfter = dateAfter.Year;
            var monthAfter = dateAfter.ToString("MMM", new CultureInfo("en-US"));
            var dayAfter = dateAfter.Day;
            var dateForUrl = dayAfter + "+" + monthAfter + "+" + yearAfter;
            return dateForUrl;
        }

        private static string CombineUrl(string departDate, string returnDate)
        {
            string urlPart1 = ("https://www.fly540.com/flights/nairobi-to-mombasa?isoneway=0&currency=USD&depairportcode=NBO&arrvairportcode=MBA&date_from=");
            string urlPart2 = ("&date_to=");
            string urlPart3 = ("&adult_no=1&children_no=0&infant_no=0&searchFlight=&change_flight=");
            string fullUrl = (urlPart1 + departDate + urlPart2 + returnDate + urlPart3);
            Uri newUri = new Uri(fullUrl);
            return fullUrl;
        }

        public static async void GetFromHtml(string url, string departDate, string returnDate)
        {
            var outDepartYear = departDate.Substring(departDate.Length -4);

            var inDepartYear = returnDate.Substring(returnDate.Length - 4);

            string filepath = Path.Combine(Environment.CurrentDirectory, $"Round_Flights_{DateTime.Now.ToFileTime()}.csv");

            List<string> output = new List<string>();

            output.Add($"outbound_departure_airport;outbound_arrival_airport;outbound_departure_time;outbound_arrival_time;inbound_departure_airport;inbound_arrival_airport;inbound_departure_time;inbound_arrival_time;total_price;taxes");

            var httpClient = new HttpClient();

            var fullHtml = await httpClient.GetStringAsync(url);

            var htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(fullHtml);

            var departFlights = htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("fly5-depart")).ToList();

            var departFlightList = departFlights[0].Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("fly5-result-")).ToList();

            var returnFlights = htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("fly5-return")).ToList();

            var returnFlightList = returnFlights[0].Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Contains("fly5-result-")).ToList();

            foreach (var departFlight in departFlightList)
            {
                var outDepartureFrom = departFlight.SelectNodes("./table/tbody/tr/td[1]/span[5]/text()[2]")
                    .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                var outDepartureDate = departFlight.SelectNodes("./table/tbody/tr/td[3]/span[3]")
                    .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                var outDepartureTime = departFlight.SelectNodes("./table/tbody/tr/td[1]/span[3]")
                    .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                var outArrivalTo = departFlight.SelectNodes("./table/tbody/tr/td[3]/span[4]/text()[2]")
                    .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                var outArrivalDate = departFlight.SelectNodes("./table/tbody/tr/td[3]/span[3]")
                    .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                var outArrivalTime = departFlight.SelectNodes("./table/tbody/tr/td[3]/span[2]")
                    .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                var outPrice = departFlight.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("flprice")).FirstOrDefault().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                foreach (var returnFlight in returnFlightList)
                {
                    var inDepartureFrom = returnFlight.SelectNodes("./table/tbody/tr/td[1]/span[5]/text()[2]")
                        .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                    var inDepartureDate = returnFlight.SelectNodes("./table/tbody/tr/td[3]/span[3]")
                        .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                    var inDepartureTime = returnFlight.SelectNodes("./table/tbody/tr/td[1]/span[3]")
                        .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                    var inArrivalTo = returnFlight.SelectNodes("./table/tbody/tr/td[3]/span[4]/text()[2]")
                        .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                    var inArrivalDate = returnFlight.SelectNodes("./table/tbody/tr/td[3]/span[3]")
                        .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                    var inArrivalTime = returnFlight.SelectNodes("./table/tbody/tr/td[3]/span[2]")
                        .First().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                    var inPrice = returnFlight.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("flprice")).FirstOrDefault().InnerText.Trim('(', ')', '\n', 'r', '\t', ' ');

                    double totalTax = 6 + 6;

                    double totalPrice = Convert.ToDouble(outPrice) + Convert.ToDouble(inPrice);

                    //Console.WriteLine($"{outDepartureFrom};{outArrivalTo};{outDepartureDate} {outDepartureTime};{outArrivalDate} {outArrivalTime};{inDepartureFrom};{inArrivalTo};{inDepartureDate} {inDepartureTime};{inArrivalDate} {inArrivalTime};{totalPrice};{totalTax}");

                    output.Add($"{outDepartureFrom};{outArrivalTo};{outDepartureDate} {outDepartureTime} {outDepartYear};{outArrivalDate} {outArrivalTime};{inDepartureFrom};{inArrivalTo};{inDepartureDate} {inDepartureTime} {inDepartYear};{inArrivalDate} {inArrivalTime};{totalPrice};{totalTax}");
                }
            }
            File.WriteAllLines(filepath, output);
        }
    }
}
