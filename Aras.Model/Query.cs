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
    public abstract class Query<T> : System.Collections.Generic.IEnumerable<T> where T : Model.Item
    {
        public Store<T> Store { get; private set; }

        protected List<T> Items;

        public T this[int Index]
        {
            get
            {
                if (this.Condition == null)
                {
                    return this.Store[Index];
                }
                else
                {
                    if (!this.Executed)
                    {
                        this.Execute();
                        this.Executed = true;
                    }

                    return this.Items[Index];
                }
            }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            if (this.Condition == null)
            {
                // No Condition, Rrfresh store and return all Items
                this.Store.Refresh();
                return this.Store.GetEnumerator();
            }
            else
            {
                if (!this.Executed)
                {
                    this.Execute();
                    this.Executed = true;
                }

                return this.Items.GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public ItemType ItemType
        {
            get
            {
                return this.Store.ItemType;
            }
        }

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

        protected abstract void Execute();

        public void Refresh()
        {
            this.Execute();
            this.Executed = true;
        }

        internal Query(Store<T> Store, Condition Condition)
        {
            this.Items = new List<T>();
            this.Store = Store;
            this._condition = Condition;
            this.Executed = false;
        }
    }
}
