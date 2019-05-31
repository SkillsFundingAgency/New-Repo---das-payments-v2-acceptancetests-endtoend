﻿using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.Model.Loose;
using SFA.DAS.Payments.AcceptanceTests.Core.Data;

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.LearnerMutators.NonLevy.BasicDay
{
    public class FM36_277 : Framework593Learner
    {
        public FM36_277(IEnumerable<Learner> learners) : base(learners, "277")
        {
                
        }

        protected override void DoSpecificMutate(MessageLearner messageLearner, Learner learner)
        {
            base.DoSpecificMutate(messageLearner, learner);

            foreach (var delivery in messageLearner.LearningDelivery)
            {
                var learnerRequestAim = learner.Aims.SingleOrDefault(aim => aim.AimReference == delivery.LearnAimRef);
                if (learnerRequestAim == null)
                {
                    continue;
                }

                SetDeliveryAsWithdrawn(delivery, learnerRequestAim);

                SetupLearningDeliveryFam(delivery, learnerRequestAim);

                SetupAppFinRecord(messageLearner, delivery, learnerRequestAim);
            }

            var functionalSkillsLearningDelivery = messageLearner.LearningDelivery.Single(learningDelivery => learningDelivery.AimType == 3);
            ProcessMessageLearnerForLearnerRequestOriginatingFromTrainingRecord(functionalSkillsLearningDelivery,
                learner.Aims.First());
        }

    }
}
