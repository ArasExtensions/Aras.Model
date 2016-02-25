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
using System.IO;
using System.Security.Cryptography;

namespace Aras.Model
{
    public class Icon
    {
        private String _iD;
        public String ID
        {
            get
            {
                if (this._iD == null)
                {
                    using (MD5 md5Hash = MD5.Create())
                    {
                        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(this.URL));
                        StringBuilder stringbuilder = new StringBuilder();

                        for (int i = 0; i < data.Length; i++)
                        {
                            stringbuilder.Append(data[i].ToString("X2"));
                        }

                        this._iD = stringbuilder.ToString();
                    }
                }

                return this._iD;
            }
        }

        public String URL { get; private set; }

        private String _name;
        public String Name
        {
            get
            {
                if (this._name == null)
                {
                    int lastsep = this.URL.LastIndexOf('/');

                    if (lastsep < 0)
                    {
                        this._name = this.URL;
                    }
                    else
                    {
                        this._name = this.URL.Substring(lastsep + 1, this.URL.Length  - lastsep - 1);
                    }
                }

                return this._name;
            }
        }

        private byte[] Data;

        public MemoryStream Read()
        {
            return new MemoryStream(this.Data, false);
        }

        internal Icon(String URL, byte[] Data)
        {
            this.URL = URL.ToLower();
            this.Data = Data;
        }
    }
}
