using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTools;

namespace Pinger
{
    class Scanner
    {
        public IPAddressRange Range { get; set; }
        public int Timeout { get; set; }

        public Scanner(string range, int timeout = 30)
        {
            Range = IPAddressRange.Parse(range);
            Timeout = timeout;
        }

        public List<Task<ScanResult>> Scan()
        {
            return this.Range.AsEnumerable().Select(ip => new Scan(ip.ToString(), this.Timeout)).Select(s => s.Run())
                .ToList();
        }
    }
}
