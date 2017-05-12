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

namespace Aras.Model.Debug
{
    class Program
    {
        public class Test
        {


            static void Main(string[] args)
            {
                Server server = new Server("http://localhost/InnovatorServer100SP4");
                Database database = server.Database("CMB");
                Session session = database.Login("admin", IO.Server.PasswordHash("innovator"));

                Query query = session.Query("Variant Context");
                query.Select = "name,list";
                query.Property("list").Select = "name";
                query.Property("list").Relationship("Value").Select = "value,label";
                query.Property("list").Relationship("Value").OrderBy = "sort_order";


                foreach(Item item in query.Store)
                {
                    Item list = (Item)item.Property("list").Value;
                }
            }
        }
    }
}
