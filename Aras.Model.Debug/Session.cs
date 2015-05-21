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
    public class Session
    {
      
        public void Execute()
        {
            Server server = new Server("http://localhost/innovatorserver100sp4");
            Database database = server.Database("Development100SP4");
            Model.Session session = database.Login("admin", "innovator");

            Console.WriteLine("User: " + session.User.Property("keyed_name").Object);

            Requests.Item partrequest = session.Request("get", "Part");
            partrequest.AddSelection("item_number,description,keyed_name,viewable_file");
            Requests.Relationship partbomrequest = partrequest.AddRelationship("Part BOM", "get");
            partbomrequest.AddSelection("quantity");
            partbomrequest.Related = partrequest;

            Response partsresponse = partrequest.Execute();

            foreach(Responses.Item partresponse in partsresponse.Items)
            {
                Item part = partresponse.Cache;

                Console.WriteLine(part.Property("item_number").Object);

                foreach(Responses.Item partbomresponse in partresponse.Relationships)
                {
                    Relationship partbom = (Relationship)partbomresponse.Cache;
                    Console.WriteLine(" - " + partbom.Related.Property("item_number").Object + " " + partbomresponse.Cache.Property("quantity"));
                }
            }

            // Update Description of Assembly
            Item assembly = partsresponse.Items.First().Cache;

            if (session.Lock(assembly))
            {
                Requests.Item updaterequest = session.Request("update", assembly);
                assembly.Property("description").Object = "Testing 9999";
                Response updateresponse = updaterequest.Execute();
                session.UnLock(assembly);
            }
            
        }

        public Session()
        {

        }
    }
}
