﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis.CommandLine;

namespace Microsoft.CodeAnalysis.CSharp.CommandLine
{
    internal sealed class Csc : CSharpCompiler
    {
        internal Csc(string responseFile, BuildPaths buildPaths, string[] args, IAnalyzerAssemblyLoader analyzerLoader)
            : base(CSharpCommandLineParser.Default, responseFile, args, buildPaths.ClientDirectory, buildPaths.WorkingDirectory, buildPaths.SdkDirectory, Environment.GetEnvironmentVariable("LIB"), analyzerLoader)
        {
        }

        internal static int Run(string[] args, BuildPaths buildPaths, TextWriter textWriter, IAnalyzerAssemblyLoader analyzerLoader)
        {
            FatalError.Handler = FailFast.OnFatalException;

            var responseFile = Path.Combine(buildPaths.ClientDirectory, CSharpCompiler.ResponseFileName);
            var compiler = new Csc(responseFile, buildPaths, args, analyzerLoader);
            return ConsoleUtil.RunWithUtf8Output(compiler.Arguments.Utf8Output, textWriter, tw => compiler.Run(tw));
        }

        protected override uint GetSqmAppID()
        {
            return SqmServiceProvider.CSHARP_APPID;
        }

        protected override void CompilerSpecificSqm(IVsSqmMulti sqm, uint sqmSession)
        {
            sqm.SetDatapoint(sqmSession, SqmServiceProvider.DATAID_SQM_ROSLYN_COMPILERTYPE, (uint)SqmServiceProvider.CompilerType.Compiler);
            sqm.SetDatapoint(sqmSession, SqmServiceProvider.DATAID_SQM_ROSLYN_LANGUAGEVERSION, (uint)Arguments.ParseOptions.LanguageVersion);
            sqm.SetDatapoint(sqmSession, SqmServiceProvider.DATAID_SQM_ROSLYN_WARNINGLEVEL, (uint)Arguments.CompilationOptions.WarningLevel);

            //Project complexity # of source files, # of references
            sqm.SetDatapoint(sqmSession, SqmServiceProvider.DATAID_SQM_ROSLYN_SOURCES, (uint)Arguments.SourceFiles.Length);
            sqm.SetDatapoint(sqmSession, SqmServiceProvider.DATAID_SQM_ROSLYN_REFERENCES, (uint)Arguments.ReferencePaths.Length);
        }
    }
}