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
using System.Xml;
using System.Net;
using System.IO;

namespace Aras.Model.IO
{
    internal enum SOAPOperation { ValidateUser, ApplyItem };

    internal class SOAPRequest
    {
        internal SOAPOperation Operation { get; private set; }

        internal Database Database { get; private set; }

        internal String Username { get; private set; }

        internal String Password { get; private set; }

        internal Item Item { get; private set; }

        private HttpWebRequest _request;
        private HttpWebRequest Request
        {
            get
            {
                if (this._request == null)
                {
                    this._request = (HttpWebRequest)WebRequest.Create(this.Database.Server.URL + "/Server/InnovatorServer.aspx");
                    this._request.Method = "POST";
                    this._request.ContentType = "text/xml; charset=utf-8";
                    this._request.Headers.Add("AUTHPASSWORD", this.Password);
                    this._request.Headers.Add("AUTHUSER", this.Username);
                    this._request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    this._request.Headers.Add("Accept-Encoding", "gzip, deflate");
                    this._request.Headers.Add("DATABASE", this.Database.Name);
                    this._request.Headers.Add("SOAPACTION", this.Operation.ToString());
                    this._request.Headers.Add("TIMEZONE_NAME", "GMT Standard Time");

                    // Get bytes for SOAP Header
                    byte[] header = System.Text.Encoding.ASCII.GetBytes("<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><" + this.Operation.ToString() + ">");
                    
                    // Get bytes for SOAP Data
                    byte[] data = null;

                    if (this.Item != null)
                    {
                        data = this.Item.GetBytes();
                    }
                    else
                    {
                        data = new byte[0];
                    }

                    // Get Bytes for SOAP Footer
                    byte[] footer = System.Text.Encoding.ASCII.GetBytes("</" + this.Operation.ToString() + "></SOAP-ENV:Body></SOAP-ENV:Envelope>");

                    // Write SOAP Message to Request and update length
                    this._request.ContentLength = header.Length + data.Length + footer.Length;

                    using (Stream poststream = this._request.GetRequestStream())
                    {
                        poststream.Write(header, 0, header.Length);
                        poststream.Write(data, 0, data.Length);
                        poststream.Write(footer, 0, footer.Length);
                    }
                }

                return this._request;
            }
        }

        internal SOAPResponse Execute()
        {
            try
            {
                using (WebResponse webresponse = this.Request.GetResponse())
                {
                    using (Stream result = webresponse.GetResponseStream())
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(result);
                        return new SOAPResponse(doc);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exceptions.ServerException("Unable to connect to Server", ex);
            }
        }

        internal async Task<SOAPResponse> ExecuteAsync()
        {
            try
            {
                using (Task<WebResponse> task = this.Request.GetResponseAsync())
                {
                    WebResponse webresponse = await task;

                    using (Stream result = webresponse.GetResponseStream())
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(result);
                        return new SOAPResponse(doc);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exceptions.ServerException("Unable to connect to Server", ex);
            }
        }

        internal SOAPRequest(SOAPOperation Operation, Session Session, Item Item)
        {
            this.Operation = Operation;
            this.Database = Session.Database;
            this.Username = Session.Username;
            this.Password = Session.Password;
            this.Item = Item;
        }

        internal SOAPRequest(SOAPOperation Operation, Database Database, String Username, String Password)
        {
            this.Operation = Operation;
            this.Database = Database;
            this.Username = Username;
            this.Password = Password;
        }
    }
}
