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

        private Dictionary<String, Action> _actions;


        internal void Add(String Name, Item Item)
        {
            if (!this._actions.ContainsKey(Item.ID))
            {
                this._actions[Item.ID] = new Action(Name, Item);
                Item.Transaction = this;
            }
        }

        public Boolean Committed { get; private set; }

        public void Commit()
        {
            if (!this.Committed && this._actions.Count() > 0)
            {
                Dictionary<String, IO.Item> dbitems = new Dictionary<string, IO.Item>();

                foreach (Action action in this._actions.Values)
                {
                    if (action.Item is Relationship)
                    {
                        Relationship relationship = (Relationship)action.Item;

                        if (dbitems.ContainsKey(relationship.Source.ID))
                        {
                            IO.Item dbrelation = new IO.Item(action.Item.ItemType.Name, action.Name);
                            dbrelation.ID = action.Item.ID;

                            if (relationship.Related != null)
                            {
                                dbrelation.SetProperty("related_id", relationship.Related.ID);
                            }

                            foreach (Property prop in relationship.Properties)
                            {
                                if (!prop.Type.Runtime && !prop.Type.ReadOnly && (prop.Modified))
                                {
                                    dbrelation.SetProperty(prop.Type.Name, prop.DBValue);
                                }
                            }

                            dbitems[relationship.Source.ID].AddRelationship(dbrelation);
                        }
                        else
                        {
                            throw new Exceptions.ArgumentException("Source Item is not part of Transaction: " + relationship.Source.ID);
                        }
                    }
                    else
                    {
                        IO.Item dbitem = new IO.Item(action.Item.ItemType.Name, action.Name);
                        dbitem.ID = action.Item.ID;

                        foreach (Property prop in action.Item.Properties)
                        {
                            if (!prop.Type.Runtime && !prop.Type.ReadOnly && (prop.Modified))
                            {
                                dbitem.SetProperty(prop.Type.Name, prop.DBValue);
                            }
                        }

                        dbitems[dbitem.ID] = dbitem;
                    }
                }

                foreach(IO.Item dbrequestitem in dbitems.Values)
                {
                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, dbrequestitem);
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        foreach (IO.Item dbitem in response.Items)
                        {
                            ItemType itemtype = Session.ItemType(dbitem.ItemType);
                            Item item = this.Session.ItemFromCache(dbitem.ID, itemtype);
                            item.UpdateProperties(dbitem);
                            item.UnLock();
                            item.Transaction = null;
                        }
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }
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
            this._actions = new Dictionary<String, Action>();
            this.Committed = false;
        }
    }
}
