/*  
  Aras.Model provides a .NET cient library for Aras Innovator

  Copyright (C) 2015 Processwall Limited.

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU Affero General Public License as published
  by the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Affero General Public License for more details.

  You should have received a copy of the GNU Affero General Public License
  along with this program.  If not, see http://opensource.org/licenses/AGPL-3.0.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aras.Model;

namespace Aras.Model.Design.Debug
{
    class Program
    {
        static void OutputOrder(Order Order)
        {
            Console.WriteLine(Order.ItemNumber);

            Part configuredpart = Order.ConfiguredPart;

            foreach (OrderContext ordercontext in Order.Store("v_Order Context"))
            {
                if (ordercontext.Action != Item.Actions.Deleted)
                {
                    Console.WriteLine(ordercontext.Property("value").Value + "\t" + ordercontext.ValueList.Value);
                }
            }

            Console.WriteLine();

            foreach(PartBOM partbom in configuredpart.Store("Part BOM"))
            {
                if (partbom.Action != Item.Actions.Deleted)
                {
                    Console.WriteLine(partbom.Related.Property("name").Value + "\t" + partbom.Quantity.ToString());
                }
            }

            Console.WriteLine();
        }

        static String ItemNumber(int cnt)
        {
            return "DX-" + cnt.ToString().PadLeft(10, '0');
        }

        static int CreateBOM(Part Part, int cnt, int depth, Transaction Transaction)
        {
            int thiscnt = cnt;

            if (depth < 3)
            {
                for(int i=0; i<5; i++)
                {
                    Part childpart = (Part)Part.Session.Store("Part").Create(Transaction);
                    childpart.ItemNumber = ItemNumber(thiscnt);
                    thiscnt++;

                    Part.Store("Part BOM").Create(childpart, Transaction);

                    thiscnt = CreateBOM(childpart, thiscnt, depth + 1, Transaction);
                }
            }

            return thiscnt;
        }

        static void Main(string[] args)
        {
            Server server = new Server("http://localhost/InnovatorServer10SP4");
            server.LoadAssembly("Aras.Model.Design");
            Database database = server.Database("CMB");
            Session session = database.Login("admin", Server.PasswordHash("innovator"));

            Order order = (Order)session.Store("v_Order").Query(Aras.Conditions.Eq("item_number", "MJC_Order004")).First();
            using (Transaction transaction = session.BeginTransaction())
            {
                order.Update(transaction);
                transaction.Commit();
            }

            /*
            int cnt = 1;

            using (Transaction transaction = session.BeginTransaction())
            {
                Part toplevel = (Part)session.Store("Part").Create(transaction);
                toplevel.ItemNumber = ItemNumber(cnt);
                cnt++;

                cnt = CreateBOM(toplevel, cnt, 0, transaction);

                transaction.Commit();
            }*/
        }
    }
}
