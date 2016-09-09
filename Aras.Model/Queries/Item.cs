/*  
  Aras.Model provides a .NET cient library for Aras Innovator

  Copyright (C) 2016 Processwall Limited.

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
    public class Item : Model.Query<Model.Item>
    {
        protected override List<Model.Item> Run()
        {
            IO.Item item = new IO.Item(this.Store.ItemType.Name, "get");
            item.Select = this.Store.Select;
            item.Where = this.Where;
            this.SetPaging(item);

            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Store.Session, item);
            IO.SOAPResponse response = request.Execute();

            List<Model.Item> ret = new List<Model.Item>();

            if (!response.IsError)
            {
                foreach (IO.Item dbitem in response.Items)
                {
                    Model.Item thisitem = (Model.Item)this.Store.Get(dbitem);
                    ret.Add(thisitem);
                }

                this.UpdateNoPages(response);
            }
            else
            {
                if (!response.ErrorMessage.Equals("No items of type " + this.Store.ItemType.Name + " found."))
                {
                    throw new Exceptions.ServerException(response);
                }
            }

            return ret;
        }

        protected override void OnRefresh()
        {

        }

        internal Item(Model.Stores.Item Store, Condition Condition)
            : base(Store, Condition)
        {

        }
    }
}
