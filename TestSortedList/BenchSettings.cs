using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSortedList {
    public class CustomLogger : ILogger {

        public string Id => "Mylogger";

        public int Priority => 123;

        bool now_new_line = true;

        public void Write(LogKind logKind, string text) {
            switch (logKind) {
                case LogKind.Statistic:
                case LogKind.Error:
                case LogKind.Warning:
                    ConsoleLogger.Default.Write(logKind, text);
                    now_new_line = false;
                    break;
            }
        }

        public void WriteLine(LogKind logKind, string text) {
            Write(logKind, text);
            WriteLine();
        }

        public void WriteLine() {
            if (now_new_line) return;
            Console.WriteLine();
            now_new_line = true;
        }

        public void Flush() {
            // Implement any necessary flush logic for your custom logger
        }
    }
    public class Config : ManualConfig {
        public Config() {
            AddLogger(new CustomLogger());
            //AddLogger(ConsoleLogger.Default);
            AddColumn(TargetMethodColumn.Method);
            AddColumn(StatisticColumn.Mean);
            AddColumn(StatisticColumn.StdDev);
        }
    }
}
