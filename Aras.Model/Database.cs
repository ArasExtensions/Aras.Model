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
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Xml;

namespace Aras.Model
{
    public class Database
    {
        public Server Server { get; private set; }

        public String Name { get; private set; }

        public Session Login(String Username, String Password)
        {
            // Get MD5 Hash of Password
            String md5password = null;

            using(MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(Password));
                StringBuilder md5string = new StringBuilder();

                for(int i=0; i<data.Length; i++)
                {
                    md5string.Append(data[i].ToString("x2"));
                }

                md5password = md5string.ToString();
            }

            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ValidateUser, this, Username, md5password);
            IO.SOAPResponse response = request.Execute();

            if (!response.IsError)
            {
                String id = response.Result.SelectSingleNode("id").InnerText;
                Session session = new Session(this, id, Username, md5password);
                return session;
            }
            else
            {
                throw new Exceptions.ServerException(response.ErrorMessage);
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal Database(Server Server, String Name)
            :base()
        {
            this.Server = Server;
            this.Name = Name;
        }
    }
}
