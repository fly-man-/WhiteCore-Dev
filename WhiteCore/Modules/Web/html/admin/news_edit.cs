﻿/*
 * Copyright (c) Contributors, http://whitecore-sim.org/, http://aurora-sim.org
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the WhiteCore-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using OpenMetaverse;
using WhiteCore.Framework.DatabaseInterfaces;
using WhiteCore.Framework.Servers.HttpServer.Implementation;

namespace WhiteCore.Modules.Web
{
    public class EditNewPage : IWebInterfacePage
    {
        public string[] FilePath
        {
            get
            {
                return new[]
                           {
                               "html/admin/news_edit.html"
                           };
            }
        }

        public bool RequiresAuthentication
        {
            get { return true; }
        }

        public bool RequiresAdminAuthentication
        {
            get { return true; }
        }

        public Dictionary<string, object> Fill(WebInterface webInterface, string filename, OSHttpRequest httpRequest,
                                               OSHttpResponse httpResponse, Dictionary<string, object> requestParameters,
                                               ITranslator translator, out string response)
        {
            response = null;
            var vars = new Dictionary<string, object>();
            IGenericsConnector connector = Framework.Utilities.DataManager.RequestPlugin<IGenericsConnector>();
            GridNewsItem news;

            if (requestParameters.ContainsKey("update"))
            {
                string id = requestParameters["newsid"].ToString();

                string title = requestParameters["NewsTitle"].ToString();
                string text = requestParameters["NewsText"].ToString();

                // some idiot proofing
                if (string.IsNullOrEmpty(title)) {
                    response = webInterface.UserMsg("!Article requires a title");
                    return null;
                }
                if (string.IsNullOrEmpty(text)) {
                    response = webInterface.UserMsg("!Article requires some content");
                    return null;
                }

                news = connector.GetGeneric<GridNewsItem>(UUID.Zero, "WebGridNews", id);
                if (news != null)
                {
                    connector.RemoveGeneric (UUID.Zero, "WebGridNews", id);             // remove the existing news record
                    /* this keeps the original posted date and time
                    GridNewsItem item = new GridNewsItem { 
                        ID = int.Parse (id),
                        Text = text,
                        Title = title,
                        NewsDateTime = news.NewsDateTime,
                        Day = news.NewsDateTime.ToString("dd"),
                        DayName = news.NewsDateTime.ToString ("dddd"),
                        Month = news.NewsDateTime.ToString ("MMM")
                    };
                    */

                    // save the update but use the current date and time
                    GridNewsItem item = new GridNewsItem {
                        ID = int.Parse(id),
                        Text = text,
                        Title = title,
                        NewsDateTime = DateTime.Now,
                        Day = DateTime.Now.ToString("dd"),
                        DayName = DateTime.Now.ToString("dddd"),
                        Month = DateTime.Now.ToString("MMM")
                    };

                    connector.AddGeneric (UUID.Zero, "WebGridNews", id, item.ToOSD ());     // add the updated record
                    response = webInterface.UserMsg("News item updated");

                } else
                    response = webInterface.UserMsg("!Unable to find news item?");
                return null;
            }


            news = connector.GetGeneric<GridNewsItem>(UUID.Zero, "WebGridNews", httpRequest.Query["newsid"].ToString());
            if (news != null)
            {
                vars.Add ("NewsTitle", news.Title);
                vars.Add ("NewsText", news.Text);
                vars.Add ("NewsID", news.ID.ToString ());

                vars.Add ("NewsItemTitle", translator.GetTranslatedString ("NewsItemTitle"));
                vars.Add ("NewsItemText", translator.GetTranslatedString ("NewsItemText"));
                vars.Add ("EditNewsText", translator.GetTranslatedString ("EditNewsText"));
                vars.Add("Cancel", translator.GetTranslatedString("Cancel"));
                vars.Add ("Submit", translator.GetTranslatedString ("Submit"));
            }
            return vars;
        }

        public bool AttemptFindPage(string filename, ref OSHttpResponse httpResponse, out string text)
        {
            text = "";
            return false;
        }
    }
}
