﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.4.0.0
//      SpecFlow Generator Version:2.4.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Tests.Non_LevyLearner_BasicDay
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Two Non Levy-Learners One Finishes Late And Other Finishes Early PV2-199")]
    public partial class TwoNonLevy_LearnersOneFinishesLateAndOtherFinishesEarlyPV2_199Feature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Two Non Levy-Learners One Finishes Late And Other Finishes Early PV2-199.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Two Non Levy-Learners One Finishes Late And Other Finishes Early PV2-199", null, ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Two non-LEVY learners, one learner finishes early, one finishes late PV2-199")]
        [NUnit.Framework.CategoryAttribute("EndToEnd")]
        [NUnit.Framework.TestCaseAttribute("R01/Current Academic Year", null)]
        [NUnit.Framework.TestCaseAttribute("R02/Current Academic Year", null)]
        [NUnit.Framework.TestCaseAttribute("R03/Current Academic Year", null)]
        [NUnit.Framework.TestCaseAttribute("R04/Current Academic Year", null)]
        [NUnit.Framework.TestCaseAttribute("R05/Current Academic Year", null)]
        [NUnit.Framework.TestCaseAttribute("R06/Current Academic Year", null)]
        [NUnit.Framework.TestCaseAttribute("R07/Current Academic Year", null)]
        [NUnit.Framework.TestCaseAttribute("R08/Current Academic Year", null)]
        [NUnit.Framework.TestCaseAttribute("R09/Current Academic Year", null)]
        [NUnit.Framework.TestCaseAttribute("R10/Current Academic Year", null)]
        [NUnit.Framework.TestCaseAttribute("R11/Current Academic Year", null)]
        [NUnit.Framework.TestCaseAttribute("R12/Current Academic Year", null)]
        public virtual void TwoNon_LEVYLearnersOneLearnerFinishesEarlyOneFinishesLatePV2_199(string collection_Period, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "EndToEnd"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Two non-LEVY learners, one learner finishes early, one finishes late PV2-199", null, @__tags);
#line 5
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Learner ID",
                        "Priority",
                        "Start Date",
                        "Planned Duration",
                        "Total Training Price",
                        "Total Training Price Effective Date",
                        "Total Assessment Price",
                        "Total Assessment Price Effective Date",
                        "Actual Duration",
                        "Completion Status",
                        "Contract Type",
                        "Aim Sequence Number",
                        "Aim Reference",
                        "Framework Code",
                        "Pathway Code",
                        "Programme Type",
                        "Funding Line Type"});
            table1.AddRow(new string[] {
                        "learner a",
                        "1",
                        "Sep/Last Academic Year",
                        "15 months",
                        "18750",
                        "1st day of Sep/Last Academic Year",
                        "0",
                        "1st day of Sep/Last Academic Year",
                        "",
                        "continuing",
                        "Act2",
                        "1",
                        "ZPROG001",
                        "403",
                        "1",
                        "25",
                        "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)"});
            table1.AddRow(new string[] {
                        "learner b",
                        "1",
                        "Sep/Last Academic Year",
                        "12 months",
                        "15000",
                        "1st day of Sep/Last Academic Year",
                        "0",
                        "1st day of Sep/Last Academic Year",
                        "",
                        "continuing",
                        "Act2",
                        "1",
                        "ZPROG001",
                        "403",
                        "1",
                        "25",
                        "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)"});
