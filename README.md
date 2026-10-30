# Offline Algorithm for ADS
## Introduction
This algorithm is part of a group project in the course Algorithms for Decision Support (INFOMADS) at Utrecht University. This project is a research project about the single machine scheduling problem with multiplicative interruption penalties. The goal of the project was to find an algorithm in the online context and conduct some proofs with it. <br/>
A major part of project as well, was to write an algorithm for the offline context. That algorithm is provided in this repository.
## Contributors
This project was conducted by:

Dinand Wesdorp (1924354) <br/>
Gijs Vermeulen (9781269) <br/>
Maksim Holly  (4920694) <br/>
Mathis Wierdeier  (8268487) <br/>
Sybren van der Meijden  (2077523) <br/> 

## How to Use
When the program runs, it expects an input instance, which it will then attempt to solve. The input starts with one line containing the number of jobs, followed by one line per job which contains its release time, processing time and interruption penalty separated by commas. When the program has found an optimal schedule, it will print one line per execution fragment of the schedule. Each line consists of three values: the job id, start time and duration of the fragment. The final line will contain the total completion time of the schedule.<br/>

### Example
Given input:<br/>
4<br/>
0, 10, 2<br/>
1, 1, 1.1<br/>
5, 15, 1.5<br/>
5, 1, 1.2<br/>

Program output:<br/>
2, 1.00, 1.00<br/>
1, 2.00, 10.00<br/>
4, 12.00, 1.00<br/>
3, 13.00, 15.00<br/>
55<br/>

## The Algorithm in Short
The algorithm searches the space of all reasonable schedules using Branch and Bound Best First Search. It does this by maintaining a min-heap (PriorityQueue in c#) of schedules, where priority is based on minimal lower bounds. It starts with enqueuing an empty schedule and iteratively branching on possible expansions of the schedule. It also tracks the lowest upper bound seen so far alongside its associated solution. When a schedule is dequeued which has a lower bound greater than or equal to the lowest upper bound, we know that no schedule can be better than the stored lowest upper bound's schedule.