// ***********************************************************************
// Assembly         : OF.Generator.B2BOrder
// Author           : Peter Kraus
// Created          : 04-14-2025
// ***********************************************************************
// <summary>CB2BOrder.cs
// Generates B2B Order responds from OrderDictionary
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
using OF.Module;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;

/// <summary>
/// The B2BOrder namespace.
/// </summary>
namespace OF.Generator.B2BOrder
{
    /// <summary>
    /// Class CB2BOrder. This class cannot be inherited.
    /// Implements the <see cref="OF.Module.MBActivity" />
    /// Implements the <see cref="OF.Module.IOFModule" />
    /// </summary>
    /// <seealso cref="OF.Module.MBActivity" />
    /// <seealso cref="OF.Module.IOFModule" />
    public sealed class CB2BOrder : MBActivity, IOFModule
    {
        /// <summary>
        /// The m identifier
        /// </summary>
        private readonly Guid mID = new Guid("5A2D1F91-7F6B-42A8-BCC3-66296CF0AEC2");
        /// <summary>
        /// The m name
        /// </summary>
        private readonly string mName = "B2BOptic Order Generator";
        /// <summary>
        /// The m description
        /// </summary>
        private readonly string mDescription = "Build B2BOptic Order Xml";

        /// <summary>
        /// The ID of this Activity 
        /// </summary>
        /// <value>The identifier.</value>
        public Guid ID
        {
            get { return mID; }
        }

        /// <summary>
        /// Name of activity
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
        /// The nfi
        /// </summary>
        private NumberFormatInfo nfi = new NumberFormatInfo();

        /// <summary>
        /// Initializes a new instance of the <see cref="CB2BOrder"/> class.
        /// </summary>
        public CB2BOrder()
        {
            PerfCount.Name = Name;
        }

        /// <summary>
        /// The wfid
        /// </summary>
        private string wFID;
        /// <summary>
        /// The job count
        /// </summary>
        private int jobCnt = 0;

        /// <summary>
        /// Gets or sets the wfid.
        /// </summary>
        /// <value>The wfid.</value>
        public InArgument<string> WFID { get; set; }
        /// <summary>
        /// OrderDictionary as input
        /// </summary>
        /// <value>The OrderDictionary.</value>
        public InArgument<OrderDictionary> Order { get; set; }

        /// <summary>
        /// OrderResponds as output
        /// </summary>
        /// <value>The order responds.</value>
        public OutArgument<string> OrderXML { get; set; }

        /// <summary>
        /// Executes the Activity
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="OptiFlowException">At least one order needs to be defined, please check workflow and input data - null</exception>
        protected override void Execute(NativeActivityContext context)
        {
            PerfCount.StartJob();

            wFID = WFID.Get(context);
            LogInfo("B2BOrderGenerator started", 1, wFID);
            OrderDictionary dicti = Order.Get(context);
            if (dicti == null)
            {
                LogError("At least one order needs to be defined, please check workflow and input data", 1, wFID);
                throw new OptiFlowException("At least one order needs to be defined, please check workflow and input data", null) { WorkflowID = wFID };
            }

            string rsp = "";

            try
            {
                if (dicti.ContainsKey("PairOrders") && dicti["PairOrders"] is OrderDictionary[])
                {
                    OrderDictionary[] orders = dicti["PairOrders"] as OrderDictionary[];

                    if (orders.Length > 0)
                        rsp = GenerateB2BOpticXml(orders[0]);
                }
            }
            catch (System.Exception ex)
            {
                LogError(ex, 805, wFID);
            }


            log4net.ThreadContext.Properties["Job"] = null;
            OrderXML.Set(context, rsp);
            PerfCount.EndJob(jobCnt);
        }


