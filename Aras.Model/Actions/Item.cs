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

namespace Aras.Model.Actions
{
    internal class Item : Action
    {
        private IO.Item Result;
        
        internal override IO.Item Commit()
        {
            if (!this.Completed)
            {
                IO.Item dbitem = this.BuildItem();
                IO.SOAPResponse response = this.SendItem(dbitem);

                if (!response.IsError)
                {
                    if (response.Items.Count() == 1)
                    {
                        this.Result = response.Items.First();
                        this.UpdateItem(this.Result);
                    }

                    this.Completed = true;
                }
                else
                {
                    this.Completed = true;
                    throw new Exceptions.ServerException(response);
                }
            }

            return this.Result;
        }

        internal override void Rollback()
        {
            if (!this.Completed)
            {
                foreach(Actions.Relationship relationship in this.Relationships)
                {
                    relationship.Rollback();
                }

                switch (this.Item.Action)
                {
                    case Model.Item.Actions.Create:

                        // Remove from Store
                        this.Item.Session.Store(this.Item.ItemType).RemoveItemFromCache(this.Item);

                        break;
                    
                    case Model.Item.Actions.Update:

                        // Unlock
                        this.Item.UnLock();

                        break;
             
                    default:
                        break;
                }

                this.Completed = true;
            }
        }

        internal Item(Transaction Transaction, String Name, Model.Item Item)
            : base(Transaction, Name, Item)
        {
            this.Result = null;
        }
    }
}
