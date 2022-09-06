Feature: CreateUser

Creating a user

@tag1
Scenario: Create user
	Given The database is up and running
	When I hit the create user endpoint
	Then The user will be created
