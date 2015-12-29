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
    public abstract class Query: System.Collections.IEnumerable
    {
        public abstract System.Collections.IEnumerator GetEnumerator();

        public ItemType Type { get; private set; }

        private String _select;
        public String Select
        {
            get
            {
                return this._select;
            }
            set
            {
                if (this._select == null)
                {
                    if (value != null)
                    {
                        this._select = value;
                        this.Refresh();
                    }
                }
                else
                {
                    if (!this._select.Equals(value))
                    {
                        this._select = value;
                        this.Refresh();
                    }
                }
            }
        }

        public abstract void Refresh();

        internal Query(ItemType Type, String Select)
        {
            this.Type = Type;
            this._select = Select;
        }
    }
}
