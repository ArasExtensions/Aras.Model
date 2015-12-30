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

namespace Aras.Model
{
    [Attributes.ItemType("User")]
    public class User : Item
    {
        private Alias _alias;
        private Alias Alias
        {
            get
            {
                this.LoadAlias();
                return this._alias;
            }
        }

        private Identity _identity;
        private Identity Identity
        {
            get
            {
                this.LoadAlias();
                return this._identity;
            }
        }

        private Boolean AliasLoaded;
        private void LoadAlias()
        {
            if (!AliasLoaded)
            {
                IO.Item dbalias = new IO.Item("Alias", "get");
                dbalias.Select = "id,related_id";
                dbalias.SetProperty("source_id", this.ID);
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.ItemType.Session, dbalias);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    dbalias = response.Items.First();
                    IO.Item dbidenity = dbalias.GetPropertyItem("related_id");
                    this._identity = (Identity)this.ItemType.Session.ItemFromCache(dbidenity.ID, this.ItemType.Session.ItemType("Identity"));
                    this._alias = (Alias)this.ItemType.Session.RelationshipFromCache(dbalias.ID, this.ItemType.RelationshipType("Alias"), this, this._identity);
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }

                this.AliasLoaded = true;
            }
        }

        public User(String ID, ItemType Type)
            :base(ID, Type)
        {
            this.AliasLoaded = false;
        }
    }
}
