﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using DCT.TestDataGenerator.Functor;
using SFA.DAS.Payments.AcceptanceTests.Core.Automation;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Extensions;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Handlers;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Helpers;
using SFA.DAS.Payments.DataLocks.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Tests.Core;
using SFA.DAS.Payments.Tests.Core.Builders;
using TechTalk.SpecFlow;
using PriceEpisode = ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output.PriceEpisode;

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
        public void GivenEndDateOfEpisode1AndStartDateOfEpisode2OccurInTheSameMonth(string priceEpisodeIdentifier1, string priceEpisodeIdentifier2) { }

        [Given("both (.*) and (.*) in the ILR matches to both Commitments (.*) and (.*), on ULN and UKPRN")]
        public async Task GivenPriceEpisodeInIlrMatchesCommitments(string priceEpisodeIdentifier1, string priceEpisodeIdentifier2, string commitmentIdentifier1, string commitmentIdentifier2)
        {
            var learner = TestSession.FM36Global.Learners.Single();
            learner.ULN = TestSession.Learner.Uln;
            learner.LearnRefNumber = TestSession.Learner.LearnRefNumber;

            var priceEpisode1 = learner.PriceEpisodes.Single(y => y.PriceEpisodeIdentifier == priceEpisodeIdentifier1);
            var learningDelivery1 = learner.LearningDeliveries.Single(x => x.AimSeqNumber == priceEpisode1.PriceEpisodeValues.PriceEpisodeAimSeqNumber);

            var priceEpisode2 = learner.PriceEpisodes.Single(y => y.PriceEpisodeIdentifier == priceEpisodeIdentifier2);
            var learningDelivery2 = learner.LearningDeliveries.Single(x => x.AimSeqNumber == priceEpisode2.PriceEpisodeValues.PriceEpisodeAimSeqNumber);

            var ids = new List<long> { TestSession.GenerateId(), TestSession.GenerateId() };

            var commitment1 = new ApprenticeshipBuilder().BuildSimpleApprenticeship(TestSession, learningDelivery1.LearningDeliveryValues, ids.Min()).WithALevyPayingEmployer().WithApprenticeshipPriceEpisode(priceEpisode1.PriceEpisodeValues).ToApprenticeshipModel();
            var commitment2 = new ApprenticeshipBuilder().BuildSimpleApprenticeship(TestSession, learningDelivery2.LearningDeliveryValues, ids.Max()).WithALevyPayingEmployer().WithApprenticeshipPriceEpisode(priceEpisode2.PriceEpisodeValues).ToApprenticeshipModel();

            TestSession.Apprenticeships.Add(commitmentIdentifier1, commitment1);
            testDataContext.Apprenticeship.Add(commitment1);
            testDataContext.ApprenticeshipPriceEpisode.AddRange(commitment1.ApprenticeshipPriceEpisodes);
            //await testDataContext.SaveChangesAsync();

            TestSession.Apprenticeships.Add(commitmentIdentifier2, commitment2);
            testDataContext.Apprenticeship.Add(commitment2);
            testDataContext.ApprenticeshipPriceEpisode.AddRange(commitment2.ApprenticeshipPriceEpisodes);
            //await testDataContext.SaveChangesAsync();

            var levyModel = TestSession.Employer.ToModel();
            levyModel.Balance = 1000000000;
            testDataContext.LevyAccount.Add(levyModel);
            await testDataContext.SaveChangesAsync();

            TestSession.FM36Global.UKPRN = TestSession.Provider.Ukprn;
        }

        //[Given("the start date of (.*) - (.*) is after the start date for Commitment (.*) - (.*)")]
        //[Given("the start date of (.*) - (.*) is before the start date for Commitment (.*) - (.*)")]
        //[Given("the start date of (.*) - (.*) is on or after the start date for Commitment (.*) - (.*)")]
        //public async Task GivenTheStartDateOfPriceEpisodeIsAfterTheStartDateForCommitment(string priceEpisodeIdentifier, DateTime priceEpisodeStartDate, string commitmentIdentifier, DateTime commitmentStartDate)
        //{
        //    var actualPriceEpisodeStartDate = TestSession.FM36Global.Learners.Single().PriceEpisodes.Single(x => x.PriceEpisodeIdentifier == priceEpisodeIdentifier).PriceEpisodeValues.EpisodeEffectiveTNPStartDate;
        //    if (priceEpisodeStartDate.Date != actualPriceEpisodeStartDate.GetValueOrDefault().Date) throw new InvalidAssumptionOnFm36GlobalFileException();

        //    TestSession.Apprenticeships[commitmentIdentifier].EstimatedStartDate = commitmentStartDate;
        //    TestSession.Apprenticeships[commitmentIdentifier].ApprenticeshipPriceEpisodes.Single().StartDate =
        //        commitmentStartDate;

        //    await testDataContext.SaveChangesAsync();
        //}

        [Given(@"(.*) and (.*) start date is after the start date for Commitment (.*)")]
        public async Task GivenPEAndPEStartDateIsAfterTheStartDateForCommitmentA(string priceEpisodeIdentifier1, string priceEpisodeIdentifier2, string commitmentIdentifier)
        {
            var priceEpisodeDates = new[]
            {
                GetPriceEpisodeByIdentifier(priceEpisodeIdentifier1).PriceEpisodeValues.EpisodeEffectiveTNPStartDate.GetValueOrDefault(),
                GetPriceEpisodeByIdentifier(priceEpisodeIdentifier2).PriceEpisodeValues.EpisodeEffectiveTNPStartDate.GetValueOrDefault(),
            };

            var minimumEpisodeStartDate = priceEpisodeDates.Min();

            var apprenticeship = TestSession.Apprenticeships[commitmentIdentifier];
            apprenticeship.EstimatedStartDate = minimumEpisodeStartDate.AddDays(-1);
            apprenticeship.ApprenticeshipPriceEpisodes.Single().StartDate = minimumEpisodeStartDate.AddDays(-1);
            testDataContext.Apprenticeship.Update(apprenticeship);
            await testDataContext.SaveChangesAsync();
        }
        
        [Given(@"(.*) start date is before commitment while (.*) is on or after the start date for Commitment (.*)")]
        public async Task GivenPEStartDateIsBeforeCommitmentWhilePEIsOnOrAfterTheStartDateForCommitmentB(string priceEpisodeIdentifier1, string priceEpisodeIdentifier2, string commitmentIdentifier)
        {
            var priceEpisode2 = GetPriceEpisodeByIdentifier(priceEpisodeIdentifier2);
            var apprenticeship = TestSession.Apprenticeships[commitmentIdentifier];
            apprenticeship.EstimatedStartDate = priceEpisode2.PriceEpisodeValues.EpisodeEffectiveTNPStartDate.GetValueOrDefault().AddDays(-1);
            apprenticeship.ApprenticeshipPriceEpisodes.Single().StartDate = apprenticeship.EstimatedStartDate;
            testDataContext.Apprenticeship.Update(apprenticeship);
            await testDataContext.SaveChangesAsync();

        }
        
        private PriceEpisode GetPriceEpisodeByIdentifier(string priceEpisodeIdentifier)
        {
            var learner = TestSession.FM36Global.Learners.Single();
            var priceEpisode = learner.PriceEpisodes.Single(y => y.PriceEpisodeIdentifier == priceEpisodeIdentifier);
            return priceEpisode;
        }

        //[Given("the start date of (.*) is after the start date for Commitment (.*)")]
        //[Given("the start date of (.*) is on or after the start date for Commitment (.*)")]
        //public async Task GivenTheStartDateOfPriceEpisodeIsAfterTheStartDateForCommitment(string priceEpisodeIdentifier,
        //    string commitmentIdentifier)
        //{
        //    var priceEpisodeStartDate = TestSession.FM36Global.Learners.Single().PriceEpisodes.Single(x => x.PriceEpisodeIdentifier == priceEpisodeIdentifier).PriceEpisodeValues.EpisodeEffectiveTNPStartDate;
        //    var commitmentStartDate = priceEpisodeStartDate.GetValueOrDefault().AddDays(-7);

        //    TestSession.Apprenticeships[commitmentIdentifier].EstimatedStartDate = commitmentStartDate;
        //    TestSession.Apprenticeships[commitmentIdentifier].ApprenticeshipPriceEpisodes.Single().StartDate = commitmentStartDate;

        //    await testDataContext.SaveChangesAsync();
        //}

        //[Given("the start date of (.*) is before the start date for Commitment (.*)")]
        //public async Task GivenTheStartDateOfPriceEpisodeIsBeforeTheStartDateForCommitment(string priceEpisodeIdentifier,
        //    string commitmentIdentifier)
        //{
        //    var priceEpisodeStartDate = TestSession.FM36Global.Learners.Single().PriceEpisodes.Single(x => x.PriceEpisodeIdentifier == priceEpisodeIdentifier).PriceEpisodeValues.EpisodeEffectiveTNPStartDate;
        //    var commitmentStartDate = priceEpisodeStartDate.GetValueOrDefault().AddDays(7);

        //    TestSession.Apprenticeships[commitmentIdentifier].EstimatedStartDate = commitmentStartDate;
        //    TestSession.Apprenticeships[commitmentIdentifier].ApprenticeshipPriceEpisodes.Single().StartDate = commitmentStartDate;

        //    await testDataContext.SaveChangesAsync();
        //}

        [Given("the course in (.*) matches the course in Commitment (.*)")]
        public async Task GivenTheCourseInPriceEpisodeMatchesTheCourseInCommitment(string priceEpisodeIdentifier, string commitmentIdentifier)
        {
            //var learner = TestSession.FM36Global.Learners.Single();
            //var priceEpisode = learner.PriceEpisodes.Single(y => y.PriceEpisodeIdentifier == priceEpisodeIdentifier);
            //var learningDelivery = learner.LearningDeliveries.Single(x => x.AimSeqNumber == priceEpisode.PriceEpisodeValues.PriceEpisodeAimSeqNumber);

            //TestSession.Apprenticeships[commitmentIdentifier].ProgrammeType = learningDelivery.LearningDeliveryValues.ProgType;
            //TestSession.Apprenticeships[commitmentIdentifier].FrameworkCode = learningDelivery.LearningDeliveryValues.FworkCode;
            //TestSession.Apprenticeships[commitmentIdentifier].PathwayCode = learningDelivery.LearningDeliveryValues.PwayCode;
            //TestSession.Apprenticeships[commitmentIdentifier].StandardCode = learningDelivery.LearningDeliveryValues.StdCode;
            //TestSession.Apprenticeships[commitmentIdentifier].ApprenticeshipPriceEpisodes.Single().Cost = priceEpisode.PriceEpisodeValues.TNP1 ?? 0;

            //await testDataContext.SaveChangesAsync();
        }

        [Given("the course in (.*) does not match the course for Commitment (.*)")]
        public void GivenTheCourseInPriceEpisodeDoesNotMatchTheCourseInCommitment(string priceEpisodeIdentifier, string commitmentIdentifier) { }

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

        private IEnumerable<PayableEarningEvent> PayableEarningsHaveBeenReceivedForLearner()
        {
            return PayableEarningEventHandler.ReceivedEvents.Where(earningEvent =>
                earningEvent.Ukprn == TestSession.Provider.Ukprn
                && earningEvent.Learner.Uln == TestSession.Learner.Uln
                && earningEvent.Learner.ReferenceNumber == TestSession.Learner.LearnRefNumber
            );
        }

        private bool HasSingleMatchForPriceEpisodeAndCommitment(string priceEpisodeIdentifier,
            string commitmentIdentifier)
        {
            return PayableEarningsHaveBeenReceivedForLearner().Count(x => x.PriceEpisodes.Any(y => y.Identifier == priceEpisodeIdentifier)) == 1;
        }

        [Then("there is a single match for (.*) with Commitment (.*)")]
        public async Task ThereIsASingleMatchForPeWithCommitment(string priceEpisodeIdentifier, string commitmentIdentifier)
        {
            await WaitForIt(() =>
                !HasDataLockErrorsForPriceEpisode(priceEpisodeIdentifier, short.Parse(TestSession.FM36Global.Year))
                && HasSingleMatchForPriceEpisodeAndCommitment(priceEpisodeIdentifier, commitmentIdentifier),
                "Failed to find a matching earning event and no datalocks."
            );
        }
    }

    public class InvalidAssumptionOnFm36GlobalFileException : Exception { }
}