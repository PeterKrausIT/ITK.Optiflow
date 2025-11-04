// ***********************************************************************
// Assembly         : UnitTest.Transformator.RuleEngine
// Author           : Peter Kraus
// Created          : 11-04-2025
// ***********************************************************************
// <summary>UnitTest.Transformator.RuleEngine.cs
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
using System.Activities;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OF.Module;

namespace UnitTestRuleEngine
{
    /// <summary>
    /// Unit tests for the RuleEngine component.
    /// </summary>
    [TestClass]
    public class RuleEngineTest
    {
        /// <summary>
        /// Tests the RuleEngine functionality with a sample order dictionary and rule script.
        /// </summary>
        [TestMethod]
        public void TestMethodRuleEngine()
        {
            #region OrderDictionary
            OF.Module.OrderDictionary Orders = new OF.Module.OrderDictionary();
            OF.Module.OrderDictionary[] OrderArray = new OF.Module.OrderDictionary[1];
            OF.Module.OrderDictionary Order = new OF.Module.OrderDictionary();
            OrderArray[0] = Order;
            Orders.Add("PairOrders", OrderArray);

            Order.SetValue("LAB", "OptiFlow");
            Order.SetValue("ACCN", (int)6969);
            Order.SetValue("FCOL", "BLACK");
            Order.SetValue("DBL", (decimal)42.0);
            Order.SetValue("EYESIZ", (int)17);
            Order.SetValue("FTYP", (int)1);
            Order.SetValue("FMFR", "Manu");
            Order.SetValue("FMAT", "Metal");
            Order.SetValue("FRAM", "FrameName");
            Order.SetValue("POLISH", (bool)true);
            Order.SetValue("ETYP", (int)3);
            Order.SetValue("ADD.L", "1.00");
            Order.SetValue("AX.L", "13");
            Order.SetValue("CYL.L", (decimal)-2.25);
            Order.SetValue("THKP.L", (decimal)1.0);
            Order.SetValue("OCHT.L", (decimal)0.0);
            Order.SetValue("IPD.L", (decimal)42.0);
            Order.SetValue("NPD.L", (decimal)39.5);
            Order.SetValue("BEVM.L", (decimal)1.1);
            Order.SetValue("BEVP.L", (int)7);
            Order.SetValue("PRVM.L", (decimal)2.25);
            Order.SetValue("PRVA.L", (decimal)2.35);
            Order.SetValue("CRIB.L", (int)69);
            Order.SetValue("CTHICK.L", (decimal)0.8);
            Order.SetValue("SGOCIN.L", (decimal)2.00);
            Order.SetValue("FRNT.L", (decimal)7.90);
            Order.SetValue("CORRLEN.L", (decimal)15);
            Order.SetValue("BVD.L", (decimal)3.5);
            Order.SetValue("SPH.L", (decimal)1.00);
            Order.SetValue("ACOAT.L", "HVLL");
            Order.SetValue("COLR", "BLUE");
            Order.SetValue("HBOX.L", (decimal)50.0);
            Order.SetValue("VBOX.L", (decimal)40.2);
            Order.SetValue("DBL", (decimal)42.0);
            Order.SetValue("FED", (int)65);
            Order.SetValue("EYESIZ", (int)17);
            Order.SetValue("FTYP", (int)1);
            Order.SetValue("CLIENT", "John Doe");
            Order.SetValue("ADD.R", (decimal)1.00);
            Order.SetValue("AX.R", (decimal)134);
            Order.SetValue("CYL.R", (decimal)-3.25);
            Order.SetValue("ETYP", (int)3);
            Order.SetValue("THKP.R", (decimal)1.5);
            Order.SetValue("OCHT.R", (decimal)19.0);
            Order.SetValue("IPD.R", (decimal)32.0);
            Order.SetValue("NPD.R", (decimal)29.5);
            Order.SetValue("BEVM.R", (decimal)1.1); //TODO: Im Code decimal, in xls String
            Order.SetValue("BEVP.R", (int)7);
            Order.SetValue("PRVM.R", (decimal)2.25);
            Order.SetValue("PRVA.R", (decimal)2.35);
            Order.SetValue("CRIB.R", (int)75);
            Order.SetValue("CTHICK.R", (decimal)0.9);
            Order.SetValue("SGOCIN.R", (decimal)2.30);
            Order.SetValue("FRNT.R", (decimal)8.2);
            Order.SetValue("CORRLEN.R", (decimal)17);
            Order.SetValue("BVD.R", (decimal)3.5);
            Order.SetValue("SPH.R", (decimal)1.00);
            Order.SetValue("ACOAT.R", "HVLL");
            Order.SetValue("COLR", "BLUE");
            Order.SetValue("HBOX.R", (decimal)54.0);
            Order.SetValue("VBOX.R", (decimal)39.0);
            Order.SetValue("DESTID", (string)"000001");
            Order.SetValue("NWD", (decimal)42.0);
            Order.SetValue("REM", "Priority!");
            Order.SetValue("ENGMARK.TXT.R", "GravurR");
            Order.SetValue("ENGMARK.TXT.L", "GravurL");
            Order.SetValue("ZTILT", (decimal)18.0);
            Order.SetValue("PANTO", (decimal)22.2);
            Order.SetValue("FUPC", "72527273070");
            Order.SetValue("FCRV", (decimal)40);
            #endregion

            string Rule = "\r\n";
            Rule += "if (Dicti.ContainsKey(\"PairOrders\") && Dicti[\"PairOrders\"] is OrderDictionary[])" + "\r\n";
            Rule += "{" + "\r\n";
            Rule += "   foreach (OrderDictionary order in (Dicti[\"PairOrders\"] as OrderDictionary[]))" + "\r\n";
            Rule += "   {" + "\r\n";
            Rule += "       if (order.ContainsKey(\"OCHT.L\") && order[\"OCHT.L\"] is decimal)" + "\r\n";
            Rule += "       {" + "\r\n";
            Rule += "           if ((decimal)order[\"OCHT.L\"] == 0m)" + "\r\n";
            Rule += "           {" + "\r\n";
            Rule += "               if (order.ContainsKey(\"VBOX.L\") && order[\"VBOX.L\"] is decimal)" + "\r\n";
            Rule += "               {" + "\r\n";
            Rule += "                   order[\"OCHT.L\"] = (decimal)order[\"VBOX.L\"] / 2m;" + "\r\n";
            Rule += "               }" + "\r\n";
            Rule += "                  else throw new System.Exception(\"Script error 3\");" + "\r\n";
            Rule += "           }" + "\r\n";
            Rule += "       }" + "\r\n";
            Rule += "       else throw new System.Exception(\"Script error 2\");" + "\r\n";
            Rule += "   }" + "\r\n";
            Rule += "}" + "\r\n";
            Rule += "else throw new System.Exception(\"Script error 1\");" + "\r\n";
            
            System.IO.File.WriteAllText(@"RuleFile.cs", Rule);

            string MappingFile = "";
            System.IO.File.WriteAllText(@"MappingFile.cs", MappingFile);

            OF.Transformation.RuleEngine.ScriptStore ScriptStore = null;
            OF.Transformation.RuleEngine.RuleEngine RuleEngine = new OF.Transformation.RuleEngine.RuleEngine();

            var input = new Dictionary<string, object>
            {
                {"Dicti", Orders },
                {"RuleFile", "RuleFile.cs" },
                {"MappingFiles", "MappingFile.cs" },
                {"ScriptObj", ScriptStore }
            };

            IDictionary<string, object> Res = new Dictionary<string, object>
            {
                {"Dicti", "" }
            };

            var output = WorkflowInvoker.Invoke<int>(RuleEngine, input, out Res, new TimeSpan(0, 0, 10));


        }
    }
}
