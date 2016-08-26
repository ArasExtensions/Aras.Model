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
            Model.Server server = new Model.Server("http://localhost/InnovatorServer100SP4/");
            //server.ProxyURL = "http://127.0.0.1:8888";
            server.LoadAssembly("Aras.Model.Design");
            Model.Database database = server.Database("CMB");
            Model.Session session = database.Login("admin", Model.Server.PasswordHash("innovator"));
            session.ItemType("CAD").AddToSelect("native_file,viewable_file");

            Model.Stores.Item<Model.Design.Order> store = new Model.Stores.Item<Model.Design.Order>(session, "v_Order", Aras.Conditions.Eq("item_number", "RJMTest10"));
            Model.Design.Order order = store.First();

            foreach(Model.Design.OrderContext ordercontext in order.OrderContexts)
            {
                VariantContext variantcontext = ordercontext.VariantContext;
                String question = variantcontext.Question;
                String value = ordercontext.Value;
                Properties.VariableList valuelist = ordercontext.ValueList;
                List list = valuelist.Values;
                IEnumerable<ListValue> values = list.Values;
                ListValue test = values.First();
            }

            using (Model.Transaction transaction = session.BeginTransaction())
            {
                order.Update(transaction);
                order.UpdateBOM();
                transaction.Commit();
            }

            /*
            Model.Stores.Item<Model.Design.Part> store = new Model.Stores.Item<Model.Design.Part>(session, "Part");
            Model.Design.Part part1 = null;

 
            using (Model.Transaction transaction = session.BeginTransaction())
            {
                part1 = store.Create(transaction);
                part1.ItemNumber = "1234-RJM";
                transaction.Commit();
            }
     

            Model.Stores.Item<Model.Design.Part> store2 = new Model.Stores.Item<Model.Design.Part>(session, "Part", Aras.Conditions.Eq("item_number", "1234-RJM"));
            Model.Design.Part part2 = store2.First();

            using (Model.Transaction transaction = session.BeginTransaction())
            {
                part2.Update(transaction);
                part2.Property("cmb_name").Value = "Testing 999";
                transaction.Commit();
            }

            using (Model.Transaction transaction = session.BeginTransaction())
            {
                part2.Delete(transaction);
                transaction.Commit();
            }
            */
        }
    }
}