#line 6
 testRunner.Given("the provider previously submitted the following learner details", ((string)(null)), table1, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Learner ID",
                        "Delivery Period",
                        "On-Programme",
                        "Completion",
                        "Balancing",
                        "SFA Contribution Percentage"});
            table2.AddRow(new string[] {
                        "learner a",
                        "Aug/Last Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner a",
                        "Sep/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner a",
                        "Oct/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner a",
                        "Nov/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner a",
                        "Dec/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner a",
                        "Jan/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner a",
                        "Feb/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner a",
                        "Mar/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner a",
                        "Apr/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner a",
                        "May/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner a",
                        "Jun/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner a",
                        "Jul/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "Aug/Last Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "Sep/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "Oct/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "Nov/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "Dec/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "Jan/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "Feb/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "Mar/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "Apr/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "May/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "Jun/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table2.AddRow(new string[] {
                        "learner b",
                        "Jul/Last Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
#line 10
 testRunner.And("the following earnings had been generated for the learner", ((string)(null)), table2, "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Learner ID",
                        "Collection Period",
                        "Delivery Period",
                        "SFA Co-Funded Payments",
                        "Employer Co-Funded Payments",
                        "Transaction Type"});
            table3.AddRow(new string[] {
                        "learner a",
                        "R02/Last Academic Year",
                        "Sep/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner a",
                        "R03/Last Academic Year",
                        "Oct/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner a",
                        "R04/Last Academic Year",
                        "Nov/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner a",
                        "R05/Last Academic Year",
                        "Dec/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner a",
                        "R06/Last Academic Year",
                        "Jan/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner a",
                        "R07/Last Academic Year",
                        "Feb/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner a",
                        "R08/Last Academic Year",
                        "Mar/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner a",
                        "R09/Last Academic Year",
                        "Apr/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner a",
                        "R10/Last Academic Year",
                        "May/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner a",
                        "R11/Last Academic Year",
                        "Jun/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner a",
                        "R12/Last Academic Year",
                        "Jul/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner b",
                        "R02/Last Academic Year",
                        "Sep/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner b",
                        "R03/Last Academic Year",
                        "Oct/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner b",
                        "R04/Last Academic Year",
                        "Nov/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner b",
                        "R05/Last Academic Year",
                        "Dec/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner b",
                        "R06/Last Academic Year",
                        "Jan/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner b",
                        "R07/Last Academic Year",
                        "Feb/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner b",
                        "R08/Last Academic Year",
                        "Mar/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner b",
                        "R09/Last Academic Year",
                        "Apr/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner b",
                        "R10/Last Academic Year",
                        "May/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner b",
                        "R11/Last Academic Year",
                        "Jun/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table3.AddRow(new string[] {
                        "learner b",
                        "R12/Last Academic Year",
                        "Jul/Last Academic Year",
                        "900",
                        "100",
                        "Learning"});
#line 36
    testRunner.And("the following provider payments had been generated", ((string)(null)), table3, "And ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Learner ID",
                        "Priority",
                        "Start Date",
                        "Planned Duration",
                        "Total Training Price",
                        "Total Training Price Effective Date",
                        "Total Assessment Price",
                        "Total Assessment Price Effective Date",
                        "Actual Duration",
                        "Completion Status",
                        "Contract Type",
                        "Aim Sequence Number",
                        "Aim Reference",
                        "Framework Code",
                        "Pathway Code",
                        "Programme Type",
                        "Funding Line Type"});
            table4.AddRow(new string[] {
                        "learner a",
                        "1",
                        "Sep/Last Academic Year",
                        "15 months",
                        "18750",
                        "1st day of Sep/Last Academic Year",
                        "0",
                        "1st day of Sep/Last Academic Year",
                        "12 months",
                        "completed",
                        "Act2",
                        "1",
                        "ZPROG001",
                        "403",
                        "1",
                        "25",
                        "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)"});
            table4.AddRow(new string[] {
                        "learner b",
                        "1",
                        "Sep/Last Academic Year",
                        "12 months",
                        "15000",
                        "1st day of Sep/Last Academic Year",
                        "0",
                        "1st day of Sep/Last Academic Year",
                        "15 months",
                        "completed",
                        "Act2",
                        "1",
                        "ZPROG001",
                        "403",
                        "1",
                        "25",
                        "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)"});
#line 60
    testRunner.But("the Provider now changes the Learner details as follows", ((string)(null)), table4, "But ");
#line 64
 testRunner.When(string.Format("the amended ILR file is re-submitted for the learners in collection period {0}", collection_Period), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Learner ID",
                        "Delivery Period",
                        "On-Programme",
                        "Completion",
                        "Balancing",
                        "SFA Contribution Percentage"});
            table5.AddRow(new string[] {
                        "learner a",
                        "Aug/Current Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner a",
                        "Sep/Current Academic Year",
                        "0",
                        "3750",
                        "3000",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner a",
                        "Oct/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner a",
                        "Nov/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner a",
                        "Dec/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner a",
                        "Jan/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner a",
                        "Feb/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner a",
                        "Mar/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner a",
                        "Apr/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner a",
                        "May/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner a",
                        "Jun/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner a",
                        "Jul/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "Aug/Current Academic Year",
                        "1000",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "Sep/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "Oct/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "Nov/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "Dec/Current Academic Year",
                        "0",
                        "3000",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "Jan/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "Feb/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "Mar/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "Apr/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "May/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "Jun/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
            table5.AddRow(new string[] {
                        "learner b",
                        "Jul/Current Academic Year",
                        "0",
                        "0",
                        "0",
                        "90%"});
#line 65
    testRunner.Then("the following learner earnings should be generated", ((string)(null)), table5, "Then ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Learner ID",
                        "Collection Period",
                        "Delivery Period",
                        "On-Programme",
                        "Completion",
                        "Balancing"});
            table6.AddRow(new string[] {
                        "learner a",
                        "R01/Current Academic Year",
                        "Aug/Current Academic Year",
                        "1000",
                        "0",
                        "0"});
            table6.AddRow(new string[] {
                        "learner a",
                        "R02/Current Academic Year",
                        "Sep/Current Academic Year",
                        "0",
                        "3750",
                        "3000"});
            table6.AddRow(new string[] {
                        "learner b",
                        "R01/Current Academic Year",
                        "Aug/Current Academic Year",
                        "1000",
                        "0",
                        "0"});
            table6.AddRow(new string[] {
                        "learner b",
                        "R05/Current Academic Year",
                        "Dec/Current Academic Year",
                        "0",
                        "3000",
                        "0"});
#line 91
    testRunner.And("only the following payments will be calculated", ((string)(null)), table6, "And ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Learner ID",
                        "Collection Period",
                        "Delivery Period",
                        "SFA Co-Funded Payments",
                        "Employer Co-Funded Payments",
                        "Transaction Type"});
            table7.AddRow(new string[] {
                        "learner a",
                        "R01/Current Academic Year",
                        "Aug/Current Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table7.AddRow(new string[] {
                        "learner a",
                        "R02/Current Academic Year",
                        "Sep/Current Academic Year",
                        "3375",
                        "375",
                        "Completion"});
            table7.AddRow(new string[] {
                        "learner a",
                        "R02/Current Academic Year",
                        "Sep/Current Academic Year",
                        "2700",
                        "300",
                        "Balancing"});
            table7.AddRow(new string[] {
                        "learner b",
                        "R01/Current Academic Year",
                        "Aug/Current Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table7.AddRow(new string[] {
                        "learner b",
                        "R05/Current Academic Year",
                        "Dec/Current Academic Year",
                        "2700",
                        "300",
                        "Completion"});
#line 97
    testRunner.And("only the following provider payments will be recorded", ((string)(null)), table7, "And ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Learner ID",
                        "Collection Period",
                        "Delivery Period",
                        "SFA Co-Funded Payments",
                        "Employer Co-Funded Payments",
                        "Transaction Type"});
            table8.AddRow(new string[] {
                        "learner a",
                        "R01/Current Academic Year",
                        "Aug/Current Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table8.AddRow(new string[] {
                        "learner a",
                        "R02/Current Academic Year",
                        "Sep/Current Academic Year",
                        "3375",
                        "375",
                        "Completion"});
            table8.AddRow(new string[] {
                        "learner a",
                        "R02/Current Academic Year",
                        "Sep/Current Academic Year",
                        "2700",
                        "300",
                        "Balancing"});
            table8.AddRow(new string[] {
                        "learner b",
                        "R01/Current Academic Year",
                        "Aug/Current Academic Year",
                        "900",
                        "100",
                        "Learning"});
            table8.AddRow(new string[] {
                        "learner b",
                        "R05/Current Academic Year",
                        "Dec/Current Academic Year",
                        "2700",
                        "300",
                        "Completion"});
#line 104
 testRunner.And("at month end only the following provider payments will be generated", ((string)(null)), table8, "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
