﻿/*  
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
    public abstract class Query
    {
        public ItemType Type { get; private set; }

        private List<PropertyType> SelectCache;

        public IEnumerable<PropertyType> SelectPropertyTypes
        {
            get
            {
                return this.SelectCache;
            }
        }

        public String Select
        {
            get
            {
                List<String> names = new List<String>();

                foreach (PropertyType proptype in this.SelectCache)
                {
                    names.Add(proptype.Name);
                }

                return String.Join(",", names);
            }
            set
            {
                this.SelectCache.Clear();

                foreach (String name in value.Split(','))
                {
                    PropertyType proptype = this.Type.PropertyType(name);

                    if (!this.SelectCache.Contains(proptype))
                    {
                        this.SelectCache.Add(proptype);
                    }
                }
            }
        }

        internal Query(ItemType Type)
        {
            this.Type = Type;
            this.SelectCache = new List<PropertyType>();
        }
    }
}
