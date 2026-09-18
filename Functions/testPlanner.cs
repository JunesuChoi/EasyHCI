using System;

namespace EasyHCI
{
    /// <summary>
    /// Works out how many HCI MemTest instances to start and how much memory each
    /// one gets. The arithmetic lives here, apart from the form, so it can be
    /// reasoned about and tested without starting a single process.
    ///
    /// Two behaviours differ from the original launcher:
    /// - The logical processor count comes from the runtime instead of a registry
    ///   probe. The registry walk returned zero when a key was missing, and the
    ///   caller then divided by that zero.
    /// - The search for the largest accepted allocation starts from the per-thread
    ///   share instead of a fixed 3530 MB ceiling. The old ceiling was also a hard
    ///   cap, so a machine with more memory per thread than that could never hand
    ///   the extra memory to MemTest.
    /// </summary>
    internal static class testPlanner
    {
        /// <summary>Memory left to Windows when the reserve field is unusable.</summary>
        internal const uint DefaultReserveMB = 300;

        /// <summary>MemTest needs a workable floor; below this a test is pointless.</summary>
        internal const uint MinimumAllocationMB = 256;

        /// <summary>Upper bound on instance count so a bad reading cannot spawn hundreds.</summary>
        internal const uint MaximumInstances = 512;

        /// <summary>MemTest reports the allocation in a ushort field, so cap there.</summary>
        internal const uint MaximumAllocationMB = 65000;

        internal static uint DetectLogicalProcessors()
        {
            int count = Environment.ProcessorCount;
            return count > 0 ? (uint)count : 1u;
        }

        /// <summary>
        /// The allocation to start probing from. It sits just above an even split so
        /// the search converges downward onto the largest value MemTest accepts,
        /// and it scales with the machine instead of stopping at a fixed ceiling.
        /// </summary>
        internal static uint ProbeCeiling(uint freeMemMB, uint threads)
        {
            if (threads == 0) threads = 1;
            uint share = freeMemMB / threads;
            uint ceiling = share + 50;
            if (ceiling < MinimumAllocationMB) return MinimumAllocationMB;
            if (ceiling > MaximumAllocationMB) return MaximumAllocationMB;
            return ceiling;
        }

        /// <summary>Parses the reserve field, falling back to the default.</summary>
        internal static uint ReserveFrom(string text)
        {
            double parsed;
            if (!string.IsNullOrEmpty(text) && double.TryParse(text, out parsed) && parsed >= 0)
                return (uint)parsed;
            return DefaultReserveMB;
        }

        /// <summary>
        /// Splits usable memory across instances. It starts at one instance per
        /// logical processor, because a free-edition instance is single threaded and
        /// more processes than processors only adds context switching. The count
        /// grows only while the per-instance share is larger than what a single
        /// process accepted.
        /// </summary>
        internal static void Distribute(uint freeMemMB, uint reserveMB, uint acceptedMaxMB, uint threads, out uint count, out uint amount)
        {
            if (threads == 0) threads = 1;
            if (reserveMB >= freeMemMB) reserveMB = 0;

            uint usable = freeMemMB - reserveMB;
            count = threads;
            amount = usable / count;

            while (amount > acceptedMaxMB && count < MaximumInstances)
            {
                ++count;
                amount = usable / count;
            }
        }
    }
}
