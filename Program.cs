using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ADS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Scheduler scheduler = Parse();
            Console.WriteLine("Hello, world!");
        }

        /*
        private static Scheduler Parse()
        {
            int num_jobs = int.Parse(Console.ReadLine()!);
            JobData[] jobs = new JobData[num_jobs];
            for (int i = 0; i < num_jobs; i++)
            {
                string line = Console.ReadLine()!;
                string[] values = line.Split(',');
                float r = float.Parse(values[0]);
                float p = float.Parse(values[1]);
                float a = float.Parse(values[2]);
                jobs[i] = new JobData(i + 1, r, p, a);
            }
            return new Scheduler(jobs);*/
        }
    }

    // this object stores all (static) job data associated with some job.
    public class JobData
    {
        public readonly int id;
        public readonly float release_time, processing_time, interruption_penalty;

        public JobData(int id, float release_time, float processing_time, float interruption_penalty)
        {
            this.id = id;
            this.release_time = release_time;
            this.processing_time = processing_time;
            this.interruption_penalty = interruption_penalty;
        }

        public override string ToString()
        {
            return $"Job {this.id}, r: {this.release_time}, p: {this.processing_time}, a: {this.interruption_penalty}";
        }
    }

    // this object represents the state of some job for use in constructing a schedule
    // as opposed to the Job object, this object also contains mutable values, used
    // for example to model interruptions, which add processing time
    public class Job
    {
        public readonly JobData data;

        // the processing time of a job including all the incurred interruption penalties
        public float total_processing_time;
        // the amount of time already dedicated to this job
        public float elapsed_processing_time;
        // the amount of time the job still needs for processing
        public float remaining_processing_time;
        // the amount of times this job gets interrupted.
        public int interruptions;

        // these execution fragments represent when and for how long we've worked on this job
        public List<ExecutionFragment> executionFragments;

        public Job(JobData job)
        {
            data = job;
            total_processing_time = job.processing_time;
            elapsed_processing_time = 0;
            executionFragments = new List<ExecutionFragment>();
            interruptions = 0;
        }

        // spend time doing this job
        public void Do(float current_time, float duration)
        {
            // check if this is a start or continuation
            if (elapsed_processing_time == 0)
            {
                executionFragments.Add(new ExecutionFragment(current_time, duration));
            }
            else // the execution fragments list should not be empty at this point because we've worked on this job in the past
            {
                // check if we continue with or without interruption
                var last_work = executionFragments.Last();
                if (last_work.end == current_time)
                {
                    last_work.duration += duration;
                }
                else // we've been interrupted, so make a new execution fragment
                {
                    executionFragments.Add(new ExecutionFragment(current_time, duration));
                }
            }

            elapsed_processing_time += duration;
        }

        public void Interrupt()
        {
            this.total_processing_time *= data.interruption_penalty;
        }
    }

    public class ExecutionFragment
    {
        public float start, duration;
        public float end => start + duration;

        public ExecutionFragment(float start, float duration)
        {
            this.start = start;
            this.duration = duration;
        }
    }




    //---------------------------------------//
    // EVERYTHING BELOW IS NOT USED, FOR NOW //
    //---------------------------------------//

    /*
    public class Scheduler
    {
        private Node tree;

        public Scheduler(JobData[] jobs)
        {
            Node initialNode = new Node(jobs);
            tree = initialNode;
        }
    }

    public class Schedule
    {
        public Job? active_job;
        public float time, cost;

        public float remaining_processing_time => jobs.Sum(j => j.remaining_processing_time);
        public List<Job> unfinished_jobs => jobs.Where(j => !j.isFinished).ToList();
        public List<Job> waiting_jobs => jobs.Where(j => !j.isFinished && j.data.release_time <= time).ToList();

        // the schedule stores the job states 
        // (they are sorted by smallest release time)
        public Job[] jobs;

        // constructor for the empty schedule
        public Schedule(JobData[] j)
        {
            active_job = null;
            time = 0f;
            cost = 0f;

            // initialize job states given job data and sort them on ascending release time
            this.jobs = j
                .ToList()
                .OrderBy(job => job.release_time)
                .Select(j => new Job(j))
                .ToArray();
        }



        // calculate the absolute minimum lower bound of cost that a valid schedule
        // (continuing from the current time with this schedule as a base)
        // can have
        //
        // the absolute minimum is a schedule which magically can do all jobs as soon as
        // they are released (or if the release time was in the past, from this current time),
        // without any interruptions (even if overlapping with another job),
        public float CalculateLowerBound()
        {
            float total = this.cost;
            foreach (Job unfinished_job in unfinished_jobs)
                total += float.Max(this.time, unfinished_job.data.release_time) + unfinished_job.remaining_processing_time;

            Debug.WriteLine($"lower bound: {total}");
            return total;
        }


        // calculate the upper bound of cost that a valid schedule
        // (continuing from the current time with this schedule as a base)
        // can have
        //
        // naturally the upper bound for valid schedules can be extremely large,
        // perhaps even unbounded, but we want one that is reasonably tight as 
        // it makes pruning more efficient
        //
        // for our upper bound we select a schedule which performs all jobs
        // without interruptions and always picks the shortest available job
        // (from this time on)
        //
        // IMPORTANT: THIS MUTATES THE JOBS OF THE SCHEDULE, SHOULD BE FIXED!
        public float CalculateUpperBound()
        {
            float total = this.cost;
            float t = this.time;


            // if there is a job busy right now, finish that first
            if (active_job != null)
            {
                float required_time = active_job.remaining_processing_time;

                // jump ahead to the time where this job is finished
                t += required_time;
                // dont forget to update the active job's state
                active_job.elapsed_processing_time += required_time;

                // add the completion time to the total
                total += t;
            }


            // we want this a complete schedule to finish each job, 
            // so we loop untill no job has any time left
            while (remaining_processing_time > 0)
            {
                // we want to perform the shortest job that is available
                var shortest_available_job = waiting_jobs
                    .MinBy(j => j.remaining_processing_time);

                // if such a job exists, we perform it. otherwise we fast forward time
                if (shortest_available_job != null)
                {
                    // finish this job without interruption and fast forward the time to when
                    // this job is finished
                    float required_time = shortest_available_job.remaining_processing_time;
                    t += required_time;

                    // dont forget to update the active job's state
                    shortest_available_job.elapsed_processing_time += required_time;

                    // add the completion time to the total
                    total += t;
                }
                else
                {
                    // we start from the time where at least one job that still needs finishing is available
                    t = unfinished_jobs
                        .Min(j => j.data.release_time);
                }

            }

            Debug.WriteLine($"upper bound: {total}");
            return total;
        }

        // constructor to expand existing schedule from a given time
        public Schedule(Schedule base_schedule, float time)
        {
            active_job = base_schedule.active_job;
            cost = base_schedule.cost;
            jobs = base_schedule.jobs;
            this.time = time;
        }

        // this function generates all reasonable next schedules which expand this one
        public List<Schedule> GenerateNextSchedules()
        {
            List<Schedule> expanded_schedules = new List<Schedule>();
            // distinguish between the machine currently being busy or not
            if (active_job == null)
            {
                // we should pick some job to do or skip to an 'eventful' time
                unfinished_jobs.Where();
            }
            // apparently we're already working on a job
            // so now we have two choices: interrupt or continue
            //
            // but how far do we continue? all possible expanded schedules
            // would include future interruptions at all possible times
            // but since we have all the data beforehand we can guarantee
            // that interruptions before the next 'eventful' time are inefficient
            else
            {
                // first we consider not interrupting
                //
                // this means we continue the current job until we need to
                // reconsider whether we should interrupt the current job
                // (i.e. the next time a decision should be made about what to
                //  do next, e.g. when a new job is released/this job finishes)
                float next_event_time = time + active_job.remaining_processing_time;
                if (waiting_jobs.Count > 0 && waiting_jobs[0].data.release_time < next_event_time && waiting_jobs[0].data.release_time > time)// remember that the jobs are sorted by release time, so the unfinished jobs are too, meaning the first item has the smallest release time
                    next_event_time = waiting_jobs[0].data.release_time;

                expanded_schedules.Add(new Schedule(this, next_event_time));

                // next we consider interrupting, but only if this makes sense
                // (i.e. there are jobs waiting to be done)
                if (waiting_jobs.Count > 0)
                {
                    // now the expanded schedules are all schedules that
                    // start/continue with a different job starting at this time
                    foreach (Job job in waiting_jobs)
                    {
                        Schedule diverging_schedule = new Schedule(this, time);
                        diverging_schedule.active_job!.Interrupt();
                        diverging_schedule.active_job = job;


                    }
                }
            }
        }
    }








    public class Node
    {
        public float lower_bound, upper_bound;
        public Schedule schedule;

        public Node? parent;
        public List<Node> children;

        public Node(Node parent, Schedule schedule, float lower_bound, float upper_bound)
        {
            this.schedule = schedule;
            this.lower_bound = lower_bound;
            this.upper_bound = upper_bound;

            this.parent = parent;
            children = new List<Node>();
        }


        public Node(JobData[] jobs)
        {
            this.schedule = new Schedule(jobs);
            this.lower_bound = schedule.CalculateLowerBound();
            this.upper_bound = schedule.CalculateUpperBound();

            this.parent = null;
            children = new List<Node>();
        }

        public void GenerateChildren()
        {

        }
    }
}*/
    