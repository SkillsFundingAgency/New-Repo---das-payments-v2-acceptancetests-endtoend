﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using NServiceBus.Features;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;
using SFA.DAS.Payments.AcceptanceTests.Core.Infrastructure;
using SFA.DAS.Payments.AcceptanceTests.Core.Services;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Data;
using SFA.DAS.Payments.AcceptanceTests.EndToEnd.Infrastructure;
using SFA.DAS.Payments.Core;
using SFA.DAS.Payments.Messages.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Tests.Core;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Steps
{

    [Binding]
    public class ApprovalsInterfaceSteps : EndToEndStepsBase
    {
        public List<ApprovalsEmployer> Employers
        {
            get => Get<List<ApprovalsEmployer>>();
            set => Set(value);
        }

        public List<ApprovalsApprenticeship> ApprovalsApprenticeships
        {
            get => Get<List<ApprovalsApprenticeship>>();
            set => Set(value);
        }

        public static IMessageSession DasMessageSession { get; set; }
        private static EndpointConfiguration dasEndpointConfiguration;
        protected TestPaymentsDataContext TestDataContext => Scope.Resolve<TestPaymentsDataContext>();

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

        [Given(@"the apprenticeships have the following price episodes")]
        public void GivenTheApprenticeshipsHaveTheFollowingPriceEpisodes(Table table)
        {
            var priceEpisodes = table.CreateSet<ApprovalsApprenticeship.PriceEpisode>();
            foreach (var priceEpisode in priceEpisodes)
            {
                Console.WriteLine($"adding price episode to apprenticeship. Price episode: {priceEpisode.ToJson()}");
                var apprenticeship = ApprovalsApprenticeships.FirstOrDefault(appr => appr.Identifier == priceEpisode.Apprenticeship);
                if (apprenticeship == null)
                    Assert.Fail($"Failed to find the apprenticeship for price episode: {priceEpisode.ToJson()}");
                apprenticeship.PriceEpisodes.Add(priceEpisode);
            }
        }

        [When(@"the Approvals service notifies the Payments service of the apprenticeships")]
        public void WhenTheApprovalsServiceNotifiesPaymentsVOfTheApprenticeships()
        {
            foreach (var approvalsApprenticeship in ApprovalsApprenticeships)
            {
                var employer = Employers.FirstOrDefault(emp => emp.Identifier == approvalsApprenticeship.Employer);
                if (employer == null)
                    Assert.Fail($"Failed to find employer: {approvalsApprenticeship.Employer}");
                var sendingEmployer =
                    Employers.FirstOrDefault(emp => emp.Identifier == approvalsApprenticeship.SendingEmployer);
                var provider = TestSession.GetProviderByIdentifier(approvalsApprenticeship.Provider);
                if (provider == null)
                    Assert.Fail($"Failed to generate provider: {approvalsApprenticeship.Provider}");
                var learner = TestSession.GetLearner(provider.Ukprn, approvalsApprenticeship.Learner) ?? throw new InvalidOperationException($"Failed to get learner for identifier: {approvalsApprenticeship.Learner}");
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
                    TrainingType = approvalsApprenticeship.StandardCode > 0 ? ProgrammeType.Standard : ProgrammeType.Framework,
                    TrainingCode = approvalsApprenticeship.StandardCode > 0 ? approvalsApprenticeship.StandardCode.ToString() : $"{approvalsApprenticeship.FrameworkCode}-{approvalsApprenticeship.ProgrammeType}-{approvalsApprenticeship.PathwayCode}",
                    TransferSenderId = sendingEmployer?.AccountId,
                    Uln = learner.Uln.ToString(),
                    PriceEpisodes = approvalsApprenticeship.PriceEpisodes.Select(pp => new CommitmentsV2.Messages.Events.PriceEpisode
                    {
                        FromDate = pp.EffectiveFrom.ToDate(),
                        ToDate = pp.EffectiveTo?.ToDate(),
                        Cost = pp.Amount
                    }).ToArray()
                };
                Console.WriteLine($"Sending CreatedApprenticeship message: {createdMessage.ToJson()}");
                DasMessageSession.Send(createdMessage).ConfigureAwait(false);
            }
        }

        [Then(@"the Payments service should record the apprenticeships")]
        public async Task ThenPaymentsVShouldRecordTheApprenticeships()
        {
            await WaitForIt(async () =>
            {
                var apprenticeshipIds = ApprovalsApprenticeships.Select(apprenticeship => apprenticeship.Id).ToArray();
                var savedApprenticeships = await TestDataContext.Apprenticeship
                    .Include(apprenticeship => apprenticeship.ApprenticeshipPriceEpisodes)
                    .Where(apprenticeship => apprenticeshipIds.Contains(apprenticeship.Id)).ToListAsync();
                var notFound = new List<ApprovalsApprenticeship>();
                foreach (var approvalsApprenticeship in ApprovalsApprenticeships)
                {
                    var employer = Employers.FirstOrDefault(e => e.Identifier == approvalsApprenticeship.Employer) ?? throw new InvalidOperationException($"Failed to find the employer: {approvalsApprenticeship.Employer}");
                    var provider = TestSession.GetProviderByIdentifier(approvalsApprenticeship.Provider) ??
                                   throw new InvalidOperationException($"Failed to find the provider: {approvalsApprenticeship.Provider}");
                    var learner = TestSession.GetLearner(provider.Ukprn, approvalsApprenticeship.Learner) ?? throw new InvalidOperationException($"Failed to find the learner.  Ukrpn: {provider.Ukprn}");
                    var savedApprenticeship = savedApprenticeships.FirstOrDefault(apprenticeship => approvalsApprenticeship.Id == apprenticeship.Id);
                    if (savedApprenticeship == null)
                    {
                        Console.WriteLine($"Failed to find apprenticeship Id: {approvalsApprenticeship.Id}, Uln: {learner.Uln}.");
                        notFound.Add(approvalsApprenticeship);
                        continue;
                    }

                    if (MatchesTrainingCode(approvalsApprenticeship,savedApprenticeship) &&
                        provider.Ukprn == savedApprenticeship.Ukprn &&
                        employer.AccountId == savedApprenticeship.AccountId && 
                        learner.Uln == savedApprenticeship.Uln)
                    {
                        Console.WriteLine($"Matched apprenticeship: {approvalsApprenticeship.Identifier}, leaner: {approvalsApprenticeship.Learner}, Employer: {approvalsApprenticeship.Employer}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to validate stored details for apprenticeship. Apprenticeship details: {approvalsApprenticeship.ToJson()}, saved details: {savedApprenticeship.ToJson()}.");
                        notFound.Add(approvalsApprenticeship);
                    }
                }
                if (!notFound.Any())
                    return true;
                notFound.ForEach(apprenticeship => Console.WriteLine($"Failed to find and/or validate apprenticeship: {apprenticeship.ToJson()}"));
                return false;
            },"Failed to find all the stored apprenticeships.");
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


        [Given(@"the following apprenticeships already exist")]
        public void GivenTheFollowingApprenticeshipsAlreadyExist(Table table)
        {
            ApprovalsApprenticeships = table.CreateSet<ApprovalsApprenticeship>().ToList();
            ApprovalsApprenticeships.ForEach(apprenticeship =>
            {
                apprenticeship.Id = TestSession.GenerateId();
                var newappRenticeship = 


                Console.WriteLine($"Apprenticeship: {apprenticeship.ToJson()}");
            });
        }

        [Given(@"the existing apprenticeships have the following price episodes")]
        public void GivenTheExistingApprenticeshipsHaveTheFollowingPriceEpisodes(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"the apprenticeships are changed has follows")]
        public void GivenTheApprenticeshipsAreChangedHasFollows(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"the apprenticeships are changes to have the following price episodes")]
        public void GivenTheApprenticeshipsAreChangesToHaveTheFollowingPriceEpisodes(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"the Approvals service notifies the Payments service of the apprenticeships changes")]
        public void WhenTheApprovalsServiceNotifiesThePaymentsServiceOfTheApprenticeshipsChanges()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the Payments service should record the chamnges yo the apprenticeships")]
        public void ThenThePaymentsServiceShouldRecordTheChamngesYoTheApprenticeships()
        {
            ScenarioContext.Current.Pending();
        }

    }
}