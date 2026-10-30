using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflineAlgorithm
{
    // this represents one fragment of work on a job in a schedule
    public class ExecutionFragment
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

        // prints the execution fragments as the desired output format
        public static void PrintSolution(List<ExecutionFragment> chronological_fragments)
        {
            int i = 0;
            while (i < chronological_fragments.Count)
            {
                ExecutionFragment fragment = chronological_fragments[i];

                // this checks whether sequences of fragments are really one longer segment, 
                // in which case they are combined and printed as one whole
                int j = i + 1;
                while (j < chronological_fragments.Count && chronological_fragments[j].id == fragment.id
                    && MathF.Abs(chronological_fragments[j].start - chronological_fragments[j - 1].end) < 1e-5f)
                {
                    fragment.duration += chronological_fragments[j].duration;
                    j++;
                }
                Console.WriteLine($"{fragment.id}, {fragment.start:F2}, {fragment.duration:F2}");
                i = j;
            }
        }
    }
}
