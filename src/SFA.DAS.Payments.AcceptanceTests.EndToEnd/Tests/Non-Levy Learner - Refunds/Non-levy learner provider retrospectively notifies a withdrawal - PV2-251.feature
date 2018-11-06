﻿Feature: Non-levy learner provider retrospectively notifies a withdrawal - PV2-251
	As a Provider
	I would like TODO
	So that TODO


Scenario:  Provider retrospectively notifies of a withdrawal for a non-levy learner after payments have already been made PV2-251
    Given the provider previously submitted the following learner details
        | ULN       | Priority | Start Date             | Planned Duration | Total Training Price | Total Training Price Effective Date | Total Assesment Price | Total Assesment Price Effective Date | Actual Duration | Programme Type | Completion Status | SFA Contribution Percentage | Contract Type        |
        | learner a | 1        | start of academic year | 12 months        | 9000                 | Aug/Current Academic Year           | 2250                  | Aug/Current Academic Year            | 12 months       | 25             | continuing        | 90%                         | ContractWithEmployer |
    And the following earnings had been generated for the learner
        | Delivery Period           | On-Programme | Completion | Balancing |
        | Aug/Current Academic Year | 750          | 0          | 0         |
        | Sep/Current Academic Year | 750          | 0          | 0         |
        | Oct/Current Academic Year | 750          | 0          | 0         |
        | Nov/Current Academic Year | 750          | 0          | 0         |
        | Dec/Current Academic Year | 750          | 0          | 0         |
        | Jan/Current Academic Year | 750          | 0          | 0         |
        | Feb/Current Academic Year | 750          | 0          | 0         |
        | Mar/Current Academic Year | 750          | 0          | 0         |
        | Apr/Current Academic Year | 750          | 0          | 0         |
        | May/Current Academic Year | 750          | 0          | 0         |
        | Jun/Current Academic Year | 750          | 0          | 0         |
        | Jul/Current Academic Year | 750          | 0          | 0         |
    And the following provider payments had been generated
        | Collection Period         | Delivery Period           | SFA Co-Funded Payments | Employer Co-Funded Payments | On-Programme Earning Type | 
        | R01/Current Academic Year | Aug/Current Academic Year | 675                    | 75                          | Learning                  |
        | R02/Current Academic Year | Sep/Current Academic Year | 675                    | 75                          | Learning                  |
        | R03/Current Academic Year | Oct/Current Academic Year | 675                    | 75                          | Learning                  |
        | R04/Current Academic Year | Nov/Current Academic Year | 675                    | 75                          | Learning                  |
        | R05/Current Academic Year | Dec/Current Academic Year | 675                    | 75                          | Learning                  |
        
    But the Provider now changes the Learner details as follows
        | ULN       | Priority | Start Date             | Planned Duration | Total Training Price | Total Training Price Effective Date | Total Assesment Price | Total Assesment Price Effective Date | Actual Duration | Programme Type | Completion Status | SFA Contribution Percentage | Contract Type        |
        | learner a | 1        | start of academic year | 12 months        | 9000                 | Aug/Current Academic Year           | 2250                  | Aug/Current Academic Year            | 3 months        | 25             | withdrawn         | 90%                         | ContractWithEmployer |
		 
	When the amended ILR file is re-submitted for the learners in collection period R06/Current Academic Year
    Then the following learner earnings should be generated
        | Delivery Period           | On-Programme | Completion | Balancing |
        | Aug/Current Academic Year | 750          | 0          | 0         |
        | Sep/Current Academic Year | 750          | 0          | 0         |
        | Oct/Current Academic Year | 750          | 0          | 0         |
        | Nov/Current Academic Year | 0            | 0          | 0         |
        | Dec/Current Academic Year | 0            | 0          | 0         |
        | Jan/Current Academic Year | 0            | 0          | 0         |
        | Feb/Current Academic Year | 0            | 0          | 0         |
        | Mar/Current Academic Year | 0            | 0          | 0         |
        | Apr/Current Academic Year | 0            | 0          | 0         |
        | May/Current Academic Year | 0            | 0          | 0         |
        | Jun/Current Academic Year | 0            | 0          | 0         |
        | Jul/Current Academic Year | 0            | 0          | 0         |
    And the following payments will be calculated
        | Collection Period         | Delivery Period           | On-Programme | Completion | Balancing |
        | R06/Current Academic Year | Nov/Current Academic Year | -750         | 0          | 0         |
        | R06/Current Academic Year | Dec/Current Academic Year | -750         | 0          | 0         |
    And the following provider payments will be recorded
        | Collection Period			| Delivery Period			| SFA Co-Funded Payments | Employer Co-Funded Payments |On-Programme Earning Type | 
        | R06/Current Academic Year | Nov/Current Academic Year | -675                   | -75                         |Learning                  |
        | R06/Current Academic Year | Dec/Current Academic Year | -675                   | -75                         |Learning                  |
    And at month end the following provider payments will be generated
        | Collection Period			| Delivery Period			| SFA Co-Funded Payments | Employer Co-Funded Payments |On-Programme Earning Type | 
        | R06/Current Academic Year | Nov/Current Academic Year | -675                   | -75                         |Learning                  |
        | R06/Current Academic Year | Dec/Current Academic Year | -675                   | -75                         |Learning                  |