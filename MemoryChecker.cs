using System.Diagnostics;

namespace aspdotnet_baza
{
    public class MemoryChecker : IMemoryCheck {
        private readonly Process currentProcess;
        public MemoryChecker() { currentProcess = Process.GetCurrentProcess(); }
        public string MemoryUsedByApp() {
            currentProcess.Refresh();
            var usedMemory = $"memory used by this app: {currentProcess.WorkingSet64 / 1024}";
            return usedMemory;
        }
    }
}
