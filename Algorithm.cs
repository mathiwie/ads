using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflineAlgorithm
{
    internal class Algorithm
    {
        List<Job> currentSolution;
        int bestUpperBound;
        Queue<AlgState> q = new Queue<AlgState>();

        public Algorithm(List<Job> jobs)
        {
            //initialize BFS
            currentSolution = jobs;
            q.Enqueue(getInitialState());
        }

        public void run()
        {
            //BFS is used for branch-and-bound
            while (q.Count > 0)
            {
                //dequeue candidate solution
                AlgState state = q.Dequeue();

                if (state.fixedJobs < state.currentSolution.Count)
                {
                    //select the first job that is has not a fixed position yet
                    Job jobToSwitch = state.currentSolution[state.fixedJobs];

                    //go to every possible ordering of jobs
                    for (int i = state.fixedJobs; i < state.currentSolution.Count; i++)
                    {
                        //switch first non fixed job with other job
                        List<Job> newJobs = state.currentSolution; //this only sets pointer, but should be different list!!!
                        Job otherJobToSwitch = state.currentSolution[i];
                        newJobs[i] = jobToSwitch;
                        newJobs[state.fixedJobs] = otherJobToSwitch;

                        //Add new states to the queue, if they are promising
                        AlgState newState = new AlgState(newJobs, state.fixedJobs + 1);

                        if (state.upperbound > newState.lowerbound)
                        {
                            q.Enqueue(newState);
                        }
                    }

                    //got to every ordering of interruptions
                    List<Job> jobsNew = state.currentSolution; //this only sets pointer, but should be different list!!!
                    for (int i = state.fixedJobs; i < state.currentSolution.Count; i++)
                    {
                        List<Job> newJobs = jobsNew;//this only sets pointer, but should be different list!!!
                        Job otherJobToSwitch = state.currentSolution[i];

                        if (otherJobToSwitch.data.release_time < jobToSwitch.data.release_time + jobToSwitch.data.processing_time && otherJobToSwitch.data.release_time > jobToSwitch.data.release_time) //not sure about this expression
                        {
                            newJobs.Insert(i, jobToSwitch);
                            jobToSwitch.interruptions += 1;
                        }

                        //Add new states to the queue, if they are promising
                        AlgState newState = new AlgState(newJobs, state.fixedJobs + 1);

                        if (state.upperbound > newState.lowerbound)
                        {
                            q.Enqueue(newState);
                        }

                        jobsNew = newJobs;
                    }

                }
            }
        }

        public AlgState getInitialState()
        {
            //sort by release time
            List<Job> initial = currentSolution.OrderBy(o => o.data.release_time).ToList();
            return new AlgState(initial, 0);
        }
    }
}

public class AlgState
{
    public List<Job> currentSolution = new List<Job>();
    public float upperbound;
    public float lowerbound;
    bool feasible = true;
    public int fixedJobs;
    public AlgState(List<Job> jobs, int fixedJobs)
    {
        currentSolution = jobs;
        calcUpperBound();
        calcLowerBound();
        this.fixedJobs = fixedJobs;
    }

    //in the upper bound, a given sequence of job executions must be run in a sequence by a single machine
    private void calcUpperBound()
    {
        float C = 0; //the sum of all finishing times (which is the objective value)
        float lastFinishingTime = 0;
        for (int i = 0; i < currentSolution.Count; i++)
        {
            Job job = currentSolution[i];

            //if there is a gap between the current release time and the finishing time of the last job
            //calculate the length of the gap, and correct the C value
            //so virtually all previous jobs get shifted to the the release time of the current job to close the gap
            if (job.data.release_time > lastFinishingTime)
            {
                float delta = job.data.release_time - lastFinishingTime;
                //correct all previous finnishing times
                C += (i + 1) * delta;
                lastFinishingTime += delta;
            }

            if (job.interruptions == 0) //if job gets finished here
            {
                lastFinishingTime += job.remaining_processing_time;
                C += lastFinishingTime;
            }
            else //if the job is finished later
            {
                float currentProcessing = currentSolution[i + 1].data.release_time - lastFinishingTime; //might crash because of the i + 1
                job.elapsed_processing_time += currentProcessing;
                job.remaining_processing_time = (job.remaining_processing_time - currentProcessing) * job.data.interruption_penalty;
                lastFinishingTime += currentProcessing;
                job.interruptions -= 1;
            }
        }

        upperbound = C;
    }

    //in the lower bound, the single machine constraint is relaxed, and jobs can be run parallely
    private void calcLowerBound()
    {
        float C = 0; //the sum of all finishing times (which is the objective value)
        float highestReleaseTime = currentSolution[0].data.release_time;
        float lastFinishingTime = 0;
        for (int i = 0; i < currentSolution.Count; i++)
        {
            Job job = currentSolution[i];
            if (job.interruptions == 0)//if job gets finished here
            {
                //if the job is released before one of the fixed jobs
                //it has to be executed right after that other fixed job
                if (job.data.release_time < highestReleaseTime)
                {
                    float finnishTime = lastFinishingTime + job.remaining_processing_time;
                    C += finnishTime;
                    lastFinishingTime = finnishTime;
                }
                //otherwise it is free from any other job and can be run directly after its own release time
                else
                {
                    float finnishTime = job.data.release_time + job.remaining_processing_time;
                    C += finnishTime;
                    highestReleaseTime = job.data.release_time;
                    lastFinishingTime = finnishTime;
                }
            }
            else //if the job is finished later
            {
                float currentProcessing = currentSolution[i + 1].data.release_time - lastFinishingTime; //how long the current part takes
                job.elapsed_processing_time += currentProcessing;
                job.remaining_processing_time = (job.remaining_processing_time - currentProcessing) * job.data.interruption_penalty;
                lastFinishingTime += currentProcessing;
                job.interruptions -= 1;
            }
        }

        lowerbound = C;
    }
}
