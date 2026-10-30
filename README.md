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
When the program is ran, it expects an input instance, which it will then attempt to solve. The input starts with one line containing the number of jobs, followed by one line per job which contains its release time, processing time and interruption penalty seperated by commas. When the program has found an optimal schedule, it will print one line per execution fragment of the schedule. Each line consists of three values: the job id, start time and duration of the fragment.<br/>

### Example


## The Algorithm in Short
