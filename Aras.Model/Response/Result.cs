using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

namespace Aras.Model.Response
{
    public class Result
    {
        public Request.Item Request { get; private set; }

        public int ItemMax { get; internal set; }

        public int PageMax { get; internal set; }

        public int Page { get; internal set; }

        internal List<Item> _items;
        public IEnumerable<Item> Items
        {
            get
            {
                return this._items;
            }
        }

        internal Result(Request.Item Request)
        {
            this.Request = Request;
            this._items = new List<Item>();
            this.ItemMax = 0;
            this.PageMax = 1;
            this.Page = 1;
        }
    }
}
