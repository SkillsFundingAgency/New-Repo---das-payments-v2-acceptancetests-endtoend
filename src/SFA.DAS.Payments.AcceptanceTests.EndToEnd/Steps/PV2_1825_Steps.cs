﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using SFA.DAS.Payments.AcceptanceTests.Core.Automation;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Extensions;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Handlers;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Helpers;
using SFA.DAS.Payments.Model.Core;
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
        public async Task GivenPriceEpisodeInIlrMatchesCommitments(string priceEpisodeIdentifier1, string priceEpisodeIdentifier2, string commitmentIdentifier1, string commitmentIdentifier2)
        {
            var learner = TestSession.FM36Global.Learners.Single();
            learner.ULN = TestSession.Learner.Uln;
            learner.LearnRefNumber = TestSession.Learner.LearnRefNumber;

            var priceEpisode1 = learner.PriceEpisodes.Single(y => y.PriceEpisodeIdentifier == priceEpisodeIdentifier1);
            var learningDelivery1 = learner.LearningDeliveries.Single(x => x.AimSeqNumber == priceEpisode1.PriceEpisodeValues.PriceEpisodeAimSeqNumber);

            var commitment1 = new ApprenticeshipBuilder().BuildSimpleApprenticeship(TestSession, learningDelivery1.LearningDeliveryValues).WithALevyPayingEmployer().WithApprenticeshipPriceEpisode(priceEpisode1.PriceEpisodeValues).ToApprenticeshipModel();
            var commitment2 = new ApprenticeshipBuilder().BuildSimpleApprenticeship(TestSession, learningDelivery1.LearningDeliveryValues).WithALevyPayingEmployer().WithApprenticeshipPriceEpisode(priceEpisode1.PriceEpisodeValues).ToApprenticeshipModel();
            TestSession.Apprenticeships.Add(commitmentIdentifier1, commitment1);
            TestSession.Apprenticeships.Add(commitmentIdentifier2, commitment2);
            testDataContext.Apprenticeship.Add(commitment1);
            testDataContext.Apprenticeship.Add(commitment2);
            testDataContext.ApprenticeshipPriceEpisode.AddRange(commitment1.ApprenticeshipPriceEpisodes);
            testDataContext.ApprenticeshipPriceEpisode.AddRange(commitment2.ApprenticeshipPriceEpisodes);

            var levyModel = TestSession.Employer.ToModel();
            levyModel.Balance = 1000000000;
            testDataContext.LevyAccount.Add(levyModel);

            await testDataContext.SaveChangesAsync();

            TestSession.FM36Global.UKPRN = TestSession.Provider.Ukprn;
        }

        [Given("the start date of (.*) ((.*)) is after the start date for Commitment (.*) ((.*))")]
        [Given("the start date of (.*) ((.*)) is before the start date for Commitment (.*) ((.*))")]
        [Given("the start date of (.*) ((.*)) is on or after the start date for Commitment (.*) ((.*))")]
        public async Task GivenTheStartDateOfPriceEpisodeIsAfterTheStartDateForCommitment(string priceEpisodeIdentifier, DateTime priceEpisodeStartDate, string commitmentIdentifier, DateTime commitmentStartDate)
        {
            var actualPriceEpisodeStartDate = TestSession.FM36Global.Learners.Single().PriceEpisodes.Single(x => x.PriceEpisodeIdentifier == priceEpisodeIdentifier).PriceEpisodeValues.EpisodeStartDate;
            if (priceEpisodeStartDate != actualPriceEpisodeStartDate) throw new InvalidAssumptionOnFm36GlobalFileException();

            TestSession.Apprenticeships[commitmentIdentifier].EstimatedStartDate = commitmentStartDate;

            await testDataContext.SaveChangesAsync();
        }

        [Given("the course in (.*) matches the course in Commitment (.*)")]
        public async Task GivenTheCourseInPriceEpisodeMatchesTheCourseInCommitment(string priceEpisodeIdentifier, string commitmentIdentifier)
        {
            var learner = TestSession.FM36Global.Learners.Single();
            var priceEpisode = learner.PriceEpisodes.Single(y => y.PriceEpisodeIdentifier == priceEpisodeIdentifier);
            var learningDelivery = learner.LearningDeliveries.Single(x => x.AimSeqNumber == priceEpisode.PriceEpisodeValues.PriceEpisodeAimSeqNumber);

            TestSession.Apprenticeships[commitmentIdentifier].ProgrammeType = learningDelivery.LearningDeliveryValues.ProgType;
            TestSession.Apprenticeships[commitmentIdentifier].FrameworkCode = learningDelivery.LearningDeliveryValues.FworkCode;
            TestSession.Apprenticeships[commitmentIdentifier].PathwayCode = learningDelivery.LearningDeliveryValues.PwayCode;
            TestSession.Apprenticeships[commitmentIdentifier].StandardCode = learningDelivery.LearningDeliveryValues.StdCode;
            TestSession.Apprenticeships[commitmentIdentifier].ApprenticeshipPriceEpisodes.Single().Cost = priceEpisode.PriceEpisodeValues.TNP1 ?? 0;

            await testDataContext.SaveChangesAsync();
        }

        [Given("the course in (.*) does not match the course for Commitment (.*)")]
        public void GivenTheCourseInPriceEpisodeDoesNotMatchTheCourseInCommitment() {}

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

        private IEnumerable<DataLockErrorCode> GetOnProgrammeDataLockErrorsForLearnerAndPriceEpisode(string priceEpisodeIdentifier, short academicYear)
        {
            return EarningFailedDataLockMatchingHandler
                .ReceivedEvents
                .Where(dataLockEvent =>
                    dataLockEvent.Ukprn == TestSession.Provider.Ukprn
                    && dataLockEvent.Learner.Uln == TestSession.Learner.Uln
                    && dataLockEvent.Learner.ReferenceNumber == TestSession.Learner.LearnRefNumber
                    && dataLockEvent.CollectionYear == academicYear
                    && dataLockEvent.CollectionPeriod.Period == TestSession.CollectionPeriod.Period
                    && dataLockEvent.PriceEpisodes.Any(episode => episode.Identifier == priceEpisodeIdentifier))
                .SelectMany(dataLockEvent =>
                    dataLockEvent.OnProgrammeEarnings.SelectMany(earning => earning.Periods.SelectMany(period => period.DataLockFailures.Select(a => a.DataLockError))));
        }

        private bool HasDataLockErrorsForPriceEpisode(string priceEpisodeIdentifier, short academicYear)
        {
            return GetOnProgrammeDataLockErrorsForLearnerAndPriceEpisode(priceEpisodeIdentifier, academicYear).Any();
        }

        private bool PayableEarningsHaveBeenReceivedForLearner()
        {
            return PayableEarningEventHandler.ReceivedEvents.Any(earningEvent =>
                earningEvent.Ukprn == TestSession.Provider.Ukprn
                && earningEvent.Learner.Uln == TestSession.Learner.Uln
                && earningEvent.Learner.ReferenceNumber == TestSession.Learner.LearnRefNumber
            );
        }

        [Then("there is a single match for (.*) with Commitment A")]
        public async Task ThereIsASingleMatchForPeWithCommitment(string priceEpisodeIdentifier)
        {
            await WaitForIt(() => !HasDataLockErrorsForPriceEpisode(priceEpisodeIdentifier, short.Parse(TestSession.FM36Global.Year)) && PayableEarningsHaveBeenReceivedForLearner(), "Failed to find a matching earning event and no datalocks.");
        }
    }

    public class InvalidAssumptionOnFm36GlobalFileException : Exception {}
}