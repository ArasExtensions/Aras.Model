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

namespace Aras.Design.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server("http://localhost/11SP1");
            Database database = server.Database("VariantsDemo11SP1");
            database.LoadAssembly(Environment.CurrentDirectory + "\\Aras.Design.dll");
            Session session = database.Login("admin", Server.PasswordHash("innovator"));

            session.ItemType("v_Order").AddToSelect("keyed_name,item_number,name,part,locked_by_id,configured_part");
            session.ItemType("v_Order Context").AddToSelect("quantity");
            session.ItemType("Variant Context").AddToSelect("name,keyed_name,context_type,list,method,question");
            

            foreach (Order order in session.Query("v_Order"))
            {
                if (order.ItemNumber == "0002")
                {
                   using(Transaction transaction = session.BeginTransaction())
                   {
                       order.Update(transaction);
                       order.Property("name").Value = "Test Company 0002";
                       OrderContext ordercontext = order.OrderContext.First();
                       VariantContext variantcontext = ordercontext.VariantContext;
                       

                       transaction.Commit();
                   }
                }
            }
        }
    }
}
