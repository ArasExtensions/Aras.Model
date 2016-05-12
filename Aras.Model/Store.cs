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
    public class StoreChangedEventArgs : EventArgs
    {
        public StoreChangedEventArgs()
            : base()
        {

        }
    }

    public delegate void StoreChangedEventHandler(object sender, StoreChangedEventArgs e);

    public abstract class Store<T> : System.Collections.Generic.IEnumerable<T> where T : Model.Item, INotifyPropertyChanged
    {
        public const Int32 MinPageSize = 5;
        public const Int32 DefaultPageSize = 25;
        public const Int32 MaxPageSize = 100;

        public event StoreChangedEventHandler StoreChanged;

        protected void OnStoreChanged()
        {
            if (this.StoreChanged != null)
            {
                StoreChanged(this, new StoreChangedEventArgs());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        private Int32 _pageSize;
        public Int32 PageSize
        {
            get
            {
                return this._pageSize;
            }
            set
            {
                if (this._pageSize != value)
                {
                    if (value >= MinPageSize && value <= MaxPageSize)
                    {
                        this._pageSize = value;
                        this.OnPropertyChanged("PageSize");
                    }
                }
            }
        }

        private Int32 _page;
        public Int32 Page
        {
            get
            {
                return this._page;
            }
            set
            {
                if (this._page != value)
                {
                    if (value >= 1)
                    {
                        this._page = value;
                        this.OnPropertyChanged("Page");
                    }
                }
            }
        }

        private Int32 _noPages;
        public Int32 NoPages
        {
            get
            {
                return this._noPages;
            }
            protected set
            {
                if (this._noPages != value)
                {
                    this._noPages = value;
                    this.OnPropertyChanged("NoPages");
                }
            }
        }

        private Boolean _paging;
        public Boolean Paging
        {
            get
            {
                return this._paging;
            }
            set
            {
                if (value != this._paging)
                {
                    this._paging = value;
                    this.OnPropertyChanged("Paging");
                }
            }
        }

        protected void SetPaging(IO.Item Item)
        {
            if (this.Paging)
            {
                Item.PageSize = this.PageSize;
                Item.Page = this.Page;
            }
        }

        protected void UpdateNoPages(IO.SOAPResponse Response)
        {
            if (this.Paging)
            {
                if (Response.Items.Count() > 0)
                {
                    this.NoPages = Response.Items.First().PageMax;
                }
                else
                {
                    this.NoPages = 0;
                }
            }
        }

        protected List<T> NewItems;

        protected List<T> Items;

        public T this[int Index]
        {
            get
            {
                if (!this.Executed)
                {
                    this.Execute();
                    this.Executed = true;
                }

                return this.Items[Index];
            }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            if (!this.Executed)
            {
                this.Execute();
                this.Executed = true;
            }

            return this.Items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerable<T> Copy()
        {
            return this.ToList();
        }

        public abstract ItemType ItemType { get; }

        private Condition _condition;
        public Condition Condition 
        { 
            get
            {
                return this._condition;
            }
            set
            {
                if (this._condition == null)
                {
                    if (value != null)
                    {
                        this._condition = value;
                        this.Executed = false;
                    }
                }
                else
                {
                    if (!this._condition.Equals(value))
                    {
                        this._condition = value;
                        this.Executed = false;
                    }
                }
            }
        }

        protected System.String Where
        {
            get
            {
                if (this.Condition == null)
                {
                    return null;
                }
                else
                {
                    return this.Condition.Where(this.ItemType);
                }
            }
        }

        private Boolean Executed;

        private void Execute()
        {
            // Clear current Items
            this.Items.Clear();

            // Run Query and add Items that are not marked for Deletion
            foreach (T item in this.Run())
            {
                if (item.Action != Item.Actions.Delete)
                {
                    this.Items.Add(item);
                }
            }

            // Refresh New Items
            foreach(T item in this.NewItems.ToList())
            {
                if (item.Action != Item.Actions.Create)
                {
                    this.NewItems.Remove(item);
                }
            }

            // Add New Items to End of Results
            foreach(T item in this.NewItems)
            {
                this.Items.Add(item);
            }

            this.OnStoreChanged();
        }

        protected abstract List<T> Run();

        protected abstract void OnRefresh();

        public void Refresh()
        {
            this.OnRefresh();
            this.Execute();
            this.Executed = true;
        }

        internal Store(Condition Condition)
        {
            this.Items = new List<T>();
            this.NewItems = new List<T>();
            this._condition = Condition;
            this.Executed = false;
            this._pageSize = DefaultPageSize;
            this._paging = false;
            this._noPages = 0;
            this._page = 1;
        }


    }
}
