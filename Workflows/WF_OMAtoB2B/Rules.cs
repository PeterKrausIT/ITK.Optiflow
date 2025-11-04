// ***********************************************************************
// Assembly         : WF_OMAtoB2B
// Author           : Peter Kraus
// Created          : 11-04-2025
// ***********************************************************************
// <summary>Rules.cs
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
#define DESIGNTIMEx

#if DESIGNTIME
//------------- DESIGN TIME CODE ----------------
// the next lines are not part of the rule file. They are just there to make the .CS valid in design time...
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OF.Module;
namespace TEMPO
{
    class CTempo
    {
        private void tempo()
        {
            OF.Module.OrderDictionary Dicti = null;
            // ------------- End of design time code -------------------------------------------------------------------
#endif


            // Rules for PairOrders....

            if (Dicti.ContainsKey("PairOrders"))
            {
                foreach (OrderDictionary od in (Dicti["PairOrders"] as OrderDictionary[]))
                {

                    try
                    {
                        // JobID for logging
                        if (od.ContainsKey("JOB"))
                            log4net.ThreadContext.Properties["Job"] = od["JOB"].ToString();
                        else
                            log4net.ThreadContext.Properties["Job"] = "";


                        // copy ACCN to CUSTOMERID:
                        if (od.ContainsKey("ACCN"))
                            od.SetValue("CUSTOMERID", od["ACCN"]);

                        // translate _VCA._RECTYPE to _EDGINGTYPE
                        // NONE, ROUGHING, ONSHAPE, GIVENFRAME, ORDEREDFRAME
                        string edgingtype = "NONE";
                        if (od.ContainsKey("_VCA._RECTYPE"))
                        {
                            string rectype = od["_VCA._RECTYPE"].ToString().ToUpper();
                            switch (rectype)
                            {
                                case "ONSHAPE":
                                    edgingtype = "ONSHAPE";
                                    break;
                                case "G":
                                    edgingtype = "GIVENFRAME";
                                    break;
                                case "S":
                                    edgingtype = "ORDEREDFRAME";
                                    break;
                                default:
                                    edgingtype = "NONE";
                                    break;
                            }
                        }
                        od.SetValue("_EDGINGTYPE", edgingtype);


                    }
                    catch (System.Exception exx)
                                {
                                    System.Console.WriteLine(exx.ToString());
                                    od.SetValue("_ERROR", exx.ToString());
                                    od.SetValue("_SKIPJOB", true);
                                    MBActivity.LogError(exx.ToString(), 1801, "B2BtoD365");
                                }

                    log4net.ThreadContext.Properties["Job"] = "";
                    Dicti.SetValue("_RuleEngine", "executed");
                }
            }
#if DESIGNTIME
            //------------- DESIGN TIME CODE ----------------
        }
    }
}
//------------- END OF DESIGN TIME CODE ---------
#endif