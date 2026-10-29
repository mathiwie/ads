using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflineAlgorithm
{
    public class Program
    {
        public static void Main(string[] args)
        {
            JobData[] jobs = Parse();
            var initial = new Schedule(jobs);

            float best = initial.CalculateUpperBound();
            Queue<Schedule> BFS = new Queue<Schedule>();
            BFS.Enqueue(initial);
            while (BFS.Count > 0)
            {
                var schedule = BFS.Dequeue();
                if (!schedule.isFinished)
                {
                    schedule.GenerateNextSchedules(BFS);
                }
                else
                {
                    if (schedule.cost < best)
                    {
                        initial = schedule;
                        best = schedule.cost;
                    }

                }
            }
            initial.PrintChronologicalFragments();
            Console.WriteLine(best);
        }

        private static JobData[] Parse()
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
            return jobs;
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
    public struct JobState
    {
        public readonly JobData data; // a pointer to the static job data
        public ExecutionFragment[] executionFragments; // these execution fragments represent when and for how long we've worked on this job
        public float total_processing_time; // the processing time of a job (including all the incurred interruption penalties)
        public float elapsed_processing_time; // the amount of time already dedicated to this job

        // derived data
        public float remaining_processing_time { get { return total_processing_time - elapsed_processing_time; } }
        public bool isFinished { get { return remaining_processing_time <= 1e-5f; } }

        // constructor for the initial job state
        public JobState(JobData job)
        {
            data = job;
            total_processing_time = job.processing_time;
            elapsed_processing_time = 0;
            executionFragments = [];
        }

        // constructor for a mutated job state where more work has been done
        private JobState(JobState previous_state, ExecutionFragment added_work)
        {
            data = previous_state.data; // static data is unchanged
            elapsed_processing_time = previous_state.elapsed_processing_time + added_work.duration; // elapse the work's processing time
            total_processing_time = previous_state.total_processing_time; // unchanged

            if (previous_state.executionFragments.Length > 0)
            {
                executionFragments = previous_state.executionFragments.ToArray(); // copy

                ExecutionFragment last = executionFragments[executionFragments.Length - 1];
                if (last.end == added_work.start)
                { // no interruption, so change last fragment
                    last.duration += added_work.duration;
                    executionFragments[executionFragments.Length - 1] = last;
                }
                else // interrupted, so create a new fragment
                    executionFragments = executionFragments.Append(added_work).ToArray();
            }
            else
                executionFragments = [added_work];
        }

        // spend time doing this job
        public JobState Do(float start_time, float duration)
        {
            var work = new ExecutionFragment(start_time, duration);
            return new JobState(this, work);
        }

        public void Interrupt()
        {
            this.total_processing_time *= data.interruption_penalty;
        }
    }

    public struct ExecutionFragment
    {
        public float start, duration;
        public float end => start + duration;

        public ExecutionFragment(float start, float duration)
        {
            this.start = start;
            this.duration = duration;
        }
    }








    public class Schedule
    {
        public void PrintChronologicalFragments()
        {
            var sortedFragments = jobs
                .SelectMany(j => j.executionFragments.Select(f => (JobId: j.data.id, Fragment: f)))
                .OrderBy(x => x.Fragment.start);

            Console.WriteLine();
            foreach (var item in sortedFragments)
            {
                Console.WriteLine($"{item.JobId}, {item.Fragment.start}, {item.Fragment.duration}");
            }
        }



        // the schedule stores the job states 
        // (they are sorted by smallest release time)
        public JobState[] jobs;

        public int active_job_index = -1; // to track if the schedule is currently assigned to a job, we store its index in the jobs list
        public float time, cost = 0f;

        HashSet<int> finished_job_indices = new HashSet<int>();

        // derived data
        public bool isActive => active_job_index >= 0;
        public float remaining_processing_time => jobs.Sum(j => j.remaining_processing_time);
        public bool isFinished => remaining_processing_time <= 1e-5f;
        public List<JobState> unfinished_jobs => jobs.Where(j => !j.isFinished).ToList();
        public List<JobState> waiting_jobs => jobs.Where(j => !j.isFinished && j.data.release_time <= time).ToList();




        // constructor for the inital (empty) schedule
        public Schedule(JobData[] jobs)
        {
            // sort the job data on shortets release time and turn them into job states
            this.jobs = jobs
                .OrderBy(job => job.release_time)
                .Select(j => new JobState(j))
                .ToArray();
        }



        // calculate the absolute minimum lower bound of cost that a valid schedule
        // (continuing from the current time with this schedule as a base) can have
        //
        // the absolute minimum is a schedule which magically can do all jobs as soon as
        // they are released (or if the release time was in the past, from this current time),
        // without any interruptions (even if overlapping with another job)
        public float CalculateLowerBound()
        {
            // use the current cost as base
            float total = this.cost;
            foreach (JobState unfinished_job in unfinished_jobs)
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
            List<JobState> unfinished = this.unfinished_jobs.ToList(); // copy

            // if there is a job busy right now, finish that first
            if (isActive)
            {
                var active = jobs[active_job_index];
                float required_time = active.remaining_processing_time;

                // jump ahead to the time where this job is finished
                t += required_time;
                // add the completion time to the total
                total += t;

                // dont forget to remove the job from the unfinished jobs
                unfinished.RemoveAll(j => j.data.id == active.data.id);
            }

            //var unfinished_sorted = unfinished.OrderBy(j => j.total_processing_time).ToList();

            // we want a complete schedule to finish each job, 
            // so we loop untill no job has any time left
            while (unfinished.Count > 0)
            {
                var waiting = unfinished.Where(j => j.data.release_time <= t);
                if (waiting.Any())
                {
                    var shortest = waiting.MinBy(j => j.total_processing_time);

                    float required_time = shortest.remaining_processing_time;
                    // finish this job without interruption and fast forward the time to when
                    // this job is finished
                    t += required_time;
                    // add the completion time to the total
                    total += t;

                    // dont forget to remove the job from the unfinished jobs
                    unfinished.RemoveAll(j => j.data.id == shortest.data.id);
                }
                else
                {
                    t = unfinished.Min(j => j.data.release_time);
                }

            }

            Debug.WriteLine($"upper bound: {total}");
            return total;
        }


        private float NextReleaseTime(float t)
        {
            // this loop gets the next job that is released
            // (only works because jobs are sorted by release time)
            foreach (JobState job in jobs)
                if (job.data.release_time > t)
                    return job.data.release_time;

            return -1f;
        }

        // this function generates all reasonable next schedules which expand this one
        public void GenerateNextSchedules(Queue<Schedule> schedules)
        {
            float next_release_time = NextReleaseTime(time);
            bool isLast = NextReleaseTime(time) < 0f;

            // distinguish between the machine currently being busy or not
            if (!isActive)
            {
                // first we consider starting some waiting job
                // now the expanded schedules are all schedules that
                // start/continue with a waiting job starting at this time
                foreach (JobState job in waiting_jobs)
                {
                    float finish_time = time + job.remaining_processing_time;

                    // if there is a new release coming up, we work until that or till the job is done
                    // but if there isnt we simply work till the job is done
                    float next_decision_time = isLast ? finish_time : float.Min(next_release_time, finish_time);
                    schedules.Enqueue(DoJob(job.data.id, time, next_decision_time));
                }


                // next we consider doing nothing and skipping to some next job's release time
                var future_jobs = unfinished_jobs.Where(j => j.data.release_time > time);
                foreach (JobState job in future_jobs)
                {
                    float finish_time = job.data.release_time + job.remaining_processing_time;

                    // if there is a new release coming up, we work until that or till the job is done
                    // but if there isnt we simply work till the job is done
                    float next_next_release_time = NextReleaseTime(job.data.release_time);
                    bool next_is_last = next_next_release_time < 0f;
                    float next_decision_time = next_is_last ? finish_time : float.Min(next_next_release_time, finish_time);
                    schedules.Enqueue(DoJob(job.data.id, job.data.release_time, next_decision_time));

                }
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
                var active = jobs[active_job_index];

                // first we consider not interrupting
                //
                // this means we continue the current job until we need to
                // reconsider whether we should interrupt the current job
                // (e.g. when this job finishes/a new job is released)
                float finish_time = time + active.remaining_processing_time;
                float next_decision_time = isLast ? finish_time : float.Min(next_release_time, finish_time);
                schedules.Enqueue(ContinueJob(next_decision_time));


                // next we consider interrupting to do a waiting job
                // now the expanded schedules are all schedules that
                // start/continue with a waiting job starting at this time
                foreach (JobState job in waiting_jobs)
                    if (job.data.id != jobs[active_job_index].data.id) // the job we're interrupting is also in this list
                    {
                        finish_time = time + job.remaining_processing_time;
                        next_decision_time = isLast ? finish_time : float.Min(next_release_time, finish_time);
                        schedules.Enqueue(InterruptJob(job.data.id, next_decision_time));
                    }

            }
        }

        private Schedule ContinueJob(float next_decision_time)
        {
            float new_time = next_decision_time; // update time
            int new_active_job_index = active_job_index; // we continue the same job
            float new_cost = cost; // cost for now
            JobState[] new_jobs = jobs.ToArray(); // copy

            // now some values might need to be updated, we start with the job state of the job we continue
            new_jobs[active_job_index] = new_jobs[active_job_index].Do(time, new_time - time); // we worked on the job for duration time

            // if the job is done, we will no longer have an active job and its finish time will be added to the cost 
            if (new_jobs[active_job_index].isFinished)
            {
                new_active_job_index = -1;
                new_cost += new_time;
            }

            return new Schedule(new_time, new_cost, new_active_job_index, new_jobs);
        }
        private Schedule InterruptJob(int next_job_id, float next_decision_time)
        {
            float new_time = next_decision_time; // update time

            JobState[] new_jobs = jobs.ToArray(); // copy
            new_jobs[active_job_index].Interrupt(); // interrupt current job

            int new_active_job_index = Array.FindIndex(new_jobs, j => j.data.id == next_job_id);
            float new_cost = cost; // cost stays the same

            // now some values might need to be updated, we start with the job state of new the job we continue/start
            new_jobs[new_active_job_index] = new_jobs[new_active_job_index].Do(time, new_time - time); // we worked on the job for duration time

            // if the job is done, we will no longer have an active job and its finish time will be added to the cost 
            if (new_jobs[new_active_job_index].isFinished)
            {
                new_active_job_index = -1;
                new_cost += new_time;
            }

            return new Schedule(new_time, new_cost, new_active_job_index, new_jobs);
        }
        private Schedule DoJob(int next_job_id, float start_time, float end_time)
        {
            float new_time = end_time; // update time
            JobState[] new_jobs = jobs.ToArray(); // copy
            int new_active_job_index = Array.FindIndex(new_jobs, j => j.data.id == next_job_id);
            float new_cost = cost; // cost stays the same

            // now some values might need to be updated, we start with the job state of the job we continue
            new_jobs[new_active_job_index] = new_jobs[new_active_job_index].Do(start_time, end_time - start_time); // we worked on the job for this duration

            // if the job is done, we will no longer have an active job and its finish time will be added to the cost 
            if (new_jobs[new_active_job_index].isFinished)
            {
                new_active_job_index = -1;
                new_cost += new_time;
            }

            return new Schedule(new_time, new_cost, new_active_job_index, new_jobs);
        }
        private Schedule(float t, float c, int aji, JobState[] js)
        {
            time = t;
            cost = c;
            jobs = js;
            active_job_index = aji;
        }
    }

}