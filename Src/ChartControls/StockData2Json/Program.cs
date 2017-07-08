using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MyParser.Dzh2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Common;

namespace Stock2Json
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Usage();
                return;
            }

            string dzhFolder = args[0];

            string prpFile = null;
            if (!Directory.Exists(dzhFolder))
            {
                if (!File.Exists(dzhFolder))
                {
                    Usage();
                    Console.WriteLine("Dzh folder not exist");
                    return;
                }
                else
                {
                    prpFile = dzhFolder;
                    dzhFolder = null;
                }
            }

            string outputFile = args[1];
            
            var dirPath = Path.GetDirectoryName(outputFile);
            if(!Directory.Exists(dirPath))
            {
                
                Usage();
                Console.WriteLine("output folder not exist");
                return;
            }

            List<string> ids = new List<string>();

            string lTag = args[2];
            if (!lTag.Equals("-l", StringComparison.OrdinalIgnoreCase))
            {
                Usage();
                return;
            }
            
            DateTime start = DateTime.Now.AddYears(-2), end = DateTime.Now;

            for (int i = 3; i < args.Length; i++)
            {
                if (args[i].Equals("-d", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        start = DateTime.Parse(args[i + 1]);

                        if(i + 2 < args.Length)
                            end = DateTime.Parse(args[i + 2]);
                    }    

                    break;
                }

                ids.Add(args[i]);
            }

            if(dzhFolder != null)
            {
                DZHFolderHelp helper = new DZHFolderHelp(dzhFolder, false);

                LoadAndConvertStock(ids, start, end, helper, outputFile);
            }
            else
            {
                PrpTradeDetailParser parser = new PrpTradeDetailParser(prpFile);
                LoadAndConvertStock(ids, parser, outputFile);
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Stock2Json DzhFolder OutputFile -l id1 id2 ... [-d StartDate EndDate]");
        }

        private const string sh = "SH";
        private const string sz = "SZ";
        private const string sb = "B$";

        private static MinDataFileParser GetDayParser(string id, DZHFolderHelp helper)
        {
            MinDataFileParser parser = null;

            if (id.StartsWith(sh, StringComparison.OrdinalIgnoreCase))
            {
                parser = helper.DayParserSH; 
            }
            else if (id.StartsWith(sz, StringComparison.OrdinalIgnoreCase))
            {
                parser = helper.DayParserSZ;
            }
            else if (id.StartsWith(sb, StringComparison.OrdinalIgnoreCase))
            {
                parser = helper.DayParserBS; 
            }
            else
            {
                parser = helper.DayParserSH; 
            }

            return parser;
        }

        private static string GetId(string id)
        {
            if (id.StartsWith(sh, StringComparison.OrdinalIgnoreCase) || id.StartsWith(sz, StringComparison.OrdinalIgnoreCase) || id.StartsWith(sb, StringComparison.OrdinalIgnoreCase))
            {
                id = id.Substring(2);
            }

            return id;
        }

        private static void LoadAndConvertStock(List<string> ids, DateTime start, DateTime end, DZHFolderHelp helper, string outputFile)
        {
            List<CompleteShare> shares = new List<CompleteShare>();
            foreach (var id in ids)
            {
                var dataParser = GetDayParser(id, helper);
                var stock = dataParser.GetOneStock(GetId(id), start, end);

                if (stock != null)
                {
                    var share = FromStock(stock);
                    shares.Add(share);
                }
            }

            if (shares.Any())
            {
                JsonSerializer jSer = new JsonSerializer();

                if (File.Exists(outputFile))
                {
                    using (JsonTextReader reader = new JsonTextReader(new StreamReader(
                        new FileStream(outputFile, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8)))
                    {
                        var oldShares = jSer.Deserialize<CompleteShare[]>(reader);
                        shares.InsertRange(0, oldShares);
                    }
                }

                using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(
                        new FileStream(outputFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None), Encoding.UTF8)))
                {
                    jSer.Serialize(writer, shares);
                }
            }
        }

        private static void LoadAndConvertStock(List<string> ids, PrpTradeDetailParser parser, string outputFile)
        {
            List<CompleteShare> shares = new List<CompleteShare>();
            foreach (var id in ids)
            {
                var tds = parser.GetOneStockTradeDetail(GetId(id));

                if (!StockTradeDetails.IsNullOrEmpty(tds))
                {
                    var stock = TradeDetails2Stock.Convert(tds, CandlePeriod.min1);
                    
                    var share = FromStock(new Stock(tds.id, stock.Data));
                    shares.Add(share);
                    var stockMa = TradeDetails2Stock.ConvertMA(tds, CandlePeriod.min1);

                    var shareMa = FromStock(new Stock(tds.id + "average", stockMa.Data));
                    shares.Add(shareMa);
                }
            }

            if (shares.Any())
            {
                JsonSerializer jSer = new JsonSerializer();

                if (File.Exists(outputFile))
                {
                    using (JsonTextReader reader = new JsonTextReader(new StreamReader(
                        new FileStream(outputFile, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8)))
                    {
                        var oldShares = jSer.Deserialize<CompleteShare[]>(reader);
                        shares.InsertRange(0, oldShares);
                    }
                }

                using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(
                        new FileStream(outputFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None), Encoding.UTF8)))
                {
                    jSer.Serialize(writer, shares);
                }
            }
        }

        private const int precision = 4;
        private static CompleteShare FromStock(Stock stock)
        {
            
            CompleteShare share = new CompleteShare()
            {
                StockId = stock.id,
                Dates = stock.Items.Select(cd => cd.DateTime).ToList(),
                Closes = stock.Items.Select(cd => Math.Round((double)cd.close, precision)).ToList(),
                Turnovers = stock.Items.Select(cd => Math.Round((double)cd.money, precision)).ToList(),
                Volumns = stock.Items.Select(cd => Math.Round((double)cd.amount, precision)).ToList(),
                Highs = stock.Items.Select(cd => Math.Round((double)cd.high, precision)).ToList(),
                Lows = stock.Items.Select(cd => Math.Round((double)cd.low, precision)).ToList(),
                Opens = stock.Items.Select(cd => Math.Round((double)cd.open, precision)).ToList()
            };

            return share;
        }
    }
}
