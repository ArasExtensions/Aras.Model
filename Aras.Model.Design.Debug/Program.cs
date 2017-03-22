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
            Model.Server server = new Model.Server("http://localhost/11SP9");

            // Load Solution
            server.LoadAssembly("Aras.Model.Design");
            
            // Get Database
            Model.Database database = server.Database("Development");
            
            // Start Session
            Model.Session session = database.Login("admin", IO.Server.PasswordHash("innovator"));

            // Find Document
            Model.Design.Document document = (Model.Design.Document)session.Store("Document").Get("17E4C652AF9D49AAA7EFFBF489D47CF7");

            // Check if Update is possible

            if (document.CanUpdate)
            {
                // Start Transaction
                using (Transaction trans = session.BeginTransaction())
                {
                    // Lock Document for Update
                    document.Update(trans);

                    // Update Properties
                    document.Property("description").Value = "New Description";

                    // Commit all Changes
                    trans.Commit(true);
                }
            }

            // Build Three Level Assembly
            using (Transaction trans = session.BeginTransaction())
            {
                Model.Item part1 = session.Store("Part").Create(trans);
                part1.Property("item_number").Value = "1234";

                Model.Item part2 = session.Store("Part").Create(trans);
                part2.Property("item_number").Value = "1235";
                part2.Store("Part BOM").Create(part1, trans);

                Model.Item part3 = session.Store("Part").Create(trans);
                part3.Property("item_number").Value = "1236";
                part3.Store("Part BOM").Create(part2, trans);

                // Commit all Changes
                trans.Commit(true);
            }
        }
    }
}
