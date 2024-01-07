using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Playground.Benchmark;

BenchmarkRunner.Run<Benchmark>(new DebugInProcessConfig());
