﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.AcceptanceTests.Core.Automation;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Extensions;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Helpers;
using SFA.DAS.Payments.Tests.Core;
using SFA.DAS.Payments.Tests.Core.Builders;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Steps
{
    [Binding]
    // ReSharper disable once InconsistentNaming
    public class PV2_1825_Steps : EndToEndStepsBase
    {
        private readonly FeatureContext featureContext;
        protected TestPaymentsDataContext testDataContext;

        [BeforeStep()]
        public void InitialiseNewTestDataContext()
        {
            testDataContext = Scope.Resolve<TestPaymentsDataContext>();
        }

        [AfterStep()]
        public void DeScopeTestDataContext()
        {
            testDataContext = null;
        }

        public PV2_1825_Steps(FeatureContext context) : base(context)
        {
            featureContext = context;
        }

        [Given(@"there is an ILR for the collection period (.*) with 2 price episodes, the end date of one occurs in the same month as the start date of the other")]
        public void GivenThereIsAnIlrWith(string collectionPeriodText)
        {
            TestSession.CollectionPeriod = new CollectionPeriodBuilder().WithSpecDate(collectionPeriodText).Build();
            TestSession.FM36Global = FM36GlobalDeserialiser.DeserialiseByFeatureForPeriod(featureContext.FeatureInfo.Title, TestSession.CollectionPeriod.Period.ToPeriodText());
        }

        [Given("end date of (.*) and the start date of (.*) occur in the same month")]
        public void GivenEndDateOfEpisode1AndStartDateOfEpisode2OccurInTheSameMonth(string priceEpisodeIdentifier1, string priceEpisodeIdentifier2) {}

        [Given("both (.*) and (.*) in the ILR matches to both Commitments (.*) and (.*), on ULN and UKPRN")]
        public async Task GivenTwoPriceEpisodeInIlrMatchesTwoCommitments(string priceEpisodeIdentifier1, string priceEpisodeIdentifier2, string commitmentIdentifier1, string commitmentIdentifier2)
        {
            var learner = TestSession.FM36Global.Learners.Single();
            learner.ULN = TestSession.Learner.Uln;
            learner.LearnRefNumber = TestSession.Learner.LearnRefNumber;

            var priceEpisode1 = learner.PriceEpisodes.Single(y => y.PriceEpisodeIdentifier == priceEpisodeIdentifier1);
            var learningDelivery1 = learner.LearningDeliveries.Single(x => x.AimSeqNumber == priceEpisode1.PriceEpisodeValues.PriceEpisodeAimSeqNumber);

            var priceEpisode2 = learner.PriceEpisodes.Single(y => y.PriceEpisodeIdentifier == priceEpisodeIdentifier2);
            var learningDelivery2 = learner.LearningDeliveries.Single(x => x.AimSeqNumber == priceEpisode2.PriceEpisodeValues.PriceEpisodeAimSeqNumber);

            var ids = new List<long>{ TestSession.GenerateId(), TestSession.GenerateId() };

            var commitment1 = new ApprenticeshipBuilder().BuildSimpleApprenticeship(TestSession, learningDelivery1.LearningDeliveryValues, ids.Min()).WithALevyPayingEmployer().WithApprenticeshipPriceEpisode(priceEpisode1.PriceEpisodeValues).ToApprenticeshipModel();
            var commitment2 = new ApprenticeshipBuilder().BuildSimpleApprenticeship(TestSession, learningDelivery2.LearningDeliveryValues, ids.Max()).WithALevyPayingEmployer().WithApprenticeshipPriceEpisode(priceEpisode2.PriceEpisodeValues).ToApprenticeshipModel();

            TestSession.Apprenticeships.GetOrAdd(commitmentIdentifier1, commitment1);
            testDataContext.Apprenticeship.Add(commitment1);
            testDataContext.ApprenticeshipPriceEpisode.AddRange(commitment1.ApprenticeshipPriceEpisodes);

            TestSession.Apprenticeships.GetOrAdd(commitmentIdentifier2, commitment2);
            testDataContext.Apprenticeship.Add(commitment2);
            testDataContext.ApprenticeshipPriceEpisode.AddRange(commitment2.ApprenticeshipPriceEpisodes);

            var levyModel = TestSession.Employer.ToModel();
            levyModel.Balance = 1000000000;
            testDataContext.LevyAccount.Add(levyModel);
            await testDataContext.SaveChangesAsync();

            TestSession.FM36Global.UKPRN = TestSession.Provider.Ukprn;
        }

        [Given(@"the course in (.*) matches the course in Commitment (.*)")]
        public void GivenTheCourseInPEMatchesTheCourseInCommitment(string priceEpisodeIdentifier, string commitmentIdentifier)
        {
            //TODO: Copy course to apprenticeship 
        }
        
        [Then(@"the course in (.*) does not match the course for Commitment (.*)")]
        public void ThenTheCourseInPEDoesNotMatchTheCourseForCommitment(string priceEpisodeIdentifier, string commitmentIdentifier)
        {
            //TODO Assert Course do not match
        }


        [When(@"the Provider submits the single price episode PE-1 in the ILR")]
        [When("the Provider submits the 2 price episodes in the ILR")]
        public async Task WhenTheProviderSubmitsThePriceEpisodesInTheIlr()
        {
            var dcHelper = Scope.Resolve<IDcHelper>();
            await dcHelper.SendIlrSubmission(
                TestSession.FM36Global.Learners,
                TestSession.Provider.Ukprn,
                TestSession.CollectionPeriod.AcademicYear,
                TestSession.CollectionPeriod.Period,
                TestSession.Provider.JobId);


        }

        private bool HasDataLockErrorsForPriceEpisode(string priceEpisodeIdentifier, short academicYear)
        {
            return EarningEventsHelper.GetOnProgrammeDataLockErrorsForLearnerAndPriceEpisode(priceEpisodeIdentifier, academicYear, TestSession).Any();
        }

        private bool HasSingleMatchForPriceEpisodeAndCommitment(string priceEpisodeIdentifier)
        {
            return EarningEventsHelper.PayableEarningsReceivedForLearner(TestSession)
                .Count(x => x.PriceEpisodes.Any(y => y.Identifier == priceEpisodeIdentifier)) == 1;
        }

        [Then("there is a single match for (.*) with Commitment (.*)")]
        public async Task ThereIsASingleMatchForPeWithCommitment(string priceEpisodeIdentifier, string commitmentIdentifier)
        {
            await WaitForIt(() => 
                !HasDataLockErrorsForPriceEpisode(priceEpisodeIdentifier, short.Parse(TestSession.FM36Global.Year)) 
                && HasSingleMatchForPriceEpisodeAndCommitment(priceEpisodeIdentifier),
                "Failed to find a matching earning event and no datalocks."
            );
        }
    }

    public class InvalidAssumptionOnFm36GlobalFileException : Exception {}
}