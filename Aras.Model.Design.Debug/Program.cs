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
using System.IO;

namespace Aras.Model.Design.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connect to Server
            Server server = new Server("http://localhost/InnovatorServer10SP4");
            server.LoadAssembly("Aras.Model.Design");
            Database database = server.Database("CMB");
            Session session = database.Login("admin", Server.PasswordHash("innovator"));
            
            // Ensure item_number selected for Parts
            session.ItemType("Part").AddToSelect("item_number");

            // Query Order
            Queries.Item orderquery = (Queries.Item)session.Store("v_Order").Query(Aras.Conditions.Eq("item_number", "DJA9th"));
            

            Order order = (Order)orderquery.First();

            Transaction transaction = session.BeginTransaction();
            order.Update(transaction);
            OrderContext ordercontext = order.OrderContexts.First();
            DateTime start = DateTime.Now;
            ordercontext.Quantity = 2;
            DateTime end = DateTime.Now;
            Console.WriteLine((end - start).Seconds.ToString());

            //start = DateTime.Now;
            //ordercontext.Quantity = 51;
            //end = DateTime.Now;
            //Console.WriteLine((end - start).Seconds.ToString());

            transaction.Commit();
            

        }
    }
}
