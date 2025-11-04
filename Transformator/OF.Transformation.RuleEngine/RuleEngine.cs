// ***********************************************************************
// Assembly         : OF.Transformation.RuleEngine
// Author           : Peter Kraus
// Created          : 03-31-2020
// ***********************************************************************
// <summary>RuleEngine.cs
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
using System.CodeDom.Compiler;
using System.Diagnostics;
using Microsoft.CSharp;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Data.SqlClient;

/// <summary>
/// The RuleEngine namespace.
/// </summary>
namespace OF.Transformation.RuleEngine
{
    /// <summary>
    /// Class that sets up Rule Engine
    /// </summary>
    public sealed class RuleEngine : MBActivity, IOFModule
    {
        /// <summary>
        /// The m identifier
        /// </summary>
        private readonly Guid mID = new Guid("17BDCF91-72A1-4B98-BCC3-61C9FCF1AEC0");
        /// <summary>
        /// The m name
        /// </summary>
        private readonly string mName = "RuleEngine";
        /// <summary>
        /// The m description
        /// </summary>
        private readonly string mDescription = "Apply rules on dictionary";

        /// <summary>
        /// The nfi
        /// </summary>
        private System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleEngine"/> class.
        /// </summary>
        public RuleEngine()
        {
            PerfCount.Name = Name;
            nfi.NumberDecimalSeparator = ".";
        }

        /// <summary>
        /// The ID of this Activity is 17BDCF91-72A1-4B98-BCC3-61C9FCF1AEC0
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
        /// The wfid
        /// </summary>
        private string wFID;
        /// <summary>
        /// The script globals
        /// </summary>
        private ScriptGlobals scriptGlobals = new ScriptGlobals();

        /// <summary>
        /// Input parameter: The rule file
        /// </summary>
        /// <value>The rule file.</value>
        public InArgument<string> RuleFile { get; set; }
        /// <summary>
        /// Input parameter: Semicolon separated list of mapping file names
        /// </summary>
        /// <value>The mapping files.</value>
        public InArgument<string> MappingFiles { get; set; }
        /// <summary>
        /// Gets or sets the wfid.
        /// </summary>
        /// <value>The wfid.</value>
        public InArgument<string> WFID { get; set; }
        /// <summary>
        /// Gets or sets the connection string for SQL Mapping Tables.
        /// </summary>
        /// <value>
        /// The connection string for SQL Mapping Tables.
        /// </value>
        public InArgument<string> ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the script object.
        /// </summary>
        /// <value>The script object.</value>
        public InOutArgument<ScriptStore> ScriptObj { get;  set; }

        /// <summary>
        /// Dictionary as input and output parameter
        /// </summary>
        /// <value>The dictionary.</value>
        public InOutArgument<OF.Module.OrderDictionary> Dicti { get; set; }

        private OF.Module.OrderDictionary dicti = null;

        /*
         * Rule example:
         * 
         * if (!(dicti.GetDecimal("OCHT", 0).GetValueOrDefault(0m) > 0m) && dicti.GetDecimal("VBOX", 0).GetValueOrDefault(0m) > 0m)
         *       dicti.SetDecimal("OCHT", dicti.GetDecimal("VBOX", 0) / 2m, 0);
         */


        /// <summary>
        /// Execution of activity
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void Execute(NativeActivityContext context)
        {
            PerfCount.StartJob();
            wFID = WFID.Get(context);

            string ruleFile = RuleFile.Get(context);

            try
            {
                LogInfo("Rule engine started", 101, wFID);
                System.Diagnostics.Stopwatch sw = new Stopwatch();
                sw.Start();

                dicti = Dicti.Get(context);

                if (ruleFile == null)
                {
                    Dicti.Set(context, dicti);
                    Result.Set(context, 1);
                    LogInfo("Rule engine - nothing to do - finished", 102, wFID);
                    PerfCount.EndJob(1);

                    return;
                }

                //Microsoft.CodeAnalysis.Scripting.Script<object>
                ScriptStore scriptObj = ScriptObj.Get(context);
                if (scriptObj == null)
                {
                    List<string> mappingFiles = new List<string>();

                    string _mappingFiles = MappingFiles.Get(context);
                    if (!string.IsNullOrEmpty(_mappingFiles))
                        mappingFiles = _mappingFiles.Split(new char[] { ';' }).ToList();

                    scriptObj = LoadScript(ruleFile, mappingFiles, context);
                    ScriptObj.Set(context, scriptObj);
                    System.Diagnostics.Debug.WriteLine("SW1=" + sw.ElapsedMilliseconds.ToString());
                }

                if (scriptObj.ScriptObject != null)
                {
                    scriptGlobals.Dicti = dicti;
                    object result = scriptObj.ScriptObject.RunAsync(scriptGlobals).Wait(-1);// 100000);
                    System.Diagnostics.Debug.WriteLine("SW2=" + sw.ElapsedMilliseconds.ToString());
                }

                //ScriptObj.Set(context, scriptObj);
                Dicti.Set(context, dicti);
                Result.Set(context, 1);
            }
            catch (System.Exception ex)
            {
                LogError(ex.ToString(), -101, wFID);
            }

            log4net.ThreadContext.Properties["Job"] = "";
            LogInfo("Rule engine finished", 102, wFID);
            PerfCount.EndJob(1);
        }

