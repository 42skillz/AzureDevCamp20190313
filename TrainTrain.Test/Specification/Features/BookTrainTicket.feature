Feature: Train Reservation
	Reserve seats in wagons on a train

Rule: For a train overall, no more than 70% of seats may be reserved in advance

	Scenario: May reserve no more than 70% of seats in advance for a train overall 
		Given one coach empty made of 10 seats
		When 3 seats are requested
		Then should assign the reservation these seats "1A, 2A, 3A"

	Scenario: Not reserve seats when it exceeds train max capacity 70%
		Given 1 coaches of 10 seats and 6 already reserved
		When 3 seats are requested
		Then the reservation should be failed

Rule: Ideally no individual coach must have no more than 70% reserved seats

	Scenario: Max 70% reserved seats in one coach
		Given 1 coaches of 10 seats and 5 already reserved
		When 2 seats are requested
		Then should assign the reservation these seats "6A, 7A"

	Scenario: Max 70% reserved seats for all coaches
		Given 2 coaches of 10 seats and 5 seats already reserved in the coach 1
		When 3 seats are requested
		Then should assign the reservation these seats "1B, 2B, 3B"

Rule: All seats for one reservation in the same coach

	Scenario: Each reservation must be booked in the same coach
		Given 2 coaches of 10 seats and 6 seats already reserved in the coach 1
		When 2 seats are requested
		Then should assign the reservation these seats "1B, 2B"

	Scenario: Reservation should failed when coaches are not available
		Given 2 coaches of 10 seats and 7 seats already reserved in the coach 1
		When 8 seats are requested
		Then the reservation should be failed