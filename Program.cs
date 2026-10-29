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
}