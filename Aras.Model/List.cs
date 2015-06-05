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
    public class List : IEquatable<List>
    {
        public Session Session { get; private set; }

        public String ID { get; private set; }

        public String Name { get; private set; }

        internal List<ListValue> ValuesCache { get; private set; }

        public IEnumerable<ListValue> Values
        {
            get
            {
                return this.ValuesCache;
            }
        }

        public ListValue Value(String Value)
        {
            foreach(ListValue listvalue in this.Values)
            {
                if (listvalue.Value == Value)
                {
                    return listvalue;
                }
            }

            return null;
        }

        public bool Equals(List other)
        {
            if (other == null)
            {
                return false;
            }
            else
            {
                return this.ID.Equals(other.ID);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if (obj is List)
                {
                    return this.Equals((List)obj);
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal List(Session Session, String ID, String Name)
        {
            this.Session = Session;
            this.ID = ID;
            this.Name = Name;
            this.ValuesCache = new List<ListValue>();
        }

    }
}
