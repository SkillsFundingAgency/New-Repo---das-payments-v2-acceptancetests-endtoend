#Levy learner, price is changed, originally mix funded and a negative amount is left to be paid - results in a refund
#    
#    Given  the apprenticeship funding band maximum is 27000
#  
#    And the employer's levy balance is:
#        | 09/18 | 10/18 | 11/18 | 12/18 |
#        | 750   | 375   | 1000  | 1000  |
#        
#    And the following commitments exist:    
#        | commitment Id | version Id | Employer   | Provider   | ULN       | start date | end date   | status | agreed price | effective from | effective to | standard code | programme type |
#        | 1             | 1          | employer 0 | provider a | learner a | 01/08/2018 | 01/08/2019 | active | 11250        | 01/08/2018     | 03/10/2019   | 25            | 25             |
#        | 1             | 2          | employer 0 | provider a | learner a | 01/08/2018 | 01/08/2019 | active | 1400         | 04/10/2018     |              | 25            | 25             |
#    
#    When an ILR file is submitted on R01 with the following data:
#        | ULN       | learner type       | start date | planned end date | actual end date | completion status | Total training price 1 | Total training price 1 effective date | Total assessment price 1 | Total assessment price 1 effective date | aim type  |  standard code | 
#        | learner a | programme only DAS | 04/08/2018 | 20/08/2019       |                 | continuing        | 9000                   | 04/08/2018                            | 2250                     | 04/08/2018                              | programme |  25            | 
#    
#    And an ILR file is submitted on R03 with the following data:
#        | ULN       | learner type       | start date | planned end date | actual end date | completion status | Total training price 1 | Total training price 1 effective date | Total assessment price 1 | Total assessment price 1 effective date | Total training price 2 | Total training price 2 effective date | Total assessment price 2 | Total assessment price 2 effective date | aim type  | standard code |
#        | learner a | programme only DAS | 04/08/2018 | 20/08/2019       |                 | continuing        | 9000                   | 04/08/2018                            | 2250                     | 04/08/2018                              | 1200                   | 04/10/2018                            | 200                      | 04/10/2018                              | programme | 25            |
#    
#    
#Then the provider earnings break down as follows for each collection period:
#    
#        | Transaction Type               | R01  | R02  | R03 | R04 | ... | R12  | 
#        | Provider Earned Total          | 750  | 750  | -100| 0   | ... | 0    | 
#        | On-program                     | 0    | 0    | 0   | 0   | ... | 0    | 
#        | Completion                     | 0    | 0    | 0   | 0   | ... | 0    | 
#        | Balancing                      | 0    | 0    | 0   | 0   | ... | 0    | 
#        | Employer 16-18 incentive       | 0    | 0    | 0   | 0   | ... | 0    | 
#      
#    And the provider payments break down for each delivery period as follows:
#       
#        | Transaction Type               | R01  | R02  | R03  | R04 | ... | R12 |
#        | Provider Paid by SFA           | 750  |712.50| -100 | 0   | ... | 0   |
#        | Refund taken by SFA            | 0    | 0    | -95  | 0   | ... | 0   | 
#        | Payment due from Employer      | 0    |37.50 | 0    | 0   | ... | 0   |
#        | Refund due to employer         | 0    | 0    | 5    | 0   | ... | 0   |
#        | Levy account debited           | 750  | 750  | 0    | 0   | ... | 0   |
#        | Levy account credited          | 0    | 0    | 50   | 0   | ... | 0   | 
#        | SFA Levy employer budget       | 0    | 375  | 0    | 0   | ... | 0   |
#        | SFA Levy co-funding budget     | 0    |337.50| 0    | 0   | ... | 0   |


	Feature:Levy standard learner, price is changed, originally mix funded and a negative amount is left to be paid - results in a refund
	As a provider,
    I want a levy learner, where price is changed, originally mix funded and a negative amount is left to be paid - results in a refund
	So that I am accurately paid my apprenticeship provision. PV2-256a

	Scenario Outline: Levy standard learner, price is changed, orginally mix funded and a negative amount is left to be paid PV2-256a

	Given the employer levy account balance in collection period <Collection_Period> is <Levy_Balance>

	And the following commitments exist
		| Identifier       | start date                   | end date                     | agreed price | Standard Code | Programme Type |status |
		| Apprenticeship a | 01/Aug/Current Academic Year | 31/Jul/Current Academic Year | 11250        | 25            | 25             |active |
       
	And the provider previously submitted the following learner details
		| Start Date                   | Planned Duration | Total Training Price | Total Training Price Effective Date | Completion Status | SFA Contribution Percentage | Contract Type | Aim Sequence Number | Aim Reference | Standard Code | Programme Type | Funding Line Type                                  |
		| 01/Aug/Current Academic Year | 12 months        | 11250                | 01/Aug/Current Academic Year        | continuing        | 90%                         | Act1          | 1                   | ZPROG001      | 25            | 25             | 16-18 Apprenticeship (From May 2017) Levy Contract |

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
	    | Collection Period         | Delivery Period           | Levy Payments | SFA Co-Funded Payments | Employer Co-Funded Payments | Transaction Type |
	    | R01/Current Academic Year | Aug/Current Academic Year | 750           | 0                      | 0                           | Learning         |
	    | R02/Current Academic Year | Sep/Current Academic Year | 375           | 337.5                  | 37.5                        | Learning         |

    But  the Commitment details are changed as follows
		| Identifier       | Learner ID | start date                   | end date                     | agreed price | standard code | programme type | status |
		| Apprenticeship a | learner a  | 01/Aug/Current Academic Year | 31/Jul/Current Academic Year | 1400         | 25            | 25             | active |

    And the Provider now changes the Learner details as follows
		| Start Date                   | Planned Duration | Actual Duration | Programme Type | Completion Status | SFA Contribution Percentage | Contract Type | Aim Reference | standard code |Funding Line Type                                  | 
		| 01/Aug/Current Academic Year | 12 months        | 12 months       | 25             | continuing        | 90%                         | Act1          | ZPROG001      | 25            |16-18 Apprenticeship (From May 2017) Levy Contract | 
																																																
	And price details as follows
        | Price Episode Id  | Total Training Price | Total Training Price Effective Date | Total Assessment Price | Total Assessment Price Effective Date | Contract Type | SFA Contribution Percentage |
        | 1st price details | 9000                 | Aug/Current Academic Year           | 2250                   | Aug/Current Academic Year             | Act1          | 90%                         |
        | 2nd price details | 1200                 | Oct/Current Academic Year           | 200                    | Oct/Current Academic Year             | Act1          | 90%                         |

	When the amended ILR file is re-submitted for the learners in collection period <Collection_Period>

	Then the following learner earnings should be generated
        | Delivery Period           | On-Programme | Completion | Balancing | Price Episode Identifier |
        | Aug/Current Academic Year | 750          | 0          | 0         | 1st price details        |
        | Sep/Current Academic Year | 750          | 0          | 0         | 1st price details        |
        | Oct/Current Academic Year | -100         | 0          | 0         | 2nd price details        |
        | Nov/Current Academic Year | 0            | 0          | 0         | 2nd price details        |
        | Dec/Current Academic Year | 0            | 0          | 0         | 2nd price details        |
        | Jan/Current Academic Year | 0            | 0          | 0         | 2nd price details        |
        | Feb/Current Academic Year | 0            | 0          | 0         | 2nd price details        |
        | Mar/Current Academic Year | 0            | 0          | 0         | 2nd price details        |
        | Apr/Current Academic Year | 0            | 0          | 0         | 2nd price details        |
        | May/Current Academic Year | 0            | 0          | 0         | 2nd price details        |
        | Jun/Current Academic Year | 0            | 0          | 0         | 2nd price details        |
        | Jul/Current Academic Year | 0            | 0          | 0         | 2nd price details        |

	
    And at month end only the following payments will be calculated
        | Collection Period         | Delivery Period           | On-Programme | Completion | Balancing |
        | R03/Current Academic Year | Oct/Current Academic Year | -100         | 0          | 0         |

	And only the following provider payments will be recorded 
        | Collection Period         | Delivery Period           | Levy Payments | SFA Co-Funded Payments | Employer Co-Funded Payments | Transaction Type |
        | R03/Current Academic Year | Oct/Current Academic Year | -50           | -45                    | -5                          | Learning         |

	And only the following provider payments will be generated
        | Collection Period         | Delivery Period           | Levy Payments | SFA Co-Funded Payments | Employer Co-Funded Payments | Transaction Type |
        | R03/Current Academic Year | Oct/Current Academic Year | -50           | -45                    | -5                          | Learning         |

Examples: 
        | Collection_Period         | Levy_Balance              |
#       | R01/Current Academic Year | 750 |
#       | R02/Current Academic Year | 375 |
        | R03/Current Academic Year | 0                         |
        | R04/Current Academic Year | 0                         |
