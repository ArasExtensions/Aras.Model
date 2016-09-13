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
            Model.Server server = new Model.Server("http://WIN-HBC9KO4SQ6E/InnovatorServer100SP4/");
            //server.ProxyURL = "http://127.0.0.1:8888";
            server.LoadAssembly("Aras.Model.Design");
            Model.Database database = server.Database("CMB");
            Model.Session session1 = database.Login("admin", Model.Server.PasswordHash("innovator"));
            Model.Session session2 = database.Login("barrettv", Model.Server.PasswordHash("innovator"));

            //session.ItemType("CAD").AddToSelect("native_file,viewable_file");

            
            Queries.Item orderquery1 = session1.Store("v_Order").Query(Aras.Conditions.Eq("item_number", "RJMTest002"));
            Model.Design.Order order1 = (Model.Design.Order)orderquery1.First();

            Queries.Item orderquery2 = session2.Store("v_Order").Query(Aras.Conditions.Eq("item_number", "RJMTest002"));
            Model.Design.Order order2 = (Model.Design.Order)orderquery2.First();

            /*
            using(Transaction transaction = session.BeginTransaction())
            {
                order.Update(transaction);
                order.Process(transaction);
                transaction.Commit(true);
            }


            using (Transaction transaction = session.BeginTransaction())
            {
                Part part = (Part)session.Store("Part").Create(transaction);
                part.ItemNumber = "RJMTest9990004";
                part.Property("cmb_name").Value = "Test RJM";
                transaction.Commit(false);
            }            */
        }
    }
}
