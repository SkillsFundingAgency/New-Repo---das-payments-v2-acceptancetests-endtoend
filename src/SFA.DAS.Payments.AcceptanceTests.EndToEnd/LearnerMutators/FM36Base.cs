﻿namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.LearnerMutators
{
    using System.Collections.Generic;
    using System.Linq;
    using DCT.TestDataGenerator;
    using DCT.TestDataGenerator.Functor;
    using ESFA.DC.ILR.Model.Loose;

    public abstract class FM36Base : ILearnerMultiMutator
    {
        private const int StandardProgrammeType = 25;
        private readonly string _featureNumber;
        protected TDG.ILearnerCreatorDataCache dataCache;

        protected FM36Base(string featureNumber)
        {
            _featureNumber = featureNumber;
        }

        public FilePreparationDateRequired FilePreparationDate()
        {
            return FilePreparationDateRequired.July;
        }

        public IEnumerable<LearnerTypeMutator> LearnerMutators(TDG.ILearnerCreatorDataCache cache)
        {
            dataCache = cache;

            return CreateLearnerTypeMutators();
        }

        protected abstract IEnumerable<LearnerTypeMutator> CreateLearnerTypeMutators();

        public string RuleName()
        {
            return "FM36_E2E";
        }

        public string LearnerReferenceNumberStub()
        {
            return $"fm36{_featureNumber}";
        }

        protected void MutateCommon(MessageLearner learner, LearnerRequest learnerRequest)
        {
            if (learnerRequest.StartDate.HasValue)
            {
                learner.DateOfBirth = learnerRequest.StartDate.Value.AddYears(-21);
            }

            learner.LearningDelivery[0].LearnPlanEndDateSpecified = learnerRequest.PlannedDurationInMonths.HasValue;

            if (learnerRequest.PlannedDurationInMonths.HasValue && learnerRequest.StartDate.HasValue)
            {
                learner.LearningDelivery[0].LearnPlanEndDate =
                    learnerRequest.StartDate.Value.AddMonths(learnerRequest.PlannedDurationInMonths.Value);
                learner.LearningDelivery[0].LearnPlanEndDateSpecified = true;
            }

            if (learnerRequest.ActualDurationInMonths.HasValue && learnerRequest.StartDate.HasValue && learnerRequest.CompletionStatus == TDG.CompStatus.Completed)
            {
                learner.LearningDelivery[0].LearnActEndDate =
                    learnerRequest.StartDate.Value.AddMonths(learnerRequest.ActualDurationInMonths.Value);
                learner.LearningDelivery[0].LearnActEndDateSpecified = true;
            }

            learner.LearningDelivery[0].LearnStartDateSpecified = learnerRequest.StartDate.HasValue;

            if (learnerRequest.StartDate.HasValue)
            {
                learner.LearningDelivery[0].LearnStartDate = learnerRequest.StartDate.Value;
                learner.LearningDelivery[0].LearnStartDateSpecified = true;
            }

            var ld = learner.LearningDelivery[0];
            var ldfams = ld.LearningDeliveryFAM.Where(s => s.LearnDelFAMType != TDG.LearnDelFAMType.ACT.ToString());
            ld.LearningDeliveryFAM = ldfams.ToArray();
            var lfams = ld.LearningDeliveryFAM.ToList();
            lfams.Add(new MessageLearnerLearningDeliveryLearningDeliveryFAM()
            {
                LearnDelFAMType = TDG.LearnDelFAMType.ACT.ToString(),
                LearnDelFAMCode = ((int)learnerRequest.ContractType).ToString(),
                LearnDelFAMDateFrom = ld.LearnStartDate,
                LearnDelFAMDateFromSpecified = true,
                LearnDelFAMDateTo = ld.LearnActEndDate,
                LearnDelFAMDateToSpecified = ld.LearnActEndDateSpecified,
            });

            ld.LearningDeliveryFAM = lfams.ToArray();

            ld.AppFinRecord[0].AFinDateSpecified = true;
            ld.AppFinRecord[0].AFinDate = ld.LearnStartDate;
            learner.LearningDelivery[0].AppFinRecord =
               learner.LearningDelivery[0].AppFinRecord.Where(af => af.AFinType != TDG.LearnDelAppFinType.TNP.ToString()).ToArray();

            if (learnerRequest.CompletionStatus == TDG.CompStatus.Completed)
            {
                // change AppFin to PMR
                var appfin = new List<MessageLearnerLearningDeliveryAppFinRecord>();
                appfin.Add(new MessageLearnerLearningDeliveryAppFinRecord()
                {
                    AFinAmount = learnerRequest.TotalTrainingPrice.Value,
                    AFinAmountSpecified = true,
                    AFinType = TDG.LearnDelAppFinType.PMR.ToString(),
                    AFinCode = (int)TDG.LearnDelAppFinCode.TrainingPayment,
                    AFinCodeSpecified = true,
                    AFinDate = learner.LearningDelivery[0].LearnActEndDate.AddMonths(-1),
                    AFinDateSpecified = true
                });

                learner.LearningDelivery[0].AppFinRecord = appfin.ToArray();
            }

            if (learnerRequest.TotalTrainingPrice.HasValue && learnerRequest.TotalTrainingPriceEffectiveDate.HasValue)
            {
                DCT.TestDataGenerator.Helpers.AddAfninRecord(learner, TDG.LearnDelAppFinType.TNP.ToString(), (int)TDG.LearnDelAppFinCode.TotalTrainingPrice, learnerRequest.TotalTrainingPrice.Value, 1, "PMR", 1, 1, learnerRequest.TotalTrainingPriceEffectiveDate);
            }

            if (learnerRequest.TotalAssessmentPriceEffectiveDate.HasValue && learnerRequest.TotalAssessmentPrice.HasValue && learnerRequest.ProgrammeType.HasValue && learnerRequest.ProgrammeType.Value == StandardProgrammeType)
            {
                TDG.Helpers.AddAfninRecord(learner, TDG.LearnDelAppFinType.TNP.ToString(), (int)TDG.LearnDelAppFinCode.TotalAssessmentPrice, learnerRequest.TotalAssessmentPrice.Value, 1, "PMR", 1, 1, learnerRequest.TotalAssessmentPriceEffectiveDate);
            }

            foreach (var lds in learner.LearningDelivery)
            {
                if (learnerRequest.ProgrammeType.HasValue)
                {
                    lds.ProgType = learnerRequest.ProgrammeType.Value;
                    lds.ProgTypeSpecified = true;
                }

                if (learnerRequest.FrameworkCode.HasValue)
                {
                    lds.FworkCode = learnerRequest.FrameworkCode.Value;
                    lds.FworkCodeSpecified = true;
                }

                if (learnerRequest.PathwayCode.HasValue)
                {
                    lds.PwayCode = learnerRequest.PathwayCode.Value;
                    lds.PwayCodeSpecified = true;
                }

                if (learnerRequest.CompletionStatus.HasValue)
                {
                    lds.CompStatus = (int)learnerRequest.CompletionStatus.Value;
                    lds.CompStatusSpecified = true;
                }

                if (ld.LearnPlanEndDateSpecified && learnerRequest.CompletionStatus == TDG.CompStatus.Completed)
                {
                    lds.LearnActEndDate = ld.LearnPlanEndDate;
                    lds.LearnActEndDateSpecified = true;
                }

                if (learnerRequest.CompletionStatus == TDG.CompStatus.Completed)
                {
                    lds.Outcome = (int)TDG.Outcome.Achieved;
                    lds.OutcomeSpecified = true;
                }
            }

            // This needs to be fixed (for now) but we need to look at a better way to define these!
            // Component Aim Reference

            learner.LearningDelivery[1].LearnStartDate = ld.LearnStartDate;
            learner.LearningDelivery[1].LearnStartDateSpecified = true;

            var lesm = learner.LearnerEmploymentStatus.ToList();
            lesm[0].DateEmpStatApp = learner.LearningDelivery[0].LearnStartDate.AddMonths(-6);
        }

        protected void MutateHE(MessageLearner learner)
        {
            var ldhe = new List<MessageLearnerLearningDeliveryLearningDeliveryHE>();

            ldhe.Add(new MessageLearnerLearningDeliveryLearningDeliveryHE()
            {
                NUMHUS = "2000812012XTT60021",
                QUALENT3 = TDG.QualificationOnEntry.X06.ToString(),
                UCASAPPID = "AB89",
                TYPEYR = (int)TDG.TypeOfyear.FEYear,
                TYPEYRSpecified = true,
                MODESTUD = (int)TDG.ModeOfStudy.NotInPopulation,
                MODESTUDSpecified = true,
                FUNDLEV = (int)TDG.FundingLevel.Undergraduate,
                FUNDLEVSpecified = true,
                FUNDCOMP = (int)TDG.FundingCompletion.NotYetCompleted,
                FUNDCOMPSpecified = true,
                STULOAD = 10.0M,
                STULOADSpecified = true,
                YEARSTU = 1,
                YEARSTUSpecified = true,
                MSTUFEE = (int)TDG.MajorSourceOfTuitionFees.NoAward,
                MSTUFEESpecified = true,
                PCFLDCS = 100,
                PCFLDCSSpecified = true,
                SPECFEE = (int)TDG.SpecialFeeIndicator.Other,
                SPECFEESpecified = true,
                NETFEE = 0,
                NETFEESpecified = true,
                GROSSFEE = 1,
                GROSSFEESpecified = true,
                DOMICILE = "ZZ",
                ELQ = (int)TDG.EquivalentLowerQualification.NotRequired,
                ELQSpecified = true
            });

            learner.LearningDelivery[1].LearningDeliveryHE = ldhe.ToArray();
        }
    }
}
