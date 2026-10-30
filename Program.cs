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
            JobData[] jobs = ParseInput();
            var empty_schedule = new Schedule(jobs);
    
            (float lowest_upper_bound, List<ExecutionFragment> solution) = empty_schedule.CalculateUpperBound();
            float lowest_lower_bound = empty_schedule.CalculateLowerBound();

            var Schedules = new PriorityQueue<Schedule, float>();
            Schedules.Enqueue(empty_schedule, lowest_lower_bound);

            Schedule current;
            while (Schedules.Count > 0)
            {
                // since we use a priority queue over lower bound values, and this is the dequeued schedule,
                // we know this lower bound is the lowest
                Schedules.TryDequeue(out current!, out lowest_lower_bound);

                if (!current.isFinished)
                {
                    // if all lower bounds are worse than the lowest upper bound, it means that upper bound is
                    // the optimal solution
                    if (lowest_lower_bound >= lowest_upper_bound) 
                        break;

                    // we check whether we've found a better upper bound, which will help with more
                    // efficient pruning
                    (float upper_bound, var s) = current.CalculateUpperBound();
                    if (upper_bound < lowest_upper_bound) 
                    {
                        lowest_upper_bound = upper_bound;
                        solution = s;
                    }
                        
                    current.GenerateNextSchedules(Schedules);
                }
                else
                {
                    // since we use a priority queue over lower bound values, and this is the dequeued schedule,
                    // it means that all other schedules have lower bound greater than or equal to this lower bound.
                    // so if this is a finished schedule where (lower bound == cost) it means any other possible
                    // finished schedule has cost of at least this cost, so this is the optimal finished schedule

                    solution = current.GetChronologicalFragments();
                    lowest_upper_bound = current.cost;
                    break;
                }
            }
            Console.WriteLine();
            Console.WriteLine(GiveOutput(solution));
            Console.WriteLine(lowest_upper_bound);
        }

        private static JobData[] ParseInput()
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

        private static string GiveOutput(List<ExecutionFragment> chronological_fragments) 
        {
            var merged = new List<ExecutionFragment>();

            foreach (var fragment in chronological_fragments)
            {
                if (merged.Count > 0)
                {
                    var last = merged[^1];
                    bool isSameJob = last.id == fragment.id;
                    bool isContiguous = MathF.Abs(last.end - fragment.start) < 1e-5f;

                    if (isSameJob && isContiguous)
                    {
                        // Extend the last fragment's duration instead of adding a new one
                        merged[^1] = new ExecutionFragment(last.id, last.start, last.duration + fragment.duration);
                        continue;
                    }
                }

                merged.Add(fragment);
            }

            var lines = merged.Select(f => $"{f.id}, {f.start:F2}, {f.duration:F2}");
            return string.Join("\n", lines);
        }
    }
}