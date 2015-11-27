using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.Model.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server("http://localhost/11SP1");
            Database database = server.Database("VariantsDemo11SP1");
            Session session = database.Login("admin", "innovator");


        }
    }
}
