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
            public void WalkPart(Item Part, Int32 Depth, Boolean Variant)
            {
                System.Console.WriteLine(Depth + " : " + Variant + " : " + Part.Property("item_number"));

                foreach(Relationship rel in Part.Relationships("Part BOM"))
                {
                    this.WalkPart(rel.Related, Depth + 1, false);
                }

                foreach (Relationship rel in Part.Relationships("Part Variants"))
                {
                    this.WalkPart(rel.Related, Depth + 1, true);
                }
            }

            public Test()
            {

            }
        }

        static void Main(string[] args)
        {       
            Server server = new Server("http://localhost/InnovatorServer100SP4");
            Database database = server.Database("CMB");
            Session session = database.Login("admin", IO.Server.PasswordHash("innovator"));

            Query partquery = session.Query("Part");
            partquery.Paging = false;
            partquery.Select = "item_number";
            partquery.Recursive = true;
            partquery.Relationship("Part BOM").Select = "quantity,related_id";
            partquery.Relationship("Part Variants").Select = "quantity,related_id";

            
            //Item part = partquery.Store.Get("0C694EB2E17FAF49879FE17C2D9A600F");
            Item part = partquery.Store.Get("6C260746B8804E62BE09F47D9D8D99E8");

            Boolean test = part.Property("item_number").ReadOnly;
            Object test2 = part.Property("item_number").Value;  
        }
    }
}
