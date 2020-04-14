﻿using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.AcceptanceTests.Core.Automation;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Handlers;
using SFA.DAS.Payments.DataLocks.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.ProviderPayments.Messages;

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Steps
{
    public static class EarningEventsHelper
    {
        public static IEnumerable<PayableEarningEvent> PayableEarningsReceivedForLearner(TestSession session)
        {
            return PayableEarningEventHandler.ReceivedEvents.Where(earningEvent =>
                earningEvent.Ukprn == session.Provider.Ukprn
                && earningEvent.Learner.Uln == session.Learner.Uln
                && earningEvent.Learner.ReferenceNumber == session.Learner.LearnRefNumber
            );
        }

        public static IEnumerable<DataLockErrorCode> GetOnProgrammeDataLockErrorsForLearnerAndPriceEpisode(string priceEpisodeIdentifier, short academicYear, TestSession session)
        {
            return EarningFailedDataLockMatchingHandler
                .ReceivedEvents
                .Where(dataLockEvent =>
                    dataLockEvent.Ukprn == session.Provider.Ukprn
                    && dataLockEvent.Learner.Uln == session.Learner.Uln
                    && dataLockEvent.Learner.ReferenceNumber == session.Learner.LearnRefNumber
                    && dataLockEvent.CollectionYear == academicYear
                    && dataLockEvent.CollectionPeriod.Period == session.CollectionPeriod.Period
                    && dataLockEvent.PriceEpisodes.Any(episode => episode.Identifier == priceEpisodeIdentifier))
                .SelectMany(dataLockEvent =>
                    dataLockEvent.OnProgrammeEarnings.SelectMany(earning => earning.Periods.SelectMany(period => period.DataLockFailures.Select(a => a.DataLockError))));
        }
    }

    public static class PaymentEventsHelper
    {
        public static IEnumerable<ProviderPaymentEvent> ProviderPaymentsReceivedForLearner(string priceEpisodeIdentifier, short academicYear, TestSession session)
        {
            return ProviderPaymentEventHandler.ReceivedEvents.Where(ppEvent =>
                ppEvent.Ukprn == session.Provider.Ukprn
                && ppEvent.Learner.Uln == session.Learner.Uln
                && ppEvent.Learner.ReferenceNumber == session.Learner.LearnRefNumber
                && ppEvent.CollectionPeriod.AcademicYear == academicYear
                && ppEvent.CollectionPeriod.Period == session.CollectionPeriod.Period
                && ppEvent.PriceEpisodeIdentifier == priceEpisodeIdentifier);
        }
    }
}