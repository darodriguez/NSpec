using System;
using System.Linq;
using System.Text.RegularExpressions;
using NSpec.Domain;
using NSpec.Domain.Formatters;
using NUnit.Framework;
using SampleSpecs.Bug;
using System.Reflection;
using FluentAssertions;

namespace NSpec.Tests
{
    [TestFixture]
    public class describe_output
    {
        [Test,
         TestCase(typeof(my_first_spec_output),
                  new [] { typeof(my_first_spec) },
                  ""),
         TestCase(typeof(describe_specifications_output),
                  new [] { typeof(describe_specifications) },
                  ""),
         TestCase(typeof(describe_before_output),
                  new [] { typeof(describe_before) },
                  ""),
         TestCase(typeof(describe_contexts_output),
                  new [] { typeof(describe_contexts) },
                  ""),
         TestCase(typeof(describe_pending_output),
                  new [] { typeof(describe_pending) },
                  ""),
         TestCase(typeof(describe_helpers_output),
                  new [] { typeof(describe_helpers) },
                  ""),
         TestCase(typeof(describe_async_helpers_output),
                  new [] { typeof(describe_async_helpers) },
                  ""),
         TestCase(typeof(describe_batman_sound_effects_as_text_output),
                  new [] { typeof(describe_batman_sound_effects_as_text) },
                  ""),
         TestCase(typeof(describe_class_level_output),
                  new [] { typeof(describe_class_level) },
                  ""),
         TestCase(typeof(given_the_sequence_continues_with_2_output),
                  new []
                  {
                      typeof(given_the_sequence_continues_with_2),
                      typeof( given_the_sequence_starts_with_1)
                  },
                  ""),
         TestCase(typeof(describe_exception_output),
                  new [] { typeof(describe_exception) },
                  ""),
         TestCase(typeof(describe_context_stack_trace_output),
                  new [] { typeof(describe_context_stack_trace) },
                  ""),
         TestCase(typeof(describe_ICollection_output),
                  new []
                  {
                      typeof(describe_ICollection),
                      typeof(describe_LinkedList),
                      typeof(describe_List)
                  },
                  ""),
         TestCase(typeof(describe_changing_stacktrace_message_output),
                  new [] { typeof(describe_changing_stacktrace_message) },
                  ""),
         TestCase(typeof(describe_changing_failure_exception_output),
                  new [] { typeof(describe_changing_failure_exception) },
                  ""),
         TestCase(typeof(describe_focus_output),
                  new [] { typeof(describe_focus) },
                  "focus"),
         TestCase(typeof(describe_capturing_example_console_output),
                    new[] { typeof(describe_capturing_example_console) },
                    ""),
         TestCase(typeof(describe_capturing_context_console_output),
                    new[] { typeof(describe_capturing_context_console) },
                    "")]

        public void output_verification(Type output, Type []testClasses, string tags)
        {
            var finder = new SpecFinder(testClasses, "");
            var tagsFilter = new Tags().Parse(tags);
            var builder = new ContextBuilder(finder, tagsFilter, new DefaultConventions());
            var consoleFormatter = new ConsoleFormatter();

            var actual = new System.Collections.Generic.List<string>();
            consoleFormatter.WriteLineDelegate = actual.Add;

            var runner = new ContextRunner(tagsFilter, consoleFormatter, false);
            runner.Run(builder.Contexts().Build());

            var expectedString = GetExpectedOutput(output)
                .ScrubNewLines()
                .ScrubStackTrace()
                .ScrubTimes();

            var actualString = String.Join("\n", actual)
                .ScrubStackTrace()
                .Trim()
                .ScrubTimes();

            actualString.Should().Be(expectedString);
        }

        private static string GetExpectedOutput(Type output)
        {
            return output.GetField("Output").GetValue(null) as string;
        }
    }

    public static class OutputStringExtensions
    {
        public static string ScrubNewLines(this string s)
        {
            return s.Trim().Replace("\r\n", "\n").Replace("\r", "");
        }

        public static string ScrubTimes(this string s)
        {
            string withoutTime = Regex.Replace(s, @" \([0-9]+(ms|s)\)", " (__ms)");

            return Regex.Replace(withoutTime, @" \([0-9]+min [0-9]+s\)", " (__ms)");
        }

        public static string ScrubStackTrace(this string s)
        {
            // Sort of a patch here: it could actually be generalized to more languages

            var withoutStackTrace = s.Split('\n')
                .Where(a => !a.Trim().StartsWith("at "))      // English OS
                .Where(a => !a.Trim().StartsWith("in "));     // Italian OS

            return String.Join("\n", withoutStackTrace).Replace("\r", "");
        }
    }
}
