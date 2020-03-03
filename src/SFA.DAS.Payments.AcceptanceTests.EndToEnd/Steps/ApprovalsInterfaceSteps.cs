﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using NServiceBus.Features;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Payments.AcceptanceTests.Core.Automation;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using SFA.DAS.Payments.AcceptanceTests.Core.Infrastructure;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Data;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Data.Approvals;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.EventMatchers;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Extensions;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Handlers;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Helpers;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Infrastructure;
using SFA.DAS.Payments.Core;
using SFA.DAS.Payments.Messages.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Tests.Core;
using SFA.DAS.Payments.Tests.Core.Builders;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using ApprenticeshipEmployerType = SFA.DAS.CommitmentsV2.Types.ApprenticeshipEmployerType;
using Payment = SFA.DAS.Payments.AcceptanceTests.Core.Data.Payment;

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Steps
{
    [Binding]
    public class FM36ImportSteps : EndToEndStepsBase
    {
        private readonly FeatureContext featureContext;
        protected TestPaymentsDataContext TestDataContext;

        [BeforeStep()]
        public void InitialiseNewTestDataContext()
        {
            TestDataContext = Scope.Resolve<TestPaymentsDataContext>();
        }

        [AfterStep()]
        public void DeScopeTestDataContext()
        {
            TestDataContext = null;
        }

        //todo step file per story
        public FM36ImportSteps(FeatureContext context) : base(context)
        {
            featureContext = context;
        }

        //[Given(@"there is an ILR with 2 price episodes, the end date of one occurs in the same month as the start date of the other")]

        [Given(@"there is an ILR with 2 price episodes, the end date of one occurs in the same month as the start date of the other")]
        public void GivenThereIsAnILRWith()
        {
            //todo sort automatically finding file name from convention
            TestSession.FM36Global = FM36GlobalDeserialiser.Deserialise("SFA.DAS.Payments.AcceptanceTests.EndToEnd.FM36TestFiles.PV2-1825-R02.json");
        }

        [Given("end date of PE-(.*) and the start date of PE-(.*) occur in the same month")]
        public void EmptyStep(string episode1, string episode2)
        {
        }

        [Given("PE-(.*) in the ILR matches to both Commitments (.*) and (.*), on ULN and UKPRN")]
        public async Task PriceEpisodeMatchToCommitments(int priceEpisodeIndex, string commitmentIdentifier1, string commitmentIdentifier2)
        {
            var commitment1 = new ApprovalBuilder().BuildSimpleApproval(TestSession, TestSession.FM36Global.Learners.Single(x => x.PriceEpisodes.Any(y => y.PriceEpisodeIdentifier == $"PE-{priceEpisodeIndex}")), 2).WithALevyPayingEmployer().WithApprenticeshipPriceEpisode().ToApprenticeshipModel();
            var commitment2 = new ApprovalBuilder().BuildSimpleApproval(TestSession, TestSession.FM36Global.Learners.Single(x => x.PriceEpisodes.Any(y => y.PriceEpisodeIdentifier == $"PE-{priceEpisodeIndex}")), 2).WithALevyPayingEmployer().WithApprenticeshipPriceEpisode().ToApprenticeshipModel();
            TestSession.Apprenticeships.Add(commitmentIdentifier1, commitment1);
            TestSession.Apprenticeships.Add(commitmentIdentifier2, commitment2);
            TestSession.FM36Global.UKPRN = TestSession.Provider.Ukprn;
            TestSession.FM36Global.Learners.ForEach(x => x.ULN = TestSession.Learner.Uln);
            TestSession.FM36Global.Learners.ForEach(x => x.LearnRefNumber = TestSession.Learner.LearnRefNumber);

            try
            {
                TestDataContext.Apprenticeship.Add(commitment1);
                TestDataContext.Apprenticeship.Add(commitment2);
                TestDataContext.ApprenticeshipPriceEpisode.AddRange(commitment1.ApprenticeshipPriceEpisodes); //todo check if this is needed
                TestDataContext.ApprenticeshipPriceEpisode.AddRange(commitment2.ApprenticeshipPriceEpisodes);

                var levyModel = TestSession.Employer.ToModel();
                levyModel.Balance = 1000000000;
                TestDataContext.LevyAccount.Add(levyModel);

                await TestDataContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            
        }

        //todo wildcard whole PE - so "PE-1" rather than "1"
        [Given("the start date of PE-(.*) is after the start date for Commitment (.*)")]
        public void GivenTheStartDateOfPriceEpisodeIsAfterTheStartDateForCommitment(int priceEpisodeIndex, string commitmentIdentifier)
        {
            //find correct PE in FM36
            var priceEpisodeStartDate = TestSession.FM36Global.Learners.Single().PriceEpisodes.Single(x => x.PriceEpisodeIdentifier == $"PE-{priceEpisodeIndex}").PriceEpisodeValues.EpisodeStartDate;

            //set date of commitment to be before that PE's start date
            TestSession.Apprenticeships[commitmentIdentifier].EstimatedStartDate = priceEpisodeStartDate.GetValueOrDefault().AddDays(-1);
        }

        [Given("the start date of PE-(.*) is before the start date for Commitment (.*)")]
        public void GivenTheStartDateOfPriceEpisodeIsBeforeTheStartDateForCommitment(int priceEpisodeIndex, string commitmentIdentifier)
        {
            //find correct PE in FM36
            var priceEpisodeStartDate = TestSession.FM36Global.Learners.Single().PriceEpisodes.Single(x => x.PriceEpisodeIdentifier == $"PE-{priceEpisodeIndex}").PriceEpisodeValues.EpisodeStartDate;

            //set date of commitment to be after that PE's start date
            TestSession.Apprenticeships[commitmentIdentifier].EstimatedStartDate = priceEpisodeStartDate.GetValueOrDefault().AddDays(1);
        }

        [When("the Provider submits the 2 price episodes in the ILR for the collection period (.*)")]
        public async Task WhenTheProviderSubmitsThePriceEpisodesInTheILR(string collectionPeriodText)
        {
            var collectionPeriod = new CollectionPeriodBuilder().WithSpecDate(collectionPeriodText).Build();
            var dcHelper = Scope.Resolve<IDcHelper>();
            await dcHelper.SendIlrSubmission(
                TestSession.FM36Global.Learners,
                TestSession.Provider.Ukprn,
                collectionPeriod.AcademicYear,
                collectionPeriod.Period,
                TestSession.Provider.JobId);


        }

        [Then("there is a single match for PE-1 with Commitment A")]
        public async Task ThereIsASingleMatchForPEWithCommitment()
        {
            await WaitForIt(async () =>
            {
                try
                {
                    //todo 2 methods here once Dlocks are working - one for PayableEarningEventHandler.ReceivedEvents and one for EarningFailedDataLockMatchingHandler.ReceivedEvents
                    var result = PayableEarningEventHandler.ReceivedEvents.Any(earningEvent =>
                        earningEvent.Ukprn == TestSession.Provider.Ukprn
                        && earningEvent.Learner.Uln == TestSession.Learner.Uln
                        && earningEvent.Learner.ReferenceNumber == TestSession.Learner.LearnRefNumber
                    );

                    var events = PayableEarningEventHandler.ReceivedEvents;

                    var dataLockEvents = EarningFailedDataLockMatchingHandler.ReceivedEvents.Where(earningEvent =>
                        earningEvent.Ukprn == TestSession.Provider.Ukprn
                        && earningEvent.Learner.Uln == TestSession.Learner.Uln
                        && earningEvent.Learner.ReferenceNumber == TestSession.Learner.LearnRefNumber
                    );

                    if (events.Any())
                    {
                        var breakpoint = "";
                    }

                    if (dataLockEvents.Any())
                    {
                        var filteredDataLockEvents = dataLockEvents.Where(x => x.Learner.Uln == TestSession.Learner.Uln);
                        filteredDataLockEvents = filteredDataLockEvents.Where(x => x.PriceEpisodes.Any(y => y.Identifier == "PE-1" || y.Identifier == "PE-2"));
                        filteredDataLockEvents = filteredDataLockEvents.Where(x => x.LearningAim.SequenceNumber == 2);



                        var onProgrammeDataLocks = dataLockEvents.SelectMany(x => x.OnProgrammeEarnings.SelectMany(y =>
                            y.Periods.SelectMany(z => z.DataLockFailures.Select(a => a.DataLockError)))).Distinct();
                        var breakpoint = "";
                    }

                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, "Failed to find all the stored apprenticeships.");
            //todo use the wait for function (see approval steps) to wait for:
            //PayableEarningEventHandler.ReceivedEvents.Where(......) don't use job id, check PayableEarningEventMatcher for example
            //price episode, delivery period, collection period, learner, academic year
            //var expectedPayments = new List<Data.Payment>
            //{

            //};
            //await MatchRequiredPayments(expectedPayments, TestSession.GetProviderByUkprn(TestSession.FM36Global.UKPRN));
        }
    }


    [Binding]
    public class ApprovalsInterfaceSteps : EndToEndStepsBase
    {

        [BeforeStep()]
        public void InitialiseNewTestDataContext()
        {
            TestDataContext = Scope.Resolve<TestPaymentsDataContext>();
        }

        [AfterStep()]
        public void DeScopeTestDataContext()
        {
            TestDataContext = null;
        }

        public List<ApprovalsEmployer> Employers
        {
            get => Get<List<ApprovalsEmployer>>();
            set => Set(value);
        }

        private List<ApprovalsApprenticeship> ApprovalsApprenticeships
        {
            get => Get<List<ApprovalsApprenticeship>>();
            set => Set(value);
        }

        private List<ApprovalsApprenticeship> PreviousApprovalsApprenticeships
        {
            get => !Context.TryGetValue<List<ApprovalsApprenticeship>>("PreviousApprovalsApprenticeships", out var previousApprovals)
                ? null : previousApprovals;
            set => Set(value, "PreviousApprovalsApprenticeships");
        }

        private List<ProviderPriority> ProviderPaymentPriorities
        {
            get => Get<List<ProviderPriority>>();
            set => Set(value);
        }

        public static IMessageSession DasMessageSession { get; set; }
        private static EndpointConfiguration dasEndpointConfiguration;
        protected TestPaymentsDataContext TestDataContext;

        public ApprovalsInterfaceSteps(FeatureContext context) : base(context)
        {
        }

        [BeforeTestRun(Order = 0)]
        public static void SetUpDasEndpoint()
        {
            var config = new TestsConfiguration();
            var endpointConfig = new EndpointConfiguration(config.AcceptanceTestsEndpointName);
            dasEndpointConfiguration = endpointConfig;
            Builder.RegisterInstance(endpointConfig)
                .Named<EndpointConfiguration>("DasEndpointConfiguration")
                .SingleInstance();
            var conventions = endpointConfig.Conventions();
            conventions.DefiningMessagesAs(type => type.IsMessage());
            conventions
                .DefiningCommandsAs(t => t.IsInNamespace("SFA.DAS.CommitmentsV2.Messages.Events"));

            endpointConfig.UsePersistence<AzureStoragePersistence>()
                .ConnectionString(config.StorageConnectionString);
            endpointConfig.DisableFeature<TimeoutManager>();

            var transportConfig = endpointConfig.UseTransport<AzureServiceBusTransport>();
            Builder.RegisterInstance(transportConfig)
                .Named<TransportExtensions<AzureServiceBusTransport>>("DasTransportConfig")
                .SingleInstance();

            transportConfig
                .UseForwardingTopology()
                .ConnectionString(config.DasServiceBusConnectionString)
                .Transactions(TransportTransactionMode.ReceiveOnly)
                .Queues()
                .DefaultMessageTimeToLive(config.DefaultMessageTimeToLive);
            var routing = transportConfig.Routing();
            routing.RouteToEndpoint(typeof(CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent).Assembly, EndpointNames.DataLocksApprovals);
            routing.RouteToEndpoint(typeof(CommitmentsV2.Messages.Events.ApprenticeshipUpdatedApprovedEvent).Assembly, EndpointNames.DataLocksApprovals);
            routing.RouteToEndpoint(typeof(CommitmentsV2.Messages.Events.DataLockTriageApprovedEvent).Assembly, EndpointNames.DataLocksApprovals);
            routing.RouteToEndpoint(typeof(CommitmentsV2.Messages.Events.ApprenticeshipStoppedEvent).Assembly, EndpointNames.DataLocksApprovals);
            routing.RouteToEndpoint(typeof(CommitmentsV2.Messages.Events.ApprenticeshipStopDateChangedEvent).Assembly, EndpointNames.DataLocksApprovals);
            routing.RouteToEndpoint(typeof(CommitmentsV2.Messages.Events.ApprenticeshipPausedEvent).Assembly, EndpointNames.DataLocksApprovals);
            routing.RouteToEndpoint(typeof(CommitmentsV2.Messages.Events.ApprenticeshipResumedEvent).Assembly, EndpointNames.DataLocksApprovals);
            routing.RouteToEndpoint(typeof(CommitmentsV2.Messages.Events.PaymentOrderChangedEvent).Assembly, EndpointNames.DataLocksApprovals);

            var sanitization = transportConfig.Sanitization();
            var strategy = sanitization.UseStrategy<ValidateAndHashIfNeeded>();
            strategy.RuleNameSanitization(
                ruleNameSanitizer: ruleName => ruleName.Split('.').LastOrDefault() ?? ruleName);
            endpointConfig.UseSerialization<NewtonsoftSerializer>();
            endpointConfig.EnableInstallers();
        }

        [BeforeTestRun(Order = 100)]
        public static void StartBus()
        {
            DasMessageSession = Endpoint.Start(dasEndpointConfiguration).Result;
        }

        [Given(@"the following employers")]
        public void GivenTheFollowingEmployer(Table employers)
        {
            Employers = employers.CreateSet<ApprovalsEmployer>().ToList();
            Employers.ForEach(employer =>
            {
                employer.AccountId = TestSession.GenerateId();
                Console.WriteLine($"Employer: {employer.ToJson()}");
            });
        }

        [Given(@"the following apprenticeships have been approved with Employer Type ""(.*)""")]
        public void GivenTheFollowingApprenticeshipsHaveBeenApprovedWithEmployerType(string employerType, Table table)
        {
            GivenTheFollowingApprenticeshipsHaveBeenApproved(table);

            ApprovalsApprenticeships.ForEach(appr => appr.EmployerType = employerType);
        }

        [Given(@"the following apprenticeships have been approved")]
        public void GivenTheFollowingApprenticeshipsHaveBeenApproved(Table table)
        {
            ApprovalsApprenticeships = table.CreateSet<ApprovalsApprenticeship>().ToList();
            ApprovalsApprenticeships.ForEach(apprenticeship =>
            {
                apprenticeship.Id = TestSession.GenerateId();

                Console.WriteLine($"Apprenticeship: {apprenticeship.ToJson()}");
            });
        }

        [Given(@"the apprenticeships are changed as follows")]
        public void GivenTheFollowingApprenticeshipsHaveBeenApprovedX(Table table)
        {
            ApprovalsApprenticeships = table.CreateSet<ApprovalsApprenticeship>().ToList();
            ApprovalsApprenticeships.ForEach(apprenticeshipSpec =>
            {

                var apprenticeship = PreviousApprovalsApprenticeships.Single(x => x.Identifier == apprenticeshipSpec.Identifier);

                apprenticeshipSpec.Id = apprenticeship.Id;

                Console.WriteLine($"Apprenticeship: {apprenticeship.ToJson()}");
            });
        }

        [Given(@"the changed apprenticeships has the following price episodes")]
        [Given(@"the apprenticeships have the following price episodes")]
        public void GivenTheApprenticeshipsHaveTheFollowingPriceEpisodes(Table table)
        {
            var priceEpisodes = table.CreateSet<ApprovalsApprenticeship.PriceEpisode>();
            foreach (var priceEpisode in priceEpisodes)
            {
                Console.WriteLine($"adding price episode to apprenticeship. Price episode: {priceEpisode.ToJson()}");
                var apprenticeship =
                    ApprovalsApprenticeships.FirstOrDefault(appr => appr.Identifier == priceEpisode.Apprenticeship);
                if (apprenticeship == null)
                    Assert.Fail($"Failed to find the apprenticeship for price episode: {priceEpisode.ToJson()}");
                apprenticeship.PriceEpisodes.Add(priceEpisode);
            }
        }

        [When(@"the Approvals service notifies the Payments service of the apprenticeships")]
        public async Task WhenTheApprovalsServiceNotifiesPaymentsVOfTheApprenticeships()
        {
            foreach (var approvalsApprenticeship in ApprovalsApprenticeships)
            {
                var (employer, sendingEmployer, provider, learner) = GetApprovalsReferenceData(approvalsApprenticeship);

                var createdMessage = new CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent
                {
                    AccountId = employer.AccountId,
                    StartDate = approvalsApprenticeship.StartDate.ToDate(),
                    EndDate = approvalsApprenticeship.EndDate.ToDate(),
                    AccountLegalEntityPublicHashedId = employer.AgreementId,
                    AgreedOn = approvalsApprenticeship.AgreedOnDate.ToDate(),
                    ApprenticeshipId = approvalsApprenticeship.Id,
                    CreatedOn = approvalsApprenticeship.CreatedOnDate.ToDate(),
                    LegalEntityName = employer.Name,
                    ProviderId = provider.Ukprn,
                    TrainingType = approvalsApprenticeship.StandardCode > 0
                        ? ProgrammeType.Standard
                        : ProgrammeType.Framework,
                    TrainingCode = approvalsApprenticeship.StandardCode > 0
                        ? approvalsApprenticeship.StandardCode.ToString()
                        : $"{approvalsApprenticeship.FrameworkCode}-{approvalsApprenticeship.ProgrammeType}-{approvalsApprenticeship.PathwayCode}",
                    TransferSenderId = sendingEmployer?.AccountId,
                    Uln = learner.Uln.ToString(),
                    PriceEpisodes = approvalsApprenticeship.PriceEpisodes.Select(pp =>
                        new CommitmentsV2.Messages.Events.PriceEpisode
                        {
                            FromDate = pp.EffectiveFrom.ToDate(),
                            ToDate = pp.EffectiveTo?.ToDate(),
                            Cost = pp.AgreedPrice
                        }).ToArray(),
                    ApprenticeshipEmployerTypeOnApproval = GetApprenticeshipEmployerTypeOnApproval(approvalsApprenticeship.EmployerType)
                };
                Console.WriteLine($"Sending CreatedApprenticeship message: {createdMessage.ToJson()}");
                await DasMessageSession.Send(createdMessage).ConfigureAwait(false);
            }
        }

        [Then(@"the Payments service should record the stopped apprenticeships")]
        [Then(@"the Payments service should record the apprenticeships")]
        [Then(@"the Payments service should record the paused apprenticeships")]
        [Then(@"the Payments service should record the resumed apprenticeships")]
        public async Task ThenPaymentsVShouldRecordTheApprenticeships()
        {
            await WaitForIt(async () =>
            {
                var apprenticeshipIds = ApprovalsApprenticeships.Select(apprenticeship => apprenticeship.Id).ToArray();

                var savedApprenticeships = await TestDataContext.Apprenticeship.AsNoTracking()
                    .Include(apprenticeship => apprenticeship.ApprenticeshipPriceEpisodes)
                    .Where(apprenticeship => apprenticeshipIds.Contains(apprenticeship.Id))
                    .ToListAsync();

                var notFound = new List<ApprovalsApprenticeship>();
                foreach (var approvalsApprenticeship in ApprovalsApprenticeships)
                {
                    var (employer, sendingEmployer, provider, learner) = GetApprovalsReferenceData(approvalsApprenticeship);
                    var savedApprenticeship = savedApprenticeships.FirstOrDefault(apprenticeship => approvalsApprenticeship.Id == apprenticeship.Id);

                    if (savedApprenticeship == null)
                    {
                        Console.WriteLine(
                            $"Failed to find apprenticeship Id: {approvalsApprenticeship.Id}, Uln: {learner.Uln}.");
                        notFound.Add(approvalsApprenticeship);
                        continue;
                    }

                    var expectedStatus = string.IsNullOrWhiteSpace(approvalsApprenticeship.Status)
                        ? savedApprenticeship.Status
                        : approvalsApprenticeship.Status.ToApprenticeshipPaymentStatus();

                    var employerTypeOnApproval =
                        GetApprenticeshipEmployerTypeOnApproval(approvalsApprenticeship.EmployerType);

                    if (MatchesTrainingCode(approvalsApprenticeship, savedApprenticeship) &&
                        MatchPriceEpisodes(approvalsApprenticeship.PriceEpisodes, savedApprenticeship.ApprenticeshipPriceEpisodes) &&
                        provider.Ukprn == savedApprenticeship.Ukprn &&
                        employer.AccountId == savedApprenticeship.AccountId &&
                        learner.Uln == savedApprenticeship.Uln &&
                        expectedStatus == savedApprenticeship.Status &&
                        (!employerTypeOnApproval.HasValue || (int)employerTypeOnApproval.Value == (int)savedApprenticeship.ApprenticeshipEmployerType))
                    {
                        Console.WriteLine(
                            $"Matched apprenticeship: {approvalsApprenticeship.Identifier}, leaner: {approvalsApprenticeship.Learner}, Employer: {approvalsApprenticeship.Employer}");
                    }
                    else
                    {
                        Console.WriteLine(
                            $"Failed to validate stored details for apprenticeship. Apprenticeship details: {approvalsApprenticeship.ToJson()}, saved details: {savedApprenticeship.ToJson()}.");
                        notFound.Add(approvalsApprenticeship);
                    }
                }

                if (!notFound.Any())
                    return true;
                notFound.ForEach(apprenticeship =>
                    Console.WriteLine($"Failed to find and/or validate apprenticeship: {apprenticeship.ToJson()}"));
                return false;
            }, "Failed to find all the stored apprenticeships.");
        }

        [Given(@"the following apprenticeships already exist with Employer Type ""(.*)""")]
        public async Task GivenTheFollowingApprenticeshipsAlreadyExistWithEmployerType(string employerType, Table table)
        {
            PreviousApprovalsApprenticeships = table.CreateSet<ApprovalsApprenticeship>().ToList();
            PreviousApprovalsApprenticeships.ForEach(appr => appr.EmployerType = employerType);
            await SavePreviousApprenticeships();
        }

        [Given(@"the following apprenticeships already exist")]
        public async Task GivenTheFollowingApprenticeshipsAlreadyExist(Table table)
        {
            PreviousApprovalsApprenticeships = table.CreateSet<ApprovalsApprenticeship>().ToList();
            await SavePreviousApprenticeships().ConfigureAwait(false);
        }
        private async Task SavePreviousApprenticeships()
        {
            foreach (var apprenticeshipSpec in PreviousApprovalsApprenticeships)
            {
                var apprenticeshipId = TestSession.GenerateId();
                apprenticeshipSpec.Id = apprenticeshipId;
                var apprenticeship = CreateApprenticeshipModel(apprenticeshipSpec);
                await TestDataContext.ClearApprenticeshipData(apprenticeshipId, apprenticeship.Uln).ConfigureAwait(false);
                await TestDataContext.Apprenticeship.AddAsync(apprenticeship).ConfigureAwait(false);
                await TestDataContext.SaveChangesAsync().ConfigureAwait(false);

                Console.WriteLine($"Existing Apprenticeship Created: {apprenticeship.ToJson()}");
            }
        }

        [Given(@"the existing apprenticeships have the following price episodes")]
        public async Task GivenTheExistingApprenticeshipsHaveTheFollowingPriceEpisodesAsync(Table table)
        {
            var priceEpisodes = table.CreateSet<ApprovalsApprenticeship.PriceEpisode>();
            foreach (var priceEpisode in priceEpisodes)
            {
                Console.WriteLine($"adding price episode to apprenticeship. Price episode: {priceEpisode.ToJson()}");
                var apprenticeship =
                    PreviousApprovalsApprenticeships.FirstOrDefault(appr => appr.Identifier == priceEpisode.Apprenticeship);
                if (apprenticeship == null)
                    Assert.Fail($"Failed to find the apprenticeship for price episode: {priceEpisode.ToJson()}");

                var newPriceEpisode = CreateApprenticeshipPriceEpisode(apprenticeship.Id, priceEpisode);

                await TestDataContext.ApprenticeshipPriceEpisode.AddAsync(newPriceEpisode).ConfigureAwait(false);
                await TestDataContext.SaveChangesAsync().ConfigureAwait(false);

                apprenticeship.PriceEpisodes.Add(priceEpisode);

                Console.WriteLine($"ApprenticeshipPriceEpisode Created: {newPriceEpisode.ToJson()}");
            }
        }

        [When(@"the Approvals service notifies the Payments service of the apprenticeships changes")]
        public async Task WhenTheApprovalsServiceNotifiesThePaymentsServiceOfTheApprenticeshipsChanges()
        {
            foreach (var approvalsApprenticeship in ApprovalsApprenticeships)
            {
                var (employer, sendingEmployer, provider, learner) = GetApprovalsReferenceData(approvalsApprenticeship);
                var createdMessage = new CommitmentsV2.Messages.Events.ApprenticeshipUpdatedApprovedEvent()
                {
                    StartDate = approvalsApprenticeship.StartDate.ToDate(),
                    EndDate = approvalsApprenticeship.EndDate.ToDate(),
                    ApprovedOn = approvalsApprenticeship.AgreedOnDate.ToDate(),
                    ApprenticeshipId = approvalsApprenticeship.Id,
                    TrainingType = approvalsApprenticeship.StandardCode > 0
                        ? ProgrammeType.Standard
                        : ProgrammeType.Framework,
                    TrainingCode = approvalsApprenticeship.StandardCode > 0
                        ? approvalsApprenticeship.StandardCode.ToString()
                        : $"{approvalsApprenticeship.FrameworkCode}-{approvalsApprenticeship.ProgrammeType}-{approvalsApprenticeship.PathwayCode}",
                    Uln = learner.Uln.ToString(),
                    PriceEpisodes = approvalsApprenticeship.PriceEpisodes.Select(pp =>
                        new CommitmentsV2.Messages.Events.PriceEpisode
                        {
                            FromDate = pp.EffectiveFrom.ToDate(),
                            ToDate = pp.EffectiveTo?.ToNullableDate(),
                            Cost = pp.AgreedPrice
                        }).ToArray(),

                };
                Console.WriteLine($"Sending ApprenticeshipUpdatedApprovedEvent message: {createdMessage.ToJson()}");
               await DasMessageSession.Send(createdMessage).ConfigureAwait(false);
            }
        }

        [When(@"the Approvals service notifies the Payments service of the apprenticeships datalock triage changes")]
        public async Task WhenTheApprovalsServiceNotifiesThePaymentsServiceOfTheApprenticeshipsDatalockTriageChanges()
        {
            foreach (var approvalsApprenticeship in ApprovalsApprenticeships)
            {
                var (employer, sendingEmployer, provider, learner) = GetApprovalsReferenceData(approvalsApprenticeship);
                var createdMessage = new CommitmentsV2.Messages.Events.DataLockTriageApprovedEvent
                {
                    ApprovedOn = approvalsApprenticeship.AgreedOnDate.ToDate(),
                    ApprenticeshipId = approvalsApprenticeship.Id,
                    TrainingType = approvalsApprenticeship.StandardCode > 0
                        ? ProgrammeType.Standard
                        : ProgrammeType.Framework,
                    TrainingCode = approvalsApprenticeship.StandardCode > 0
                        ? approvalsApprenticeship.StandardCode.ToString()
                        : $"{approvalsApprenticeship.FrameworkCode}-{approvalsApprenticeship.ProgrammeType}-{approvalsApprenticeship.PathwayCode}",

                    PriceEpisodes = approvalsApprenticeship.PriceEpisodes.Select(pp =>
                        new CommitmentsV2.Messages.Events.PriceEpisode
                        {
                            FromDate = pp.EffectiveFrom.ToDate(),
                            ToDate = pp.EffectiveTo?.ToNullableDate(),
                            Cost = pp.AgreedPrice
                        }).ToArray(),
                };
                Console.WriteLine($"Sending DataLockTriageApprovedEvent message: {createdMessage.ToJson()}");
              await  DasMessageSession.Send(createdMessage).ConfigureAwait(false);
            }
        }

        [Given(@"the apprenticeship stop date is changed as follows")]
        [Given(@"the apprenticeship is stopped as follows")]
        [Given(@"the apprenticeship is paused as follows")]
        [Given(@"the apprenticeship resumed date is changed as follows")]
        public void GivenTheApprenticeshipIsChangedAsFollows(Table table)
        {
            var apprenticeshipChangeModels = table.CreateSet<ApprovalsApprenticeshipChangeModel>();

            ApprovalsApprenticeships = PreviousApprovalsApprenticeships ?? throw new NullReferenceException($"{nameof(PreviousApprovalsApprenticeships)} can't be null");

            foreach (var changedApprenticeshipSpec in apprenticeshipChangeModels)
            {
                var changedApprenticeship =
                    ApprovalsApprenticeships.FirstOrDefault(x => x.Identifier == changedApprenticeshipSpec.Identifier) ??
                    throw new InvalidOperationException("Can't find changed apprenticeship");

                if (!string.IsNullOrWhiteSpace(changedApprenticeshipSpec.Status))
                    changedApprenticeship.Status = changedApprenticeshipSpec.Status;

                if (!string.IsNullOrWhiteSpace(changedApprenticeshipSpec.StoppedOnDate))
                    changedApprenticeship.StoppedOnDate = changedApprenticeshipSpec.StoppedOnDate;

                if (!string.IsNullOrWhiteSpace(changedApprenticeshipSpec.PausedOnDate))
                    changedApprenticeship.PauseOnDate = changedApprenticeshipSpec.PausedOnDate;

                if (!string.IsNullOrWhiteSpace(changedApprenticeshipSpec.ResumedOnDate))
                    changedApprenticeship.ResumedOnDate = changedApprenticeshipSpec.ResumedOnDate;

                if (!string.IsNullOrWhiteSpace(changedApprenticeshipSpec.StoppedOnDate))
                    changedApprenticeship.PriceEpisodes.ForEach(pe => pe.EffectiveTo = changedApprenticeshipSpec.StoppedOnDate);

            }
        }

        [When(@"the Approvals service notifies the Payments service that the apprenticeships has stopped")]
        public async Task WhenTheApprovalsServiceNotifiesThePaymentsServiceOfThatTheApprenticeshipsHasStopped()
        {
            foreach (var approvalsApprenticeship in ApprovalsApprenticeships)
            {
                var (employer, sendingEmployer, provider, learner) = GetApprovalsReferenceData(approvalsApprenticeship);
                var createdMessage = new CommitmentsV2.Messages.Events.ApprenticeshipStoppedEvent()
                {
                    ApprenticeshipId = approvalsApprenticeship.Id,
                    StopDate = approvalsApprenticeship.StoppedOnDate.ToDate(),
                };
                Console.WriteLine($"Sending ApprenticeshipStoppedEvent message: {createdMessage.ToJson()}");
               await DasMessageSession.Send(createdMessage).ConfigureAwait(false);
            }
        }

        [When(@"the Approvals service notifies the Payments service that the apprenticeships stop date has changed")]
        public async Task WhenTheApprovalsServiceNotifiesThePaymentsServiceThatTheApprenticeshipsStopDateHasChanged()
        {
            foreach (var approvalsApprenticeship in ApprovalsApprenticeships)
            {
                var createdMessage = new CommitmentsV2.Messages.Events.ApprenticeshipStopDateChangedEvent()
                {
                    ApprenticeshipId = approvalsApprenticeship.Id,
                    StopDate = approvalsApprenticeship.StoppedOnDate.ToDate(),
                };
                Console.WriteLine($"Sending ApprenticeshipStopDateChangedEvent message: {createdMessage.ToJson()}");
               await  DasMessageSession.Send(createdMessage).ConfigureAwait(false);
            }
        }

        [When(@"the Approvals service notifies the Payments service that the apprenticeships has been paused")]
        public async Task WhenTheApprovalsServiceNotifiesThePaymentsServiceThatTheApprenticeshipsHasBeenPaused()
        {
            foreach (var approvalsApprenticeship in ApprovalsApprenticeships)
            {
                var createdMessage = new CommitmentsV2.Messages.Events.ApprenticeshipPausedEvent()
                {
                    PausedOn = approvalsApprenticeship.PauseOnDate.ToDate(),
                    ApprenticeshipId = approvalsApprenticeship.Id,

                };
                Console.WriteLine($"Sending ApprenticeshipPausedEvent message: {createdMessage.ToJson()}");
                await DasMessageSession.Send(createdMessage).ConfigureAwait(false);
            }
        }

        [Then(@"the Payments service should record the paused apprenticeships history")]
        public async Task ThenThePaymentsServiceShouldRecordThePausedApprenticeshipsHistory()
        {
            await WaitForIt(async () =>
            {
                var apprenticeshipIds = ApprovalsApprenticeships.Select(apprenticeship => apprenticeship.Id).ToArray();

                var savedApprenticeshipPauses = await TestDataContext.ApprenticeshipPause.AsNoTracking()
                    .Where(o => apprenticeshipIds.Contains(o.ApprenticeshipId))
                    .ToListAsync();

                var notFound = new List<ApprovalsApprenticeship>();
                foreach (var approvalsApprenticeship in ApprovalsApprenticeships)
                {
                    var (employer, sendingEmployer, provider, learner) = GetApprovalsReferenceData(approvalsApprenticeship);

                    var apprenticeshipPause = savedApprenticeshipPauses.FirstOrDefault(o => approvalsApprenticeship.Id == o.ApprenticeshipId);

                    if (apprenticeshipPause == null)
                    {
                        Console.WriteLine($"Failed to find apprenticeship pause for apprenticeship Id: {approvalsApprenticeship.Id}, Uln: {learner.Uln}.");
                        notFound.Add(approvalsApprenticeship);
                        continue;
                    }

                    if (approvalsApprenticeship.PauseOnDate.ToDate() == apprenticeshipPause.PauseDate &&
                        approvalsApprenticeship.ResumedOnDate.ToNullableDate() == apprenticeshipPause.ResumeDate)
                    {
                        Console.WriteLine(
                            $"Matched pause for apprenticeship: {approvalsApprenticeship.Identifier}, leaner: {approvalsApprenticeship.Learner}, Employer: {approvalsApprenticeship.Employer}");
                    }
                    else
                    {
                        Console.WriteLine(
                            $"Failed to validate stored apprenticeship pause details. Apprenticeship details: {approvalsApprenticeship.ToJson()}, saved details: {apprenticeshipPause.ToJson()}.");
                        notFound.Add(approvalsApprenticeship);
                    }
                }

                if (!notFound.Any())
                    return true;
                notFound.ForEach(apprenticeship => Console.WriteLine($"Failed to find and/or validate paused apprenticeship: {apprenticeship.ToJson()}"));
                return false;
            }, "Failed to find all the stored apprenticeships paused details.");
        }

        [When(@"the Approvals service notifies the Payments service that the apprenticeships has been resumed")]
        public async Task WhenTheApprovalsServiceNotifiesThePaymentsServiceThatTheApprenticeshipsHasBeenResumed()
        {
            foreach (var approvalsApprenticeship in ApprovalsApprenticeships)
            {
                var createdMessage = new CommitmentsV2.Messages.Events.ApprenticeshipResumedEvent()
                {
                    ResumedOn = approvalsApprenticeship.ResumedOnDate.ToDate(),
                    ApprenticeshipId = approvalsApprenticeship.Id,
                };
                Console.WriteLine($"Sending ApprenticeshipPausedEvent message: {createdMessage.ToJson()}");
                await DasMessageSession.Send(createdMessage).ConfigureAwait(false);
            }
        }

        [Given(@"the existing apprenticeships has the following pause history")]
        public async Task GivenTheExistingApprenticeshipsHasTheFollowingPauseHistoryAsync(Table table)
        {
            var pausedModels = table.CreateSet<ApprovalsApprenticeshipPausedModel>();

            foreach (var pausedModel in pausedModels)
            {
                var previousApprenticeship = PreviousApprovalsApprenticeships
                    .FirstOrDefault(x => x.Identifier == pausedModel.Apprenticeship) ??
                                             throw new InvalidOperationException($"Can't find apprenticeship {pausedModel.Apprenticeship}");

                previousApprenticeship.PauseOnDate = pausedModel.PausedOnDate;

                var pauseDbModel = new ApprenticeshipPauseModel
                {
                    ApprenticeshipId = previousApprenticeship.Id,
                    PauseDate = previousApprenticeship.PauseOnDate.ToDate()
                };

                await TestDataContext.ApprenticeshipPause.AddAsync(pauseDbModel).ConfigureAwait(false);
                await TestDataContext.SaveChangesAsync().ConfigureAwait(false);

            }
        }

        [Given(@"the employers provider priority order is as follows")]
        [Given(@"employers change provider priority order as follows")]
        public void GivenTheEmployersProviderPriorityOrderIsAsFollows(Table table)
        {
            ProviderPaymentPriorities = table.CreateSet<ProviderPriority>().ToList();
        }

        [Given(@"the Approvals service notifies the Payments service of Employer Provider Payment Priority Change")]
        [When(@"the Approvals service notifies the Payments service of changes to Employer Provider Payment Priority")]
        public async Task WhenTheApprovalsServiceNotifiesThePaymentsServiceOfEmployerProviderPaymentPriorityChange()
        {
            var createdMessage = new CommitmentsV2.Messages.Events.PaymentOrderChangedEvent
            {
                AccountId = Employers.Single().AccountId,
                PaymentOrder = ProviderPaymentPriorities
                    .Select(x => (int)TestSession.GetProviderByIdentifier(x.ProviderIdentifier).Ukprn)
                    .ToArray()
            };

            Console.WriteLine($"Sending PaymentOrderChangedEvent message: {createdMessage.ToJson()}");
            await DasMessageSession.Send(createdMessage);
        }

        [Then(@"the Payments service should record the Employer Provider Priority")]
        public async Task ThenThePaymentsServiceShouldRecordTheEmployerProviderPriority()
        {
            await WaitForIt(async () =>
            {
                var employer = Employers.Single();
                var savedProviderPriorities = await TestDataContext.EmployerProviderPriority.AsNoTracking()
                    .Where(o => o.EmployerAccountId == employer.AccountId)
                    .ToListAsync()
                    .ConfigureAwait(false);

                var notFound = new List<ProviderPriority>();
                foreach (var expectedProviderPriority in ProviderPaymentPriorities)
                {
                    var provider = TestSession.GetProviderByIdentifier(expectedProviderPriority.ProviderIdentifier);

                    var actualProviderPriority = savedProviderPriorities.FirstOrDefault(o => o.Ukprn == provider.Ukprn);

                    if (actualProviderPriority == null)
                    {
                        Console.WriteLine($"Failed to find Employer Provider Priority  for Ukprn Id: {provider.Ukprn},Employer : {employer.Identifier} Account Id {employer.AccountId}.");
                        notFound.Add(expectedProviderPriority);
                        continue;
                    }

                    if (actualProviderPriority.Order == expectedProviderPriority.Priority)
                    {
                        Console.WriteLine($"Matched Employer Provider Priority  for Ukprn Id: {provider.Ukprn}, Employer : {employer.Identifier} Account Id {employer.AccountId}.");
                    }
                    else
                    {
                        Console.WriteLine(
                            $"Failed to validate stored Employer Provider Priority details. Expected Provider Priority details: {expectedProviderPriority.ToJson()}, saved details: {actualProviderPriority.ToJson()}.");
                        notFound.Add(expectedProviderPriority);
                    }
                }

                if (!notFound.Any())
                    return true;
                notFound.ForEach(apprenticeship => Console.WriteLine($"Failed to find and/or validate employer provider priority: {apprenticeship.ToJson()}"));
                return false;
            }, "Failed to find all the stored employer payment provider priority details.");
        }

        private ApprenticeshipModel CreateApprenticeshipModel(ApprovalsApprenticeship apprenticeshipSpec)
        {
            var (employer, sendingEmployer, provider, learner) = GetApprovalsReferenceData(apprenticeshipSpec);
            var apprenticeshipModel = new ApprenticeshipModel
            {
                Id = apprenticeshipSpec.Id,
                Ukprn = provider.Ukprn,
                AccountId = employer.AccountId,
                TransferSendingEmployerAccountId = sendingEmployer?.AccountId,
                Uln = learner.Uln,
                FrameworkCode = apprenticeshipSpec.FrameworkCode, //TODO change when app bug is fixed
                ProgrammeType = apprenticeshipSpec.ProgrammeType,
                PathwayCode = apprenticeshipSpec.PathwayCode,
                StandardCode = apprenticeshipSpec.StandardCode,
                Status = string.IsNullOrWhiteSpace(apprenticeshipSpec.Status)
                    ? ApprenticeshipStatus.Active
                    : apprenticeshipSpec.Status.ToApprenticeshipPaymentStatus(),
                LegalEntityName = employer.Name,
                EstimatedStartDate = apprenticeshipSpec.StartDate.ToDate(),
                EstimatedEndDate = apprenticeshipSpec.EndDate.ToDate(),
                AgreedOnDate = string.IsNullOrWhiteSpace(apprenticeshipSpec.AgreedOnDate)
                    ? DateTime.UtcNow
                    : apprenticeshipSpec.AgreedOnDate.ToDate(),
                IsLevyPayer = true,
                StopDate = apprenticeshipSpec.StoppedOnDate.ToNullableDate(),
                ApprenticeshipEmployerType = GetNonNullableApprenticeshipEmployerTypeOnApproval(apprenticeshipSpec.EmployerType)
            };

            return apprenticeshipModel;
        }

        private static ApprenticeshipEmployerType? GetApprenticeshipEmployerTypeOnApproval(string employerType)
        {
            switch (employerType)
            {
                case "Levy":
                    return ApprenticeshipEmployerType.Levy;
                case "Non-Levy":
                    return ApprenticeshipEmployerType.NonLevy;
                default:
                    return default(ApprenticeshipEmployerType?);
            }
        }

        private static Model.Core.Entities.ApprenticeshipEmployerType GetNonNullableApprenticeshipEmployerTypeOnApproval(string employerType)
        {
            switch (employerType)
            {
                case "Levy":
                    return Model.Core.Entities.ApprenticeshipEmployerType.Levy;
                default:
                    return Model.Core.Entities.ApprenticeshipEmployerType.NonLevy;
            }
        }

        private static bool MatchesTrainingCode(ApprovalsApprenticeship approvalsApprenticeship, ApprenticeshipModel savedApprenticeship)
        {
            return approvalsApprenticeship.StandardCode > 0
                ? approvalsApprenticeship.StandardCode == savedApprenticeship.StandardCode &&
                  savedApprenticeship.ProgrammeType == 25
                : approvalsApprenticeship.FrameworkCode == savedApprenticeship.FrameworkCode &&
                  approvalsApprenticeship.PathwayCode == savedApprenticeship.PathwayCode &&
                  approvalsApprenticeship.ProgrammeType == savedApprenticeship.ProgrammeType;
        }

        private (ApprovalsEmployer employer, ApprovalsEmployer sendingEmployer, Provider provider, Learner learner) GetApprovalsReferenceData(ApprovalsApprenticeship approvalsApprenticeship)
        {
            var employer = Employers.FirstOrDefault(emp => emp.Identifier == approvalsApprenticeship.Employer);
            if (employer == null) Assert.Fail($"Failed to find employer: {approvalsApprenticeship.Employer}");
            var sendingEmployer =
                Employers.FirstOrDefault(emp => emp.Identifier == approvalsApprenticeship.SendingEmployer);
            var provider = TestSession.GetProviderByIdentifier(approvalsApprenticeship.Provider);
            if (provider == null) Assert.Fail($"Failed to generate provider: {approvalsApprenticeship.Provider}");
            var learner = TestSession.GetLearner(provider.Ukprn, approvalsApprenticeship.Learner) ??
                          throw new InvalidOperationException(
                              $"Failed to get learner for identifier: {approvalsApprenticeship.Learner}");
            return (employer, sendingEmployer, provider, learner);
        }

        private static ApprenticeshipPriceEpisodeModel CreateApprenticeshipPriceEpisode(long apprenticeshipId, ApprovalsApprenticeship.PriceEpisode priceEpisode)
        {
            return new ApprenticeshipPriceEpisodeModel
            {
                ApprenticeshipId = apprenticeshipId,
                Cost = priceEpisode.AgreedPrice,
                StartDate = priceEpisode.EffectiveFrom.ToDate(),
                EndDate = priceEpisode.EffectiveTo.ToNullableDate()
            };
        }

        private static bool MatchPriceEpisodes(List<ApprovalsApprenticeship.PriceEpisode> expectedPriceEpisodes, List<ApprenticeshipPriceEpisodeModel> actualPriceEpisodes)
        {
            if (expectedPriceEpisodes == null) return true;


            actualPriceEpisodes = actualPriceEpisodes.Where(x => !x.Removed).ToList();

            if (expectedPriceEpisodes.Count != actualPriceEpisodes.Count) return false;

            foreach (var expectedPriceEpisode in expectedPriceEpisodes)
            {
                var actualPriceEpisode = actualPriceEpisodes
                    .FirstOrDefault(x => x.Removed == false &&
                                         x.StartDate == expectedPriceEpisode.EffectiveFrom.ToDate() &&
                                         x.Cost == expectedPriceEpisode.AgreedPrice &&
                                         x.EndDate == expectedPriceEpisode.EffectiveTo.ToNullableDate());

                if (actualPriceEpisode == null) return false;

            }

            return true;
        }
    }
}