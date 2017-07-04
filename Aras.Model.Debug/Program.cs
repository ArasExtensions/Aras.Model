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
                query.Select = "item_number,name,cmb_name,description,major_rev,keyed_name,state,cmb_ibs_transfer_lock,cmb_ibs_commodity_code,cmb_ibs_eng_order_no,cmb_ibs_field19,cmb_ibs_lead_time,cmb_ibs_mat_move_code,cmb_ibs_part_type,cmb_ibs_planner_code,cmb_ibs_product_code,cmb_ibs_product_group,cmb_name_no_cr,make_buy,unit";
                query.Condition = Aras.Conditions.Eq("item_number", "2317M");
                Model.Item part1 = query.Store.First();

                foreach(Property prop in part1.Properties)
                {
                    String test = prop.ToString();
                }
            }
        }
    }
}
