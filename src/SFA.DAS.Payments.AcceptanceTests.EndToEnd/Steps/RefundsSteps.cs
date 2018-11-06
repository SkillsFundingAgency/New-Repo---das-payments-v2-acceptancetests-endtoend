﻿using Autofac;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using NServiceBus;
using SFA.DAS.Payments.AcceptanceTests.Core;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Data;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.Internal.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Payments.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Steps
{
    [Binding]
    public class RefundsSteps : EndToEndStepsBase
    {
        public RefundsSteps(FeatureContext context) : base(context)
        {
        }

        [Given(@"the provider previously submitted the following learner details")]
        public void GivenTheProviderPreviouslySubmittedTheFollowingLearnerDetails(Table table)
        {
            var ilr = table.CreateSet<Training>().ToList();
            PreviousIlr = ilr;
        }

        [Given(@"the following earnings had been generated for the learner")]
        public void GivenTheFollowingEarningsHadBeenGeneratedForTheLearner(Table table)
        {
            var earnings = table.CreateSet<OnProgrammeEarning>().ToList();
            PreviousEarnings = earnings;
        }

        [Given(@"the following provider payments had been generated")]
        public void GivenTheFollowingProviderPaymentsHadBeenGenerated(Table table)
        {
            var previousJobId = TestSession.GenerateId("previous_job_id");
            var previousSubmissionTime = DateTime.UtcNow.AddHours(-1);
            Console.WriteLine($"Previous job id: {previousJobId}");
            var payments = table.CreateSet<ProviderPayment>().ToList();
            var previousPayments = payments.SelectMany(p => CreatePayments(p, PreviousIlr.First(), previousJobId, previousSubmissionTime));

            var dataContext = Container.Resolve<IPaymentsDataContext>();
            dataContext.Payment.AddRange(previousPayments);
            dataContext.SaveChanges();
        }

        [Given(@"the Provider now changes the Learner details as follows")]
        public void GivenTheProviderNowChangesTheLearnerDetailsAsFollows(Table table)
        {
            var ilr = table.CreateSet<Training>().ToList();
            CurrentIlr = ilr;
        }

        [When(@"the amended ILR file is re-submitted for the learners in collection period (.*)")]
        public async Task WhenTheAmendedILRFileIsRe_SubmittedForTheLearnersInCollectionPeriodRCurrentAcademicYear(string collectionPeriod)
        {
            var period = collectionPeriod.ToDate().ToCalendarPeriod();
            Console.WriteLine($"Current collection period is: {period.Name}.");
            CurrentCollectionPeriod = period;
            foreach (var training in CurrentIlr)
            {
                var learner = new FM36Learner();
                PopulateLearner(learner, training);
                var command = new ProcessLearnerCommand
                {
                    Learner = learner,
                    CollectionPeriod = period.Period,
                    CollectionYear = period.Name.Split('-').FirstOrDefault(),
                    Ukprn = TestSession.Ukprn,
                    JobId = TestSession.JobId,
                    IlrSubmissionDateTime = TestSession.IlrSubmissionTime,
                    RequestTime = DateTimeOffset.UtcNow,
                    SubmissionDate = TestSession.IlrSubmissionTime, //TODO: ????
                };
                Console.WriteLine($"Sending process learner command to the earning events service. Command: {command.ToJson()}");
                await MessageSession.Send(command);
            }
        }

        [Then(@"the following provider payments will be recorded")]
        public void ThenTheFollowingProviderPaymentsWillBeRecorded(Table table)
        {
            var expectedPayments = table.CreateSet<ProviderPayment>().ToList();
            var dataContext = Container.Resolve<IPaymentsDataContext>();
            WaitForIt(() =>
            {
                var payments = dataContext.Payment.Where(p => p.JobId == TestSession.JobId &&
                                                              p.LearnerReferenceNumber ==
                                                              TestSession.Learner.LearnRefNumber)
                    .ToList();
                Console.WriteLine($"Found {payments.Count} recorded payments for job id: {TestSession.JobId}, learner ref: {TestSession.Learner.LearnRefNumber}");
                return expectedPayments.All(expected => payments.Any(p => expected.CollectionPeriod.ToDate().ToCalendarPeriod().Name == p.CollectionPeriod.Name &&
                                                                          expected.TransactionType == p.TransactionType && 
                                                                          CurrentIlr.First().ContractType == p.ContractType &&
                                                                          ((p.FundingSource == FundingSourceType.CoInvestedSfa && expected.SfaCoFundedPayments == p.Amount) || 
                                                                           (p.FundingSource == FundingSourceType.CoInvestedEmployer && expected.EmployerCoFundedPayments == p.Amount))));

            }, "Failed to find all the expected stored provider payments.");
        }
    }
}
