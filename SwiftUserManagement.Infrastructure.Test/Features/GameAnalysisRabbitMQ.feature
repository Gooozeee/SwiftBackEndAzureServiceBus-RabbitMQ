﻿Feature: GameAnalysisRabbitMQ

The C# API sends data via RabbitMQ over to the Python AI Analysis code so that the game results can be analysed
and the user can see if there has been a chance of stroke.

This is an integration test

@tag1
Scenario: Emitting game analysis to the RabbitMQ container instance	
	Given The results to be sent through are valid
	| result1 | result2 |
	| 25      | 50      |
	When The Emit Game analysis function is called
	Then The result from this method call should be true

@tag2
Scenario: Emitting invalid game analysis data to the RabbitMQ container instance
	Given The results to be sent through are invalid
	| result1 | result2 |
	| 0       | 50      |
	When The Emit Game analysis function is called
	Then The result from this method call should be false
