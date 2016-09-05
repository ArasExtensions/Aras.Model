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
        static void Main(string[] args)
        {
            Server server = new Server("http://localhost/InnovatorServer100SP4");
            Database database = server.Database("CMB");
            Session session = database.Login("admin", Server.PasswordHash("innovator"));

            Stores.Item<Item> partstore = new Stores.Item<Item>(session, "Part", Aras.Conditions.Eq("item_number", "RJMTest12"));
            Item part = partstore.First();

            Stores.Relationship<Relationship> partbomstore = new Stores.Relationship<Relationship>(part, "Part BOM");

            List<Item> deletedrelated = new List<Item>();

            using (Transaction trans = session.BeginTransaction())
            {
                part.Update(trans);

                foreach (Relationship rel in partbomstore)
                {
                    deletedrelated.Add(rel.Related);
                    rel.Delete(trans);
    
                }

                trans.Commit();
            }

            using(Transaction trans = session.BeginTransaction())
            {
                part.Update(trans);

                foreach(Item related in deletedrelated)
                {
                    partbomstore.Create(related, trans);
                }

                trans.Commit();
            }


        }
    }
}
