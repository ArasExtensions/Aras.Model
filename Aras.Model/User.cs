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
        private Stores.Relationship<Alias> _aliases;
        private Stores.Relationship<Alias> Aliases
        {
            get
            {
                if (this._aliases == null)
                {
                    this._aliases = new Stores.Relationship<Alias>(this, "Alias");
                }

                return this._aliases;
            }
        }

        public Alias Alias
        {
            get
            {
                return (Alias)this.Aliases.First();
            }
        }

        public Identity Identity
        {
            get
            {
                return (Identity)this.Alias.Related;
            }
        }

        public Vault Vault
        {
            get
            {
                return (Vault)this.Property("default_vault").Value;
            }
            set
            {
                this.Property("default_vault").Value = value;
            }
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();

            if (this._aliases != null)
            {
                this._aliases.Refresh();
            }
        }

        internal User(ItemType ItemType, Transaction Transaction)
            : base(ItemType, Transaction)
        {
          
        }

        internal User(ItemType ItemType, IO.Item DBItem)
            : base(ItemType, DBItem)
        {
          
        }
    }
}
