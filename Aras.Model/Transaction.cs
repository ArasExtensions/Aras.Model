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
    public class Transaction : IDisposable
    {
        public Session Session { get; private set; }

        private List<Action> _actions;

        internal void Add(String Name, Item Item)
        {
            this._actions.Add(new Action(Name, Item));
        }

        public Boolean Committed { get; private set; }

        public void Commit()
        {
            if (!this.Committed && this._actions.Count() > 0)
            {
                List<IO.Item> dbitems = new List<IO.Item>();

                foreach (Action action in this._actions)
                {
                    if (action.Item is Relationship)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        IO.Item dbitem = new IO.Item(action.Item.Type.Name, action.Name);
                        dbitem.ID = action.Item.ID;
                        dbitem.ConfigID = action.Item.ConfigID;

                        foreach (Property prop in action.Item.Properties)
                        {
                            if (!prop.Type.ReadOnly && (prop.Modified))
                            {
                                dbitem.SetProperty(prop.Type.Name, prop.DBValue);
                            }
                        }

                        dbitems.Add(dbitem);
                    }
                }

                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, dbitems);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    foreach (IO.Item dbitem in response.Items)
                    {
                        ItemType itemtype = Session.ItemType(dbitem.ItemType);
                        Item item = this.Session.ItemFromCache(dbitem.ID, dbitem.ConfigID, itemtype);
                        item.UpdateProperties(dbitem);
                        item.UnLock();
                    }
                }
                else
                {
                    throw new Exceptions.ServerException(response.ErrorMessage);
                }
            }

            this.Committed = true;
        }

        public void RollBack()
        {

        }

        public void Dispose()
        {
            if (!this.Committed)
            {
                this.RollBack();
            }
        }

        internal Transaction(Session Session)
        {
            this.Session = Session;
            this._actions = new List<Action>();
            this.Committed = false;
        }
    }
}
