using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace semLinqTask
{
    public static class IEnumerableExtention
    {
        public static void PrintCollection<T>(this IEnumerable<T> collection)
        {
            foreach (var elem in collection)
            {
                Console.WriteLine(elem);
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //Нужно дополнить модель WeatherEvent, создать список этого типа List<>
            //И заполнить его, читая файл с данными построчно через StreamReader
            //Ссылка на файл https://www.kaggle.com/sobhanmoosavi/us-weather-events

            //Написать Linq-запросы, используя синтаксис методов расширений
            //и продублировать его, используя синтаксис запросов
            //(возможно с вкраплениями методов расширений, ибо иногда первого может быть недостаточно)

            //0. Linq - сколько различных городов есть в датасете.
            //1. Сколько записей за каждый из годов имеется в датасете.
            //Потом будут еще запросы

            WeatherEvent we = new WeatherEvent()
            {
                EventId = "W-1",
                Type = WeatherEventType.Rain,
                Severity = Severity.Light,
                StartTime = DateTime.Now
            };

            string file_path = "../../../Data.csv";

            List<WeatherEvent> arr = new List<WeatherEvent>();

            using (StreamReader sr = new StreamReader(file_path))
            {
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    string inp = sr.ReadLine();
                    arr.Add(new WeatherEvent(inp));
                }
            }

            #region 0. Linq - сколько различных городов есть в датасете

            Console.WriteLine("0. сколько различных городов есть в датасете");
            var cityCnt1 = arr.GroupBy(elem => elem.City).Count();
            Console.WriteLine(cityCnt1);

            var cityCnt2 =
                (from elem in arr
                 group elem by elem.City).Count();
            Console.WriteLine(cityCnt2);

            #endregion

            #region 1.Сколько записей за каждый из годов имеется в датасете.

            Console.WriteLine("\n1.Сколько записей за каждый из годов имеется в датасете.");
            var entryPerYear1 = arr
                .GroupBy(elem => elem.StartTime.Year)
                .Select(g => new { Year = g.Key, Quantity = g.Count() });

            entryPerYear1.PrintCollection();

            Console.WriteLine(new string('-', 5));

            var entryPerYear2 =
                from elem in arr
                group elem by elem.StartTime.Year into yearGroup
                select new { Year = yearGroup.Key, Quantity = yearGroup.Count() };

            entryPerYear2.PrintCollection();
            #endregion

            #region 2. Вывести количество зафиксированных природных явлений в Америке в 2018 году

            Console.WriteLine("\n2.Вывести количество зафиксированных природных явлений в Америке в 2018 году");

            var typeCnt1 = arr
                .Where(elem => elem.StartTime.Year == 2018)
                .GroupBy(elem => elem.Type)
                .Count();

            var typeCnt2 = (from elem in arr
                            where elem.StartTime.Year == 2018
                            group elem by elem.Type).Count();

            var TypeInfo =
                 from elem in arr
                 where elem.StartTime.Year == 2018
                 group elem by elem.Type into g
                 select new { WeatherEventType = g.Key, Quantity = g.Count() };

            Console.WriteLine(typeCnt1);
            Console.WriteLine(typeCnt2);
            TypeInfo.PrintCollection();
            #endregion

            #region 3.Вывести топ 3 самых дождливых города в 2019 году в порядке убывания количества дождей (вывести город и количество дождей)

            Console.WriteLine("\n3.Вывести топ 3 самых дождливых города в 2019 году в порядке убывания количества дождей");
            var rainCities1 = arr
                .Where(elem => elem.StartTime.Year == 2019 &&
                elem.Type == WeatherEventType.Rain)
                .GroupBy(elem => elem.City)
                .Select(g => new { City = g.Key, Quantity = g.Count() })
                .OrderByDescending(pair => pair.Quantity)
                .Take(3);

            var rainCities2 =
                (from elem in arr
                 where elem.StartTime.Year == 2019 && elem.Type == WeatherEventType.Rain
                 group elem by elem.City into g
                 select new { City = g.Key, Quantity = g.Count() } into pair
                 orderby pair.Quantity descending
                 select pair).Take(3);

            rainCities1.PrintCollection();

            Console.WriteLine(new string('-', 5));

            rainCities2.PrintCollection();

            #endregion

            #region 4.Вывести данные самых долгих (топ-1) снегопадов в Америке по годам (за каждый из годов) - с какого времени, по какое время, в каком городе

            Console.WriteLine("4.Вывести данные самых долгих снегопадов в Америке по годам");

            var longSnowCol1 = arr
                .Where(elem => elem.Type == WeatherEventType.Snow)
                .GroupBy(elem => elem.StartTime.Year)
                .Select(g => g.OrderByDescending(elem => elem.EndTime - elem.StartTime))
                .Select(g => new
                {
                    g.First().StartTime.Year,
                    g.First().StartTime,
                    g.First().EndTime,
                    g.First().City
                });

            var longSnowCol2 =
                from elem in arr
                where elem.Type == WeatherEventType.Snow
                group elem by elem.StartTime.Year into gr
                select gr.OrderByDescending(elem => elem.EndTime - elem.StartTime) into g
                let f = g.First()
                select new { f.StartTime.Year, f.StartTime, f.EndTime, f.City };

            longSnowCol1.PrintCollection();

            Console.WriteLine(new string('-', 5));

            longSnowCol2.PrintCollection();

            #endregion
        }
    }

    //Дополнить модель, согласно данным из файла
    class WeatherEvent
    {
        public string EventId { get; set; }
        public WeatherEventType Type { get; set; }
        public Severity Severity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string TimeZone { get; set; }
        public string AirportCode { get; set; }
        public double LocationLat { get; set; }
        public double LocationLng { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public WeatherEvent() { }
        public WeatherEvent(string inp)
        {
            var inpArr = inp.Split(',');

            if (inpArr.Length != 13)
            {
                throw new ArgumentException($"Некорректный ввод: {inp}");
            }

            EventId = inpArr[0];
            Type = ParseEventType(inpArr[1]);
            Severity = ParseSeverity(inpArr[2]);

            DateTime.TryParse(inpArr[3], out DateTime tStart);
            DateTime.TryParse(inpArr[4], out DateTime tEnd);

            StartTime = tStart;
            EndTime = tEnd;

            TimeZone = inpArr[5];
            AirportCode = inpArr[6];

            double.TryParse(inpArr[7], out double Lat);
            double.TryParse(inpArr[8], out double Lng);

            LocationLat = Lat;
            LocationLng = Lng;
            City = inpArr[9];

            County = inpArr[10];
            State = inpArr[11];
            ZipCode = inpArr[12];
        }
        WeatherEventType ParseEventType(string inp)
        {
            return inp switch
            {
                "Snow" => WeatherEventType.Snow,
                "For" => WeatherEventType.Fog,
                "Rain" => WeatherEventType.Rain,
                "Cold" => WeatherEventType.Cold,
                "Hail" => WeatherEventType.Hail,
                "Storm" => WeatherEventType.Storm,
                "Precipitation" => WeatherEventType.Precipitation,
                _ => WeatherEventType.Unknown
            };
        }
        Severity ParseSeverity(string inp)
        {
            return inp switch
            {
                "Light" => Severity.Light,
                "Severe" => Severity.Severe,
                "Moderate" => Severity.Moderate,
                "Heavy" => Severity.Heavy,
                "UNK" => Severity.UNK,
                "Other" => Severity.Other,
                _ => Severity.Unknown
            };
        }
        public override string ToString()
        {
            return $"Id: {EventId}\tType: {Type}\t" +
                $"Severity: {Severity}\tDate: {StartTime}";
        }

    }

    //Дополнить перечисления
    enum WeatherEventType
    {
        Unknown,
        Snow,
        Fog,
        Rain,
        Cold,
        Hail,
        Storm,
        Precipitation
    }
    enum Severity
    {
        Unknown,
        Light,
        Severe,
        Moderate,
        Heavy,
        UNK,
        Other
    }
}