        /// <summary>
        /// Converts the string to SQL XML.
        /// </summary>
        /// <param name="xmlData">The XML data.</param>
        /// <returns>SqlXml.</returns>
        private static SqlXml ConvertString2SqlXml(string xmlData)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            MemoryStream m = new MemoryStream(encoding.GetBytes(xmlData));
            return new SqlXml(m);
        }
        /// <summary>
        /// Loads the script.
        /// </summary>
        /// <param name="ruleFile">The rule file.</param>
        /// <param name="mappingFiles">The mapping files.</param>
        /// <returns>ScriptStore.</returns>
        /// <exception cref="OptiFlowException">Can't load rule from file " + ruleFile</exception>
        /// <exception cref="OptiFlowException">Error while loading mapping from file " + actMappingFile</exception>
        /// <exception cref="OptiFlowException">Compilation error in rule file " + ruleFile</exception>
        /// <exception cref="OptiFlowException">Can't load rule file " + ruleFile</exception>
        private ScriptStore LoadScript (string ruleFile, List<string> mappingFiles, NativeActivityContext context)
        {
            LogInfo("Start loading rule", 1200, wFID);

            // Try to read rule file
            string ruleSource = "";
            try
            {
                ruleSource = System.IO.File.ReadAllText(ruleFile, Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                // if a given script can't be loaded, the whole workflow shall terminate as we can't use any rules
                LogError("Can't load rule from file " + ruleFile, 1, wFID);
                throw new OptiFlowException("Can't load rule from file " + ruleFile, ex) { Source = wFID };
                
            }
            LogInfo("Rule from file " + ruleFile + " loaded", 1201, wFID);

            // How to generate XML files with SQL Data:
            // SELECT * FROM[BevelTypes] FOR XML AUTO, ROOT('BevelType'), XMLSCHEMA
            // SELECT * FROM[BevelTypes] FOR XML RAW, ROOT('BevelType'), XMLSCHEMA

            // Try to load mapping files
            Dictionary<string, EnumerableRowCollection<DataRow>> mapping = new Dictionary<string, EnumerableRowCollection<DataRow>>();
            string actMappingFile = "";
            try
            {
                foreach (string mappingFile in mappingFiles)
                {
                    actMappingFile = mappingFile;
                    if (mappingFile.ToUpper().EndsWith(".SQL"))
                        ReadMappingFromSQL(mapping, mappingFile.Replace(".SQL",""), context);
                    else
                        ReadMappingFromFile(mapping, mappingFile);

                }
            }
            catch (System.Exception ex)
            {
                // if given mappings can't be loaded, the whole workflow shall terminate 
                LogError("Error while loading mapping from file " + actMappingFile, 1, wFID);
                throw new OptiFlowException("Error while loading mapping from file " + actMappingFile, ex) { Source = wFID };
            }

            Microsoft.CodeAnalysis.Scripting.Script<object> scriptObj = null;
            ScriptStore actStore = new ScriptStore();

            // pre compile code
            try
            {
                var encoding = Encoding.UTF8;
                scriptGlobals.Mapping = mapping;
                
                if (!string.IsNullOrEmpty(ruleSource))
                {
                    //scriptObj = CSharpScript.Create(ruleSource,
                    Script<object> so = CSharpScript.Create(ruleSource,
                        ScriptOptions.Default
                        .WithImports("OF.Module", "System", "System.Linq", "System.Data", "System.Data.SqlTypes", "System.Runtime.InteropServices", "System.Text", "System.Collections.Generic", "System.Text.RegularExpressions")
                        .WithEmitDebugInformation(true) // un-comment this for debugging
                        .WithFilePath(ruleFile) // un-comment this for debugging
                        .WithFileEncoding(encoding)
                        .AddReferences(
                            Assembly.GetAssembly(typeof(OF.Module.OrderDictionary)),
                            typeof(System.Data.DataSet).Assembly,
                            typeof(System.Data.DataRowExtensions).Assembly
                        ),
                    typeof(ScriptGlobals));


                    actStore.ScriptObject = so;

                    LogInfo("Script loaded", 1203, wFID);
                }
            }
            catch (CompilationErrorException e1)
            {
                LogError("Compilation error in rule file " + ruleFile, 1, wFID);
                throw new OptiFlowException("Compilation error in rule file " + ruleFile, e1) { JobID = "", MessageID = "" };
            }
            catch (System.IO.FileLoadException ex)
            {
                LogError("Can't load rule dependencies: " + ex.ToString(), 1, wFID);
                throw new OptiFlowException("Can't load rule dependencies: " + ex.ToString(), ex) { JobID = "", MessageID = "" };
            }
            catch (System.IO.FileNotFoundException ex)
            {
                LogError("Can't load rule dependencies: " + ex.ToString(), 1, wFID);
                throw new OptiFlowException("Can't load rule dependencies: " + ex.ToString(), ex) { JobID = "", MessageID = "" };
            }
            catch (System.Exception ex)
            {
                LogError("Can't load rule from file " + ruleFile, 1, wFID);
                throw new OptiFlowException("Can't load rule file " + ruleFile, ex) { JobID = "", MessageID = "" };
            }

            return actStore;// scriptObj;
        }

        /// <summary>
        /// Reads mapping data from a file into the mapping dictionary.
        /// </summary>
        /// <param name="mapping">The dictionary to store the mapping data.</param>
        /// <param name="mappingFile">The path to the mapping file.</param>
        private void ReadMappingFromFile(Dictionary<string, EnumerableRowCollection<DataRow>> mapping, string mappingFile)
        {
            string xmltxt = System.IO.File.ReadAllText(mappingFile);
            System.Data.SqlTypes.SqlXml sqlXml = ConvertString2SqlXml(xmltxt);
            DataSet ds = new DataSet();
            ds.ReadXml(sqlXml.CreateReader());

            if (ds.Tables.Count == 1)
            {
                DataTable myDataTable = ds.Tables[0];
                if (!mapping.ContainsKey(myDataTable.TableName))
                    mapping.Add(myDataTable.TableName, myDataTable.AsEnumerable());
                // Usage examples:
                // int erg = Mapping["BevelTypes"].Where(myRow => myRow.Field<string>("Name").Contains("ve")).Single().Field<int>("ID");
                // int erg = Mapping["BevelTypes"].Where(myRow => myRow.Field<string>("Name").Contains("ve")).First().Field<int>("ID");
                // int erg = Mapping["BevelTypes"].Where(myRow => myRow.Field<string>("Name").Contains("ve")).First().Field<int>("ID");
                // int erg = Mapping["BevelTypes"].Where(myRow => myRow.Field<string>("Name").Contains("ve")).FirstOrDefault().Field<int>("ID");
                LogInfo("Mapping " + myDataTable.TableName + " from file " + mappingFile + " loaded", 1202, wFID);
            }
        }

        /// <summary>
        /// Reads mapping data from a SQL database table into the mapping dictionary.
        /// </summary>
        /// <param name="mapping">The dictionary to store the mapping data.</param>
        /// <param name="mappingTable">The name of the SQL table containing the mapping data.</param>
        /// <param name="context">The native activity context.</param>
        private void ReadMappingFromSQL(Dictionary<string, EnumerableRowCollection<DataRow>> mapping, string mappingTable, NativeActivityContext context)
        {
            DataTable myDataTable = new DataTable();
            string connString = ConnectionString.Get(context);
            string query = "select * from " + mappingTable;

            SqlConnection conn = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();

            // create data adapter
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(myDataTable);
            conn.Close();
            da.Dispose();
            myDataTable.TableName = mappingTable;

            if (!mapping.ContainsKey(myDataTable.TableName))
                mapping.Add(myDataTable.TableName, myDataTable.AsEnumerable());
            // Usage examples:
            // int erg = Mapping["BevelTypes"].Where(myRow => myRow.Field<string>("Name").Contains("ve")).Single().Field<int>("ID");
            // int erg = Mapping["BevelTypes"].Where(myRow => myRow.Field<string>("Name").Contains("ve")).First().Field<int>("ID");
            // int erg = Mapping["BevelTypes"].Where(myRow => myRow.Field<string>("Name").Contains("ve")).First().Field<int>("ID");
            // int erg = Mapping["BevelTypes"].Where(myRow => myRow.Field<string>("Name").Contains("ve")).FirstOrDefault().Field<int>("ID");
            LogInfo("Mapping " + myDataTable.TableName + " from SQL DB loaded", -1, wFID);

        }


        /// <summary>
        /// Registers the metadata.
        /// </summary>
        public static void RegisterMetadata()
       {
       }
    }

    /// <summary>
    /// Class ScriptGlobals.
    /// </summary>
    public class ScriptGlobals
    {
        /// <summary>
        /// The job as OrderDictionary 
        /// </summary>
        public OF.Module.OrderDictionary Dicti;
        /// <summary>
        /// The mapping
        /// </summary>
        public Dictionary<string, EnumerableRowCollection<DataRow>> Mapping;
    }

    /// <summary>
    /// Class ScriptStore.
    /// </summary>
    public class ScriptStore
    {
        /// <summary>
        /// The script object
        /// </summary>
        public Microsoft.CodeAnalysis.Scripting.Script<object> ScriptObject = null;
    }
}
