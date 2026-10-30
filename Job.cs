using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflineAlgorithm
{
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
        public readonly JobData data;                   // a pointer to the static job data
        public ExecutionFragment[] executionFragments;  // these execution fragments represent when and for how long we've worked on this job
        public float total_processing_time;             // the processing time of a job (including all the incurred interruption penalties)
        public float elapsed_processing_time;           // the amount of time already dedicated to this job

        // derived data
        public float remaining_processing_time          // how much time a job has left untill it is finished
        { 
            get 
            { 
                return total_processing_time - elapsed_processing_time; 
            } 
        }
        public bool isFinished                          // true if a job is completely finished, false if not
        { 
            get 
            { 
                return remaining_processing_time <= 1e-5f; 
            } 
        }

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
            data = previous_state.data;                                                             // static data is unchanged
            elapsed_processing_time = previous_state.elapsed_processing_time + added_work.duration; // elapse the work's processing time
            total_processing_time = previous_state.total_processing_time;                           // unchanged
            executionFragments = previous_state.executionFragments.Append(added_work).ToArray();    // with added execution fragment
        }

        // spend time doing this job
        public JobState Do(float start_time, float duration)
        {
            var work = new ExecutionFragment(data.id, start_time, duration);
            return new JobState(this, work);
        }

        public void Interrupt()
        {
            this.total_processing_time *= data.interruption_penalty;
        }
    }

    public struct ExecutionFragment
    {
        public int id;
        public float start, duration;
        public float end => start + duration;

        public ExecutionFragment(int id, float start, float duration)
        {
            this.id = id;
            this.start = start;
            this.duration = duration;
        }
    }
}