        /// <summary>
        /// Generates B2BOptic XML from the order dictionary.
        /// </summary>
        /// <param name="dict">The order dictionary containing order data.</param>
        /// <returns>The generated B2BOptic XML as a string.</returns>
        private string GenerateB2BOpticXml(Dictionary<string, object> dict)
        {
            var b2b = new XElement("b2bOptic",
                GenerateHeader(dict),
                new XElement("items",
                    new XElement("item",
                        ElemIf("referenceNo", Get(dict, "REFERENCENO")),
                        ElemIf("referenceText", Get(dict, "REFERENCETEXT")),
                        GeneratePair(dict)
                    )
                )
            );

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), b2b);
            using (var sw = new StringWriter())
            {
                doc.Save(sw);
                return sw.ToString();
            }
        }

        /// <summary>
        /// Generates the header element for B2BOptic XML.
        /// </summary>
        /// <param name="dict">The order dictionary containing header data.</param>
        /// <returns>The header XElement.</returns>
        private XElement GenerateHeader(Dictionary<string, object> dict)
        {
            var header = new XElement("header", new XAttribute("msgType", "ORDER"));

            // customersOrderId
            var coid = ElemIf("customersOrderId", Get(dict, "CUSTOMERORDERID"));
            if (coid != null) header.Add(coid);

            // distributorsOrderId
            var doid = ElemIf("distributorsOrderId", Get(dict, "SUPPLIERORDERID"));
            if (doid != null) header.Add(doid);

            // timeStamps -> dateTime step=CREATE
            var dateTimeValue = ToIsoDate(dict, "ORDERDATE");
            if (!string.IsNullOrWhiteSpace(dateTimeValue))
            {
                var dt = new XElement("dateTime", new XAttribute("step", "CREATE"), dateTimeValue);
                header.Add(new XElement("timeStamps", dt));
            }

            // orderParties ORIGINATOR
            var originator = new XElement("orderParties", new XAttribute("role", "ORIGINATOR"));
            var originId = ElemIf("id", Get(dict, "CUSTOMERID"));
            if (originId != null) originator.Add(originId);
            if (originator.HasElements) header.Add(originator);

            // orderParties SUPPLIER
            var supplier = new XElement("orderParties", new XAttribute("role", "SUPPLIER"));
            var supplierId = ElemIf("id", Get(dict, "SUPPLIERID"));
            if (supplierId != null) supplier.Add(supplierId);
            if (supplier.HasElements) header.Add(supplier);

            // orderParties SHIPTO
            var shipto = new XElement("orderParties", new XAttribute("role", "SHIPTO"));
            var shipId = ElemIf("id", Get(dict, "DELIVERYID"));
            if (shipId != null) shipto.Add(shipId);
            if (shipto.HasElements) header.Add(shipto);

            // orderParties PATIENT
            var patient = new XElement("orderParties", new XAttribute("role", "PATIENT"));
            var pname = ElemIf("name", Get(dict, "CLIENT"));
            if (pname != null) patient.Add(pname);
            var pgender = ElemIf("gender", Get(dict, "PATIENTGENDER"));
            if (pgender != null) patient.Add(pgender);
            var pbirth = ElemIf("birthDate", ToIsoDate(dict, "PATIENTBIRTH"));
            if (pbirth != null) patient.Add(pbirth);
            if (patient.HasElements) header.Add(patient);

            // software (static, always present)
            header.Add(new XElement("software",
                new XAttribute("typeOf", "SENDER"),
                new XElement("name", "OptiFlow"),
                new XElement("version", "1.0")
            ));

            // productCatalog
            var pcName = Get(dict, "CATALOG");
            var pc = new XElement("productCatalog");
            var pcNameEl = ElemIf("name", pcName);
            if (pcNameEl != null) pc.Add(pcNameEl);
            pc.Add(ElemIf("release", "0"));
            if (pc.HasElements) header.Add(pc);

            // portalOrderId
            var portal = ElemIf("portalOrderId", Get(dict, "PORTALORDERID"));
            if (portal != null) header.Add(portal);

            return header;
        }

        /// <summary>
        /// Generates the pair element for B2BOptic XML.
        /// </summary>
        /// <param name="dict">The order dictionary containing pair data.</param>
        /// <returns>The pair XElement.</returns>
        private XElement GeneratePair(Dictionary<string, object> dict)
        {
            var pair = new XElement("pair");

            foreach (var side in new[] { "RIGHT", "LEFT" })
            {
                var suffix = side == "RIGHT" ? ".R" : ".L";

                // take quantity from dictionary, "DO" where
                // "R" means right quantity 1 and left quantity 0
                // "L" means left quantity 1 and right quantity 0
                // "B" means both quantity 1
                // "0" means both quantity 0
                var qtyCode = Get(dict, "DO");
                var qty = qtyCode == "B" ||
                    (qtyCode == "R" && side == "RIGHT") ||
                    (qtyCode == "L" && side == "LEFT") ? "1" : "0";

                var lens = new XElement("lens",
                    new XAttribute("side", side),
                    new XAttribute("quantity", qty));

                var commercialCode = ElemIf("commercialCode", Get(dict, "LNAM" + suffix));
                if (commercialCode != null) lens.Add(commercialCode);

                // rxData
                XElement rxData = new XElement("rxData");

                var sphere = ElemIf("sphere", ToDecimal(dict, "SPH" + suffix));
                if (sphere != null)
                    rxData.Add(sphere);

                var power = ElemIf("power", ToDecimal(dict, "CYL" + suffix));
                var axis = ElemIf("axis", ToInt(dict, "AX" + suffix));
                XElement cylinder = null;
                if (power != null && axis != null)
                {
                    cylinder = new XElement("cylinder");
                    cylinder.Add(power);
                    cylinder.Add(axis);
                    rxData.Add(cylinder);
                }

                // addition to rxData
                var addition = ElemIf("addition", ToDecimal(dict, "ADD" + suffix));
                if (addition != null) rxData.Add(addition);

                // prism to rxData
                var prism = ElemIf("power", ToDecimal(dict, "PRVM" + suffix));
                var prismBase = ElemIf("base", Get(dict, "PRVA" + suffix));
                XElement prismEl = null;
                if (prism != null && prismBase != null)
                {
                    prismEl = new XElement("prism");
                    prismEl.Add(prism);
                    prismEl.Add(prismBase);
                    rxData.Add(prismEl);
                }

                if (rxData.HasElements)
                    lens.Add(rxData);

                // centration
                XElement centration = null;
                var ipd = ElemIf("monocularCentrationDistance", ToDecimal(dict, "IPD" + suffix));
                if (ipd != null)
                {
                    var ipdEl = new XElement("monocularCentrationDistance", new XAttribute("reference", "FAR"), ipd.Value);
                    // add as XElement with content
                    centration = centration ?? new XElement("centration");
                    centration.Add(ipdEl);
                }
                var heightVal = ToDecimal(dict, "OCHT" + suffix);
                if (!string.IsNullOrWhiteSpace(heightVal))
                {
                    var heightEl = new XElement("height",
                        new XAttribute("referenceHeight", "OVERBOX"),
                        new XAttribute("reference", "FAR"),
                        heightVal);
                    centration = centration ?? new XElement("centration");
                    centration.Add(heightEl);
                }
                if (centration != null) lens.Add(centration);

                var geometry = new XElement("geometry");

                // diameter -> physical
                var dia = ElemIf("physical", ToDecimal(dict, "DIA" + suffix));
                if (dia != null)
                {
                    var diameter = new XElement("diameter");
                    diameter.Add(dia);

                    geometry.Add(diameter);

                }

                // b2boptic → items → item → pair → lens → geometry → thickness
                // from MINEDG with attribute reference=EDGE
                /*
                 <xs:simpleType name="ThicknessReferences">
                  <xs:restriction base="xs:string">
                    <xs:enumeration value="CENTER" />
                    <xs:enumeration value="EDGE" />
                    <xs:enumeration value="DRILLHOLE" />
                  </xs:restriction>
                </xs:simpleType>
                 * */

                var thickness = ElemIf("thickness", ToDecimal(dict, "MINEDG" + suffix));
                if (thickness != null)
                {
                    var thicknessEl = new XElement("thickness", new XAttribute("reference", "EDGE"), thickness.Value);
                    geometry.Add(thicknessEl);
                }


                // just add geometry in case its not empty:
                if (geometry.HasElements) lens.Add(geometry);


                // coating
                var coatingCode = Get(dict, "ACOAT" + suffix);
                if (!string.IsNullOrWhiteSpace(coatingCode))
                {
                    lens.Add(new XElement("coating", new XAttribute("coatingType", "ANTIREFLEX"),
                        new XElement("commercialCode", coatingCode)));
                }

                // tinting
                var colorCode = Get(dict, "TINT" + suffix);
                if (!string.IsNullOrWhiteSpace(colorCode) && (colorCode != "0"))
                {
                    lens.Add(new XElement("coating", new XAttribute("coatingType", "COLOR"),
                        new XElement("commercialCode", colorCode)));
                }

                pair.Add(lens);
            }

            // frame (quantity = 0) - add only if it has any non-empty children beyond quantity attribute
            var frame = new XElement("frame", new XAttribute("quantity", "0"));

            // in FTYP we have (int):
            /*
            0  – Undefined
            1  – Plastic
            2  – Metal
            3  – Rimless
            4  – Optyl
             * ************************************************************/
            // and in
            // b2boptic → items → item → pair → frame → material
            // we need (string):
            /*
             <xs:simpleType name="FrameMaterials">
              <xs:restriction base="xs:string">
                <xs:enumeration value="METAL" />
                <xs:enumeration value="PLASTIC" />
                <xs:enumeration value="OPTYL" />
                <xs:enumeration value="NYLOR" />
                <xs:enumeration value="DRILLED" />
                <xs:enumeration value="SPECIAL" />
                <xs:enumeration value="ORGANICMATERIALS"/>
                <xs:enumeration value="TITAN"/>
              </xs:restriction>
            </xs:simpleType>
             */
            var ftypStr = ToInt(dict, "FTYP");
            string ftyp = "";
            switch (ftypStr)
            {
                case "1":
                    ftyp = "PLASTIC";
                    break;
                case "2":
                    ftyp = "METAL";
                    break;
                case "3":
                    ftyp = "DRILLED";
                    break;
                case "4":
                    ftyp = "OPTYL";
                    break;
                default:
                    ftyp = "";
                    break;
            }
            var material = ElemIf("material", ftyp);
            if (material != null) frame.Add(material);

            // b2boptic → items → item → pair → frame → manufacturer
            // aus FMFR
            var fmfr = ElemIf("manufacturer", Get(dict, "FMFR"));
            if (fmfr != null) frame.Add(fmfr);

            // b2boptic → items → item → pair → frame → commercialCode
            // aus FUPC
            var fupc = ElemIf("commercialCode", Get(dict, "FUPC"));
            if (fupc != null) frame.Add(fupc);

            // shape -> tracerData
            if (dict.ContainsKey("_OMASTD") && dict["_OMASTD"].ToString().Length > 100)
            {
                var tracerType = Get(dict, "_TRACERTYPE");
                var tracerVersion = Get(dict, "_TRACERVERSION");
                var tracerBinary = Get(dict, "_OMASTD");
                var tracerAdjustion = ToDecimal(dict, "_TRACERADJUSTION");


                XElement tracerData = null;
                if (!string.IsNullOrWhiteSpace(tracerType) || !string.IsNullOrWhiteSpace(tracerVersion) ||
                    !string.IsNullOrWhiteSpace(tracerBinary) || !string.IsNullOrWhiteSpace(tracerAdjustion))
                {
                    tracerData = new XElement("tracerData");
                    if (!string.IsNullOrWhiteSpace(tracerType)) tracerData.Add(new XElement("tracerType", tracerType));
                    if (!string.IsNullOrWhiteSpace(tracerVersion)) tracerData.Add(new XElement("tracerVersion", tracerVersion));
                    // tracerBinary als Hex (bin2hex)
                    if (!string.IsNullOrWhiteSpace(tracerBinary))
                    {
                        var hex = ToHex(tracerBinary);
                        tracerData.Add(new XElement("binaries", new XAttribute("format", "OMA3.02"), hex));
                    }
                    if (!string.IsNullOrWhiteSpace(tracerAdjustion)) tracerData.Add(new XElement("adjustion", tracerAdjustion));
                }

                if (tracerData != null)
                {
                    var shape = new XElement("shape");
                    shape.Add(tracerData);
                    frame.Add(shape);
                }
            }

            var boxWidth = ElemIf("boxWidth", ToDecimal(dict, "HBOX.R"));
            if (boxWidth != null) frame.Add(boxWidth);
            var boxHeight = ElemIf("boxHeight", ToDecimal(dict, "VBOX.R"));
            if (boxHeight != null) frame.Add(boxHeight);
            var dbl = ElemIf("distanceBetweenLenses", ToDecimal(dict, "DBL"));
            if (dbl != null) frame.Add(dbl);

            // add frame only if it has content besides the quantity attribute
            if (frame.HasElements) pair.Add(frame);

            // edging
            var edging = new XElement("edging");

            // add edging type
            /*
             <xs:simpleType name="EdgingType">
              <xs:restriction base="xs:string">
                <xs:enumeration value="NONE" />
                <xs:enumeration value="ROUGHING" />
                <xs:enumeration value="ONSHAPE" />
                <xs:enumeration value="GIVENFRAME" />
                <xs:enumeration value="ORDEREDFRAME" />
              </xs:restriction>
            </xs:simpleType>
             * */
            // b2boptic → items → item → pair → edging → edgingType
            // from dictionary we get _EDGINGTYPE (string) with the same values as above
            // provided by rules if needed.
            var edgingType = ElemIf("edgingType", Get(dict, "_EDGINGTYPE"));
            if (edgingType != null)
                edging.Add(edgingType);


            // add bevel type.
            // from schema (b2boptic → items → item → pair → edging → bevel → type):
            /*
            <xs:simpleType name="BevelTypes">
              <xs:restriction base="xs:string">
                <xs:enumeration value="BEVEL" />
                <xs:enumeration value="BEVEL_T" />
                <xs:enumeration value="BEVEL_U" />
                <xs:enumeration value="BEVELSPORT" />
                <xs:enumeration value="FLAT" />
                <xs:enumeration value="GROOVED" />
              </xs:restriction>
            </xs:simpleType>
             */
            // and from dictionary we get:
            // ETYP: (int):
            /*
            -1 - Uncut
            1  – Bevel
            2  – Rimless (“Flat”)
            3  – Groove
            4  – Mini - bevel(a smaller - than - traditional “V”)
            5  – “T” bevel
            */

            var etypStr = ToInt(dict, "ETYP");
            string bevelType = "";
            switch (etypStr)
            {
                case "1":
                    bevelType = "BEVEL";
                    break;
                case "2":
                    bevelType = "FLAT";
                    break;
                case "3":
                    bevelType = "GROOVED";
                    break;
                case "4":
                    bevelType = "BEVEL";  //????
                    break;
                case "5":
                    bevelType = "BEVEL_T";
                    break;
                default:
                    bevelType = "";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(bevelType))
            {
                var bevel = new XElement("bevel",
                    new XElement("type", bevelType)
                );
                edging.Add(bevel);
            }

            // b2boptic → items → item → pair → edging → chamfer
            /****************************************
             <xs:simpleType name="ChamferIntensity">
              <xs:restriction base="xs:string">
                <xs:enumeration value="THIN" />
                <xs:enumeration value="MEDIUM" />
                <xs:enumeration value="LARGE" />
              </xs:restriction>
            </xs:simpleType>
             ****************************************/
            // from dictionary we get PINB (decimal):
            // for now we define 0.0-0.5 = THIN, 0.6-1.0 = MEDIUM, >1.0 = LARGE
            var pinb = GetDecimal(dict, "PINB");
            string chamferIntensity = "";
            if (pinb >= 0.0m && pinb <= 0.5m)
                chamferIntensity = "THIN";
            else if (pinb > 0.5m && pinb <= 1.0m)
                chamferIntensity = "MEDIUM";
            else if (pinb > 1.0m)
                chamferIntensity = "LARGE";
            if (!string.IsNullOrWhiteSpace(chamferIntensity))
            {
                var chamfer = new XElement("chamfer",
                    new XElement("intensity", chamferIntensity)
                );
                edging.Add(chamfer);
            }
            // b2boptic → items → item → pair → edging → polish
            // from dictionary we get POLISH (bool)
            var polishStr = Get(dict, "POLISH");
            if (!string.IsNullOrWhiteSpace(polishStr) && (polishStr.Equals("true", StringComparison.OrdinalIgnoreCase) || polishStr == "1"))
            {
                var polish = new XElement("polish", "true");
                edging.Add(polish);
            }

            if (edging.HasElements)
                pair.Add(edging);

            return pair;
        }

        /// <summary>
        /// Converts a string to hexadecimal representation.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <returns>The hexadecimal representation of the input string.</returns>
        private static string ToHex(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            byte[] bytes;
            try
            {
                // Falls der String Base64 ist, als solche dekodieren
                bytes = Convert.FromBase64String(input);
            }
            catch
            {
                // sonst als UTF-8-Bytes behandeln
                bytes = Encoding.UTF8.GetBytes(input);
            }

            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// Creates an XElement with the specified name and value if the value is not null or whitespace.
        /// </summary>
        /// <param name="name">The element name.</param>
        /// <param name="value">The element value.</param>
        /// <returns>An XElement if value is not null or whitespace; otherwise, null.</returns>
        private static XElement ElemIf(string name, string value) =>
            string.IsNullOrWhiteSpace(value) ? null : new XElement(name, value);

        /// <summary>
        /// Creates an XAttribute with the specified name and value if the value is not null or whitespace.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>An XAttribute if value is not null or whitespace; otherwise, null.</returns>
        private static XAttribute AttrIf(string name, string value) =>
            string.IsNullOrWhiteSpace(value) ? null : new XAttribute(name, value);

        /// <summary>
        /// Gets a string value from the dictionary for the specified key.
        /// </summary>
        /// <param name="dict">The dictionary to retrieve the value from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The string value if found; otherwise, an empty string.</returns>
        private static string Get(Dictionary<string, object> dict, string key) =>
            dict.TryGetValue(key, out var val) && val != null ? val.ToString() : string.Empty;

        /// <summary>
        /// Gets a decimal value from the dictionary for the specified key.
        /// </summary>
        /// <param name="dict">The dictionary to retrieve the value from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The decimal value if found and parseable; otherwise, 0.</returns>
        private static decimal GetDecimal(Dictionary<string, object> dict, string key) =>
            decimal.TryParse(Get(dict, key), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0m;

        /// <summary>
        /// Converts a date value from the dictionary to ISO 8601 format.
        /// </summary>
        /// <param name="dict">The dictionary to retrieve the value from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The ISO 8601 formatted date string if parseable; otherwise, an empty string.</returns>
        private static string ToIsoDate(Dictionary<string, object> dict, string key) =>
            DateTime.TryParse(Get(dict, key), out var dt) ? dt.ToString("s") : string.Empty;

        /// <summary>
        /// Converts a decimal value from the dictionary to a string using invariant culture.
        /// </summary>
        /// <param name="dict">The dictionary to retrieve the value from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The decimal value as a string if parseable; otherwise, an empty string.</returns>
        private static string ToDecimal(Dictionary<string, object> dict, string key) =>
            decimal.TryParse(Get(dict, key), NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val.ToString(CultureInfo.InvariantCulture) : string.Empty;

        /// <summary>
        /// Converts an integer value from the dictionary to a string.
        /// </summary>
        /// <param name="dict">The dictionary to retrieve the value from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The integer value as a string if parseable; otherwise, an empty string.</returns>
        private static string ToInt(Dictionary<string, object> dict, string key) =>
            int.TryParse(Get(dict, key), out var val) ? val.ToString() : string.Empty;
    }

}