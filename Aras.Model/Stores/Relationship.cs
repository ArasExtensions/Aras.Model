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

namespace Aras.Model.Stores
{
    public class Relationship : Store<Model.Relationship>
    {
        public Model.Item Source
        {
            get
            {
                return ((Caches.Relationship)this.Cache).Source;
            }
        }

        public RelationshipType RelationshipType
        {
            get
            {
                return (RelationshipType)this.ItemType;
            }
        }

        protected override List<Model.Relationship> Run()
        {
            IO.Item item = new IO.Item(this.ItemType.Name, "get");
            item.Select = this.Cache.Select;
            item.SetProperty("source_id", this.Source.ID);
            item.Where = this.Where;
            this.SetPaging(item);

            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Cache.Session, item);
            IO.SOAPResponse response = request.Execute();

            List<Model.Relationship> ret = new List<Model.Relationship>();

            if (!response.IsError)
            {
                foreach (IO.Item dbitem in response.Items)
                {
                    Model.Relationship relationship = this.Cache.Get(dbitem);
                    ret.Add(relationship);
                }

                this.UpdateNoPages(response);
            }
            else
            {
                if (!response.ErrorMessage.Equals("No items of type " + this.RelationshipType.Name + " found."))
                {
                    throw new Exceptions.ServerException(response);
                }
            }

            return ret;
        }

        public Model.Relationship Create(Model.Item Related, Transaction Transaction)
        {
            Model.Relationship relationship = ((Caches.Relationship)this.Cache).Create(Related, Transaction);
            this.NewItems.Add(relationship);
            this.Items.Add(relationship);
            return relationship;
        }

        public Model.Relationship Create(Model.Item Related)
        {
            return ((Caches.Relationship)this.Cache).Create(Related, null);
        }

        internal Relationship(Cache<Model.Relationship> Store, Condition Condition)
            : base(Store, Condition)
        {

        }

        internal Relationship(Cache<Model.Relationship> Store)
            :this(Store, null)
        {

        }
    }
}
