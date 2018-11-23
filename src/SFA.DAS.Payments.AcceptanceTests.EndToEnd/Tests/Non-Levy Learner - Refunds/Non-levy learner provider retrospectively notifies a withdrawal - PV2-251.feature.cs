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
namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Tests.Non_LevyLearner_Refunds
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Non-levy learner provider retrospectively notifies a withdrawal - PV2-251")]
    public partial class Non_LevyLearnerProviderRetrospectivelyNotifiesAWithdrawal_PV2_251Feature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Non-levy learner provider retrospectively notifies a withdrawal - PV2-251.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Non-levy learner provider retrospectively notifies a withdrawal - PV2-251", "\tAs a Provider\r\n\tI would like TODO\r\n\tSo that TODO", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Provider retrospectively notifies of a withdrawal for a non-levy learner after pa" +
            "yments have already been made PV2-251")]
        public virtual void ProviderRetrospectivelyNotifiesOfAWithdrawalForANon_LevyLearnerAfterPaymentsHaveAlreadyBeenMadePV2_251()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Provider retrospectively notifies of a withdrawal for a non-levy learner after pa" +
                    "yments have already been made PV2-251", null, ((string[])(null)));
#line 7
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Priority",
                        "Start Date",
                        "Planned Duration",
                        "Total Training Price",
                        "Total Training Price Effective Date",
                        "Total Assessment Price",
                        "Total Assessment Price Effective Date",
                        "Actual Duration",
                        "Completion Status",
                        "SFA Contribution Percentage",
                        "Contract Type",
                        "Aim Sequence Number",
                        "Aim Reference",
                        "Framework Code",
                        "Pathway Code",
                        "Programme Type",
                        "Funding Line Type"});
            table1.AddRow(new string[] {
                        "1",
                        "start of academic year",
                        "12 months",
                        "9000",
                        "Aug/Current Academic Year",
                        "2250",
                        "Aug/Current Academic Year",
                        "",
                        "continuing",
                        "90%",
                        "Act2",
                        "1",
                        "ZPROG001",
                        "403",
                        "1",
                        "25",
                        "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)"});
#line 8
    testRunner.Given("the provider previously submitted the following learner details", ((string)(null)), table1, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Delivery Period",
                        "On-Programme",
                        "Completion",
                        "Balancing"});
            table2.AddRow(new string[] {
                        "Aug/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table2.AddRow(new string[] {
                        "Sep/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table2.AddRow(new string[] {
                        "Oct/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table2.AddRow(new string[] {
                        "Nov/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table2.AddRow(new string[] {
                        "Dec/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table2.AddRow(new string[] {
                        "Jan/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table2.AddRow(new string[] {
                        "Feb/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table2.AddRow(new string[] {
                        "Mar/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table2.AddRow(new string[] {
                        "Apr/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table2.AddRow(new string[] {
                        "May/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table2.AddRow(new string[] {
                        "Jun/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table2.AddRow(new string[] {
                        "Jul/Current Academic Year",
                        "750",
                        "0",
                        "0"});
#line 11
    testRunner.And("the following earnings had been generated for the learner", ((string)(null)), table2, "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Collection Period",
                        "Delivery Period",
                        "SFA Co-Funded Payments",
                        "Employer Co-Funded Payments",
                        "Transaction Type"});
            table3.AddRow(new string[] {
                        "R01/Current Academic Year",
                        "Aug/Current Academic Year",
                        "675",
                        "75",
                        "Learning"});
            table3.AddRow(new string[] {
                        "R02/Current Academic Year",
                        "Sep/Current Academic Year",
                        "675",
                        "75",
                        "Learning"});
            table3.AddRow(new string[] {
                        "R03/Current Academic Year",
                        "Oct/Current Academic Year",
                        "675",
                        "75",
                        "Learning"});
            table3.AddRow(new string[] {
                        "R04/Current Academic Year",
                        "Nov/Current Academic Year",
                        "675",
                        "75",
                        "Learning"});
            table3.AddRow(new string[] {
                        "R05/Current Academic Year",
                        "Dec/Current Academic Year",
                        "675",
                        "75",
                        "Learning"});
