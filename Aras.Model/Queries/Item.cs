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
    public class Item : Query
    {
        public IEnumerable<Model.Item> Execute()
        {
            List<Model.Item> ret = new List<Model.Item>();

            IO.Item item = new IO.Item(this.Type.Name, "get");

            if (this.SelectPropertyTypes.Count() > 0)
            {
                item.Select = "id,config_id," + this.Select;
            }
            else
            {
                item.Select = "id,config_id";
            }

            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Type.Session, item);
            IO.SOAPResponse response = request.Execute();

            if (!response.IsError)
            {
                foreach(IO.Item dbitem in response.Items)
                {
                    Model.Item cacheitem = this.Type.Session.ItemFromCache(dbitem.ID, dbitem.ConfigID, this.Type);

                    foreach(PropertyType proptype in this.SelectPropertyTypes)
                    {
                        cacheitem.Property(proptype).DBValue = dbitem.GetProperty(proptype.Name);
                    }

                    ret.Add(cacheitem);
                }
            }
            else
            {
                throw new Exceptions.ServerException(response);
            }

            return ret;
        }

        internal Item(ItemType Type)
            :base(Type)
        {

        }
    }
}
