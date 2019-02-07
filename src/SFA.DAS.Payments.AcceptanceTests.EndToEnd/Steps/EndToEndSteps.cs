﻿using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using NServiceBus;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Data;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.EventMatchers;
using SFA.DAS.Payments.ProviderPayments.Messages.Internal.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using SFA.DAS.Payments.AcceptanceTests.Core.Automation;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Learner = SFA.DAS.Payments.AcceptanceTests.Core.Data.Learner;

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Steps
{
    [Binding]
    public class EndToEndSteps : EndToEndStepsBase
    {
        public EndToEndSteps(FeatureContext context) : base(context)
        {
        }

        [BeforeScenario()]
        public void ResetJob()
        {
            if (!Context.ContainsKey("new_feature"))
                NewFeature = true;
            var newJobId = TestSession.GenerateId();
            Console.WriteLine($"Using new job. Previous job id: {TestSession.JobId}, new job id: {newJobId}");
           TestSession.Providers.ForEach(p => p.JobId = newJobId);
        }

        [AfterScenario()]
        public void CleanUpScenario()
        {
            NewFeature = false;
        }

        [Given(@"the provider is providing training for the following learners")]
        public void GivenTheProviderIsProvidingTrainingForTheFollowingLearners(Table table)
        {
            CurrentIlr = table.CreateSet<Training>().ToList();
            AddTestLearners(CurrentIlr, TestSession.Provider.Ukprn);
        }

        [Given(@"the provider previously submitted the following learner details in collection period ""(.*)""")]
        public void GivenTheProviderPreviouslySubmittedTheFollowingLearnerDetailsInCollectionPeriod(string previousCollectionPeriod, Table table)
        {
            SetCollectionPeriod(previousCollectionPeriod);
            var ilr = table.CreateSet<Training>().ToList();
            PreviousIlr = ilr;
            AddTestLearners(PreviousIlr, TestSession.Provider.Ukprn);
        }

        [Given(@"the Provider now changes the Learner details as follows")]
        public void GivenTheProviderNowChangesTheLearnerDetailsAsFollows(Table table)
        {
            AddNewIlr(table, TestSession.Provider);
        }

        [Given(@"the ""(.*)"" now changes the Learner details as follows")]
        public void GivenTheNowChangesTheLearnerDetailsAsFollows(string providerIdentifier, Table table)
        {
            if (!TestSession.AtLeastOneScenarioCompleted)
            {
                var provider = TestSession.GetProviderByIdentifier(providerIdentifier);
                var currentIlr = table.CreateSet<Training>().ToList();
                AddTestLearners(currentIlr, provider.Ukprn);
                CurrentIlr.AddRange(currentIlr);
            }
        }

        [Given("the Learner has now changed to \"(.*)\" as follows")]
        public void GivenTheLearnerChangesProvider(string providerId, Table table)
        {
            if (!TestSession.AtLeastOneScenarioCompleted)
            {
                AddNewIlr(table, TestSession.Provider);
                //Update all Session Learners and Training to use new provider Ukprn
                TestSession.RegenerateUkprn();
                CurrentIlr.ForEach(l => l.Ukprn = TestSession.Provider.Ukprn);
                TestSession.Learners.ForEach(l => l.Ukprn = TestSession.Provider.Ukprn);
              }
        }

        [Given(@"the following learners")]
        public void GivenTheFollowingLearners(Table table)
        {
            var learners = table.CreateSet<Learner>();
            AddTestLearners(learners, TestSession.Provider.Ukprn);
        }

        [Given(@"aims details are changed as follows")]
        public void GivenAimsDetailsAreChangedAsFollows(Table table)
        {
            AddTestAims(table.CreateSet<Aim>().ToList(), TestSession.Provider.Ukprn);
        }

        [Given(@"the following aims")]
        public void GivenTheFollowingAims(Table table)
        {
            var aims = table.CreateSet<Aim>().ToList();
            AddTestAims(aims, TestSession.Provider.Ukprn);
        }

        private static readonly HashSet<long> PriceEpisodesProcessedForJob = new HashSet<long>();

        [Given(@"price details are changed as follows")]
        public void GivenPriceDetailsAreChangedAsFollows(Table table)
        {
            PriceEpisodesProcessedForJob.Remove(TestSession.JobId);
            GivenPriceDetailsAsFollows(table);
        }

        [Given(@"the following commitments exist")]
        public void GivenTheFollowingCommitmentsExist(Table table)
        {
            var commitments = table.CreateSet<Commitment>();
            AddTestCommitments(commitments);
        }

        [Given(@"price details as follows")]
        public void GivenPriceDetailsAsFollows(Table table)
        {
            if (TestSession.AtLeastOneScenarioCompleted)
            {
                return;
            }

            var newPriceEpisodes = table.CreateSet<Price>().ToList();
            CurrentPriceEpisodes = newPriceEpisodes;

            if (TestSession.Learners.Any(x => x.Aims.Count > 0))
            {
                foreach (var newPriceEpisode in newPriceEpisodes)
                {
                    Aim aim;
                    try
                    {
                        aim = TestSession.Learners.SelectMany(x => x.Aims)
                            .SingleOrDefault(x => x.AimSequenceNumber == newPriceEpisode.AimSequenceNumber);
                    }
                    catch (Exception)
                    {
                        throw new Exception("There are too many aims with the same sequence number");
                    }

                    if (aim == null)
                    {
                        throw new Exception("There is a price episode without a matching aim");
                    }

                    aim.PriceEpisodes.Add(newPriceEpisode);
                }
            }
        }

        [Then(@"the following learner earnings should be generated")]
        public async Task ThenTheFollowingLearnerEarningsShouldBeGenerated(Table table)
        {
            await GeneratedAndValidateEarnings(table, TestSession.Provider);
        }
        
        [Then(@"the following learner earnings should be generated for ""(.*)""")]
        public async Task ThenTheFollowingLearnerEarningsShouldBeGeneratedFor(string providerIdentifier, Table table)
        {
            var provider = TestSession.GetProviderByIdentifier(providerIdentifier);
           await GeneratedAndValidateEarnings(table, provider);
        }
        
        [Then(@"only the following payments will be calculated")]
        public async Task ThenTheFollowingPaymentsWillBeCalculated(Table table)
        {
            var expectedPayments = CreatePayments(table, TestSession.Provider.Ukprn);
            var matcher = new RequiredPaymentEventMatcher(TestSession.Provider, TestSession, CurrentCollectionPeriod, expectedPayments, CurrentIlr, CurrentPriceEpisodes);
            await WaitForIt(() => matcher.MatchPayments(), "Required Payment event check failure");
        }

        [Then(@"no payments will be calculated")]
        public async Task ThenNoPaymentsWillBeCalculated()
        {
            var matcher = new RequiredPaymentEventMatcher(TestSession.Provider, TestSession, CurrentCollectionPeriod);
            await WaitForUnexpected(() => matcher.MatchNoPayments(), "Required Payment event check failure");
        }

        [Then(@"and only the following provider payments will be generated")]
        public async Task ThenOnlyTheFollowingProviderPaymentsWillBeGenerated(Table table)
        {
            await MatchOnlyProviderPayments(table);
        }

        [Then(@"at month end only the following provider payments will be generated")]
        public async Task ThenTheFollowingProviderPaymentsWillBeGenerated(Table table)
        {
            await ValidateGeneratedProviderPayments( table, TestSession.Provider);
        }
        
        [Then(@"at month end only the following payments will be calculated for ""(.*)""")]
        public async Task ThenAtMonthEndOnlyTheFollowingPaymentsWillBeCalculatedFor(string providerIdentifier, Table table)
        {
            var provider = TestSession.GetProviderByIdentifier(providerIdentifier);
            await ValidateGeneratedProviderPayments(table, provider);
        }
        
        [Then(@"no provider payments will be recorded")]
        public async Task ThenNoProviderPaymentsWillBeRecorded()
        {
            var matcher = new ProviderPaymentModelMatcher(TestSession.Provider, DataContext, TestSession, CurrentCollectionPeriod);
            await WaitForUnexpected(() => matcher.MatchNoPayments(), "Payment history check failure");
        }

        [Then(@"no learner earnings should be generated")]
        public async Task ThenNoLearnerEarningsWillBeRecorded()
        {
            var matcher =  new EarningEventMatcher(TestSession.Provider, CurrentPriceEpisodes,CurrentIlr, null, TestSession, CurrentCollectionPeriod, null);
            await WaitForUnexpected(() => matcher.MatchNoPayments(), "Earning Event check failure");
        }

        [Then(@"at month end no provider payments will be generated")]
        public async Task ThenAtMonthEndNoProviderPaymentsWillBeGenerated()
        {
            var monthEndCommand = new ProcessProviderMonthEndCommand
            {
                CollectionPeriod = CurrentCollectionPeriod,
                Ukprn = TestSession.Ukprn,
                JobId = TestSession.JobId
            };
            await MessageSession.Send(monthEndCommand);
            var matcher = new ProviderPaymentEventMatcher(TestSession.Provider, CurrentCollectionPeriod, TestSession);
            await WaitForUnexpected(() => matcher.MatchNoPayments(), "Provider Payment event check failure");
        }
    }
}
