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
    public class Relationship : Query<Model.Relationship>
    {
        private Stores.Relationship RelationshipStore
        {
            get
            {
                return (Stores.Relationship)this.Store;
            }
        }

        protected override List<Model.Relationship> Run()
        {
            IO.Item item = new IO.Item(this.Store.ItemType.Name, "get");
            item.Select = this.Store.Select;
            item.SetProperty("source_id", this.RelationshipStore.Source.ID);
            item.Where = this.Where;
            this.SetPaging(item);

            IO.Request request = this.Store.Session.IO.Request(IO.Request.Operations.ApplyItem, item);
            IO.Response response = request.Execute();

            List<Model.Relationship> ret = new List<Model.Relationship>();

            if (!response.IsError)
            {
                foreach (IO.Item dbitem in response.Items)
                {
                    Model.Relationship relationship = this.Store.Get(dbitem);
                    ret.Add(relationship);
                }

                this.UpdateNoPages(response);
            }
            else
            {
                if (!response.ErrorMessage.Equals("No items of type " + this.RelationshipStore.RelationshipType.Name + " found."))
                {
                    throw new Exceptions.ServerException(response);
                }
            }

            return ret;
        }

        protected override void OnRefresh()
        {
            
        }

        internal Relationship(Stores.Relationship Store, Condition Condition)
            :base(Store, Condition)
        {

        }
    }
}
