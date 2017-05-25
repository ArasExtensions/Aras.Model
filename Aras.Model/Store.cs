/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
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
    public class StoreChangedEventArgs : EventArgs
    {
        public StoreChangedEventArgs()
            : base()
        {
      
        }
    }

    public delegate void StoreChangedEventHandler(object sender, StoreChangedEventArgs e);

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

        public event StoreChangedEventHandler Changed;

        internal void OnChanged()
        {
            if (this.Changed != null)
            {
                Changed(this, new StoreChangedEventArgs());
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

        private void AddToCache(Model.Item Item)
        {
            if (!this.Cache.ContainsKey(Item.ID))
            {
                this.Cache[Item.ID] = Item;

                Item.Deleted += Item_Deleted;
            }
        }

        private void Item_Deleted(object sender, DeletedEventArgs e)
        {
            Model.Item item = (Model.Item)sender;

            // Remove Item from List if Present
            if (this.Items.Remove(item))
            {
                this.OnChanged();
            }

            // Remove from Cache if Present
            if (this.Cache.ContainsKey(item.ID))
            {
                this.Cache.Remove(item.ID);
            }
        }

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

        public List<Model.Item> CurrentItems()
        {
            List<Model.Item> items = new List<Model.Item>();

            foreach(Model.Item item in this)
            {
                if (item.Action != Item.Actions.Delete)
                {
                    items.Add(item);
                }
            }

            return items;
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
            // Clear Items
            if (this.Items == null)
            {
                this.Items = new List<Item>();
            }
            else
            {
                this.Items.Clear();
            }

            // Add Created Items to start of Items
            List<Item> newcreateditems = new List<Item>();

            foreach (Item createditem in this.CreatedItems)
            {
                if (createditem.State == Item.States.New)
                {
                    this.Items.Add(createditem);
                    newcreateditems.Add(createditem);
                }
            }

            this.CreatedItems = newcreateditems;

            // Load Items from Server
            if (DBItems != null)
            {
                foreach (IO.Item dbitem in DBItems)
                {
                    this.Items.Add(this.Create(dbitem));
                }
            }

            // Trigger Changed Event
            this.OnChanged();
        }

        public Model.Item Create(Transaction Transaction)
        {
            Model.Item item = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(Store), typeof(Transaction) }).Invoke(new object[] { this, Transaction });

            // Add to Cache
            this.AddToCache(item);

            // Add to Created Items
            this.CreatedItems.Add(item);

            if (this.Items == null)
            {
                this.Items = new List<Item>();
            }

            this.Items.Add(item);

            // Add to Transaction
            Transaction.Add("add", item);

            // Trigger Changed Event
            this.OnChanged();

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
                // Create Item
                ret = (Model.Item)this.ItemType.Class.GetConstructor(new Type[] { typeof(Store), typeof(IO.Item) }).Invoke(new object[] { this, DBItem });
 
                // Add to Cache
                this.AddToCache(ret);
            }

            return ret;
        }

        internal void Refresh(Item Item)
        {
            if (this.Cache.ContainsKey(Item.ID))
            {
                // Run Query
                IO.Request request = this.ItemType.Session.IO.Request(IO.Request.Operations.ApplyItem, this.Query.DBQuery(Item.ID));
                IO.Response response = request.Execute();

                if (!response.IsError)
                {
                    if (response.Items.Count() == 1)
                    {
                        // Update Item
                        this.Cache[Item.ID].UpdateProperties(response.Items.First());
                    }
                    else
                    {
                        throw new Exceptions.ServerException("Item ID not found: " + Item.ID);
                    }
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }
            }
            else
            {
                throw new Exceptions.ServerException("Item ID not found: " + Item.ID);
            }
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
