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

namespace Aras.Model.Relationships
{
    [Attributes.ItemType("Access")]
    public class Access : Relationship
    {
        public Items.Identity Identity
        {
            get
            {
                return (Items.Identity)this.Related;
            }
        }

        public Boolean IdentityCanGet
        {
            get
            {
                if (this.Property("can_get").Value == null)
                {
                    return false;
                }
                else
                {
                    return (Boolean)this.Property("can_get").Value;
                }
            }
        }

        public Boolean IdentityCanUpdate
        {
            get
            {
                if (this.Property("can_update").Value == null)
                {
                    return false;
                }
                else
                {
                    return (Boolean)this.Property("can_update").Value;
                }
            }
        }

        public Boolean IdentityCanDelete
        {
            get
            {
                if (this.Property("can_delete").Value == null)
                {
                    return false;
                }
                else
                {
                    return (Boolean)this.Property("can_delete").Value;
                }
            }
        }

        public Boolean IdentityCanDiscover
        {
            get
            {
                if (this.Property("can_discover").Value == null)
                {
                    return false;
                }
                else
                {
                    return (Boolean)this.Property("can_discover").Value;
                }
            }
        }

        public Boolean IdentityCanChangeAccess
        {
            get
            {
                if (this.Property("can_change_access").Value == null)
                {
                    return false;
                }
                else
                {
                    return (Boolean)this.Property("can_change_access").Value;
                }
            }
        }

        public Access(Store Store, Transaction Transaction)
            : base(Store, Transaction)
        {
      
        }

        public Access(Store Store, IO.Item DBItem)
            : base(Store, DBItem)
        {

        }
    }
}
