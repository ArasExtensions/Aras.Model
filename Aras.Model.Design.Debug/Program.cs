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

        static void Main(string[] args)
        {
            Server server = new Server("http://localhost/11SP1");
            server.LoadAssembly("Aras.Model.Design");
            Database database = server.Database("VariantsDemo11SP1");
            Session session = database.Login("admin", Server.PasswordHash("innovator"));

            Order order = (Order)session.Store("v_Order").Query(Aras.Conditions.Eq("item_number", "400_1111"))[0];

            using (Transaction transaction = session.BeginTransaction())
            {
                order.Update(transaction);
                OrderContext neckerconfig = (OrderContext)order.Store("v_Order Context")[0];
                IEnumerable<ListValue> test = neckerconfig.ValueList.Values.Values;
                neckerconfig.ValueList.Selected = 1;
                OutputOrder(order);
                neckerconfig.Quantity = 11;
                OutputOrder(order);
                neckerconfig.ValueList.Selected = 0;
                OutputOrder(order);
                //transaction.Commit();
            }
        }
    }
}
