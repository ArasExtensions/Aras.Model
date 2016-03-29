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
            session.ItemType("Part").AddToSelect("item_number,keyed_name");

  
            Queries.Item part1query = (Queries.Item)session.Store("Part").Query(Aras.Conditions.Eq("item_number", "DX0001"));
            Part part1 = (Part)part1query.First();
            Queries.Item part2query = (Queries.Item)session.Store("Part").Query(Aras.Conditions.Eq("item_number", "DX0002"));
            Part part2 = (Part)part2query.First();
            Queries.Item part3query = (Queries.Item)session.Store("Part").Query(Aras.Conditions.Eq("item_number", "DX0003"));
            Part part3 = (Part)part3query.First();

            using(Transaction trans = session.BeginTransaction())
            {
                part1.Update(trans);

                foreach(PartBOM partbom in part1.Store("Part BOM"))
                {
                    partbom.Delete(trans);
                }

                trans.Commit();
            }

            int test1 = part1.Store("Part BOM").Count();

            using (Transaction trans = session.BeginTransaction())
            {
                part1.Update(trans);

                PartBOM partbom = (PartBOM)part1.Store("Part BOM").Create(part2, trans);

                trans.Commit();
            }

            int test2 = part1.Store("Part BOM").Count();

            using (Transaction trans = session.BeginTransaction())
            {
                part1.Update(trans);

                part1.Store("Part BOM").Create(part3, trans);

                trans.Commit();
            }

            int test3 = part1.Store("Part BOM").Count();

            using (Transaction trans = session.BeginTransaction())
            {
                part1.Update(trans);

                foreach (PartBOM partbom in part1.Store("Part BOM"))
                {
                    if (partbom.Related.Equals(part2))
                    {
                        partbom.Delete(trans);
                    }
                }

                trans.Commit();
            }

            int test4 = part1.Store("Part BOM").Count();
        }
    }
}