#line 25
    testRunner.And("the following provider payments had been generated", ((string)(null)), table3, "And ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Priority",
                        "Start Date",
                        "Planned Duration",
                        "Total Training Price",
                        "Total Training Price Effective Date",
                        "Total Assessment Price",
                        "Total Assessment Price Effective Date",
                        "Actual Duration",
                        "Completion Status",
                        "SFA Contribution Percentage",
                        "Contract Type",
                        "Aim Sequence Number",
                        "Aim Reference",
                        "Framework Code",
                        "Pathway Code",
                        "Programme Type",
                        "Funding Line Type"});
            table4.AddRow(new string[] {
                        "1",
                        "start of academic year",
                        "12 months",
                        "9000",
                        "Aug/Current Academic Year",
                        "2250",
                        "Aug/Current Academic Year",
                        "3 months",
                        "withdrawn",
                        "90%",
                        "Act2",
                        "1",
                        "ZPROG001",
                        "403",
                        "1",
                        "25",
                        "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)"});
#line 33
    testRunner.But("the Provider now changes the Learner details as follows", ((string)(null)), table4, "But ");
#line 37
 testRunner.When("the amended ILR file is re-submitted for the learners in collection period R06/Cu" +
                    "rrent Academic Year", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Delivery Period",
                        "On-Programme",
                        "Completion",
                        "Balancing"});
            table5.AddRow(new string[] {
                        "Aug/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "Sep/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "Oct/Current Academic Year",
                        "750",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "Nov/Current Academic Year",
                        "0",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "Dec/Current Academic Year",
                        "0",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "Jan/Current Academic Year",
                        "0",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "Feb/Current Academic Year",
                        "0",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "Mar/Current Academic Year",
                        "0",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "Apr/Current Academic Year",
                        "0",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "May/Current Academic Year",
                        "0",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "Jun/Current Academic Year",
                        "0",
                        "0",
                        "0"});
            table5.AddRow(new string[] {
                        "Jul/Current Academic Year",
                        "0",
                        "0",
                        "0"});
#line 39
    testRunner.Then("the following learner earnings should be generated", ((string)(null)), table5, "Then ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Collection Period",
                        "Delivery Period",
                        "On-Programme",
                        "Completion",
                        "Balancing"});
            table6.AddRow(new string[] {
                        "R06/Current Academic Year",
                        "Nov/Current Academic Year",
                        "-750",
                        "0",
                        "0"});
            table6.AddRow(new string[] {
                        "R06/Current Academic Year",
                        "Dec/Current Academic Year",
                        "-750",
                        "0",
                        "0"});
#line 53
    testRunner.And("only the following payments will be calculated", ((string)(null)), table6, "And ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Collection Period",
                        "Delivery Period",
                        "SFA Co-Funded Payments",
                        "Employer Co-Funded Payments",
                        "Transaction Type"});
            table7.AddRow(new string[] {
                        "R06/Current Academic Year",
                        "Nov/Current Academic Year",
                        "-675",
                        "-75",
                        "Learning"});
            table7.AddRow(new string[] {
                        "R06/Current Academic Year",
                        "Dec/Current Academic Year",
                        "-675",
                        "-75",
                        "Learning"});
#line 57
    testRunner.And("only the following provider payments will be recorded", ((string)(null)), table7, "And ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Collection Period",
                        "Delivery Period",
                        "SFA Co-Funded Payments",
                        "Employer Co-Funded Payments",
                        "Transaction Type"});
            table8.AddRow(new string[] {
                        "R06/Current Academic Year",
                        "Nov/Current Academic Year",
                        "-675",
                        "-75",
                        "Learning"});
            table8.AddRow(new string[] {
                        "R06/Current Academic Year",
                        "Dec/Current Academic Year",
                        "-675",
                        "-75",
                        "Learning"});
#line 61
    testRunner.And("at month end only the following provider payments will be generated", ((string)(null)), table8, "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
