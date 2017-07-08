using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MyParser.Dzh2;
using DataSource.Errors;
using Common;

namespace DataSource
{
    public enum DataType
    {
        Simple,
        SimpleVolumn,
        Complete
    }

    public enum Market
    {
        Unknow,
        SH,
        SZ,
        BS
    }

    public class DataController : Controller
    {
        //
        // GET: /Data/
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetShareData(Market market, string id, DateTime? start, DateTime? end, DataType type, string context)
        {
            var stock = LoadStock(market, id, start, end);

            SimpleShare share = null;

            if (stock != null)
            {
                switch (type)
                {
                    case DataType.Simple:
                        share = FromStock(stock);
                        break;
                    case DataType.SimpleVolumn:
                        share = FromStock2(stock);
                        break;
                    case DataType.Complete:
                        share = FromStock3(stock);
                        break;
                }
            }

            if (share != null)
            {
                return Json(share, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new Error() {
                    Code = ErrorCode.InternalError,
                    Message = "Cannot get share"
                }, JsonRequestBehavior.AllowGet);
            }
            
        }

        private MinDataFileParser GetParser(Market market, DZHFolderHelp help)
        {
            MinDataFileParser parser = null;
            switch (market)
            {
                case Market.SH:
                    parser = help.DayParserSH;
                    break;
                case Market.SZ:
                    parser = help.DayParserSZ;
                    break;
                case Market.BS:
                    parser = help.DayParserBS;
                    break;
                default:
                    parser = help.DayParserSH;
                    break;
            }

            return parser;
        }

        private Stock LoadStock(Market market, string id, DateTime? start, DateTime? end)
        {
            Stock s = null;

            var dzhHelper = this.HttpContext.Application.Get(MvcApplication.dzhKey) as DZHFolderHelp;
            if (dzhHelper != null)
            {
                var parser = GetParser(market, dzhHelper);
                if (parser != null)
                {
                    if (start != null && end != null)
                        s = parser.GetOneStock(id, start.Value, end.Value);
                    else if (start != null)
                    {
                        s = parser.GetOneStock(id, start.Value);
                    }
                    else
                    {
                        s = parser.GetOneStock(id, new DateTime(DateTime.Now.Year - 1, 1, 1));
                    }
                }
            }
            return s;
        }

        private SimpleShare FromStock(Stock stock)
        {
            SimpleShare share = new SimpleShare()
            {
                StockId = stock.id,
                Dates = stock.Items.Select(cd => cd.DateTime).ToList(),
                Closes = stock.Items.Select(cd => (double)cd.close).ToList()
            };

            return share;
        }

        private SimpleShareVolumn FromStock2(Stock stock)
        {
            SimpleShareVolumn share = new SimpleShareVolumn()
            {
                StockId = stock.id,
                Dates = stock.Items.Select(cd => cd.DateTime).ToList(),
                Closes = stock.Items.Select(cd => (double)cd.close).ToList(),
                Turnovers = stock.Items.Select(cd => (double)cd.money).ToList(),
                Volumns = stock.Items.Select(cd => (double)cd.amount).ToList(),
            };

            return share;
        }

        private CompleteShare FromStock3(Stock stock)
        {
            CompleteShare share = new CompleteShare()
            {
                StockId = stock.id,
                Dates = stock.Items.Select(cd => cd.DateTime).ToList(),
                Closes = stock.Items.Select(cd => (double)cd.close).ToList(),
                Turnovers = stock.Items.Select(cd => (double)cd.money).ToList(),
                Volumns = stock.Items.Select(cd => (double)cd.amount).ToList(),
                Highs = stock.Items.Select(cd => (double)cd.high).ToList(),
                Lows = stock.Items.Select(cd => (double)cd.low).ToList(),
                Opens = stock.Items.Select(cd => (double)cd.open).ToList(),
            };

            return share;
        }
    }
}
