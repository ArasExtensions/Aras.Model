/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
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

                Query query = session.Query("Part");
                query.Select = "item_number,name";
                query.Relationship("Part BOM").Select = "quantity,related_id";

                Query query2 = session.Query("Part");
                query2.Select = "item_number,name";

                Item part1 = null;
                Item part2 = null;
                Relationship rel = null;

                using(Transaction trans = session.BeginTransaction())
                {
                    part1 = query.Store.Create(trans);
                    part1.Property("item_number").Value = "TEST001";

                    part2 = query.Store.Create(trans);
                    part2.Property("item_number").Value = "TEST002";

                    rel = (Relationship)part1.Relationships("Part BOM").Create(trans);

                    part1.Relationships("Part BOM").Changed += Test_Changed;

                    rel.Property("quantity").Value = 1.0;
                    rel.Related = part2;

                    trans.Commit(true);
                }

                Model.Item part3 = query2.Store.Add(part1);

                String test = (String)part3.Property("item_number").Value;

                /*
                List<Item> rels = part1.Relationships("Part BOM").ToList();

                using (Transaction trans = session.BeginTransaction())
                {
                    part1.Update(trans);
                    rel.Delete(trans);
                    trans.Commit(true);
                }

                rels = part1.Relationships("Part BOM").ToList();

                using (Transaction trans = session.BeginTransaction())
                {
                    part1.Update(trans);
                    rel = (Relationship)part1.Relationships("Part BOM").Create(trans);
                    rel.Property("quantity").Value = 2.0;
                    rel.Related = part2;

                    rels = part1.Relationships("Part BOM").ToList();

                    trans.Commit(true);
                }

                rels = part1.Relationships("Part BOM").ToList();
                */
            }

            static void Test_Changed(object sender, StoreChangedEventArgs e)
            {
                Store store = (Store)sender;
                List<Item> items = store.ToList();
            }
        }
    }
}
