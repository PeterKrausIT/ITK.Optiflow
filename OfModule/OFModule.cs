// ***********************************************************************
// Assembly         : OF.Module
// Author           : Peter Kraus
// Created          : 03-23-2020
// ***********************************************************************
// <summary>OFModule.cs
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
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Activities;
using log4net;

/// <summary>
/// The Module namespace.
/// </summary>
namespace OF.Module
{
    /// <summary>
    /// Interface IOFModule
    /// </summary>
    public interface IOFModule
    {
        //void Init_();
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        Guid ID
        {
            get;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name
		{
			get;
		}

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        string Description
		{
			get;
		}        
	}

    /// <summary>
    /// Class MBActivity.
    /// Implements the <see cref="System.Activities.NativeActivity{System.Int32}" />
    /// </summary>
    /// <seealso cref="System.Activities.NativeActivity{System.Int32}" />
    public class MBActivity : NativeActivity<int>
    {
        /// <summary>
        /// The log
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(MBActivity));
        /// <summary>
        /// The m perf count
        /// </summary>
        protected readonly MB_PerfCount mPerfCount = new MB_PerfCount("Anonymous workflow");

        /// <summary>
        /// Gets performance counter
        /// </summary>
        /// <value>The perf count.</value>
        public MB_PerfCount PerfCount
        {
            get { return mPerfCount; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MBActivity"/> class.
        /// </summary>
        public MBActivity()
        {
            
        }

        /// <summary>
        /// Writes new  entry to verbose log
        /// </summary>
        /// <param name="eventtxt">The eventtxt.</param>
        /// <param name="eventid">The eventid.</param>
        /// <param name="wfid">The wfid.</param>
        public static void LogVerbose(object eventtxt, int eventid, string wfid)
        {
            //System.Console.WriteLine(eventtxt.ToString());
            log.Debug(eventtxt);
        }

        /// <summary>
        /// Writes new  entry to info log
        /// </summary>
        /// <param name="eventtxt">The eventtxt.</param>
        /// <param name="eventid">The eventid.</param>
        /// <param name="wfid">The wfid.</param>
        public static void LogInfo(object eventtxt, int eventid, string wfid)
        {
            //System.Console.WriteLine(eventtxt.ToString());
            log.Info(eventtxt);
        }

        /// <summary>
        /// Writes new  entry to error log
        /// </summary>
        /// <param name="eventtxt">The eventtxt.</param>
        /// <param name="eventid">The eventid.</param>
        /// <param name="wfid">The wfid.</param>
        public static void LogError(object eventtxt, int eventid, string wfid)
        {
            System.Console.WriteLine(eventtxt.ToString());
            log.Error(eventtxt);
        }

        /// <summary>
        /// When implemented in a derived class, performs the execution logic of the activity.
        /// </summary>
        /// <param name="context">The execution context in which the activity is executed.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void Execute(NativeActivityContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class OptiFlowException.
    /// Implements the <see cref="System.Exception" />
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable()]
    public class OptiFlowException : Exception
    {
        /// <summary>
        /// Gets or sets the message identifier.
        /// </summary>
        /// <value>The message identifier.</value>
        public string MessageID{ get; set; }
        /// <summary>
        /// Gets or sets the job identifier.
        /// </summary>
        /// <value>The job identifier.</value>
        public string JobID { get; set; }
        /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        /// <value>The workflow identifier.</value>
        public string WorkflowID { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="OptiFlowException"/> class.
        /// </summary>
        public OptiFlowException()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="OptiFlowException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OptiFlowException(string message) : base (message)
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="OptiFlowException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public OptiFlowException(string message, Exception innerException) : base (message, innerException)
        {
        }
    }


    /// <summary>
    /// Class MB_PerfCount.
    /// </summary>
    [Serializable()]
	public class MB_PerfCount
	{
        /// <summary>
        /// The m name
        /// </summary>
        private string mName = "N.N.";
        /// <summary>
        /// The total operations
        /// </summary>
        public PerformanceCounter _TotalOperations;
        /// <summary>
        /// The counter timer
        /// </summary>
        public PerformanceCounter _CounterTimer;
        /// <summary>
        /// The operations per second
        /// </summary>
        public PerformanceCounter _OperationsPerSecond;
        /// <summary>
        /// The average duration
        /// </summary>
        public PerformanceCounter _AverageDuration;
        /// <summary>
        /// The average duration base
        /// </summary>
        public PerformanceCounter _AverageDurationBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="MB_PerfCount"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MB_PerfCount(string name)
        {
            mName = name;
            //if (!PerformanceCounterCategory.Exists("OptiFlow"))
            //{
            //    CounterCreationDataCollection counters = new CounterCreationDataCollection();

            //    // 1. counter for counting totals: PerformanceCounterType.NumberOfItems32
            //    CounterCreationData totalOps = new CounterCreationData();
            //    totalOps.CounterName = "# jobs executed";
            //    totalOps.CounterHelp = "Total number of operations jobs";
            //    totalOps.CounterType = PerformanceCounterType.NumberOfItems32;
            //    counters.Add(totalOps);

            //    // 2. counter for counting operations per second: PerformanceCounterType.RateOfCountsPerSecond32
            //    CounterCreationData opsPerSecond = new CounterCreationData();
            //    opsPerSecond.CounterName = "# jobs / sec";
            //    opsPerSecond.CounterHelp = "Number of jobs executed per second";
            //    opsPerSecond.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
            //    counters.Add(opsPerSecond);

            //    // 3. counter for counting average time per operation: PerformanceCounterType.AverageTimer32
            //    CounterCreationData avgDuration = new CounterCreationData();
            //    avgDuration.CounterName = "average time per job";
            //    avgDuration.CounterHelp = "Average duration per job execution";
            //    avgDuration.CounterType = PerformanceCounterType.AverageTimer32;
            //    counters.Add(avgDuration);

            //    // 4. base counter for counting average time per operation: PerformanceCounterType.AverageBase
            //    CounterCreationData avgDurationBase = new CounterCreationData();
            //    avgDurationBase.CounterName = "average time per job base";
            //    avgDurationBase.CounterHelp = "Average duration per job execution base";
            //    avgDurationBase.CounterType = PerformanceCounterType.AverageBase;
            //    counters.Add(avgDurationBase);

            //    // 5. counter for active job execution time
            //    CounterCreationData workload = new CounterCreationData();
            //    workload.CounterName = "workload";
            //    workload.CounterHelp = "percentage of job executing time to total time";
            //    workload.CounterType = PerformanceCounterType.CounterTimer;
            //    counters.Add(workload);

            //    // create new category with the counters above
            //    PerformanceCounterCategory.Create("OptiFlow", "Hoya/Seiko Middleware - OptiFlow", PerformanceCounterCategoryType.MultiInstance, counters);

            //    System.Threading.Thread.Sleep(1000);
            //}
            //// create counters to work with
            //_TotalOperations = new PerformanceCounter();
            //_TotalOperations.CategoryName = "OptiFlow";
            //_TotalOperations.CounterName = "# jobs executed";
            //_TotalOperations.MachineName = ".";
            //_TotalOperations.InstanceLifetime = PerformanceCounterInstanceLifetime.Global;
            //_TotalOperations.InstanceName = name;
            //_TotalOperations.ReadOnly = false;
            //_TotalOperations.RawValue = 0;


            //_OperationsPerSecond = new PerformanceCounter();
            //_OperationsPerSecond.CategoryName = "OptiFlow";
            //_OperationsPerSecond.CounterName = "# jobs / sec";
            //_OperationsPerSecond.MachineName = ".";
            //_OperationsPerSecond.InstanceLifetime = PerformanceCounterInstanceLifetime.Global;
            //_OperationsPerSecond.InstanceName = name;
            //_OperationsPerSecond.ReadOnly = false;
            //_OperationsPerSecond.RawValue = 0;


            //_AverageDuration = new PerformanceCounter();
            //_AverageDuration.CategoryName = "OptiFlow";
            //_AverageDuration.CounterName = "average time per job";
            //_AverageDuration.MachineName = ".";
            //_AverageDuration.InstanceLifetime = PerformanceCounterInstanceLifetime.Global;
            //_AverageDuration.InstanceName = name;
            //_AverageDuration.ReadOnly = false;
            //_AverageDuration.RawValue = 0;


            //_AverageDurationBase = new PerformanceCounter();
            //_AverageDurationBase.CategoryName = "OptiFlow";
            //_AverageDurationBase.CounterName = "average time per job base";
            //_AverageDurationBase.MachineName = ".";
            //_AverageDurationBase.InstanceLifetime = PerformanceCounterInstanceLifetime.Global;
            //_AverageDurationBase.InstanceName = name;
            //_AverageDurationBase.ReadOnly = false;
            //_AverageDurationBase.RawValue = 0;


            //_CounterTimer = new PerformanceCounter();
            //_CounterTimer.CategoryName = "OptiFlow";
            //_CounterTimer.CounterName = "workload";
            //_CounterTimer.MachineName = ".";
            //_CounterTimer.InstanceLifetime = PerformanceCounterInstanceLifetime.Global;
            //_CounterTimer.InstanceName = name;
            //_CounterTimer.ReadOnly = false;
            //_CounterTimer.RawValue = 0;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return mName;
            }

            set
            {
                mName = value;
                //_TotalOperations.InstanceName = mName;
                //_OperationsPerSecond.InstanceName = mName;
                //_AverageDuration.InstanceName = mName;
                //_AverageDurationBase.InstanceName = mName;
                //_CounterTimer.InstanceName = mName;
            }
		}

        /// <summary>
        /// The start time
        /// </summary>
        private long startTime = 0;
        /// <summary>
        /// Starts the job.
        /// </summary>
        public void StartJob()
		{
			startTime = Stopwatch.GetTimestamp();
		}

        /// <summary>
        /// Ends the job.
        /// </summary>
        public void EndJob()
        {
            EndJob(1);
        }

        /// <summary>
        /// Ends the job.
        /// </summary>
        /// <param name="NoOfItems">The no of items.</param>
        public void EndJob(int NoOfItems)
		{
			//// simply increment the counters
			//_TotalOperations.IncrementBy((long)NoOfItems);
			//_OperationsPerSecond.IncrementBy((long)NoOfItems);
			//_AverageDuration.IncrementBy(Stopwatch.GetTimestamp() - startTime);
			//_AverageDurationBase.IncrementBy((long)NoOfItems);
			//_CounterTimer.IncrementBy(Stopwatch.GetTimestamp() - startTime);
		}
        /// <summary>
        /// The total operations ls
        /// </summary>
        [NonSerialized]
		private CounterSample TotalOperationsLS;
        /// <summary>
        /// The total operations as
        /// </summary>
        [NonSerialized]
		private CounterSample TotalOperationsAS;
        /// <summary>
        /// Counts total operations.
        /// </summary>
        /// <returns>Number of total operations</returns>
        public float TotalOperations()
		{
			TotalOperationsAS = _TotalOperations.NextSample();
			float f = CounterSampleCalculator.ComputeCounterValue(TotalOperationsLS, TotalOperationsAS);
			TotalOperationsLS = TotalOperationsAS;
			return f;
		}
        /// <summary>
        /// The operations per second ls
        /// </summary>
        [NonSerialized]
		private CounterSample OperationsPerSecondLS;
        /// <summary>
        /// The operations per second as
        /// </summary>
        [NonSerialized]
		private CounterSample OperationsPerSecondAS;
        /// <summary>
        /// Counts the operations per second.
        /// </summary>
        /// <returns>Number of operations per second</returns>
        public float OperationsPerSecond()
		{
			OperationsPerSecondAS = _OperationsPerSecond.NextSample();
			float f = CounterSampleCalculator.ComputeCounterValue(OperationsPerSecondLS, OperationsPerSecondAS);
			OperationsPerSecondLS = OperationsPerSecondAS;
			return f;
		}
        /// <summary>
        /// The average duration ls
        /// </summary>
        [NonSerialized]
		private CounterSample AverageDurationLS;
        /// <summary>
        /// The average duration as
        /// </summary>
        [NonSerialized]
		private CounterSample AverageDurationAS;

        /// <summary>
        /// Counts the average duration
        /// </summary>
        /// <returns>The average duration</returns>
        public float AverageDuration()
		{
			AverageDurationAS = _AverageDuration.NextSample();
			float f = CounterSampleCalculator.ComputeCounterValue(AverageDurationLS, AverageDurationAS);
			AverageDurationLS = AverageDurationAS;
			return f;
		}
        /// <summary>
        /// The workload ls
        /// </summary>
        [NonSerialized]
		private CounterSample WorkloadLS;
        /// <summary>
        /// The workload as
        /// </summary>
        [NonSerialized]
		private CounterSample WorkloadAS;
        /// <summary>
        /// Workloads this instance.
        /// </summary>
        /// <returns>System.Single.</returns>
        public float Workload()
		{
			WorkloadAS = _CounterTimer.NextSample();
			float f = CounterSampleCalculator.ComputeCounterValue(WorkloadLS, WorkloadAS);
			WorkloadLS = WorkloadAS;
			return f;
		}
	}

    /// <summary>
    /// Class OrderDictionary.
    /// Implements the <see cref="System.Collections.Generic.Dictionary{System.String, System.Object}" />
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{System.String, System.Object}" />
    [Serializable()]
    public class OrderDictionary : Dictionary<string, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDictionary"/> class.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
        public OrderDictionary(SerializationInfo info, StreamingContext context) : base(info, context) 
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDictionary"/> class.
        /// </summary>
        public OrderDictionary()
        {
        }

        //[NonSerialized]
        /// <summary>
        /// Gets or sets the rawdata.
        /// </summary>
        /// <value>The rawdata.</value>
        public byte[] rawdata
        {
            get 
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
            set 
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(value);
                var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                this.Clear();
                ((OrderDictionary)bf.Deserialize(ms)).ToList().ForEach(x => this.Add(x.Key, x.Value));
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        public void SetValue(string key, object val)
        {
            if (this.ContainsKey(key))
                this[key] = val;
            else
                this.Add(key, val);
        }

        /// <summary>
        /// Gets the value associated with the specified key, or returns a default value if the key does not exist.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="def">The default value to return if the key is not found or if the value is null.</param>
        /// <returns>The value associated with the key if found and not null; otherwise, the default value.</returns>
        public object ValueOrDefault(string key, object def)
        {
            if (!this.ContainsKey(key)) return def;
            return this[key] ?? def;
        }

        /// <summary>
        /// Defines options for merging two OrderDictionary instances.
        /// </summary>
        public enum MergeOptions
        {
            /// <summary>
            /// Take the first value when keys conflict.
            /// </summary>
            TakeFirst,
            /// <summary>
            /// Take the last value when keys conflict.
            /// </summary>
            TakeLast,
            /// <summary>
            /// Throw an exception when keys conflict.
            /// </summary>
            ExceptionOnOverwrite
        }

        /// <summary>
        /// Merges another OrderDictionary into this instance using the specified merge option.
        /// </summary>
        /// <param name="second">The OrderDictionary to merge into this instance.</param>
        /// <param name="option">The merge option to use when handling key conflicts.</param>
        /// <exception cref="OptiFlowException">Thrown when attempting to merge dictionaries with the same key and option is ExceptionOnOverwrite.</exception>
        public void Merge(OrderDictionary second, MergeOptions option)
        {
            foreach(KeyValuePair<string,object> kvp in second)
            {
                if (this.ContainsKey(kvp.Key))
                {
                    if (option == MergeOptions.TakeFirst)
                        ; // do nothing
                    else if (option == MergeOptions.TakeLast)
                        SetValue(kvp.Key, kvp.Value);
                    else if (option == MergeOptions.ExceptionOnOverwrite)
                        throw new OptiFlowException("Try to merge sets with same key", null);
                } else
                    SetValue(kvp.Key, kvp.Value);
            }
        }
    }
}
