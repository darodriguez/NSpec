﻿using NSpec;
using NSpec.Domain;
using NSpecSpecs.WhenRunningSpecs;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSpecSpecs.describe_RunningSpecs
{
    [TestFixture]
    [Category("RunningSpecs")]
    [Category("Async")]
    public class describe_async_after : when_running_specs
    {
        class SpecClass : nspec
        {
            public static int state = 0;

            void given_async_after_is_set()
            {
                it["Should have a specification"] = () => state.should_be(0);

                afterAsync = async () =>
                {
                    state = -1;

                    await Task.Run(() => state = 1);
                };
            }

            void given_async_after_fails()
            {
                afterAsync = async () =>
                {
                    await Task.Run(() =>
                    {
                        throw new InvalidCastException("Some error message");
                    });
                };

                it["Should fail"] = () => true.should_be_true();
            }

            void given_both_sync_and_async_after_are_set()
            {
                after = () =>
                {
                    state = 2;
                };

                afterAsync = async () =>
                {
                    state = -1;

                    await Task.Run(() => state = 1);
                };

                it["Should not know what to do"] = () => true.should_be_true();
            }
        }

        [SetUp]
        public void setup()
        {
            Run(typeof(SpecClass));
        }

        [Test]
        public void async_after_waits_for_task_to_complete()
        {
            ExampleBase example = TheExample("Should have a specification");

            example.HasRun.should_be_true();

            example.Exception.should_be_null();

            SpecClass.state.should_be(1);
        }

        [Test]
        public void async_after_with_exception_fails()
        {
            ExampleBase example = TheExample("Should fail");

            example.HasRun.should_be_true();

            example.Exception.should_not_be_null();
        }

        [Test]
        public void context_with_both_sync_and_async_after_always_fails()
        {
            ExampleBase example = TheExample("Should not know what to do");

            example.HasRun.should_be_true();

            example.Exception.should_not_be_null();
        }
    }
}
