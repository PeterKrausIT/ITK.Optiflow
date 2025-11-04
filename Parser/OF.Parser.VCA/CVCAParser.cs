// ***********************************************************************
// Assembly         : OF.Parser.VCA
// Author           : Peter Kraus
// Created          : 10-07-2025
// ***********************************************************************
// <summary>CVCAParser.cs
// Parses VCA format OMA/DCA into data dictionary.
// For format definition see official docs.
// </summary>
// ***********************************************************************
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OF.Module;
using System.Activities;
using System.ComponentModel;
using System.Activities.Presentation.Metadata;
using System.Globalization;

/// <summary>
/// The VCA namespace.
/// </summary>
namespace OF.Parser.VCA
{
    /// <summary>
    /// Activity to parse OMA message
    /// </summary>
    public sealed class CVCAParser : MBActivity, IOFModule
    {
        /// <summary>
        /// The m identifier
        /// </summary>
        private readonly Guid mID = new Guid("243DAF91-7A61-4F48-BCE3-62496AFFAEC9");
        /// <summary>
        /// The m name
        /// </summary>
        private readonly string mName = "VCA Parser";
        /// <summary>
        /// The m description
        /// </summary>
        private readonly string mDescription = "Parsing VCA's OMA/DCS format.";

        /// <summary>
        /// Initializes a new instance of the <see cref="CVCAParser"/> class.
        /// </summary>
        public CVCAParser()
        {
            PerfCount.Name = Name;
        }

        /// <summary>
        /// The ID of this Activity is 243DAF91-7A61-4F48-BCE3-62496AFFAEC9
        /// </summary>
        /// <value>The identifier.</value>
        public Guid ID
        {
            get { return mID; }
        }

