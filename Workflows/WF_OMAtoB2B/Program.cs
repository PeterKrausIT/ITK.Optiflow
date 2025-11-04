// ***********************************************************************
// Assembly         : WF_OMAtoB2B
// Author           : Peter Kraus
// Created          : 11-04-2025
// ***********************************************************************
// <summary>Program.cs
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
using System.Linq;
using System.Activities;
using System.Activities.Statements;

/// <summary>
/// The WF_OMAtoB2B namespace.
/// </summary>
namespace WF_OMAtoB2B
{
    /// <summary>
    /// Main program class for the OMAtoB2B workflow.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            Activity workflow1 = new Workflow1();
            WorkflowInvoker.Invoke(workflow1);
        }
    }
}
