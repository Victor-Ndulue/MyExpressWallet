using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers.TransactionHelpers
{
    public class bankData
    {
        public string name {  get; set; }
        public string code { get; set; }
        public string country { get; set; }
        public bool pay_with_bank { get; set; }
        public bool active { get; set; }
        public string currency { get; set; }
        public string type { get; set; }
        public int id { get; set; }
    }
}
