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
 * 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.Model.Queries
{
    public class Item<T> : Query<T> where T:Model.Item
    {
        public override void Execute()
        {
            Aras.IOM.Item request = this.Session.Innovator.newItem(this.ItemType, "get");
            request.setAttribute("select", "id,config_id,keyed_name");
            Aras.IOM.Item response = request.apply();

            if (!response.isError())
            {
                this.Clear();

                for(int i=0; i<response.getItemCount(); i++)
                {
                    Aras.IOM.Item thisitem = response.getItemByIndex(i);
                    T item = (T)this.Session.ItemFromCache(typeof(T), thisitem.getID(), thisitem.getProperty("config_id"), thisitem.getProperty("keyed_name"));
                    this.Add(item);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Item(Session Session)
            :base(Session)
        {

        }
    }
}
