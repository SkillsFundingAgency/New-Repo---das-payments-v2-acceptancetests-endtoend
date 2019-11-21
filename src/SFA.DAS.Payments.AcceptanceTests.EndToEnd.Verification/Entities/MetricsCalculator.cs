﻿using System;

namespace SFA.DAS.Payments.AcceptanceTests.EndToEnd.Verification.Entities
{
    public class MetricsCalculator
    {
        public MetricsCalculator(PaymentsValues paymentsValues, DcValues dcValues)
        {
            PaymentsValues = paymentsValues ?? throw new ArgumentNullException(nameof(paymentsValues));
            DcValues = dcValues ?? throw new ArgumentNullException(nameof(dcValues));
        }

        public PaymentsValues PaymentsValues { get; set; }
        public DcValues DcValues { get; set; }



        public decimal DasEarningVsDcEarnings =>
            PaymentsValues.DasEarnings != decimal.Zero ? DcValues.Total / PaymentsValues.DasEarnings : 0m;


        public decimal AccountedForRequiredPayments => 
            PaymentsValues.ExpectedPaymentsAfterPeriodEnd +
            PaymentsValues.AdjustedDataLocks + 
            PaymentsValues.HeldBackCompletionThisMonth;

        public decimal AccountedForDcEarnings =>
            (DcValues.Total != decimal.Zero) ? AccountedForRequiredPayments / DcValues.Total: 0m;

        public decimal AccountedForDasEarnings =>
            (PaymentsValues.DasEarnings != Decimal.Zero) ? AccountedForRequiredPayments / PaymentsValues.DasEarnings :0m;

        public decimal NotAccountedForRequiredPayments =>
            DcValues.Total - AccountedForRequiredPayments;

        public decimal NotAccountedForActualPayments =>
            DcValues.Total - PaymentsValues.AdjustedDataLocks - PaymentsValues.TotalPaymentsYtd;


    }
}