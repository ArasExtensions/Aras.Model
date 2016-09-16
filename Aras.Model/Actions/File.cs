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
using System.Net;
using System.Net.Http;
using System.IO;
using System.Xml;

namespace Aras.Model.Actions
{
    internal class File : Action
    {
        private IO.Item Result;
        
        internal override IO.Item Commit()
        {
            if (!this.Completed)
            {
                // Read Cached File
                byte[] filebytes = ((Model.File)this.Item).GetCacheBytes();

                // Build Request
                String contentboundary = "-------------S36Ut9A3ZtWwum";
                MultipartFormDataContent content = new MultipartFormDataContent(contentboundary);

                StringContent soapaction = new StringContent("ApplyItem");
                content.Add(soapaction, "SOAPACTION");

                StringContent authuser = new StringContent(this.Item.Session.Username);
                content.Add(authuser, "AUTHUSER");

                StringContent password = new StringContent(this.Item.Session.Password);
                content.Add(password, "AUTHPASSWORD");

                StringContent database = new StringContent(this.Item.Session.Database.Name);
                content.Add(database, "DATABASE");

                StringContent locale = new StringContent("");
                content.Add(locale, "LOCALE");

                StringContent timezone = new StringContent("GMT Standard Time");
                content.Add(timezone, "TIMEZONE_NAME");

                StringContent vault = new StringContent(this.Item.Session.User.Vault.ID);
                content.Add(vault, "VAULTID");

                IO.Item dbfile = new IO.Item("File", "add");
                dbfile.ID = this.Item.ID;
                dbfile.SetProperty("filename", ((Model.File)this.Item).VaultFilename);
                dbfile.SetProperty("file_size", filebytes.Length.ToString());
                IO.Item dbloacted = new IO.Item("Located", "add");
                dbloacted.SetProperty("related_id", this.Item.Session.User.Vault.ID);
                dbfile.AddRelationship(dbloacted);

                String xmldata = "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><ApplyItem>" + dbfile.GetString() + "</ApplyItem></SOAP-ENV:Body></SOAP-ENV:Envelope>";
                StringContent xml = new StringContent(xmldata);
                content.Add(xml, "XMLdata");

                ByteArrayContent filedata = new ByteArrayContent(filebytes);
                content.Add(filedata, this.Item.ID, ((Model.File)this.Item).VaultFilename);

                // Post Request to Vault Server
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Item.Session.User.Vault.URL);
                request.CookieContainer = this.Item.Session.Cookies;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.ContentType = "multipart/form-data; boundary=" + contentboundary;
                request.Method = "POST";
                request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                request.Headers.Add("Cache-Control", "no-cache");
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
              
                using (Stream poststream = request.GetRequestStream())
                {
                    content.CopyToAsync(poststream);
                }

                using (HttpWebResponse webresponse = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream result = webresponse.GetResponseStream())
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(result);
                        IO.SOAPResponse response = new IO.SOAPResponse(webresponse.Cookies, doc);

                        if (!response.IsError)
                        {
                            if (response.Items.Count() == 1)
                            {
                                this.Result = response.Items.First();

                                if (this.Result.ConfigID.Equals(this.Item.ConfigID))
                                {
                                    if (!this.Result.ID.Equals(this.Item.ID))
                                    {
                                        // New Version of Item
                                        Model.Item newversion = this.Item.Session.Store(this.Item.ItemType).Get(this.Result);
                                        Model.Item oldversion = this.Item;
                                        this.Item = newversion;
                                        this.UpdateItem(this.Result);
                                        oldversion.OnSuperceded(newversion);
                                    }
                                    else
                                    {
                                        this.UpdateItem(this.Result);
                                    }

                                    this.Item.UpdateProperties(this.Result);
                                }
                                else
                                {
                                    // Result does not match Item
                                    throw new Exceptions.ServerException("Server response does not match original Item");
                                }
                            }
                        }
                    }
                }

                this.Completed = true;
            }

            return this.Result;
        }

        internal override void Rollback()
        {
            if (!this.Completed)
            {
                switch (this.Item.Action)
                {
                    case Model.Item.Actions.Create:

                        // Remove from Cache
                        this.Item.Session.Store(this.Item.ItemType).Delete(this.Item);

                        break;
             
                    default:
                        break;
                }

                this.Completed = true;
            }
        }

        internal override void UpdateStore()
        {
            if (this.Item.Action == Model.Item.Actions.Delete)
            {
                // Trigger Deleted Event
                this.Item.OnDeleted();

                // Remove from Cache
                this.Item.Session.Store(this.Item.ItemType).Delete(this.Item);
            }
        }

        internal File(Transaction Transaction, String Name, Model.Item Item)
            : base(Transaction, Name, Item)
        {
            this.Result = null;
        }
    }
}
