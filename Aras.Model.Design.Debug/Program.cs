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
            Server server = new Server("http://localhost/11SP1");
            server.LoadAssembly("Aras.Model.Design");
            Database database = server.Database("VariantsDemo11SP1");
            Session session = database.Login("admin", Server.PasswordHash("innovator"));

            Stores.Item<Order> store = new Stores.Item<Order>(session,  "v_Order", Aras.Conditions.Eq("item_number", "400_1111"));

            Order order = store.First();

            using (Transaction trans = session.BeginTransaction())
            {
                order.Update(trans);
                order.OrderContexts.First().Value = "1";
                order.OrderContexts.First().Quantity = 11;
                order.UpdateBOM();
                int noboms = order.ConfiguredPart.PartBOMS.Count();

                order.OrderContexts.First().Value = "0";
                order.UpdateBOM();
                noboms = order.ConfiguredPart.PartBOMS.Count();

                order.OrderContexts.First().Value = "1";
                order.UpdateBOM();
                noboms = order.ConfiguredPart.PartBOMS.Count();

                trans.Commit();
            }

        }
    }
}
