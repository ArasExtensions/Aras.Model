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
    public abstract class Query<T> : System.Collections.Generic.IEnumerable<T>
    {
        public delegate void QueryChangedEventHandler(object sender, EventArgs e);

        public event QueryChangedEventHandler QueryChanged;

        protected void OnQueryChanged()
        {
            if (this.QueryChanged != null)
            {
                QueryChanged(this, new EventArgs());
            }
        }

        public List<T> Copy()
        {
            List<T> ret = new List<T>();

            foreach(T item in this)
            {
                ret.Add(item);
            }

            return ret;
        }

        public abstract System.Collections.Generic.IEnumerator<T> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public ItemType Type { get; private set; }

        private Condition _condition;
        public Condition Condition 
        { 
            get
            {
                return this._condition;
            }
            set
            {
                this._condition = value;
            }
        }

        public abstract void Refresh();

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
                    return this.Condition.Where(this.Type);
                }
            }
        }

        internal Query(ItemType Type, Condition Condition)
        {
            this.Type = Type;
            this._condition = Condition;
        }
    }
}
