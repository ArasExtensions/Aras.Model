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
using System.ComponentModel;

namespace Aras.Model
{
    public class Store : System.Collections.Generic.IEnumerable<Model.Item>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        public Item Source { get; private set; }

        public Boolean Paging
        {
            get
            {
                return this.Query.Paging;
            }
            set
            {
                this.Query.Paging = value;
            }
        }

        public Int32 PageSize
        {
            get
            {
                return this.Query.PageSize;
            }
            set
            {
                this.Query.PageSize = value;
            }
        }

        public Int32 Page
        {
            get
            {
                return this.Query.Page;
            }
            set
            {
                this.Query.Page = value;
            }
        }

        private Int32 _noPages;
        public Int32 NoPages
        {
            get
            {
                return this._noPages;
            }
            private set
            {
                if (this._noPages != value)
                {
                    this._noPages = value;
                    this.OnPropertyChanged("NoPages");
                }
            }
        }

        public Session Session
        {
            get
            {
                return this.ItemType.Session;
            }
        }

        public ItemType ItemType
        {
            get
            {
                return this.Query.ItemType;
            }
        }

        private Query _query;
        public Query Query
        {
            get
            {
                return this._query;
            }
            private set
            {
                if (value != null)
                {
                    this._query = value;
                    this._query.PropertyChanged += Query_PropertyChanged;
                }
                else
                {
                    throw new Exceptions.ArgumentException("Query must be specified");
                }
            }
        }

        void Query_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "PageSize":
                case "Paging":
                    this.OnPropertyChanged(e.PropertyName);
                    break;
                default:
                    break;
            }
        }

        private Dictionary<String, Model.Item> Cache;

        private List<Model.Item> Items;

        private List<Model.Item> CreatedItems;

        public System.Collections.Generic.IEnumerator<Model.Item> GetEnumerator()
        {
            if (this.Items == null)
            {
                this.Refresh();
            }

            return this.Items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Model.Item this[int Index]
        {
            get
            {
                if (this.Items == null)
                {
                    this.Refresh();
                }

                return this.Items[Index];
            }
        }

        public void Refresh()
        {
            // Run Query
            IO.Item dbquery = this.Query.DBQuery();

            if (dbquery != null)
            {
                if (this.Source != null)
                {
                    dbquery.SetProperty("source_id", this.Source.ID);
                }

                IO.Request request = this.ItemType.Session.IO.Request(IO.Request.Operations.ApplyItem, dbquery);
                IO.Response response = request.Execute();

                if (!response.IsError)
                {
                    this.Load(response.Items);

                    if (this.Paging)
                    {
                        if (response.Items.Count() > 0)
                        {
                            this.NoPages = response.Items.First().PageMax;
                        }
                        else
                        {
                            this.NoPages = 0;
                        }
                    }
                    else
                    {
                        this.NoPages = 0;
                    }
                }
                else
                {
                    if (!response.ErrorMessage.Equals("No items of type " + this.ItemType.Name + " found."))
                    {
                        throw new Exceptions.ServerException(response);
                    }
                    else
                    {
                        this.Load(null);
                    }
                }
            }
        }

        internal void Load(IEnumerable<IO.Item> DBItems)
        {
            if (this.Items == null)
            {
                this.Items = new List<Item>();
            }
            else
            {
                this.Items.Clear();
            }

            if (DBItems != null)
            {
                foreach (IO.Item dbitem in DBItems)
                {
                    this.Items.Add(this.Create(dbitem));
                }
            }

            // Add Created Items to end of Items
            List<Item> newcreateditems = new List<Item>();

            foreach(Item createditem in this.CreatedItems)
            {
                if (createditem.State == Item.States.New)
                {
                    this.Items.Add(createditem);
                    newcreateditems.Add(createditem);
                }
            }

            this.CreatedItems = newcreateditems;
        }

        public Model.Item Create(Transaction Transaction)
        {
            Model.Item item = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(Store), typeof(Transaction) }).Invoke(new object[] { this, Transaction }); 
            this.Cache[item.ID] = item;
            this.CreatedItems.Add(item);
            this.Items.Add(item);
            return item;
        }

        internal Model.Item Create(IO.Item DBItem)
        {
            Model.Item ret = null;

            if (this.Cache.ContainsKey(DBItem.ID))
            {
                ret = this.Cache[DBItem.ID];
                ret.UpdateProperties(DBItem);
            }
            else
            {
                ret = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(Store), typeof(IO.Item) }).Invoke(new object[] { this, DBItem }); 
                this.Cache[ret.ID] = ret;
            }

            return ret;
        }

        public Item Get(String ID)
        {
            if (!this.Cache.ContainsKey(ID))
            {
                // Run Query
                IO.Request request = this.ItemType.Session.IO.Request(IO.Request.Operations.ApplyItem, this.Query.DBQuery(ID));
                IO.Response response = request.Execute();

                if (!response.IsError)
                {
                    if (response.Items.Count() == 1)
                    {
                        // Add to Cache
                        this.Create(response.Items.First());
                    }
                    else
                    {
                        throw new Exceptions.ServerException("Item ID not found: " + ID);
                    }
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }
            }

            return this.Cache[ID];
        }

        internal Store(Query Query)
            :this(Query, null)
        {

        }

        internal Store(Query Query, Item Source)
        {
            this.Cache = new Dictionary<String, Item>();
            this.CreatedItems = new List<Item>();
            this.Query = Query;
            this.Source = Source;
            this._noPages = 0;
        }
    }
}
