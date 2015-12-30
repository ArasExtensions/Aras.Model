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

namespace Aras.Model.Queries
{
    public class Item : Query
    {
        private List<Model.Item> Items;

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        public override void Refresh()
        {
            this.Items.Clear();

            IO.Item item = new IO.Item(this.Type.Name, "get");
            item.Select = "id," + this.Select;
            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Type.Session, item);
            IO.SOAPResponse response = request.Execute();

            if (!response.IsError)
            {
                foreach(IO.Item dbitem in response.Items)
                {
                    Model.Item cacheitem = this.Type.Session.ItemFromCache(dbitem.ID, this.Type);
                    cacheitem.UpdateProperties(dbitem);
                    this.Items.Add(cacheitem);
                }
            }
            else
            {
                if (!response.ErrorMessage.Equals("No items of type " + this.Type.Name + " found."))
                {
                    throw new Exceptions.ServerException(response);
                }
            }
        }

        internal Item(ItemType Type, String Select)
            :base(Type, Select)
        {
            this.Items = new List<Model.Item>();
            this.Refresh();
        }
    }
}