        /// <summary>
        /// Gets name of activity
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return mName; }
        }

        /// <summary>
        /// Gets description of activity
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return mDescription; }
        }

        /// <summary>
        /// Logs the verbose.
        /// </summary>
        /// <param name="eventtxt">The event text.</param>
        /// <param name="eventid">The event id.</param>
        public void LogVerbose(object eventtxt, int eventid)
        {
            LogVerbose(eventtxt, eventid, wFID);
        }
        /// <summary>
        /// Logs the information.
        /// </summary>
        /// <param name="eventtxt">The event text.</param>
        /// <param name="eventid">The event id.</param>
        public void LogInfo(object eventtxt, int eventid)
        {
            LogInfo(eventtxt, eventid, wFID);
        }
        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="eventtxt">The event text.</param>
        /// <param name="eventid">The event id.</param>
        public void LogError(object eventtxt, int eventid)
        {
            LogError(eventtxt, eventid, wFID);
        }

        /// <summary>
        /// The OMA as string
        /// </summary>
        /// <value>The oma MSG.</value>
        [DefaultValue(null)]
        public InArgument<string> OMAMsg { get; set; }
        /// <summary>
        /// The Workflow ID for logging and exceptions
        /// </summary>
        /// <value>The wfid.</value>
        public InArgument<string> WFID { get; set; }
        /// <summary>
        /// The MessageID to be stored with the order
        /// </summary>
        /// <value>The MSG identifier.</value>
        public InArgument<string> MsgID { get; set; }

        /// <summary>
        /// OrderDictionary as output
        /// </summary>
        /// <value>The order.</value>
        public OutArgument<OF.Module.OrderDictionary> Order { get; set; }

        /// <summary>
        /// The oma MSG
        /// </summary>
        private string OMAMsg_ = "";
        /// <summary>
        /// The w fid
        /// </summary>
        private string wFID;
        /// <summary>
        /// The MSG identifier
        /// </summary>
        private string msgID;
        /// <summary>
        /// The dictionary
        /// </summary>
        private OF.Module.OrderDictionary dictionary = null;
        /// <summary>
        /// The nfi
        /// </summary>
        private NumberFormatInfo nfi = new NumberFormatInfo();

        /// <summary>
        /// Main method of the workflow activity
        /// Parses the message initial into a PairOrder array.
        /// This could be changed if needed using the Rules-Activity
        /// </summary>
        /// <param name="context">The Context</param>
        protected override void Execute(NativeActivityContext context)
        {
            PerfCount.StartJob();
            OMAMsg_ = OMAMsg.Get(context);

            wFID = WFID.Get(context);
            msgID = MsgID.Get(context);

            log4net.ThreadContext.Properties["Message"] = msgID;
            LogInfo("VCAParser started", 1, wFID);

            dictionary = new OrderDictionary();
            int NoOfItems = ParseOMA();

            this.Order.Set(context, dictionary);
            this.Result.Set(context, NoOfItems);

            LogInfo("VCAParser parsed " + NoOfItems.ToString() + " order(s)", 1, wFID);

            PerfCount.EndJob(NoOfItems);
        }

        /// <summary>
        /// Parses the oma.
        /// </summary>
        /// <returns>Number of orders (integer).</returns>
        public int ParseOMA()
        {
            int counter = 1;
            try
            {
                string omaShapeString = "";
                nfi.NumberDecimalSeparator = ".";

                OrderDictionary[] orders = new OrderDictionary[1];
                OrderDictionary order = orders[0] = new OrderDictionary();
                
                string[] lines = OMAMsg_.Split(new char[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    #region OneLine

                    string[] tags = line.Replace("\"", "").Split(new char[] { '=', ';' });
                    string[] tags2 = line.Replace("\"", "").Split(new char[] { '=' });
                    string key = "";
                    string key2 = "";
                    string val2 = "";
                    string valB = "";
                    decimal valR = 0m;
                    decimal valL = 0m;
                    int ivalR = 0;
                    int ivalL = 0;
                    string svalR = "";
                    string svalL = "";

                    if (tags.Length > 0)
                        key = tags[0].ToUpper();
                    if (tags.Length > 1)
                        svalR = tags[1];
                    if (tags.Length > 2)
                        svalL = tags[2];
                    if (tags2.Length > 1)
                        valB = tags2[1].Trim();
                    if (tags2.Length > 2)
                    {
                        key2 = tags2[1].Trim();
                        val2 = tags2[2].Trim();
                    }

                    decimal.TryParse(svalR, System.Globalization.NumberStyles.Float, nfi, out valR);
                    decimal.TryParse(svalL, System.Globalization.NumberStyles.Float, nfi, out valL);
                    int.TryParse(svalR, out ivalR);
                    int.TryParse(svalL, out ivalL);

                    if (key == "JOB")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key, (string)valB);
                        log4net.ThreadContext.Properties["Job"] = valB;
                    }
                    else if ((key == "ENGMARK") || (key == "_ENGMARK"))
                    {
                        int cnt = 0;
                        while (order.ContainsKey("ENGMARK" + ((cnt == 0) ? "" : ("." + cnt.ToString()))))
                            cnt++;
                        order.SetValue("ENGMARK" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (string)valB);
                    }
                    else if (key == "TINT")
                    {
                        order.SetValue(key + ".R", (string)svalR);
                        order.SetValue(key + ".L", (string)svalL);
                    }
                    else if (key == "ACOAT")
                    {
                        order.SetValue(key + ".R", (string)svalR);
                        order.SetValue(key + ".L", (string)svalL);
                    }
                    else if (key == "FCOAT")
                    {
                        order.SetValue(key + ".R", (string)svalR);
                        order.SetValue(key + ".L", (string)svalL);
                    }
                    else if (key == "DO")
                    {
                        order.SetValue(key, (string)valB);
                    }
                    else if (key == "REM")
                    {
                        order.SetValue(key, (string)valB);
                    }
                    else if (key == "CLIENT")
                    {
                        order.SetValue(key, (string)valB);
                    }
                    else if (key == "CLIENTF")
                    {
                        order.SetValue(key, (string)valB);
                    }
                    else if (key == "ACCN")
                    {
                        order.SetValue(key, (string)valB);
                    }
                    else if ((key == "R") || (key == "A") || (key == "Z") || (key == "ZA") || (key == "ZFMT"))
                    {
                        omaShapeString += line + "\r\n";
                    }
                    else if (key == "TRCFMT")
                    {
                        omaShapeString += line + "\r\n";
                    }
                    else if (key == "CIRC")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "HBOX")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "VBOX")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "FCRV")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "ZTILT")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "CIRC3D")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "FRAM")
                    {
                        order.SetValue(key, (string)valB);
                    }
                    // FMFR
                    else if (key == "FMFR")
                    {
                        order.SetValue(key, (string)valB);
                    }
                    // FUPC
                    else if (key == "FUPC")
                    {
                        order.SetValue(key, (string)valB);
                    }
                    else if (key == "DBL")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key, (decimal)valR);
                    }
                    else if (key == "CSIZ")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "BSIZ")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "IPD")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "NPD")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "OCHT")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "SEGHT")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "SEGDHT")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "SPH")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "CYL")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "ADD")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "AX")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "DIA")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "CRIB")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "PRVA")
                    {
                        int cnt = 0;
                        while (order.ContainsKey(key + ".R" + ((cnt == 0) ? "" : ("."+cnt.ToString()))))
                            cnt++;
                        order.SetValue(key + ".R" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valR);
                        order.SetValue(key + ".L" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valL);
                    }
                    else if (key == "PRVA1")
                    {
                        int cnt = 0;
                        while (order.ContainsKey("PRVA.R" + ((cnt == 0) ? "" : ("." + cnt.ToString()))))
                            cnt++;
                        order.SetValue("PRVA.R" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valR);
                        order.SetValue("PRVA.L" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valL);
                    }
                    else if (key == "PRVA2")
                    {
                        int cnt = 0;
                        while (order.ContainsKey("PRVA.R" + ((cnt == 0) ? "" : ("." + cnt.ToString()))))
                            cnt++;
                        order.SetValue("PRVA.R" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valR);
                        order.SetValue("PRVA.L" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valL);
                    }
                    else if (key == "PRVM")
                    {
                        int cnt = 0;
                        while (order.ContainsKey(key + ".R" + ((cnt == 0) ? "" : ("." + cnt.ToString()))))
                            cnt++;
                        order.SetValue(key + ".R" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valR);
                        order.SetValue(key + ".L" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valL);
                    }
                    else if (key == "PRVM1")
                    {
                        int cnt = 0;
                        while (order.ContainsKey("PRVM.R" + ((cnt == 0) ? "" : ("." + cnt.ToString()))))
                            cnt++;
                        order.SetValue("PRVM.R" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valR);
                        order.SetValue("PRVM.L" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valL);
                    }
                    else if (key == "PRVM2")
                    {
                        int cnt = 0;
                        while (order.ContainsKey("PRVM.R" + ((cnt == 0) ? "" : ("." + cnt.ToString()))))
                            cnt++;
                        order.SetValue("PRVM.R" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valR);
                        order.SetValue("PRVM.L" + ((cnt == 0) ? "" : ("." + cnt.ToString())), (decimal)valL);
                    }
                    else if (key == "FCOCIN")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "FCSGIN")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "FCOCUP")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "SGOCIN")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "SGOCUP")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "FCSGUP")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "PINB")
                    {
                        order.SetValue(key, (decimal)valR);
                    }
                    else if (key == "FPINB")
                    {
                        order.SetValue(key, (decimal)valR);
                    }
                    else if (key == "POLISH")
                    {
                        order.SetValue(key, (bool)(ivalR == 1));
                    }
                    else if (key == "ERSGIN")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "ERSGUP")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "ERDRIN")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "ERDRUP")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "ERNRIN")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "ERNRUP")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "SBEV")
                    {
                        order.SetValue(key + ".R", (int)(ivalR));
                        order.SetValue(key + ".L", (int)(ivalL));
                    }
                    else if (key == "LIND")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "CTHICK")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "MINEDG")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "CTHK")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "THKP")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "FRNT")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "HASENG")
                    {
                        order.SetValue(key + ".R", (bool)(ivalR == 1));
                        order.SetValue(key + ".L", (bool)(ivalL == 1));
                    }
                    else if (key == "GRADIENT")
                    {
                        order.SetValue(key + ".R", (bool)(ivalR == 1));
                        order.SetValue(key + ".L", (bool)(ivalL == 1));
                    }
                    else if (key == "POLAR")
                    {
                        order.SetValue(key + ".R", (bool)(ivalR == 1));
                        order.SetValue(key + ".L", (bool)(ivalL == 1));
                    }
                    else if (key == "SWIDTH")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key.StartsWith("LTYP"))
                    {
                        order.SetValue(key + ".R", (string)svalR);
                        order.SetValue(key + ".L", (string)svalL);
                    }
                    else if (key == "LMATTYPE")
                    {
                        order.SetValue(key + ".R", (int)ivalR);
                        order.SetValue(key + ".L", (int)ivalL);
                    }

                    else if (key == "MESG")
                    {
                        int cnt = 0;
                        while (order.ContainsKey(key+ ((cnt == 0) ? "" : ("." + cnt.ToString()))))
                            cnt++;
                        order.SetValue(key + ((cnt == 0) ? "" : ("." + cnt.ToString())), (string)valB);
                    }
                    else if (key == "DRILL")
                    {
                        omaShapeString += line + "\r\n";
                    }
                    else if (key == "DRILLE")
                    {
                        omaShapeString += line + "\r\n";
                    }
                    else if (key == "FTYP")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key, (int)ivalR);
                    }
                    else if (key == "ETYP")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key, (int)ivalR);
                        order.SetValue(key + ".R", (int)ivalR);
                        order.SetValue(key + ".L", (int)ivalL);
                    }

                    else if (key == "_ETYP2")
                    {
                        order.SetValue(key + ".R", (int)ivalR);
                        order.SetValue(key + ".L", (int)ivalL);
                    }
                    else if (key == "_IDSEQ")
                    {
                        order.SetValue(key + ".R", (int)ivalR);
                        order.SetValue(key + ".L", (int)ivalL);
                    }
                    else if (key == "FRNT")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "_FRNT")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "THKP")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "LNAM")
                    {
                        order.SetValue(key + ".R", (string)svalR);
                        order.SetValue(key + ".L", (string)svalL);
                    }
                    else if (key == "MBASE")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "TIND")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "_LCOAT")
                    {
                        order.SetValue("LCOAT.R", (string)svalR);
                        order.SetValue("LCOAT.L", (string)svalL);
                    }
                    else if (key == "GDEPTH")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "GWIDTH")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "BEVM")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }

                    else if (key == "BEVC")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "BEVP")
                    {
                        omaShapeString += line + "\r\n";
                        order.SetValue(key + ".R", (int)ivalR);
                        order.SetValue(key + ".L", (int)ivalL);
                    }
                    else if (key == "LDDRSPH")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "LDDRCYL")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "LDDRAX")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "_LDDRPRVM")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "_LDDRPRVA")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "LDNRSPH")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "LDNRCYL")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "LDNRAX")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "_NEWVBOX")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "_NEWHBOX")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else if (key == "_RESIZEMODE")
                    {
                        order.SetValue(key + ".R", (decimal)valR);
                        order.SetValue(key + ".L", (decimal)valL);
                    }
                    else
                    {
                        key = "_VCA." + key;
                        int cnt = 0;
                        while (order.ContainsKey(key + ((cnt == 0) ? "" : ("." + cnt.ToString()))))
                            cnt++;
                        order.SetValue(key + ((cnt == 0) ? "" : ("." + cnt.ToString())), (string)valB);
                    }
                    #endregion
                }

                if (omaShapeString.Length > 0)
                    order.SetValue("_OMASTD", (string)omaShapeString);

                dictionary.SetValue("PairOrders", orders);
            }
            catch (Exception ex)
            {
                
            }
            return counter;
        }
    }
}
